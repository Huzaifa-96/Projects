"""
Supply Chain Analytics — Realistic Data Generator
Generates ~18 months of operational data across 5 warehouses,
8 suppliers, 60 SKUs, with seasonal demand, delays, and stockouts.
"""

import sqlite3
import random
import numpy as np
from datetime import datetime, timedelta
import os

random.seed(42)
np.random.seed(42)

DB_PATH = "/home/claude/supply_chain_project/data/supply_chain.db"
os.makedirs(os.path.dirname(DB_PATH), exist_ok=True)

conn = sqlite3.connect(DB_PATH)
conn.execute("PRAGMA foreign_keys = ON")
c = conn.cursor()

# Read and execute schema
with open("/home/claude/supply_chain_project/sql/01_schema.sql") as f:
    conn.executescript(f.read())

# ─────────────────────────────────────────────
# REFERENCE DATA
# ─────────────────────────────────────────────

START_DATE = datetime(2023, 1, 1)
END_DATE   = datetime(2024, 6, 30)

CATEGORIES = {
    "Electronics":  {"margin": 0.35, "seasonal_peak": [11, 12], "velocity": "FAST"},
    "Apparel":      {"margin": 0.55, "seasonal_peak": [3, 4, 9, 10], "velocity": "MEDIUM"},
    "Food":         {"margin": 0.20, "seasonal_peak": [6, 7, 8], "velocity": "FAST"},
    "Industrial":   {"margin": 0.28, "seasonal_peak": [], "velocity": "SLOW"},
    "Pharma":       {"margin": 0.45, "seasonal_peak": [1, 2, 11, 12], "velocity": "MEDIUM"},
}

