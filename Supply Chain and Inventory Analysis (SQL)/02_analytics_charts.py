"""
Supply Chain Analytics — Python Analytics Layer
Runs all major analyses, generates charts and exports results.
"""

import sqlite3
import pandas as pd
import numpy as np
import matplotlib
matplotlib.use('Agg')
import matplotlib.pyplot as plt
import matplotlib.patches as mpatches
import matplotlib.gridspec as gridspec
from matplotlib.colors import LinearSegmentedColormap
import warnings
warnings.filterwarnings('ignore')

DB_PATH  = "/home/claude/supply_chain_project/data/supply_chain.db"
OUT_PATH = "/home/claude/supply_chain_project/outputs"

conn = sqlite3.connect(DB_PATH)

# ─── Global Style ────────────────────────────────────────────────────────────
PALETTE = {
    "navy":     "#0D1B2A",
    "blue":     "#1565C0",
    "teal":     "#00838F",
    "green":    "#2E7D32",
    "amber":    "#F57F17",
    "red":      "#C62828",
    "light":    "#F5F7FA",
    "text":     "#212529",
    "grid":     "#DEE2E6",
    "accent1":  "#1976D2",
    "accent2":  "#EF6C00",
    "accent3":  "#6A1B9A",
}

plt.rcParams.update({
    'font.family':       'DejaVu Sans',
    'axes.spines.top':   False,
    'axes.spines.right': False,
    'axes.grid':         True,
    'grid.color':        PALETTE["grid"],
    'grid.linewidth':    0.6,
    'axes.labelcolor':   PALETTE["text"],
    'xtick.color':       PALETTE["text"],
    'ytick.color':       PALETTE["text"],
    'figure.facecolor':  'white',
    'axes.facecolor':    'white',
})

# ─── Load Data ────────────────────────────────────────────────────────────────

print("Loading data...")

df_demand = pd.read_sql("""
    SELECT dh.*, p.category, p.product_name, p.sku, w.warehouse_name
    FROM demand_history dh
    JOIN products p ON dh.product_id = p.product_id
    JOIN warehouses w ON dh.warehouse_id = w.warehouse_id
""", conn, parse_dates=['period_date'])

df_po = pd.read_sql("""
    SELECT po.*, s.supplier_name, s.country, p.category
    FROM purchase_orders po
    JOIN suppliers s ON po.supplier_id = s.supplier_id
    JOIN products p ON po.product_id = p.product_id
    WHERE po.status IN ('RECEIVED','PARTIAL') AND po.received_date IS NOT NULL
""", conn, parse_dates=['order_date','expected_date','received_date'])

df_inv = pd.read_sql("""
    SELECT i.*, p.category, p.product_name, p.sku, p.unit_cost, p.unit_price,
           w.warehouse_name
    FROM inventory i
    JOIN products p ON i.product_id = p.product_id
    JOIN warehouses w ON i.warehouse_id = w.warehouse_id
""", conn)

df_so = pd.read_sql("""
    SELECT so.*, p.category, p.product_name, p.sku, w.warehouse_name
    FROM sales_orders so
    JOIN products p ON so.product_id = p.product_id
    JOIN warehouses w ON so.warehouse_id = w.warehouse_id
""", conn, parse_dates=['order_date'])

df_lt = pd.read_sql("""
    SELECT lt.*, s.supplier_name, p.category
    FROM lead_times lt
    JOIN suppliers s ON lt.supplier_id = s.supplier_id
    JOIN products p ON lt.product_id = p.product_id
""", conn)

df_returns = pd.read_sql("""
    SELECT r.*, p.category, p.product_name, so.unit_price
    FROM returns r
    JOIN products p ON r.product_id = p.product_id
    JOIN sales_orders so ON r.so_id = so.so_id
""", conn, parse_dates=['return_date'])

print("  Data loaded.")

# ─────────────────────────────────────────────────────────────────────────────
# CHART 1: SUPPLIER PERFORMANCE SCORECARD (Bar chart)
# ─────────────────────────────────────────────────────────────────────────────

df_po['days_late'] = (df_po['received_date'] - df_po['expected_date']).dt.days
df_po['on_time'] = (df_po['days_late'] <= 0).astype(int)
df_po['fill_rate'] = df_po['quantity_received'] / df_po['quantity_ordered'] * 100

