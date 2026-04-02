-- =============================================================================
-- SUPPLY CHAIN ANALYTICS — ADVANCED SQL QUERY LIBRARY
-- All queries tested against the SupplyChain SQLite database
-- =============================================================================

-- ─────────────────────────────────────────────────────────────────────────────
-- SECTION 1: ABC INVENTORY CLASSIFICATION
-- Classifies products by cumulative revenue contribution (80/15/5 rule)
-- ─────────────────────────────────────────────────────────────────────────────

WITH revenue_by_product AS (
    SELECT
        p.product_id,
        p.sku,
        p.product_name,
        p.category,
        p.unit_cost,
        p.unit_price,
        SUM(so.quantity_fulfilled * so.unit_price)  AS total_revenue,
        SUM(so.quantity_fulfilled)                  AS total_units_sold
    FROM sales_orders so
    JOIN products p ON so.product_id = p.product_id
    WHERE so.quantity_fulfilled > 0
    GROUP BY p.product_id, p.sku, p.product_name, p.category, p.unit_cost, p.unit_price
),
ranked AS (
    SELECT *,
        SUM(total_revenue) OVER ()                                       AS grand_total_revenue,
        SUM(total_revenue) OVER (ORDER BY total_revenue DESC
                                  ROWS BETWEEN UNBOUNDED PRECEDING
                                           AND CURRENT ROW)             AS running_revenue,
        RANK() OVER (ORDER BY total_revenue DESC)                        AS revenue_rank
    FROM revenue_by_product
),
abc_classified AS (
    SELECT *,
        running_revenue / grand_total_revenue * 100                      AS cumulative_pct,
        CASE
            WHEN running_revenue / grand_total_revenue <= 0.80 THEN 'A — High Value'
            WHEN running_revenue / grand_total_revenue <= 0.95 THEN 'B — Medium Value'
            ELSE                                                      'C — Low Value'
        END AS abc_class
    FROM ranked
)
SELECT
    revenue_rank,
    sku,
    product_name,
    category,
    abc_class,
    ROUND(total_revenue, 2)      AS total_revenue,
    total_units_sold,
    ROUND(cumulative_pct, 2)     AS cumulative_revenue_pct,
    ROUND(unit_price - unit_cost, 2) AS unit_margin,
    ROUND((unit_price - unit_cost) / unit_price * 100, 1) AS margin_pct
FROM abc_classified
ORDER BY revenue_rank;


-- ─────────────────────────────────────────────────────────────────────────────
-- SECTION 2: STOCKOUT RISK ANALYSIS
-- Products most at risk based on current stock vs projected demand
-- ─────────────────────────────────────────────────────────────────────────────

