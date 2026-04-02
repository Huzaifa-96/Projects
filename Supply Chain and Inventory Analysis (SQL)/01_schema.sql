-- =============================================================================
-- SUPPLY CHAIN & INVENTORY ANALYTICS PROJECT
-- Schema: SupplyChain_Analytics
-- Author: Supply Chain Analytics Team
-- Version: 1.0
-- Description: Full operational schema for supply chain, inventory, and
--              demand analytics across multiple warehouses and suppliers
-- =============================================================================

-- -------------------------
-- DIMENSION TABLES
-- -------------------------

CREATE TABLE IF NOT EXISTS products (
    product_id          INTEGER PRIMARY KEY,
    sku                 TEXT NOT NULL UNIQUE,
    product_name        TEXT NOT NULL,
    category            TEXT NOT NULL,           -- Electronics, Apparel, Food, Industrial, Pharma
    subcategory         TEXT,
    unit_cost           REAL NOT NULL,
    unit_price          REAL NOT NULL,
    weight_kg           REAL,
    volume_m3           REAL,
    shelf_life_days     INTEGER,                 -- NULL = non-perishable
    min_order_qty       INTEGER DEFAULT 1,
    reorder_point       INTEGER,                 -- Calculated dynamically in analytics
    safety_stock        INTEGER,                 -- Calculated dynamically in analytics
    is_active           INTEGER DEFAULT 1        -- Boolean flag
);

CREATE TABLE IF NOT EXISTS suppliers (
    supplier_id         INTEGER PRIMARY KEY,
    supplier_name       TEXT NOT NULL,
    country             TEXT NOT NULL,
    region              TEXT,
    contact_email       TEXT,
    payment_terms_days  INTEGER DEFAULT 30,
    reliability_score   REAL,                    -- 1-10 internal score
    is_preferred        INTEGER DEFAULT 0,
    onboarded_date      TEXT
);

CREATE TABLE IF NOT EXISTS warehouses (
    warehouse_id        INTEGER PRIMARY KEY,
    warehouse_name      TEXT NOT NULL,
    city                TEXT NOT NULL,
    country             TEXT NOT NULL,
    region              TEXT,
    total_capacity_m3   REAL NOT NULL,
    operational_cost_monthly REAL,
    manager_name        TEXT,
    is_active           INTEGER DEFAULT 1
);

CREATE TABLE IF NOT EXISTS lead_times (
    lead_time_id        INTEGER PRIMARY KEY,
    supplier_id         INTEGER NOT NULL,
    product_id          INTEGER NOT NULL,
    warehouse_id        INTEGER NOT NULL,
    avg_lead_days       REAL NOT NULL,
    min_lead_days       INTEGER,
    max_lead_days       INTEGER,
    std_dev_days        REAL,
    last_updated        TEXT,
    FOREIGN KEY (supplier_id)  REFERENCES suppliers(supplier_id),
    FOREIGN KEY (product_id)   REFERENCES products(product_id),
    FOREIGN KEY (warehouse_id) REFERENCES warehouses(warehouse_id)
);

-- -------------------------
-- INVENTORY & MOVEMENT
-- -------------------------

CREATE TABLE IF NOT EXISTS inventory (
    inventory_id        INTEGER PRIMARY KEY,
    product_id          INTEGER NOT NULL,
    warehouse_id        INTEGER NOT NULL,
    quantity_on_hand    INTEGER NOT NULL DEFAULT 0,
    quantity_reserved   INTEGER NOT NULL DEFAULT 0,   -- Allocated to open orders
    quantity_in_transit INTEGER NOT NULL DEFAULT 0,   -- Inbound not yet received
    quantity_damaged    INTEGER NOT NULL DEFAULT 0,
    last_count_date     TEXT,
    avg_holding_cost_pct REAL DEFAULT 0.25,           -- 25% of unit cost per year
    FOREIGN KEY (product_id)   REFERENCES products(product_id),
    FOREIGN KEY (warehouse_id) REFERENCES warehouses(warehouse_id),
    UNIQUE(product_id, warehouse_id)
);

CREATE TABLE IF NOT EXISTS stock_movements (
    movement_id         INTEGER PRIMARY KEY,
    product_id          INTEGER NOT NULL,
    warehouse_id        INTEGER NOT NULL,
    movement_type       TEXT NOT NULL,   -- RECEIPT, SALE, RETURN, TRANSFER_IN, TRANSFER_OUT, ADJUSTMENT, DAMAGED
    quantity            INTEGER NOT NULL,
    reference_id        TEXT,           -- Links to PO, SO, or shipment ID
    movement_date       TEXT NOT NULL,
    unit_cost_at_time   REAL,
    notes               TEXT,
    FOREIGN KEY (product_id)   REFERENCES products(product_id),
    FOREIGN KEY (warehouse_id) REFERENCES warehouses(warehouse_id)
);

-- -------------------------
-- PROCUREMENT
-- -------------------------