supplier_perf = df_po.groupby('supplier_name').agg(
    on_time_pct=('on_time', lambda x: x.mean() * 100),
    avg_fill_rate=('fill_rate', 'mean'),
    total_orders=('po_id', 'count'),
    avg_delay=('days_late', 'mean'),
).round(1).reset_index()
supplier_perf = supplier_perf.sort_values('on_time_pct', ascending=True)

fig, axes = plt.subplots(1, 2, figsize=(14, 6))
fig.suptitle('Supplier Performance Scorecard', fontsize=16, fontweight='bold',
             color=PALETTE['navy'], y=1.01)

# On-time delivery
colors_ot = [PALETTE['red'] if v < 70 else PALETTE['amber'] if v < 85 else PALETTE['green']
             for v in supplier_perf['on_time_pct']]
bars = axes[0].barh(supplier_perf['supplier_name'], supplier_perf['on_time_pct'],
                    color=colors_ot, edgecolor='white', linewidth=0.8)
axes[0].axvline(85, color=PALETTE['blue'], linestyle='--', linewidth=1.5, label='Target 85%')
axes[0].axvline(95, color=PALETTE['green'], linestyle='--', linewidth=1.5, label='World Class 95%')
axes[0].set_xlabel('On-Time Delivery %')
axes[0].set_title('On-Time Delivery Rate', fontweight='bold')
axes[0].legend(fontsize=9)
for bar, val in zip(bars, supplier_perf['on_time_pct']):
    axes[0].text(bar.get_width() + 0.5, bar.get_y() + bar.get_height()/2,
                 f'{val:.1f}%', va='center', fontsize=9)

# Fill rate
colors_fr = [PALETTE['red'] if v < 85 else PALETTE['amber'] if v < 95 else PALETTE['green']
             for v in supplier_perf['avg_fill_rate']]
bars2 = axes[1].barh(supplier_perf['supplier_name'], supplier_perf['avg_fill_rate'],
                     color=colors_fr, edgecolor='white', linewidth=0.8)
axes[1].axvline(95, color=PALETTE['blue'], linestyle='--', linewidth=1.5, label='Target 95%')
axes[1].set_xlabel('Average Fill Rate %')
axes[1].set_title('Order Fill Rate', fontweight='bold')
axes[1].legend(fontsize=9)
for bar, val in zip(bars2, supplier_perf['avg_fill_rate']):
    axes[1].text(bar.get_width() + 0.2, bar.get_y() + bar.get_height()/2,
                 f'{val:.1f}%', va='center', fontsize=9)

plt.tight_layout()
plt.savefig(f'{OUT_PATH}/chart_01_supplier_performance.png', dpi=150, bbox_inches='tight')
plt.close()
print("  Chart 1: Supplier Performance — saved")

# ─────────────────────────────────────────────────────────────────────────────
# CHART 2: ABC INVENTORY CLASSIFICATION — Treemap-style
# ─────────────────────────────────────────────────────────────────────────────

df_so['revenue'] = df_so['quantity_fulfilled'] * df_so['unit_price']
rev_by_sku = df_so.groupby(['sku', 'category', 'product_name'])['revenue'].sum().reset_index()
rev_by_sku = rev_by_sku.sort_values('revenue', ascending=False)
rev_by_sku['cum_pct'] = rev_by_sku['revenue'].cumsum() / rev_by_sku['revenue'].sum() * 100
rev_by_sku['abc'] = pd.cut(rev_by_sku['cum_pct'], bins=[0, 80, 95, 100],
                            labels=['A — High Value', 'B — Medium Value', 'C — Low Value'])

abc_summary = rev_by_sku.groupby('abc').agg(
    sku_count=('sku', 'count'),
    total_revenue=('revenue', 'sum')
).reset_index()
abc_summary['rev_pct'] = abc_summary['total_revenue'] / abc_summary['total_revenue'].sum() * 100

fig, axes = plt.subplots(1, 2, figsize=(13, 6))
fig.suptitle('ABC Inventory Classification', fontsize=16, fontweight='bold', color=PALETTE['navy'])