WITH weekly_demand AS (
    -- Rolling 8-week average demand per product/warehouse
    SELECT
        product_id,
        warehouse_id,
        AVG(units_demanded)          AS avg_weekly_demand,
        STDDEV(units_demanded)       AS demand_std_dev,   -- Not native SQLite but illustrative
        MAX(units_demanded)          AS peak_weekly_demand,
        SUM(stockout_occurred)       AS stockout_count,
        COUNT(*)                     AS total_weeks
    FROM demand_history
    WHERE period_date >= DATE('2024-01-01')
    GROUP BY product_id, warehouse_id
),
lead_time_stats AS (
    SELECT
        product_id,
        warehouse_id,
        MIN(avg_lead_days)           AS best_lead_time,
        MAX(avg_lead_days)           AS worst_lead_time,
        AVG(avg_lead_days)           AS avg_lead_time,
        AVG(std_dev_days)            AS avg_lead_std_dev
    FROM lead_times
    GROUP BY product_id, warehouse_id
),
safety_stock_calc AS (
    -- Safety Stock = Z * sqrt(avg_lt) * demand_std + avg_demand * lt_std
    -- Using Z=1.65 for 95% service level; approximating std_dev from peak variance
    SELECT
        wd.product_id,
        wd.warehouse_id,
        wd.avg_weekly_demand,
        wd.peak_weekly_demand,
        wd.stockout_count,
        lt.avg_lead_time,
        lt.avg_lead_std_dev,
        -- Reorder point = avg demand during lead time + safety stock
        ROUND(wd.avg_weekly_demand / 7 * lt.avg_lead_time
              + 1.65 * (wd.peak_weekly_demand - wd.avg_weekly_demand) / 2, 0)  AS safety_stock,
        ROUND(wd.avg_weekly_demand / 7 * lt.avg_lead_time
              + 1.65 * (wd.peak_weekly_demand - wd.avg_weekly_demand) / 2
              + wd.avg_weekly_demand / 7 * lt.avg_lead_time, 0)                 AS reorder_point
    FROM weekly_demand wd
    JOIN lead_time_stats lt USING (product_id, warehouse_id)
),
risk_assessment AS (
    SELECT
        p.sku,
        p.product_name,
        p.category,
        w.warehouse_name,
        i.quantity_on_hand,
        i.quantity_reserved,
        i.quantity_in_transit,
        (i.quantity_on_hand - i.quantity_reserved)   AS available_stock,
        ss.reorder_point,
        ss.safety_stock,
        ss.avg_weekly_demand,
        ss.stockout_count,
        ROUND((i.quantity_on_hand - i.quantity_reserved)
              / NULLIF(ss.avg_weekly_demand, 0), 1)   AS weeks_of_stock_remaining,
        CASE
            WHEN (i.quantity_on_hand - i.quantity_reserved) <= 0             THEN '🔴 CRITICAL — Stockout'
            WHEN (i.quantity_on_hand - i.quantity_reserved) < ss.safety_stock THEN '🟠 HIGH — Below Safety Stock'
            WHEN (i.quantity_on_hand - i.quantity_reserved) < ss.reorder_point THEN '🟡 MEDIUM — Reorder Now'
            WHEN ss.stockout_count > 3                                         THEN '🟡 MEDIUM — Frequent Stockouts'
            ELSE                                                                '🟢 LOW — Adequate Stock'
        END AS risk_level
    FROM safety_stock_calc ss
    JOIN inventory i   USING (product_id, warehouse_id)
    JOIN products p    ON i.product_id   = p.product_id
    JOIN warehouses w  ON i.warehouse_id = w.warehouse_id
)
SELECT *
FROM risk_assessment
WHERE risk_level NOT LIKE '%LOW%'
ORDER BY
    CASE risk_level
        WHEN '🔴 CRITICAL — Stockout'           THEN 1
        WHEN '🟠 HIGH — Below Safety Stock'     THEN 2
        WHEN '🟡 MEDIUM — Reorder Now'          THEN 3
        ELSE 4
    END,
    weeks_of_stock_remaining;


-- ─────────────────────────────────────────────────────────────────────────────
-- SECTION 3: OVERSTOCK & CAPITAL LOCK-UP ANALYSIS
-- Identifies products with excess inventory tying up working capital
-- ─────────────────────────────────────────────────────────────────────────────

WITH avg_weekly_sales AS (
    SELECT
        product_id,
        warehouse_id,
        AVG(units_fulfilled) AS avg_weekly_sales,
        SUM(units_fulfilled) AS total_units_sold_18m
    FROM demand_history
    GROUP BY product_id, warehouse_id
),
overstock AS (
    SELECT
        p.sku,
        p.product_name,
        p.category,
        w.warehouse_name,
        i.quantity_on_hand,
        aws.avg_weekly_sales,
        -- Weeks of cover = units on hand / weekly sales rate
        ROUND(i.quantity_on_hand / NULLIF(aws.avg_weekly_sales, 0), 1) AS weeks_of_cover,
        -- Holding cost = quantity × unit_cost × annual holding rate / 52
        ROUND(i.quantity_on_hand * p.unit_cost * 0.25 / 52, 2)         AS weekly_holding_cost,
        ROUND(i.quantity_on_hand * p.unit_cost, 2)                       AS inventory_value,
        -- Excess stock = on hand minus 8 weeks of demand (target cover)
        MAX(0, i.quantity_on_hand - ROUND(aws.avg_weekly_sales * 8, 0)) AS excess_units,
        ROUND(MAX(0, i.quantity_on_hand - aws.avg_weekly_sales * 8)
              * p.unit_cost, 2)                                          AS excess_value_locked
    FROM inventory i
    JOIN products p    ON i.product_id   = p.product_id
    JOIN warehouses w  ON i.warehouse_id = w.warehouse_id
    JOIN avg_weekly_sales aws USING (product_id, warehouse_id)
    WHERE i.quantity_on_hand > 0
)
SELECT *,
    CASE
        WHEN weeks_of_cover > 52 THEN '🔴 SEVERE — Dead Stock Risk'
        WHEN weeks_of_cover > 26 THEN '🟠 HIGH — Excess Stock'
        WHEN weeks_of_cover > 16 THEN '🟡 MEDIUM — Elevated Cover'
        ELSE                          '🟢 NORMAL'
    END AS overstock_flag
