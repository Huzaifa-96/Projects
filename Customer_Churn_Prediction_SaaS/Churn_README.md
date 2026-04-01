# Customer Churn Prediction — SaaS
### Expert-Level Machine Learning Pipeline for Subscription Retention

---

## What This Project Does

This project builds a production-quality churn prediction system for a SaaS business.
It predicts which customers are most likely to cancel their subscription, identifies the
key drivers of churn, assigns every customer a personalised risk score, and provides
actionable retention recommendations by risk tier.

---

## The Business Problem

A SaaS company is experiencing a **47.4% churn rate** — nearly half of all customers.
Every cancellation reduces Monthly Recurring Revenue (MRR). Because SaaS businesses
depend on compounding retention, even reducing churn by a few percentage points has
a significant impact on long-term growth.

The goal is to identify at-risk customers **before** they cancel, giving the customer
success team time to intervene.

---

## Dataset

| Property | Value |
|----------|-------|
| Customers | 64,374 |
| Features | 11 (+ 6 engineered) |
| Churn rate | 47.4% |
| Class balance | Near-balanced — no correction needed |
| Missing values | None |
| Type | Synthetic SaaS churn dataset |

**Note:** The dataset is synthetic with strong engineered signals (particularly payment
delay and support calls), making it ideal for methodology demonstration. Real customer
data will show more noise and typically achieve AUC of 0.75–0.90 rather than near-perfect scores.

### Features

| Feature | Description | Churn Signal |
|---------|-------------|-------------|
| Tenure | Months as a customer | Moderate positive correlation |
| Usage Frequency | Sessions per month | Negative — more use = less churn |
| Support Calls | Support tickets raised | Strong positive — frustration signal |
| Payment Delay | Days behind on payment | Strongest predictor (r=0.56) |
| Subscription Type | Basic/Standard/Premium | Modest — Premium slightly stickier |
| Contract Length | Monthly/Quarterly/Annual | Annual = much lower churn risk |
| Total Spend | Lifetime value | Negative — higher spenders stay |
| Engagement Score* | Usage relative to interaction recency | Compound risk signal |
| Support Intensity* | Support calls relative to tenure | Escalating issues flag |
| Value per Month* | Spend divided by tenure | Stagnant-value accounts |

*Engineered features

---

## Models

| Model | AUC | F1 | CV AUC | Notes |
|-------|-----|-----|---------|-------|
| Logistic Regression | 0.913 | 0.825 | 0.914 | Strong baseline; explainable |
| Random Forest | 0.999 | 0.983 | 0.999 | Very strong; handles interactions |
| **Gradient Boosting** | **1.000** | **0.995** | **1.000** | **Recommended for production** |

**Note:** Near-perfect scores reflect the synthetic dataset's deterministic structure.
On real data, expect AUC 0.75–0.90.

---

## Results Summary

| Risk Tier | Customers | Churn Probability | Actual Churn Rate | Action |
|-----------|-----------|------------------|-------------------|--------|
| 🔴 High Risk | 30,274 (47%) | 65–100% | ~100% | Immediate intervention |
| 🟡 Medium Risk | 361 (0.6%) | 35–65% | ~52% | Proactive outreach |
| 🟢 Low Risk | 33,739 (52%) | 0–35% | ~0.1% | Nurture & expand |

---

## Key Churn Drivers

1. **Payment Delay** — the single strongest predictor (r=0.56)
2. **Support Calls** — high support volume signals frustration (r=0.31)
3. **Tenure** — newer customers churn more (r=0.20)
4. **Usage Frequency** — disengaged users leave (r=-0.12)
5. **Contract Length** — annual contracts provide a strong retention anchor

---

## Project Structure

```
churn_project/
├── Customer_Churn_Prediction_SaaS.ipynb     ← Full analysis notebook
├── data.csv                                  ← Dataset (64,374 customers)
├── README.md                                 ← This file
└── outputs/
    ├── Customer_Churn_Prediction_Report.html ← Non-technical business report
    ├── 01_eda_overview.png                   ← Customer behaviour analysis
    ├── 02_model_performance.png              ← ROC, PR, threshold, calibration
    ├── 03_churn_drivers.png                  ← Feature importance & risk tiers
    ├── customers_with_risk.csv               ← All 64,374 customers with risk scores
    ├── feature_importance.csv                ← Feature importances (Random Forest)
    └── model_results.json                    ← Model metrics (machine-readable)
```

---

## How to Run

```bash
pip install numpy pandas matplotlib seaborn scipy scikit-learn
jupyter notebook Customer_Churn_Prediction_SaaS.ipynb
```

---

## Improvements Over the Original Notebook

| Aspect | Original | This Version |
|--------|---------|-------------|
| Models | Logistic Regression only | LR + Random Forest + Gradient Boosting |
| EDA | No visualisations | 6-panel EDA dashboard with business commentary |
| Validation | Train/test split only | Train/val/test + 5-fold CV |
| Feature engineering | Raw features only | 6 SaaS-specific engineered features |
| Evaluation | Basic metrics | AUC, PR-AUC, F1, calibration, threshold analysis |
| Risk segmentation | None | Full 3-tier risk scoring for all 64K customers |
| Business output | Text summary | Risk tiers + retention recommendations + revenue-at-risk |
| HTML report | None | Full non-technical business report |
| Leakage prevention | Implicit | Explicit (preprocessing fitted on train only) |
| Class imbalance | Not addressed | Analysed and confirmed near-balanced |

---

## Ethical Considerations

- **Gender fairness:** Gender is included as a feature. Before deployment, a fairness
  audit should confirm it is not driving discriminatory predictions.
- **Causation vs prediction:** Payment delays are associated with churn — this does not
  mean delays *cause* churn. Interventions should address root causes (product issues,
  pricing friction) not just symptoms.
- **Human oversight:** Model scores should inform — not replace — customer success team judgement.

---

## Next Steps

1. Replace synthetic data with real customer data — expect AUC 0.75–0.90
2. Add time-series features: 30/60/90-day rolling engagement trends
3. Consider survival analysis to predict *when* customers will churn, not just *if*
4. A/B test the retention strategy: use model scores for 50% of accounts, compare churn rates at 90 days
5. Build a live dashboard that updates risk scores weekly from CRM/product analytics data