abc_colors = [PALETTE['green'], PALETTE['teal'], PALETTE['amber']]
wedge, texts, autotexts = axes[0].pie(
    abc_summary['rev_pct'],
    labels=[f"{r['abc']}\n({r['sku_count']} SKUs)" for _, r in abc_summary.iterrows()],
    autopct='%1.1f%%', colors=abc_colors, startangle=90,
    wedgeprops=dict(linewidth=2, edgecolor='white'))
for t in autotexts:
    t.set_fontsize(11)
    t.set_fontweight('bold')
axes[0].set_title('Revenue Share by Class', fontweight='bold')

# Bar showing SKU count vs revenue contribution
x = np.arange(len(abc_summary))
width = 0.35
bars1 = axes[1].bar(x - width/2,
                    abc_summary['sku_count'] / abc_summary['sku_count'].sum() * 100,
                    width, color=[c + 'BB' for c in abc_colors], label='SKU %', edgecolor='white')
bars2 = axes[1].bar(x + width/2, abc_summary['rev_pct'],
                    width, color=abc_colors, label='Revenue %', edgecolor='white')
axes[1].set_xticks(x)
axes[1].set_xticklabels(['A — High', 'B — Medium', 'C — Low'])
axes[1].set_ylabel('Percentage (%)')
axes[1].set_title('SKU Count vs Revenue Contribution', fontweight='bold')
axes[1].legend()

plt.tight_layout()
plt.savefig(f'{OUT_PATH}/chart_02_abc_classification.png', dpi=150, bbox_inches='tight')
plt.close()
print("  Chart 2: ABC Classification — saved")

# ─────────────────────────────────────────────────────────────────────────────
# CHART 3: DEMAND TREND & SEASONALITY
# ─────────────────────────────────────────────────────────────────────────────

monthly = df_demand.copy()
monthly['month'] = monthly['period_date'].dt.to_period('M')
monthly_agg = monthly.groupby(['month', 'category']).agg(
    demand=('units_demanded', 'sum'),
    fulfilled=('units_fulfilled', 'sum'),
    stockouts=('stockout_occurred', 'sum'),
).reset_index()
monthly_agg['month_dt'] = monthly_agg['month'].dt.to_timestamp()

fig, axes = plt.subplots(2, 1, figsize=(14, 10))
fig.suptitle('Demand Trends & Seasonal Patterns (18 Months)', fontsize=16,
             fontweight='bold', color=PALETTE['navy'])

cat_colors = {
    'Electronics': PALETTE['blue'],
    'Apparel':     PALETTE['teal'],
    'Food':        PALETTE['green'],
    'Industrial':  PALETTE['amber'],
    'Pharma':      PALETTE['accent3'],
}

for cat, col in cat_colors.items():
    cat_data = monthly_agg[monthly_agg['category'] == cat].sort_values('month_dt')
    axes[0].plot(cat_data['month_dt'], cat_data['demand'],
                 label=cat, color=col, linewidth=2.2, marker='o', markersize=4)

axes[0].set_title('Monthly Units Demanded by Category', fontweight='bold')
axes[0].set_ylabel('Units Demanded')
axes[0].legend(loc='upper left', fontsize=9)
axes[0].fill_between([], [], alpha=0.1)

# Overall demand vs fulfilled (service level view)
overall = monthly_agg.groupby('month_dt')[['demand', 'fulfilled', 'stockouts']].sum().reset_index()
axes[1].fill_between(overall['month_dt'], overall['demand'], alpha=0.15,
                     color=PALETTE['blue'], label='Total Demand')
axes[1].plot(overall['month_dt'], overall['demand'],
             color=PALETTE['blue'], linewidth=2.0)
axes[1].fill_between(overall['month_dt'], overall['fulfilled'], alpha=0.4,
                     color=PALETTE['green'], label='Fulfilled')
axes[1].plot(overall['month_dt'], overall['fulfilled'],
             color=PALETTE['green'], linewidth=2.0, linestyle='--')
axes[1].set_title('Total Demand vs Fulfilled — Service Gap View', fontweight='bold')
axes[1].set_ylabel('Units')
axes[1].legend()

plt.tight_layout()
plt.savefig(f'{OUT_PATH}/chart_03_demand_trends.png', dpi=150, bbox_inches='tight')
plt.close()
print("  Chart 3: Demand Trends — saved")