FROM overstock
WHERE weeks_of_cover > 12   -- More than 3 months cover
ORDER BY excess_value_locked DESC
LIMIT 50;


-- ─────────────────────────────────────────────────────────────────────────────
-- SECTION 4: SUPPLIER PERFORMANCE SCORECARD
-- Measures on-time delivery, fill rate, and lead time variance by supplier
-- ─────────────────────────────────────────────────────────────────────────────

WITH po_analysis AS (
    SELECT
        po.supplier_id,
        po.po_id,
        po.quantity_ordered,
        po.quantity_received,
        po.order_date,
        po.expected_date,
        po.received_date,
        -- Days late (positive = late, negative = early)
        CAST(JULIANDAY(po.received_date) - JULIANDAY(po.expected_date) AS INTEGER) AS days_deviation,
        -- Fill rate per order
        ROUND(CAST(po.quantity_received AS REAL) / po.quantity_ordered * 100, 1) AS order_fill_rate,
        CASE WHEN po.received_date <= po.expected_date THEN 1 ELSE 0 END AS on_time_flag,
        CASE WHEN po.status = 'PARTIAL' THEN 1 ELSE 0 END AS partial_flag
    FROM purchase_orders po
    WHERE po.status IN ('RECEIVED', 'PARTIAL')
      AND po.received_date IS NOT NULL
),
supplier_metrics AS (
    SELECT
        pa.supplier_id,
        COUNT(*)                                        AS total_orders,
        ROUND(AVG(pa.order_fill_rate), 1)              AS avg_fill_rate_pct,
        ROUND(SUM(pa.on_time_flag) * 100.0 / COUNT(*), 1) AS on_time_delivery_pct,
        ROUND(AVG(pa.days_deviation), 1)               AS avg_days_deviation,
        MAX(pa.days_deviation)                         AS worst_delay_days,
        SUM(pa.partial_flag)                           AS partial_deliveries,
        ROUND(AVG(JULIANDAY(po.received_date)
                  - JULIANDAY(po.order_date)), 1)      AS actual_avg_lead_days,
        SUM(po.quantity_ordered)                       AS total_qty_ordered,
        SUM(po.quantity_received)                      AS total_qty_received
    FROM po_analysis pa
    JOIN purchase_orders po ON pa.po_id = po.po_id
    GROUP BY pa.supplier_id
),
ranked_suppliers AS (
    SELECT
        s.supplier_name,
        s.country,
        s.reliability_score          AS internal_reliability_score,
        sm.*,
        -- Composite performance score (weighted)
        ROUND(sm.on_time_delivery_pct * 0.4
              + sm.avg_fill_rate_pct  * 0.4
              + (10 - ABS(sm.avg_days_deviation)) / 10 * 100 * 0.2, 1) AS composite_score,
        RANK() OVER (ORDER BY sm.on_time_delivery_pct DESC,
                              sm.avg_fill_rate_pct DESC)                AS performance_rank
    FROM supplier_metrics sm
    JOIN suppliers s ON sm.supplier_id = s.supplier_id
)
SELECT
    performance_rank,
    supplier_name,
    country,
    total_orders,
    on_time_delivery_pct,
    avg_fill_rate_pct,
    avg_days_deviation,
    worst_delay_days,
    partial_deliveries,
    actual_avg_lead_days,
    composite_score,
    internal_reliability_score,
    CASE
        WHEN composite_score >= 85 THEN '⭐ Excellent'
        WHEN composite_score >= 70 THEN '✅ Good'
        WHEN composite_score >= 55 THEN '⚠️ Needs Improvement'
        ELSE                           '🚨 Critical — Review Contract'
    END AS supplier_tier
