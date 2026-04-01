# Price Optimisation — E-Commerce
### Expert-Level Demand Modelling, Elasticity Estimation & Profit Maximisation

---

## What This Project Does

This project builds a production-quality pricing intelligence system for an e-commerce retailer.
It estimates how sensitive demand is to price changes for each product, builds a machine learning
model that predicts daily units sold at any price point, and finds the profit-maximising price
for each product — supported by scenario testing and seasonal analysis.

---

## The Business Problem

E-commerce pricing is a constant balancing act: price too high and customers leave,
price too low and margin suffers. This project answers three specific questions:

1. **How sensitive is demand to price changes?** (Price elasticity per product)
2. **What price maximises daily profit for each product?** (Optimisation model)
3. **What happens to the portfolio if prices move up or down?** (Scenario simulation)

---

## Dataset

| Property | Original | Enriched (this project) |
|----------|---------|------------------------|
| Rows | 1,825 | **5,840** |
| Products | 5 | **8** |
| Categories | 1 | **5** |
| Time period | 1 year | **2 years** |
| Discount rate | No | **Yes** |
| Holiday flag | No | **Yes** |
| Weekend flag | No | **Yes** |
| Competitor gap % | No | **Yes** |

The original dataset was too limited for reliable seasonality and holiday detection.
The enriched dataset preserves all original products and their pricing structures.

### Features Used

| Feature | Description | Importance |
|---------|-------------|------------|
| Effective Price | Actual price after discount | Primary driver |
| Competitor Price | Market benchmark | Relative positioning |
| Marketing Spend | Daily advertising spend | Demand amplifier |
| Discount Rate | % reduction from list | Volume vs margin trade-off |
| Visibility Index | Product prominence score | Organic demand driver |
| Day of Week / Weekend | Behavioural segmentation | Demand pattern |
| Seasonality (sin/cos) | Cyclical annual pattern | Prevents year-end discontinuity |
| Holiday Flag | Black Friday / Christmas / New Year | 60-80% demand uplift |

---

## Models

| Model | R² | RMSE | MAE | MAPE | Notes |
|-------|-----|------|-----|------|-------|
| Ridge Regression | 0.832 | 9.02 | 4.17 | 29.4% | Transparent baseline |
| Random Forest | 0.821 | 9.28 | 4.32 | 31.0% | Robust; handles interactions |
| **Gradient Boosting** | **0.876** | **7.74** | **3.89** | **27.9%** | **Best — used for optimisation** |

---

## Price Elasticity Results

| Product | Elasticity | Sensitivity | Action |
|---------|-----------|-------------|--------|
| ThetaMat | −3.22 | 🔴 High | Reduce price |
| GammaBand | −2.94 | 🔴 High | Reduce price |
| BetaBuds | −2.65 | 🔴 High | Reduce price |
| ZetaHub | −2.45 | 🔴 High | Reduce price |
| AlphaWatch | −1.95 | 🟡 Medium | Small reduction |
| EpsilonCam | −1.86 | 🟡 Medium | Small reduction |
| EtaDesk | −1.50 | 🟢 Low | Can increase |
| DeltaDock | −1.40 | 🟢 Low | Should increase |

---

## Price Recommendations

| Product | Current | Optimal | Change | Profit Uplift |
|---------|---------|---------|--------|--------------|
| BetaBuds | £52.73 | £42.88 | ↓ £9.85 | **+69.8%** |
| ThetaMat | £31.19 | £27.37 | ↓ £3.82 | **+38.4%** |
| GammaBand | £37.88 | £33.36 | ↓ £4.52 | **+28.1%** |
| EpsilonCam | £115.36 | £104.01 | ↓ £11.35 | +10.8% |
| AlphaWatch | £84.47 | £76.49 | ↓ £7.98 | +10.6% |
| ZetaHub | £46.16 | £37.51 | ↓ £8.65 | +8.3% |
| DeltaDock | £63.20 | £81.37 | ↑ £18.17 | +4.2% |
| EtaDesk | £167.70 | £186.20 | ↑ £18.50 | −1.1% |

**Total daily profit uplift: ~£572 (~£209,000/year)**

---

## Project Structure

```
price_project/
├── Price_Optimisation_Ecommerce.ipynb      ← Full analysis notebook
├── data.csv                                 ← Original dataset (1,825 rows)
├── data_enriched.csv                        ← Enriched dataset (5,840 rows)
├── README.md                                ← This file
└── outputs/
    ├── Price_Optimisation_Report.html       ← Non-technical business report
    ├── 01_eda_overview.png                  ← Pricing data exploration
    ├── 02_model_elasticity.png              ← Model performance & elasticity
    ├── 03_pricing_strategy.png              ← Recommendations & scenarios
    ├── price_recommendations.csv            ← Optimal prices for all products
    └── elasticities.json                    ← Elasticity estimates (machine-readable)
```

---

## How to Run

```bash
pip install numpy pandas matplotlib seaborn scipy scikit-learn
jupyter notebook Price_Optimisation_Ecommerce.ipynb
```

---

## Improvements Over the Original Notebook

| Aspect | Original | This Version |
|--------|---------|-------------|
| Dataset size | 1,825 rows · 5 products | 5,840 rows · 8 products |
| Dataset features | Basic (price, cost, revenue) | + discounts, holidays, weekends, price gap |
| Models | GBM only | Ridge + RF + GBM with full comparison |
| Evaluation | R², MAE only | R², RMSE, MAE, MAPE |
| Elasticity | Log-log per product | Log-log with controls + R² per product |
| Optimisation | Price grid search | Scipy bounded optimisation (faster, exact) |
| Scenario testing | None | 5-scenario portfolio simulation |
| Seasonality | None | Sine/cosine encoding + holiday flags |
| Interpretability | None | Feature importance (top 10 drivers) |
| Business output | Text print | Full HTML report + recommendations table |
| Validation | Train/test only | Train/val/test + leakage-safe preprocessing |

---

## Ethical and Practical Considerations

- Model recommendations should be validated through A/B testing before full rollout
- Competitor reactions to price changes are not modelled — actual elasticities may differ
- Holiday demand estimates assume similar promotional conditions to historical periods
- Do not reduce prices on DeltaDock and EtaDesk — these are correctly identified as inelastic