# ─────────────────────────────────────────────────────────────────────────────
# CHART 4: STOCKOUT RISK HEATMAP
# ─────────────────────────────────────────────────────────────────────────────

stockout_matrix = df_demand.groupby(['category', 'warehouse_name'])['stockout_occurred'].mean() * 100
stockout_matrix = stockout_matrix.unstack(fill_value=0).round(1)

fig, ax = plt.subplots(figsize=(12, 6))
fig.suptitle('Stockout Risk Heatmap — % of Weeks with Stockout by Category & Warehouse',
             fontsize=14, fontweight='bold', color=PALETTE['navy'])

cmap = LinearSegmentedColormap.from_list('risk',
    ['#e8f5e9', '#fff176', '#ff8f00', '#c62828'])

im = ax.imshow(stockout_matrix.values, cmap=cmap, aspect='auto',
               vmin=0, vmax=stockout_matrix.values.max())

ax.set_xticks(range(len(stockout_matrix.columns)))
ax.set_yticks(range(len(stockout_matrix.index)))
ax.set_xticklabels([c.replace(' ', '\n') for c in stockout_matrix.columns], fontsize=10)
ax.set_yticklabels(stockout_matrix.index, fontsize=11)

for i in range(len(stockout_matrix.index)):
    for j in range(len(stockout_matrix.columns)):
        val = stockout_matrix.values[i, j]
        color = 'white' if val > stockout_matrix.values.max() * 0.6 else PALETTE['navy']
        ax.text(j, i, f'{val:.1f}%', ha='center', va='center',
                fontsize=11, fontweight='bold', color=color)

plt.colorbar(im, ax=ax, label='Stockout Rate (%)', shrink=0.8)
plt.tight_layout()
plt.savefig(f'{OUT_PATH}/chart_04_stockout_heatmap.png', dpi=150, bbox_inches='tight')
plt.close()
print("  Chart 4: Stockout Heatmap — saved")

# ─────────────────────────────────────────────────────────────────────────────
# CHART 5: INVENTORY VALUE & OVERSTOCK BY CATEGORY
# ─────────────────────────────────────────────────────────────────────────────

df_inv['inventory_value'] = df_inv['quantity_on_hand'] * df_inv['unit_cost']
df_inv['damaged_value']   = df_inv['quantity_damaged']  * df_inv['unit_cost']

inv_cat = df_inv.groupby('category').agg(
    total_value=('inventory_value', 'sum'),
    damaged_value=('damaged_value', 'sum'),
    total_units=('quantity_on_hand', 'sum'),
    sku_count=('sku', 'nunique'),
).reset_index()
inv_cat['healthy_value'] = inv_cat['total_value'] - inv_cat['damaged_value']

fig, axes = plt.subplots(1, 2, figsize=(13, 6))
fig.suptitle('Inventory Value Distribution by Category', fontsize=16,
             fontweight='bold', color=PALETTE['navy'])

cats = inv_cat['category'].tolist()
x = np.arange(len(cats))
width = 0.5

bars_h = axes[0].bar(x, inv_cat['healthy_value'] / 1000, width,
                     color=PALETTE['blue'], label='Healthy Stock', edgecolor='white')
bars_d = axes[0].bar(x, inv_cat['damaged_value'] / 1000, width,
                     bottom=inv_cat['healthy_value'] / 1000,
                     color=PALETTE['red'], label='Damaged Stock', edgecolor='white')
axes[0].set_xticks(x)
axes[0].set_xticklabels(cats, rotation=15, ha='right')
axes[0].set_ylabel('Inventory Value (£ 000s)')
axes[0].set_title('Total Inventory Value', fontweight='bold')
axes[0].legend()

for bar, val in zip(bars_h, inv_cat['total_value'] / 1000):
    axes[0].text(bar.get_x() + bar.get_width()/2, val + 2,
                 f'£{val:.0f}k', ha='center', va='bottom', fontsize=9, fontweight='bold')

# Damaged as % per category
dmg_pct = (inv_cat['damaged_value'] / inv_cat['total_value'] * 100).round(1)
bar_colors = [PALETTE['red'] if v > 5 else PALETTE['amber'] if v > 2 else PALETTE['green']
              for v in dmg_pct]