FROM ranked_suppliers
ORDER BY performance_rank;


-- ─────────────────────────────────────────────────────────────────────────────
-- SECTION 5: WAREHOUSE UTILISATION & EFFICIENCY
-- ─────────────────────────────────────────────────────────────────────────────

WITH warehouse_inventory_value AS (
    SELECT
        i.warehouse_id,
        SUM(i.quantity_on_hand * p.unit_cost)              AS total_inventory_value,
        SUM(i.quantity_damaged * p.unit_cost)              AS damaged_goods_value,
        SUM(i.quantity_on_hand * p.volume_m3)              AS occupied_volume_m3,
        COUNT(DISTINCT i.product_id)                       AS sku_count,
        SUM(i.quantity_on_hand)                            AS total_units
    FROM inventory i
    JOIN products p ON i.product_id = p.product_id
    GROUP BY i.warehouse_id
),
warehouse_sales AS (
    SELECT
        warehouse_id,
        SUM(quantity_fulfilled * unit_price)               AS total_revenue_18m,
        SUM(quantity_fulfilled)                            AS total_units_sold,
        COUNT(*)                                           AS total_orders,
        SUM(CASE WHEN is_backorder = 1 THEN 1 ELSE 0 END) AS backorders,
        ROUND(SUM(CASE WHEN quantity_fulfilled >= quantity_ordered THEN 1.0 ELSE 0 END)
              * 100.0 / COUNT(*), 1)                       AS order_fill_rate_pct
    FROM sales_orders
    GROUP BY warehouse_id
),
warehouse_returns AS (
    SELECT
        warehouse_id,
        SUM(quantity_returned)                             AS total_returns,
        ROUND(AVG(CASE WHEN condition = 'DAMAGED' THEN 1.0 ELSE 0 END) * 100, 1) AS damage_rate_pct
    FROM returns
    GROUP BY warehouse_id
)
SELECT
    w.warehouse_name,
    w.city,
    w.total_capacity_m3,
    ROUND(wiv.occupied_volume_m3, 1)                        AS occupied_m3,
    ROUND(wiv.occupied_volume_m3 / w.total_capacity_m3 * 100, 1) AS utilisation_pct,
    wiv.sku_count,
    ROUND(wiv.total_inventory_value, 0)                     AS inventory_value,
    ROUND(wiv.damaged_goods_value, 0)                       AS damaged_value,
    ws.total_revenue_18m,
    ws.order_fill_rate_pct,
    ws.backorders,
    wr.total_returns,
    wr.damage_rate_pct,
    w.operational_cost_monthly,
    -- Revenue per £ of operational cost (efficiency ratio)
    ROUND(ws.total_revenue_18m / (w.operational_cost_monthly * 18), 2) AS revenue_per_cost_ratio,
    CASE
        WHEN wiv.occupied_volume_m3 / w.total_capacity_m3 > 0.90 THEN '🔴 Over-utilised'
        WHEN wiv.occupied_volume_m3 / w.total_capacity_m3 > 0.75 THEN '🟡 High Utilisation'
        WHEN wiv.occupied_volume_m3 / w.total_capacity_m3 < 0.40 THEN '🔵 Under-utilised'
        ELSE '🟢 Optimal Range'
    END AS utilisation_status
FROM warehouses w
JOIN warehouse_inventory_value wiv ON w.warehouse_id = wiv.warehouse_id
JOIN warehouse_sales ws            ON w.warehouse_id = ws.warehouse_id
LEFT JOIN warehouse_returns wr     ON w.warehouse_id = wr.warehouse_id
ORDER BY utilisation_pct DESC;


-- ─────────────────────────────────────────────────────────────────────────────
-- SECTION 6: INVENTORY TURNOVER & DAYS INVENTORY OUTSTANDING (DIO)
-- ─────────────────────────────────────────────────────────────────────────────