CREATE TABLE IF NOT EXISTS purchase_orders (
    po_id               INTEGER PRIMARY KEY,
    po_number           TEXT NOT NULL UNIQUE,
    supplier_id         INTEGER NOT NULL,
    warehouse_id        INTEGER NOT NULL,          -- Destination warehouse
    product_id          INTEGER NOT NULL,
    order_date          TEXT NOT NULL,
    expected_date       TEXT NOT NULL,
    received_date       TEXT,                      -- NULL = not yet received
    quantity_ordered    INTEGER NOT NULL,
    quantity_received   INTEGER DEFAULT 0,
    unit_cost           REAL NOT NULL,
    status              TEXT DEFAULT 'OPEN',       -- OPEN, PARTIAL, RECEIVED, CANCELLED
    notes               TEXT,
    FOREIGN KEY (supplier_id)  REFERENCES suppliers(supplier_id),
    FOREIGN KEY (warehouse_id) REFERENCES warehouses(warehouse_id),
    FOREIGN KEY (product_id)   REFERENCES products(product_id)
);

CREATE TABLE IF NOT EXISTS shipments (
    shipment_id         INTEGER PRIMARY KEY,
    po_id               INTEGER NOT NULL,
    carrier             TEXT,
    tracking_number     TEXT,
    shipped_date        TEXT,
    expected_arrival    TEXT,
    actual_arrival      TEXT,
    quantity_shipped    INTEGER NOT NULL,
    quantity_received   INTEGER DEFAULT 0,
    shipment_status     TEXT DEFAULT 'IN_TRANSIT', -- PENDING, IN_TRANSIT, DELIVERED, PARTIAL, LOST
    delay_reason        TEXT,
    FOREIGN KEY (po_id) REFERENCES purchase_orders(po_id)
);

-- -------------------------
-- SALES & DEMAND
-- -------------------------

CREATE TABLE IF NOT EXISTS sales_orders (
    so_id               INTEGER PRIMARY KEY,
    so_number           TEXT NOT NULL UNIQUE,
    product_id          INTEGER NOT NULL,
    warehouse_id        INTEGER NOT NULL,
    order_date          TEXT NOT NULL,
    required_date       TEXT NOT NULL,
    fulfilled_date      TEXT,
    quantity_ordered    INTEGER NOT NULL,
    quantity_fulfilled  INTEGER DEFAULT 0,
    unit_price          REAL NOT NULL,
    customer_segment    TEXT,                      -- RETAIL, WHOLESALE, ECOMMERCE, ENTERPRISE
    channel             TEXT,                      -- ONLINE, STORE, DISTRIBUTOR
    is_backorder        INTEGER DEFAULT 0,
    FOREIGN KEY (product_id)   REFERENCES products(product_id),
    FOREIGN KEY (warehouse_id) REFERENCES warehouses(warehouse_id)
);

CREATE TABLE IF NOT EXISTS returns (
    return_id           INTEGER PRIMARY KEY,
    so_id               INTEGER NOT NULL,
    product_id          INTEGER NOT NULL,
    warehouse_id        INTEGER NOT NULL,
    return_date         TEXT NOT NULL,
    quantity_returned   INTEGER NOT NULL,
    return_reason       TEXT,       -- DEFECTIVE, WRONG_ITEM, DAMAGED_IN_TRANSIT, CUSTOMER_CHANGE, OVERSTOCK
    condition           TEXT,       -- RESALABLE, DAMAGED, SCRAP
    credit_issued       REAL,
    FOREIGN KEY (so_id)           REFERENCES sales_orders(so_id),
    FOREIGN KEY (product_id)      REFERENCES products(product_id),
    FOREIGN KEY (warehouse_id)    REFERENCES warehouses(warehouse_id)
);

CREATE TABLE IF NOT EXISTS demand_history (
    demand_id           INTEGER PRIMARY KEY,
    product_id          INTEGER NOT NULL,
    warehouse_id        INTEGER NOT NULL,
    period_date         TEXT NOT NULL,             -- First day of the period
    period_type         TEXT DEFAULT 'WEEKLY',     -- DAILY, WEEKLY, MONTHLY
    units_demanded      INTEGER NOT NULL,
    units_fulfilled     INTEGER NOT NULL,
    stockout_occurred   INTEGER DEFAULT 0,         -- Boolean
    avg_selling_price   REAL,
    FOREIGN KEY (product_id)   REFERENCES products(product_id),
    FOREIGN KEY (warehouse_id) REFERENCES warehouses(warehouse_id)
);

-- -------------------------
-- INDEXES FOR PERFORMANCE
-- -------------------------

CREATE INDEX IF NOT EXISTS idx_inventory_product      ON inventory(product_id);
CREATE INDEX IF NOT EXISTS idx_inventory_warehouse    ON inventory(warehouse_id);
CREATE INDEX IF NOT EXISTS idx_movements_date         ON stock_movements(movement_date);
CREATE INDEX IF NOT EXISTS idx_movements_product      ON stock_movements(product_id);
CREATE INDEX IF NOT EXISTS idx_po_supplier            ON purchase_orders(supplier_id);
CREATE INDEX IF NOT EXISTS idx_po_status              ON purchase_orders(status);
CREATE INDEX IF NOT EXISTS idx_so_product             ON sales_orders(product_id);
CREATE INDEX IF NOT EXISTS idx_so_date                ON sales_orders(order_date);
CREATE INDEX IF NOT EXISTS idx_demand_product_period  ON demand_history(product_id, period_date);
CREATE INDEX IF NOT EXISTS idx_shipments_po           ON shipments(po_id);