axes[1].bar(cats, dmg_pct, color=bar_colors, edgecolor='white')
axes[1].axhline(3, color=PALETTE['blue'], linestyle='--', linewidth=1.5, label='3% Threshold')
axes[1].set_ylabel('Damaged Stock %')
axes[1].set_title('Damaged Goods as % of Inventory', fontweight='bold')
axes[1].set_xticklabels(cats, rotation=15, ha='right')
axes[1].legend()

plt.tight_layout()
plt.savefig(f'{OUT_PATH}/chart_05_inventory_value.png', dpi=150, bbox_inches='tight')
plt.close()
print("  Chart 5: Inventory Value — saved")

# ─────────────────────────────────────────────────────────────────────────────
# CHART 6: LEAD TIME VARIANCE BY SUPPLIER
# ─────────────────────────────────────────────────────────────────────────────

lt_summary = df_lt.groupby('supplier_name').agg(
    avg_lt=('avg_lead_days', 'mean'),
    avg_std=('std_dev_days', 'mean'),
    max_lt=('max_lead_days', 'max'),
).reset_index()
lt_summary['cv'] = (lt_summary['avg_std'] / lt_summary['avg_lt'] * 100).round(1)
lt_summary = lt_summary.sort_values('cv', ascending=False)

fig, ax = plt.subplots(figsize=(12, 6))
fig.suptitle('Lead Time Reliability by Supplier\nAverage vs Worst-Case Lead Days',
             fontsize=14, fontweight='bold', color=PALETTE['navy'])

y = np.arange(len(lt_summary))
# Draw error bars manually
bar_colors = [PALETTE['red'] if cv > 50 else PALETTE['amber'] if cv > 30
              else PALETTE['green'] for cv in lt_summary['cv']]
ax.barh(y, lt_summary['avg_lt'], height=0.5, color=bar_colors, alpha=0.8, label='Avg Lead Days')
ax.barh(y, lt_summary['max_lt'] - lt_summary['avg_lt'],
        left=lt_summary['avg_lt'], height=0.5,
        color=[c + '55' for c in bar_colors], label='Worst Case Extension')

ax.set_yticks(y)
ax.set_yticklabels(lt_summary['supplier_name'])
ax.set_xlabel('Lead Days')
ax.legend(loc='lower right')

for i, (_, row) in enumerate(lt_summary.iterrows()):
    ax.text(row['max_lt'] + 0.5, i, f"CV: {row['cv']:.0f}%",
            va='center', fontsize=9, color=PALETTE['navy'])

ax.axvline(14, color=PALETTE['blue'], linestyle=':', alpha=0.7, label='2-week benchmark')
plt.tight_layout()
plt.savefig(f'{OUT_PATH}/chart_06_lead_time_variance.png', dpi=150, bbox_inches='tight')
plt.close()
print("  Chart 6: Lead Time Variance — saved")

# ─────────────────────────────────────────────────────────────────────────────
# CHART 7: WAREHOUSE PERFORMANCE OVERVIEW
# ─────────────────────────────────────────────────────────────────────────────

wh_perf = df_so.groupby('warehouse_name').agg(
    total_revenue=('revenue', 'sum'),
    fill_rate=('quantity_fulfilled', lambda x: x.sum() / df_so.loc[x.index, 'quantity_ordered'].sum() * 100),
    backorders=('is_backorder', 'sum'),
    total_orders=('so_id', 'count'),
).reset_index()
wh_perf['backorder_rate'] = wh_perf['backorders'] / wh_perf['total_orders'] * 100

wh_inv_val = df_inv.groupby('warehouse_name')['inventory_value'].sum().reset_index()
wh_perf = wh_perf.merge(wh_inv_val, on='warehouse_name')

fig, axes = plt.subplots(1, 3, figsize=(15, 5))
fig.suptitle('Warehouse Performance Overview', fontsize=16, fontweight='bold',
             color=PALETTE['navy'])

wh_colors = [PALETTE['blue'], PALETTE['teal'], PALETTE['green'],
             PALETTE['accent3'], PALETTE['amber']]

# Revenue
axes[0].bar(wh_perf['warehouse_name'].str.replace(' ', '\n', 1),
            wh_perf['total_revenue'] / 1e6, color=wh_colors, edgecolor='white')