WITH cogs_by_product AS (
    SELECT
        p.product_id,
        p.sku,
        p.product_name,
        p.category,
        SUM(ABS(sm.quantity) * sm.unit_cost_at_time)      AS total_cogs,
        SUM(ABS(sm.quantity))                             AS units_sold
    FROM stock_movements sm
    JOIN products p ON sm.product_id = p.product_id
    WHERE sm.movement_type = 'SALE'
    GROUP BY p.product_id, p.sku, p.product_name, p.category
),
avg_inventory_value AS (
    SELECT
        product_id,
        AVG(quantity_on_hand) * MAX(p.unit_cost)           AS avg_inventory_value
    FROM inventory i
    JOIN products p USING (product_id)
    GROUP BY product_id
),
turnover AS (
    SELECT
        c.sku,
        c.product_name,
        c.category,
        ROUND(c.total_cogs, 2)                             AS total_cogs_18m,
        ROUND(aiv.avg_inventory_value, 2)                  AS avg_inventory_value,
        -- Annualised inventory turnover
        ROUND(c.total_cogs / NULLIF(aiv.avg_inventory_value, 0) * (12/18.0), 2) AS inventory_turnover_annual,
        -- Days Inventory Outstanding = 365 / turnover
        ROUND(365 / NULLIF(
            c.total_cogs / NULLIF(aiv.avg_inventory_value, 0) * (12/18.0), 0), 0) AS dio_days
    FROM cogs_by_product c
    JOIN avg_inventory_value aiv ON c.product_id = aiv.product_id
)
SELECT *,
    CASE
        WHEN dio_days <= 15                             THEN 'FAST MOVING'
        WHEN dio_days <= 45                             THEN 'MEDIUM MOVING'
        WHEN dio_days <= 90                             THEN 'SLOW MOVING'
        WHEN dio_days > 90 OR dio_days IS NULL          THEN 'DEAD / OBSOLETE'
    END AS velocity_class,
    DENSE_RANK() OVER (ORDER BY inventory_turnover_annual DESC NULLS LAST) AS turnover_rank
FROM turnover
ORDER BY inventory_turnover_annual DESC NULLS LAST;


-- ─────────────────────────────────────────────────────────────────────────────
-- SECTION 7: DEMAND TREND & ANOMALY DETECTION
-- Month-over-month comparison with spike identification
-- ─────────────────────────────────────────────────────────────────────────────

WITH monthly_demand AS (
    SELECT
        product_id,
        STRFTIME('%Y-%m', period_date)          AS yr_month,
        SUM(units_demanded)                     AS monthly_demand,
        SUM(units_fulfilled)                    AS monthly_fulfilled,
        SUM(stockout_occurred)                  AS stockout_weeks,
        AVG(avg_selling_price)                  AS avg_price
    FROM demand_history
    GROUP BY product_id, STRFTIME('%Y-%m', period_date)
),
with_lag AS (
    SELECT *,
        LAG(monthly_demand, 1)  OVER (PARTITION BY product_id ORDER BY yr_month) AS prev_month_demand,
        LAG(monthly_demand, 12) OVER (PARTITION BY product_id ORDER BY yr_month) AS same_month_prior_year,
        AVG(monthly_demand)     OVER (PARTITION BY product_id
                                      ORDER BY yr_month
                                      ROWS BETWEEN 2 PRECEDING AND CURRENT ROW) AS rolling_3m_avg,
        AVG(monthly_demand)     OVER (PARTITION BY product_id)                  AS overall_avg_demand
    FROM monthly_demand
),
anomaly_flagged AS (
    SELECT *,
        ROUND((monthly_demand - prev_month_demand) / NULLIF(prev_month_demand, 0) * 100, 1) AS mom_change_pct,
        ROUND((monthly_demand - same_month_prior_year) / NULLIF(same_month_prior_year, 0) * 100, 1) AS yoy_change_pct,
        -- Flag if this month is more than 2x the rolling average (anomaly)
        CASE WHEN monthly_demand > rolling_3m_avg * 2.0 THEN 1 ELSE 0 END AS is_demand_spike,
        CASE WHEN monthly_demand < rolling_3m_avg * 0.4 THEN 1 ELSE 0 END AS is_demand_trough
    FROM with_lag
)
SELECT
    p.sku,
    p.product_name,
    p.category,
    af.yr_month,
    af.monthly_demand,
    af.monthly_fulfilled,
    af.stockout_weeks,
    ROUND(af.rolling_3m_avg, 0)       AS rolling_3m_avg,
    af.mom_change_pct,
    af.yoy_change_pct,
    af.is_demand_spike,
    af.is_demand_trough,
    ROUND(af.monthly_demand * af.avg_price, 2) AS monthly_revenue