PRODUCTS_DATA = [
    # Electronics
    ("PRD-001", "Wireless Headphones Pro", "Electronics", "Audio",        89.99,  149.99, 0.35, None),
    ("PRD-002", "USB-C Hub 7-Port",        "Electronics", "Accessories",  22.50,   39.99, 0.18, None),
    ("PRD-003", "Bluetooth Speaker XL",    "Electronics", "Audio",        55.00,   99.99, 0.40, None),
    ("PRD-004", "Laptop Stand Aluminium",  "Electronics", "Accessories",  18.00,   34.99, 0.25, None),
    ("PRD-005", "Smart Watch Band",        "Electronics", "Wearables",    12.00,   24.99, 0.15, None),
    ("PRD-006", "Portable Charger 20000mAh","Electronics","Power",        28.00,   54.99, 0.30, None),
    ("PRD-007", "Wireless Keyboard",       "Electronics", "Peripherals",  34.00,   64.99, 0.28, None),
    ("PRD-008", "Gaming Mouse RGB",        "Electronics", "Peripherals",  19.50,   39.99, 0.22, None),
    ("PRD-009", "Webcam 4K HD",            "Electronics", "Peripherals",  45.00,   89.99, 0.35, None),
    ("PRD-010", "LED Desk Lamp Smart",     "Electronics", "Lighting",     24.00,   49.99, 0.32, None),
    ("PRD-011", "Phone Case Premium",      "Electronics", "Accessories",   6.50,   14.99, 0.08, None),
    ("PRD-012", "Screen Protector Pack",   "Electronics", "Accessories",   3.00,    7.99, 0.05, None),
    # Apparel
    ("PRD-013", "Running Shoes Men",       "Apparel",     "Footwear",     42.00,   89.99, 0.50, None),
    ("PRD-014", "Running Shoes Women",     "Apparel",     "Footwear",     42.00,   89.99, 0.50, None),
    ("PRD-015", "Sports T-Shirt",          "Apparel",     "Tops",          8.50,   22.99, 0.20, None),
    ("PRD-016", "Yoga Pants Women",        "Apparel",     "Bottoms",      15.00,   39.99, 0.25, None),
    ("PRD-017", "Winter Jacket Men",       "Apparel",     "Outerwear",    65.00,  149.99, 0.80, None),
    ("PRD-018", "Winter Jacket Women",     "Apparel",     "Outerwear",    65.00,  149.99, 0.80, None),
    ("PRD-019", "Gym Bag Large",           "Apparel",     "Accessories",  18.00,   44.99, 0.40, None),
    ("PRD-020", "Compression Socks Pack",  "Apparel",     "Accessories",   5.00,   12.99, 0.15, None),
    # Food
    ("PRD-021", "Protein Powder Vanilla",  "Food",        "Supplements",  22.00,   44.99, 0.80, 365),
    ("PRD-022", "Protein Powder Choc",     "Food",        "Supplements",  22.00,   44.99, 0.80, 365),
    ("PRD-023", "Energy Bars Box 24",      "Food",        "Snacks",       14.00,   28.99, 0.30, 180),
    ("PRD-024", "Vitamin C 1000mg x90",    "Food",        "Vitamins",     8.00,    18.99, 0.20, 730),
    ("PRD-025", "Omega 3 Fish Oil x120",   "Food",        "Vitamins",     10.00,   24.99, 0.25, 730),
    ("PRD-026", "Whey Protein Isolate",    "Food",        "Supplements",  30.00,   59.99, 0.90, 365),
    ("PRD-027", "Meal Replacement Shake",  "Food",        "Supplements",  18.00,   36.99, 0.70, 365),
    ("PRD-028", "Electrolyte Tablets x20", "Food",        "Drinks",        4.00,    9.99, 0.10, 540),
    # Industrial
    ("PRD-029", "Safety Gloves Heavy",     "Industrial",  "PPE",           6.00,   14.99, 0.20, None),
    ("PRD-030", "Hard Hat Class E",        "Industrial",  "PPE",          12.00,   28.99, 0.30, None),
    ("PRD-031", "Safety Vest Hi-Vis",      "Industrial",  "PPE",           8.50,   19.99, 0.25, None),
    ("PRD-032", "Ear Protection Muffs",    "Industrial",  "PPE",          15.00,   34.99, 0.35, None),
    ("PRD-033", "Cable Ties Bulk 500",     "Industrial",  "Fasteners",     5.00,   12.99, 0.10, None),
    ("PRD-034", "Industrial Tape 50m",     "Industrial",  "Fasteners",     7.00,   16.99, 0.15, None),
    ("PRD-035", "Dust Mask N95 Box50",     "Industrial",  "PPE",          18.00,   38.99, 0.40, 1825),
    ("PRD-036", "Work Boot Steel Toe",     "Industrial",  "Footwear",     55.00,  109.99, 0.70, None),
    ("PRD-037", "Nitrile Gloves Box100",   "Industrial",  "PPE",          11.00,   24.99, 0.20, 1825),
    ("PRD-038", "Safety Goggles Anti-Fog", "Industrial",  "PPE",           8.00,   18.99, 0.20, None),
    # Pharma
    ("PRD-039", "Ibuprofen 400mg x100",    "Pharma",      "Pain Relief",   5.00,   12.99, 0.10, 1460),
    ("PRD-040", "Paracetamol 500mg x100",  "Pharma",      "Pain Relief",   3.00,    7.99, 0.08, 1460),
    ("PRD-041", "Antihistamine x30",       "Pharma",      "Allergy",       7.00,   17.99, 0.15, 1095),
    ("PRD-042", "Antiseptic Cream 50g",    "Pharma",      "First Aid",     4.00,   10.99, 0.10, 730),
    ("PRD-043", "Hand Sanitiser 500ml",    "Pharma",      "Hygiene",       3.50,    8.99, 0.30, 730),
    ("PRD-044", "Bandage Roll 10cm x5",    "Pharma",      "First Aid",     4.50,   11.99, 0.20, None),
    ("PRD-045", "Digital Thermometer",     "Pharma",      "Diagnostics",  12.00,   26.99, 0.20, None),
    ("PRD-046", "Blood Pressure Monitor",  "Pharma",      "Diagnostics",  35.00,   74.99, 0.50, None),
    ("PRD-047", "Vitamin D3 2000IU x365",  "Pharma",      "Vitamins",      9.00,   22.99, 0.20, 1095),
    ("PRD-048", "Melatonin 5mg x60",       "Pharma",      "Sleep",         6.00,   14.99, 0.15, 730),
    # Slow-movers / Dead stock candidates
    ("PRD-049", "Legacy Cable Adapter",    "Electronics", "Accessories",   2.00,    4.99, 0.05, None),
    ("PRD-050", "Obsolete Keyboard PS2",   "Electronics", "Peripherals",   5.00,    9.99, 0.08, None),
    ("PRD-051", "Niche Industrial Valve",  "Industrial",  "Components",   45.00,   89.99, 0.60, None),
    ("PRD-052", "Surplus Safety Sign",     "Industrial",  "Signage",       8.00,   16.99, 0.25, None),
    ("PRD-053", "Discontinued Supplement", "Food",        "Supplements",  20.00,   38.99, 0.70, 180),
    ("PRD-054", "Old Model Thermometer",   "Pharma",      "Diagnostics",   8.00,   18.99, 0.20, None),
    ("PRD-055", "Overstock Winter Coat",   "Apparel",     "Outerwear",    75.00,  159.99, 0.80, None),
    ("PRD-056", "Legacy Power Adapter",    "Electronics", "Power",        10.00,   19.99, 0.15, None),
    ("PRD-057", "Bulk Stapler Industrial", "Industrial",  "Office",        22.00,   44.99, 0.40, None),
    ("PRD-058", "Expired Promo Packaging", "Food",        "Packaging",     1.00,    2.49, 0.05, 90),
    ("PRD-059", "Niche Yoga Block",        "Apparel",     "Accessories",   7.00,   15.99, 0.20, None),
    ("PRD-060", "Smart Scale Legacy",      "Electronics", "Health",        25.00,   49.99, 0.35, None),
]

