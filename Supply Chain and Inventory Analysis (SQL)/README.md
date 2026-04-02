# Supply Chain & Inventory Analytics
### A Portfolio-Grade Case Study | FlowTech Operations Ltd

---

## Project Overview

A complete, end-to-end supply chain analytics solution built to demonstrate advanced SQL, Python analytics, and business intelligence capabilities. The project analyses 18 months of operational data across 5 UK warehouses, 8 suppliers, and 60 product SKUs — uncovering critical inventory failures, supplier risk, and actionable replenishment strategies.

**Analysis Period:** January 2023 – June 2024  
**Scope:** 59,035+ sales orders · 2,595 purchase orders · 21,291 demand history records

---

## Business Problem

FlowTech Operations Ltd is experiencing a severe supply chain breakdown:
- **21% fill rate** — the business fulfils fewer than 1 in 4 units demanded
- **80% backorder rate** — most orders cannot be completed on time
- **£52,900 in damaged inventory** — 30% of total stock value is impaired
- Two suppliers (QuickShip Partners, FastTrack Logistics) have on-time delivery rates below 10%

---

## Files in This Project

| File | Description |
|------|-------------|
| `01_schema.sql` | Full relational schema (11 tables, FK constraints, indexes) |
| `02_advanced_analytics_queries.sql` | 12 advanced SQL modules (ABC, stockout risk, DIO, supplier scorecard, etc.) |
| `01_generate_data.py` | Realistic data generator — 18 months of operational data |
| `02_analytics_charts.py` | Python analytics layer — charts, feature engineering, KPIs |
| `supply_chain.db` | SQLite database with all generated data |
| `Supply_Chain_Executive_Report.docx` | Full executive report (non-technical, for stakeholders) |
| `chart_01_supplier_performance.png` | Supplier OTD & fill rate comparison |
| `chart_02_abc_classification.png` | ABC inventory classification |
| `chart_03_demand_trends.png` | 18-month demand trends by category |
| `chart_04_stockout_heatmap.png` | Stockout risk heatmap by category/warehouse |
| `chart_05_inventory_value.png` | Inventory value & damaged stock by category |
| `chart_06_lead_time_variance.png` | Lead time reliability by supplier |
| `chart_07_warehouse_performance.png` | Warehouse fill rate, revenue, backorder comparison |

---

## Database Schema

```
products ──< inventory >── warehouses
    │             │
    │         stock_movements
    │
    ├──< purchase_orders >── suppliers
    │         │
    │      shipments
    │
    ├──< sales_orders >── warehouses
    │         │
    │       returns
    │
    ├──< demand_history >── warehouses
    │
    └──< lead_times >── suppliers, warehouses
```

---

## SQL Analytics Modules

### 1. ABC Inventory Classification
Window functions, cumulative revenue ranking, 80/15/5 segmentation.

### 2. Stockout Risk Analysis
Rolling demand averages, safety stock formula (Z=1.65), reorder point calculation per SKU/warehouse.

### 3. Overstock & Capital Lock-Up
Weeks of cover calculation, excess units, holding cost quantification.

### 4. Supplier Performance Scorecard
On-time delivery %, fill rate, days deviation, composite weighted score, RANK() window function.

### 5. Warehouse Utilisation & Efficiency
Revenue per operational cost ratio, backorder rate, damage rate per warehouse.

### 6. Inventory Turnover & DIO
COGS tracking via stock movements, annualised turnover, Days Inventory Outstanding, velocity classification (Fast/Medium/Slow/Dead).

### 7. Demand Trend & Anomaly Detection
LAG/LEAD window functions, MoM & YoY comparisons, 3-month rolling average, 2x spike detection.

### 8. Service Level & Fill Rate by Category
Line fill rate, perfect order rate, backorder rate analysis.

### 9. Reorder Priority Queue
Days-to-stockout, recommended order quantity, estimated PO value, urgency flagging.

### 10. Returns Impact Analysis
Return rate %, damage rate %, credit cost, defective return classification.

### 11. Lead Time Variance Impact
Coefficient of variation, P95 lead time estimate, extra safety stock required per supplier.

### 12. Executive KPI Dashboard Query
Single-pass summary query for all headline business metrics.

---

## Key Findings

| Finding | Detail |
|---------|--------|
| Fill Rate | 21% overall — 11% (Food) to 45% (Industrial) |
| Backorder Rate | 80% overall — Food at 90% |
| Supplier Risk | QuickShip: 8% OTD, CV=83%. FastTrack: 7% OTD, CV=66% |
| Damaged Stock | £52,900 — 30.1% of total inventory value |
| Stockout Rate | 73% of trading weeks include at least one stockout |
| Dead Stock | 12 SKUs with negligible demand — consuming space and capital |
| London Central DC | 81% stockout rate — highest in the network |

---

## Strategic Recommendations

**Immediate (0-30 days)**
- Emergency replenishment orders for 30+ critical SKUs
- Performance improvement notice to QuickShip Partners and FastTrack Logistics
- Full damaged goods audit and clearance across all 5 warehouses

**Short-term (30-90 days)**
- Recalculate safety stock using actual demand variability and lead time standard deviation
- Implement dynamic reorder point triggers in the ERP system
- Dead stock liquidation programme for 12 obsolete SKUs

**Long-term (90-180 days)**
- Supplier diversification for Class A Electronics and Apparel SKUs
- Seasonal pre-positioning calendar (6-8 weeks before peak periods)
- Inventory rebalancing across warehouses based on demand density

---

## Technical Stack

- **Database:** SQLite (portable; production equivalent: PostgreSQL / Snowflake)
- **SQL:** Advanced CTE pipelines, window functions, subqueries, aggregations
- **Python:** pandas, numpy, matplotlib, scikit-learn
- **Reporting:** python-docx / docx-js for executive Word report
- **Visualisation:** matplotlib (charts), Chart.js (interactive dashboard)

---

## Interview Talking Points

**"Walk me through your SQL approach"**
> I structured the analytics as independent, modular CTE pipelines — each addressing a specific business question. The stockout risk module uses a nested CTE: first calculating rolling demand stats, then joining to lead time data, then applying the safety stock formula before scoring risk tiers. This approach makes each layer testable and reusable.

**"How did you handle demand variability?"**
> I used LAG/LEAD window functions to compare demand week-over-week and month-over-month, and flagged anomalies where weekly demand exceeded 2x the 3-month rolling average. For safety stock, I used the standard formula with Z=1.65 (95% service level) and incorporated both demand standard deviation and lead time variability.

**"What's the most impactful finding?"**
> The combination of a 21% fill rate and a 30% damaged stock rate tells a clear story: the business isn't just ordering the wrong amounts — it's also failing to protect the stock it does hold. Addressing both simultaneously (safety stock recalibration + damaged goods clearance) would have the largest near-term impact on availability.

**"How would you scale this to a production environment?"**
> Replace SQLite with PostgreSQL or Snowflake, schedule the analytics queries as dbt models, connect to a BI tool (Looker/Power BI) for the dashboard layer, and add an event-driven reorder alert system triggered by the reorder priority queue logic.

---

## Running the Project

```bash
# 1. Generate the database
python 01_generate_data.py

# 2. Run analytics and generate charts
python 02_analytics_charts.py

# 3. Open the database
sqlite3 supply_chain.db

# 4. Run any SQL module from 02_advanced_analytics_queries.sql
.read 02_advanced_analytics_queries.sql
```

---

*Built as a portfolio demonstration of advanced SQL analytics, supply chain domain knowledge, and business-focused reporting.*