FROM anomaly_flagged af
JOIN products p ON af.product_id = p.product_id
WHERE af.is_demand_spike = 1 OR af.is_demand_trough = 1
ORDER BY af.yr_month DESC, ABS(af.mom_change_pct) DESC;


-- ─────────────────────────────────────────────────────────────────────────────
-- SECTION 8: SERVICE LEVEL & FILL RATE BY CATEGORY
-- ─────────────────────────────────────────────────────────────────────────────

WITH category_service AS (
    SELECT
        p.category,
        COUNT(*)                                                 AS total_order_lines,
        SUM(so.quantity_ordered)                                AS total_qty_ordered,
        SUM(so.quantity_fulfilled)                              AS total_qty_fulfilled,
        SUM(so.is_backorder)                                    AS backorder_lines,
        ROUND(SUM(so.quantity_fulfilled) * 100.0
              / NULLIF(SUM(so.quantity_ordered), 0), 2)         AS line_fill_rate_pct,
        ROUND(SUM(CASE WHEN so.quantity_fulfilled >= so.quantity_ordered THEN 1.0 ELSE 0 END)
              * 100.0 / COUNT(*), 2)                            AS perfect_order_rate_pct,
        ROUND(SUM(so.is_backorder) * 100.0 / COUNT(*), 2)      AS backorder_rate_pct,
        ROUND(SUM(so.quantity_fulfilled * so.unit_price), 2)    AS total_revenue
    FROM sales_orders so
    JOIN products p ON so.product_id = p.product_id
    GROUP BY p.category
)
SELECT *,
    CASE
        WHEN line_fill_rate_pct >= 98 THEN '✅ World Class'
        WHEN line_fill_rate_pct >= 95 THEN '🟢 Good'
        WHEN line_fill_rate_pct >= 90 THEN '🟡 Acceptable'
        ELSE                               '🔴 Below Target'
    END AS service_level_rating
FROM category_service
ORDER BY line_fill_rate_pct DESC;


-- ─────────────────────────────────────────────────────────────────────────────
-- SECTION 9: REORDER PRIORITY QUEUE
-- Products that need immediate replenishment action
-- ─────────────────────────────────────────────────────────────────────────────