SUPPLIERS_DATA = [
    (1, "TechSource Global",        "China",          "Asia",       30, 7.8, 1, "2020-03-15"),
    (2, "EuroSupply Co",            "Germany",        "Europe",     45, 8.5, 1, "2019-07-22"),
    (3, "FastTrack Logistics",      "United Kingdom", "Europe",     21, 6.2, 0, "2021-01-10"),
    (4, "Pacific Rim Distributors", "Vietnam",        "Asia",       30, 7.1, 0, "2020-09-05"),
    (5, "Apex Industrial Supply",   "USA",            "Americas",   30, 8.9, 1, "2018-11-30"),
    (6, "MedPharma Direct",         "Ireland",        "Europe",     30, 9.2, 1, "2017-04-18"),
    (7, "GlobalFit Wholesale",      "Portugal",       "Europe",     45, 7.4, 0, "2022-02-14"),
    (8, "QuickShip Partners",       "Poland",         "Europe",     14, 5.8, 0, "2023-01-05"),
]

WAREHOUSES_DATA = [
    (1, "London Central DC",    "London",      "UK",      "Europe",   25000, 185000, "Sarah Chen"),
    (2, "Manchester North Hub",  "Manchester",  "UK",      "Europe",   18000, 132000, "James Riley"),
    (3, "Birmingham Midlands",   "Birmingham",  "UK",      "Europe",   15000, 110000, "Priya Patel"),
    (4, "Glasgow Scotland WH",   "Glasgow",     "UK",      "Europe",   10000,  78000, "Ewan MacDonald"),
    (5, "Bristol Southwest DC",  "Bristol",     "UK",      "Europe",   12000,  95000, "Laura Morris"),
]

# ─────────────────────────────────────────────
# INSERT REFERENCE DATA
# ─────────────────────────────────────────────