axes[0].set_ylabel('Revenue (£M)')
axes[0].set_title('Total Revenue (18M)', fontweight='bold')

# Fill rate
fr_colors = [PALETTE['green'] if v >= 95 else PALETTE['amber'] if v >= 90
             else PALETTE['red'] for v in wh_perf['fill_rate']]
axes[1].bar(wh_perf['warehouse_name'].str.replace(' ', '\n', 1),
            wh_perf['fill_rate'], color=fr_colors, edgecolor='white')
axes[1].axhline(95, color=PALETTE['blue'], linestyle='--', linewidth=1.5)
axes[1].set_ylabel('Fill Rate (%)')
axes[1].set_ylim(85, 100)
axes[1].set_title('Order Fill Rate', fontweight='bold')

# Backorder rate
bo_colors = [PALETTE['red'] if v > 10 else PALETTE['amber'] if v > 5
             else PALETTE['green'] for v in wh_perf['backorder_rate']]
axes[2].bar(wh_perf['warehouse_name'].str.replace(' ', '\n', 1),
            wh_perf['backorder_rate'], color=bo_colors, edgecolor='white')
axes[2].axhline(5, color=PALETTE['blue'], linestyle='--', linewidth=1.5, label='5% target')
axes[2].set_ylabel('Backorder Rate (%)')
axes[2].set_title('Backorder Rate', fontweight='bold')
axes[2].legend()

plt.tight_layout()
plt.savefig(f'{OUT_PATH}/chart_07_warehouse_performance.png', dpi=150, bbox_inches='tight')
plt.close()
print("  Chart 7: Warehouse Performance — saved")

# ─────────────────────────────────────────────────────────────────────────────
# COMPUTE KEY METRICS FOR REPORT
# ─────────────────────────────────────────────────────────────────────────────

total_revenue = (df_so['quantity_fulfilled'] * df_so['unit_price']).sum()
total_inv_val = (df_inv['quantity_on_hand'] * df_inv['unit_cost']).sum()
overall_fill  = df_so['quantity_fulfilled'].sum() / df_so['quantity_ordered'].sum() * 100
backorder_rate = df_so['is_backorder'].mean() * 100
avg_lt = df_lt['avg_lead_days'].mean()
damaged_val = (df_inv['quantity_damaged'] * df_inv['unit_cost']).sum()
total_returns_val = (df_returns['quantity_returned'] * df_returns['unit_price']).sum()

# Supplier on-time %
df_po['on_time'] = (df_po['received_date'] <= df_po['expected_date'])
overall_ot = df_po['on_time'].mean() * 100

# Stockout weeks
so_pct = df_demand['stockout_occurred'].mean() * 100

print("\n=== KEY METRICS ===")
print(f"  Total Revenue (18m):     £{total_revenue:,.0f}")
print(f"  Total Inventory Value:   £{total_inv_val:,.0f}")
print(f"  Overall Fill Rate:       {overall_fill:.1f}%")
print(f"  Backorder Rate:          {backorder_rate:.1f}%")
print(f"  On-Time Delivery:        {overall_ot:.1f}%")
print(f"  Average Lead Time:       {avg_lt:.1f} days")
print(f"  Stockout Rate (weeks):   {so_pct:.1f}%")
print(f"  Damaged Stock Value:     £{damaged_val:,.0f}")
print(f"  Total Returns Value:     £{total_returns_val:,.0f}")

# Save metrics to CSV for report
metrics = pd.DataFrame({
    'metric': ['Total Revenue', 'Inventory Value', 'Fill Rate %', 'Backorder Rate %',
               'On-Time Delivery %', 'Avg Lead Time (days)', 'Stockout Rate %',
               'Damaged Stock Value', 'Returns Value'],
    'value': [total_revenue, total_inv_val, overall_fill, backorder_rate,
              overall_ot, avg_lt, so_pct, damaged_val, total_returns_val]
})
metrics.to_csv(f'{OUT_PATH}/kpi_metrics.csv', index=False)

# Supplier comparison CSV
supplier_perf.to_csv(f'{OUT_PATH}/supplier_performance.csv', index=False)
inv_cat.to_csv(f'{OUT_PATH}/inventory_by_category.csv', index=False)

print("\nAll charts and CSVs saved to:", OUT_PATH)
conn.close()