WITH demand_stats AS (
    SELECT
        product_id,
        warehouse_id,
        AVG(units_demanded)          AS avg_weekly_demand,
        MAX(units_demanded)          AS max_weekly_demand
    FROM demand_history
    WHERE period_date >= DATE('2024-01-01')
    GROUP BY product_id, warehouse_id
),
lead_time_info AS (
    SELECT
        product_id,
        warehouse_id,
        MIN(supplier_id)             AS preferred_supplier_id,
        MIN(avg_lead_days)           AS best_avg_lead_days
    FROM lead_times
    GROUP BY product_id, warehouse_id
),
reorder_calc AS (
    SELECT
        p.sku,
        p.product_name,
        p.category,
        w.warehouse_name,
        i.quantity_on_hand,
        i.quantity_in_transit,
        i.quantity_reserved,
        ds.avg_weekly_demand,
        lt.best_avg_lead_days,
        -- Reorder point = (demand/day × lead days) + safety stock
        ROUND(ds.avg_weekly_demand / 7 * lt.best_avg_lead_days
              + 1.65 * (ds.max_weekly_demand - ds.avg_weekly_demand) / 2, 0) AS calc_reorder_point,
        -- Recommended order qty = EOQ approximation (3 weeks demand)
        ROUND(ds.avg_weekly_demand * 3 + 1.65 * (ds.max_weekly_demand - ds.avg_weekly_demand), 0) AS recommended_order_qty,
        -- Days until stockout at current demand rate
        ROUND((i.quantity_on_hand - i.quantity_reserved)
              / NULLIF(ds.avg_weekly_demand / 7, 0), 0)                       AS days_to_stockout,
        ROUND(p.unit_cost * ROUND(ds.avg_weekly_demand * 3, 0), 2)            AS estimated_po_value,
        s.supplier_name                                                        AS recommended_supplier
    FROM inventory i
    JOIN products p        ON i.product_id   = p.product_id
    JOIN warehouses w      ON i.warehouse_id = w.warehouse_id
    JOIN demand_stats ds   ON i.product_id   = ds.product_id
                          AND i.warehouse_id  = ds.warehouse_id
    JOIN lead_time_info lt ON i.product_id   = lt.product_id
                          AND i.warehouse_id  = lt.warehouse_id
    JOIN suppliers s       ON lt.preferred_supplier_id = s.supplier_id
    WHERE ds.avg_weekly_demand > 0
)
SELECT *,
    CASE
        WHEN days_to_stockout <= 7                                   THEN '🚨 URGENT — Order Today'
        WHEN days_to_stockout <= best_avg_lead_days                  THEN '🔴 CRITICAL — Stock Arrives After Stockout'
        WHEN (quantity_on_hand - quantity_reserved) < calc_reorder_point THEN '🟡 ORDER NOW — Below Reorder Point'
        ELSE                                                              '🟢 OK'
    END AS action_required
FROM reorder_calc
WHERE (quantity_on_hand - quantity_reserved) < calc_reorder_point
   OR days_to_stockout <= best_avg_lead_days
ORDER BY days_to_stockout ASC NULLS FIRST
LIMIT 30;


-- ─────────────────────────────────────────────────────────────────────────────
-- SECTION 10: RETURNS IMPACT ANALYSIS
-- ─────────────────────────────────────────────────────────────────────────────

WITH return_metrics AS (
    SELECT
        p.category,
        p.product_name,
        p.sku,
        COUNT(r.return_id)                              AS total_returns,
        SUM(r.quantity_returned)                        AS total_units_returned,
        SUM(r.credit_issued)                            AS total_credit_cost,
        SUM(CASE WHEN r.condition = 'DAMAGED' THEN r.quantity_returned ELSE 0 END) AS damaged_units,
        SUM(CASE WHEN r.condition = 'SCRAP'   THEN r.quantity_returned ELSE 0 END) AS scrapped_units,
        SUM(CASE WHEN r.return_reason = 'DEFECTIVE' THEN 1 ELSE 0 END)            AS defective_returns,
        ROUND(AVG(r.quantity_returned), 1)              AS avg_return_qty,
        -- Compare to total sales
        SUM(so.quantity_fulfilled)                      AS total_units_sold
    FROM returns r
    JOIN products p      ON r.product_id  = p.product_id
    JOIN sales_orders so ON r.so_id       = so.so_id
    GROUP BY p.category, p.product_name, p.sku
),
with_rate AS (
    SELECT *,
        ROUND(total_units_returned * 100.0 / NULLIF(total_units_sold, 0), 2) AS return_rate_pct,
        ROUND(damaged_units * 100.0 / NULLIF(total_units_returned, 0), 2)    AS damage_rate_pct
    FROM return_metrics
)
SELECT *,
    CASE
        WHEN return_rate_pct > 10                       THEN '🔴 High Return Rate'
        WHEN return_rate_pct > 5                        THEN '🟡 Elevated Returns'
        WHEN defective_returns > 5                      THEN '⚠️ Quality Issue'
        ELSE                                                 '🟢 Normal'
    END AS return_flag
FROM with_rate
ORDER BY total_credit_cost DESC;


-- ─────────────────────────────────────────────────────────────────────────────
-- SECTION 11: LEAD TIME VARIANCE IMPACT ON AVAILABILITY
-- ─────────────────────────────────────────────────────────────────────────────