c.execute("DELETE FROM demand_history")
c.execute("DELETE FROM returns")
c.execute("DELETE FROM sales_orders")
c.execute("DELETE FROM shipments")
c.execute("DELETE FROM purchase_orders")
c.execute("DELETE FROM stock_movements")
c.execute("DELETE FROM inventory")
c.execute("DELETE FROM lead_times")
c.execute("DELETE FROM warehouses")
c.execute("DELETE FROM suppliers")
c.execute("DELETE FROM products")

for i, p in enumerate(PRODUCTS_DATA, 1):
    sku, name, cat, subcat, cost, price, weight, shelf = p
    c.execute("""
        INSERT INTO products(product_id, sku, product_name, category, subcategory,
                             unit_cost, unit_price, weight_kg, shelf_life_days, min_order_qty)
        VALUES(?,?,?,?,?,?,?,?,?,?)
    """, (i, sku, name, cat, subcat, cost, price, weight, shelf, random.randint(5,50)))

for s in SUPPLIERS_DATA:
    c.execute("""
        INSERT INTO suppliers(supplier_id, supplier_name, country, region, payment_terms_days,
                              reliability_score, is_preferred, onboarded_date)
        VALUES(?,?,?,?,?,?,?,?)
    """, s)

for w in WAREHOUSES_DATA:
    c.execute("""
        INSERT INTO warehouses(warehouse_id, warehouse_name, city, country, region,
                               total_capacity_m3, operational_cost_monthly, manager_name)
        VALUES(?,?,?,?,?,?,?,?)
    """, w)

# ─────────────────────────────────────────────
# LEAD TIMES
# ─────────────────────────────────────────────

# Assign products to suppliers (multiple suppliers per product)
product_supplier_map = {}
for pid in range(1, 61):
    cat = PRODUCTS_DATA[pid-1][2]
    if cat == "Electronics":    sids = [1, 4]
    elif cat == "Apparel":      sids = [7, 8]
    elif cat == "Food":         sids = [7, 3]
    elif cat == "Industrial":   sids = [5, 2]
    else:                       sids = [6, 2]
    product_supplier_map[pid] = sids

lt_id = 1
for pid in range(1, 61):
    for sid in product_supplier_map[pid]:
        for wid in range(1, 6):
            base_lt = random.randint(5, 25)
            std_dev = random.uniform(1.0, 6.0)
            # Supplier 8 (QuickShip) is consistently unreliable
            if sid == 8: std_dev *= 2.5
            if sid == 3: std_dev *= 1.8
            c.execute("""
                INSERT INTO lead_times(lead_time_id, supplier_id, product_id, warehouse_id,
                                       avg_lead_days, min_lead_days, max_lead_days, std_dev_days, last_updated)
                VALUES(?,?,?,?,?,?,?,?,?)
            """, (lt_id, sid, pid, wid, base_lt,
                  max(1, int(base_lt - std_dev*2)),
                  int(base_lt + std_dev*3),
                  round(std_dev, 1),
                  "2024-06-01"))
            lt_id += 1

# ─────────────────────────────────────────────
# INVENTORY (starting positions)
# ─────────────────────────────────────────────

for pid in range(1, 61):
    cat = PRODUCTS_DATA[pid-1][2]
    for wid in range(1, 6):
        # Dead-stock items — very low or zero movement
        if pid >= 49:
            qoh = random.randint(0, 80)
            dmg = random.randint(0, 5)
        elif cat == "Electronics":
            qoh = random.randint(80, 600)
            dmg = random.randint(0, 20)
        elif cat == "Apparel":
            qoh = random.randint(100, 800)
            dmg = random.randint(0, 15)
        elif cat == "Food":
            qoh = random.randint(60, 400)
            dmg = random.randint(0, 10)
        elif cat == "Industrial":
            qoh = random.randint(200, 1200)
            dmg = random.randint(0, 30)
        else:
            qoh = random.randint(100, 500)
            dmg = random.randint(0, 20)

        reserved = random.randint(0, int(qoh * 0.15))
        in_transit = random.randint(0, int(qoh * 0.20))
        c.execute("""
            INSERT INTO inventory(product_id, warehouse_id, quantity_on_hand,
                                  quantity_reserved, quantity_in_transit, quantity_damaged,
                                  last_count_date, avg_holding_cost_pct)
            VALUES(?,?,?,?,?,?,?,?)
        """, (pid, wid, qoh, reserved, in_transit, dmg, "2024-06-01", 0.25))

# ─────────────────────────────────────────────
# PURCHASE ORDERS & SHIPMENTS
# ─────────────────────────────────────────────

po_id = 1
ship_id = 1
po_number_counter = 1000

current = START_DATE
while current <= END_DATE:
    orders_per_week = random.randint(20, 45)
    for _ in range(orders_per_week):
        pid = random.randint(1, 60)
        cat = PRODUCTS_DATA[pid-1][2]
        sid = random.choice(product_supplier_map[pid])
        wid = random.randint(1, 5)

        lt_row = c.execute("""
            SELECT avg_lead_days, std_dev_days FROM lead_times
            WHERE supplier_id=? AND product_id=? AND warehouse_id=?
        """, (sid, pid, wid)).fetchone()

        if not lt_row:
            continue

        avg_lt, std_lt = lt_row
        # Unreliable suppliers have more delay
        if sid == 8:
            actual_lt = max(1, int(avg_lt + abs(np.random.normal(0, std_lt * 2))))
        elif sid == 3:
            actual_lt = max(1, int(avg_lt + abs(np.random.normal(0, std_lt * 1.5))))
        else:
            actual_lt = max(1, int(avg_lt + np.random.normal(0, std_lt)))

        order_date    = current + timedelta(days=random.randint(0, 6))
        expected_date = order_date + timedelta(days=int(avg_lt))
        received_date = order_date + timedelta(days=actual_lt)

        qty = random.randint(50, 500)
        unit_cost = PRODUCTS_DATA[pid-1][5]  # unit_cost

        # Some orders still open at end of data
        if received_date > END_DATE:
            status = "OPEN"
            received_date = None
            qty_received = 0
        else:
            # Partial deliveries ~15% of the time
            if random.random() < 0.15:
                status = "PARTIAL"
                qty_received = int(qty * random.uniform(0.4, 0.85))
            else:
                status = "RECEIVED"
                qty_received = qty

        po_num = f"PO-{po_number_counter:05d}"
        c.execute("""
            INSERT INTO purchase_orders(po_id, po_number, supplier_id, warehouse_id, product_id,
                                         order_date, expected_date, received_date,
                                         quantity_ordered, quantity_received, unit_cost, status)
            VALUES(?,?,?,?,?,?,?,?,?,?,?,?)
        """, (po_id, po_num, sid, wid, pid,
              order_date.strftime("%Y-%m-%d"),
              expected_date.strftime("%Y-%m-%d"),
              received_date.strftime("%Y-%m-%d") if received_date else None,
              qty, qty_received, unit_cost, status))

        # Shipments for received POs
        if status in ("RECEIVED", "PARTIAL"):
            delay = actual_lt - int(avg_lt)
            ship_status = "DELIVERED" if status == "RECEIVED" else "PARTIAL"
            delay_reason = None
            if delay > 5:
                delay_reason = random.choice(["Customs delay", "Carrier issue", "Supplier backlog",
                                              "Weather disruption", "Port congestion"])
            c.execute("""
                INSERT INTO shipments(shipment_id, po_id, carrier, tracking_number,
                                       shipped_date, expected_arrival, actual_arrival,
                                       quantity_shipped, quantity_received, shipment_status, delay_reason)
                VALUES(?,?,?,?,?,?,?,?,?,?,?)
            """, (ship_id, po_id,
                  random.choice(["DHL", "FedEx", "UPS", "Royal Mail", "DPD"]),
                  f"TRK{ship_id:08d}",
                  (order_date + timedelta(days=2)).strftime("%Y-%m-%d"),
                  expected_date.strftime("%Y-%m-%d"),
                  received_date.strftime("%Y-%m-%d") if received_date else None,
                  qty, qty_received, ship_status, delay_reason))
            ship_id += 1

        # Stock movement (receipt)
        if qty_received > 0 and received_date:
            rd = datetime.strptime(received_date.strftime("%Y-%m-%d") if not isinstance(received_date, str) else received_date, "%Y-%m-%d")
            c.execute("""
                INSERT INTO stock_movements(product_id, warehouse_id, movement_type, quantity,
                                            reference_id, movement_date, unit_cost_at_time)
                VALUES(?,?,?,?,?,?,?)
            """, (pid, wid, "RECEIPT", qty_received, po_num,
                  received_date.strftime("%Y-%m-%d") if not isinstance(received_date, str) else received_date,
                  unit_cost))

        po_id += 1
        po_number_counter += 1
    current += timedelta(weeks=1)