WITH lt_variance AS (
    SELECT
        s.supplier_name,
        p.category,
        lt.avg_lead_days,
        lt.std_dev_days,
        lt.max_lead_days,
        -- Coefficient of variation = std_dev / mean (measures relative variability)
        ROUND(lt.std_dev_days / NULLIF(lt.avg_lead_days, 0) * 100, 1)  AS cv_pct,
        -- 95th percentile lead time estimate
        ROUND(lt.avg_lead_days + 1.65 * lt.std_dev_days, 0)            AS p95_lead_days,
        -- Extra safety stock needed due to variability
        ROUND(1.65 * lt.std_dev_days * 10, 0)                          AS extra_safety_units_needed
    FROM lead_times lt
    JOIN suppliers s ON lt.supplier_id = s.supplier_id
    JOIN products  p ON lt.product_id  = p.product_id
),
aggregated AS (
    SELECT
        supplier_name,
        category,
        ROUND(AVG(avg_lead_days), 1)              AS avg_lead_days,
        ROUND(AVG(std_dev_days), 1)               AS avg_std_dev,
        ROUND(AVG(cv_pct), 1)                     AS avg_cv_pct,
        ROUND(MAX(p95_lead_days), 0)              AS worst_case_lead_days,
        SUM(extra_safety_units_needed)             AS total_extra_safety_needed
    FROM lt_variance
    GROUP BY supplier_name, category
)
SELECT *,
    RANK() OVER (ORDER BY avg_cv_pct DESC) AS variability_rank,
    CASE
        WHEN avg_cv_pct > 50 THEN '🔴 Very High Variability — Major Risk'
        WHEN avg_cv_pct > 30 THEN '🟠 High Variability'
        WHEN avg_cv_pct > 15 THEN '🟡 Moderate Variability'
        ELSE                     '🟢 Stable Lead Times'
    END AS variability_rating
FROM aggregated
ORDER BY avg_cv_pct DESC;


-- ─────────────────────────────────────────────────────────────────────────────
-- SECTION 12: KPI SUMMARY DASHBOARD QUERY
-- Single query for executive dashboard headline metrics
-- ─────────────────────────────────────────────────────────────────────────────

WITH kpi_base AS (
    SELECT
        (SELECT ROUND(SUM(quantity_fulfilled * unit_price), 0)
         FROM sales_orders)                                              AS total_revenue,
        (SELECT ROUND(SUM(quantity_on_hand * unit_cost), 0)
         FROM inventory i JOIN products p ON i.product_id = p.product_id) AS total_inventory_value,
        (SELECT ROUND(SUM(quantity_fulfilled) * 100.0 / SUM(quantity_ordered), 2)
         FROM sales_orders)                                              AS overall_fill_rate_pct,
        (SELECT ROUND(SUM(is_backorder) * 100.0 / COUNT(*), 2)
         FROM sales_orders)                                              AS backorder_rate_pct,
        (SELECT COUNT(DISTINCT product_id || '-' || warehouse_id)
         FROM inventory WHERE quantity_on_hand = 0)                     AS sku_locations_stocked_out,
        (SELECT ROUND(AVG(avg_lead_days), 1)
         FROM lead_times)                                               AS avg_lead_time_days,
        (SELECT ROUND(SUM(quantity_returned * unit_price) * 100.0
                      / SUM(quantity_fulfilled * unit_price), 2)
         FROM returns r
         JOIN sales_orders so ON r.so_id = so.so_id
         JOIN products p ON r.product_id = p.product_id)               AS return_rate_pct,
        (SELECT ROUND(SUM(quantity_damaged * unit_cost), 0)
         FROM inventory i JOIN products p ON i.product_id = p.product_id) AS total_damaged_stock_value
)
SELECT
    total_revenue,
    total_inventory_value,
    ROUND(total_revenue / total_inventory_value, 2)     AS inventory_turns_proxy,
    overall_fill_rate_pct,
    backorder_rate_pct,
    sku_locations_stocked_out,
    avg_lead_time_days,
    return_rate_pct,
    total_damaged_stock_value,
    ROUND(total_damaged_stock_value / total_inventory_value * 100, 2) AS damaged_stock_pct
FROM kpi_base;