# ─────────────────────────────────────────────
# SALES ORDERS & DEMAND HISTORY
# ─────────────────────────────────────────────

so_id = 1
so_number_counter = 5000
ret_id = 1
mv_id = 1

# Weekly demand simulation
current = START_DATE
while current <= END_DATE:
    month = current.month
    week_date = current.strftime("%Y-%m-%d")

    for pid in range(1, 61):
        cat = PRODUCTS_DATA[pid-1][2]
        cat_info = CATEGORIES[cat]
        price = PRODUCTS_DATA[pid-1][6]

        # Base demand per SKU per warehouse (per week)
        if pid >= 49:  # Dead/slow stock
            base_demand = random.randint(0, 3)
        elif cat == "Electronics":
            base_demand = random.randint(15, 80)
        elif cat == "Apparel":
            base_demand = random.randint(20, 100)
        elif cat == "Food":
            base_demand = random.randint(30, 120)
        elif cat == "Industrial":
            base_demand = random.randint(10, 60)
        else:
            base_demand = random.randint(20, 90)

        # Seasonal multiplier
        seasonal_boost = 1.0
        if month in cat_info["seasonal_peak"]:
            seasonal_boost = random.uniform(1.4, 2.2)

        # Random demand spike (anomaly) ~3% of weeks
        if random.random() < 0.03:
            base_demand = int(base_demand * random.uniform(2.5, 4.0))

        for wid in range(1, 6):
            # Warehouse size affects demand allocation
            wh_factor = [1.0, 0.75, 0.65, 0.45, 0.55][wid-1]
            weekly_demand = max(0, int(base_demand * seasonal_boost * wh_factor
                                       + np.random.normal(0, base_demand * 0.15)))

            if weekly_demand == 0:
                continue

            # Check inventory (simplified)
            inv = c.execute("""
                SELECT quantity_on_hand, quantity_reserved FROM inventory
                WHERE product_id=? AND warehouse_id=?
            """, (pid, wid)).fetchone()

            available = (inv[0] - inv[1]) if inv else 0
            stockout = 0

            if available <= 0:
                # Stockout scenario
                fulfilled = 0
                stockout = 1
            elif available < weekly_demand:
                # Partial fulfilment
                fulfilled = available
                stockout = 1
            else:
                fulfilled = weekly_demand

            # Create individual sales orders (a sample — not every unit)
            num_orders = max(1, int(weekly_demand / random.randint(5, 30)))
            for _ in range(num_orders):
                so_qty = max(1, int(weekly_demand / num_orders))
                so_fulfilled = max(0, int(fulfilled / num_orders))
                req_date = current + timedelta(days=random.randint(1, 7))
                fulfilled_date = req_date + timedelta(days=random.randint(0, 3)) if so_fulfilled > 0 else None
                is_backorder = 1 if stockout else 0

                c.execute("""
                    INSERT INTO sales_orders(so_id, so_number, product_id, warehouse_id,
                                             order_date, required_date, fulfilled_date,
                                             quantity_ordered, quantity_fulfilled, unit_price,
                                             customer_segment, channel, is_backorder)
                    VALUES(?,?,?,?,?,?,?,?,?,?,?,?,?)
                """, (so_id, f"SO-{so_number_counter:06d}", pid, wid,
                      week_date,
                      req_date.strftime("%Y-%m-%d"),
                      fulfilled_date.strftime("%Y-%m-%d") if fulfilled_date else None,
                      so_qty, so_fulfilled, price,
                      random.choice(["RETAIL", "WHOLESALE", "ECOMMERCE", "ENTERPRISE"]),
                      random.choice(["ONLINE", "STORE", "DISTRIBUTOR"]),
                      is_backorder))

                # Stock movement (sale)
                if so_fulfilled > 0:
                    c.execute("""
                        INSERT INTO stock_movements(product_id, warehouse_id, movement_type, quantity,
                                                    reference_id, movement_date, unit_cost_at_time)
                        VALUES(?,?,?,?,?,?,?)
                    """, (pid, wid, "SALE", -so_fulfilled, f"SO-{so_number_counter:06d}",
                          week_date, PRODUCTS_DATA[pid-1][5]))

                so_id += 1
                so_number_counter += 1

            # Returns (~5% return rate)
            if fulfilled > 0 and random.random() < 0.05:
                ret_qty = max(1, int(fulfilled * random.uniform(0.02, 0.08)))
                ret_date = current + timedelta(days=random.randint(7, 21))
                if ret_date <= END_DATE:
                    cond = random.choices(["RESALABLE", "DAMAGED", "SCRAP"], weights=[0.6, 0.3, 0.1])[0]
                    reason = random.choice(["DEFECTIVE", "WRONG_ITEM", "DAMAGED_IN_TRANSIT",
                                            "CUSTOMER_CHANGE", "OVERSTOCK"])
                    c.execute("""
                        INSERT INTO returns(return_id, so_id, product_id, warehouse_id,
                                            return_date, quantity_returned, return_reason,
                                            condition, credit_issued)
                        VALUES(?,?,?,?,?,?,?,?,?)
                    """, (ret_id, so_id-1, pid, wid,
                          ret_date.strftime("%Y-%m-%d"),
                          ret_qty, reason, cond,
                          round(ret_qty * price * (0.8 if cond == "RESALABLE" else 0.3), 2)))
                    ret_id += 1

            # Demand history record
            c.execute("""
                INSERT INTO demand_history(product_id, warehouse_id, period_date, period_type,
                                           units_demanded, units_fulfilled, stockout_occurred, avg_selling_price)
                VALUES(?,?,?,?,?,?,?,?)
            """, (pid, wid, week_date, "WEEKLY", weekly_demand, fulfilled, stockout, price))

            # Update inventory (simplified balance update)
            if inv:
                new_qoh = max(0, inv[0] - so_fulfilled)
                c.execute("""
                    UPDATE inventory SET quantity_on_hand=? WHERE product_id=? AND warehouse_id=?
                """, (new_qoh, pid, wid))

    current += timedelta(weeks=1)

conn.commit()

# Verification
print("=== DATA GENERATION COMPLETE ===")
for table in ["products", "suppliers", "warehouses", "inventory", "purchase_orders",
              "shipments", "sales_orders", "returns", "stock_movements", "demand_history", "lead_times"]:
    count = c.execute(f"SELECT COUNT(*) FROM {table}").fetchone()[0]
    print(f"  {table:<25}: {count:>8,} rows")

conn.close()
print("\nDatabase written to:", DB_PATH)
