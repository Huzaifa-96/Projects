# A/B Testing & Uplift Modelling for Marketing Campaigns
### A Professional End-to-End Data Science Project

---

## Overview

This project presents a rigorous, production-quality analysis of a **randomised controlled trial (RCT)** evaluating a targeted email discount campaign for an e-commerce retailer. It combines classical frequentist A/B testing with modern **uplift modelling** (causal ML) to answer two distinct business questions:

1. **Did the campaign work on average?** → A/B hypothesis testing
2. **Which individual customers respond best?** → Uplift modelling (ITE estimation)

---

## Business Context

An e-commerce retailer randomly assigns ~5,000 customers to:
- **Control (T=0):** No email — organic behaviour observed
- **Treatment (T=1):** Personalised discount email sent

**Goal:** Maximise incremental conversions and ROI by targeting only the customers who genuinely benefit from the treatment (*persuadables*), not those who would convert anyway (*sure things*) or not respond at all (*lost causes*).

---

## Dataset

| Column | Description |
|--------|-------------|
| `customer_id` | Unique customer identifier |
| `age` | Customer age (18–70) |
| `gender` | Male / Female |
| `loyalty_score` | 0–100 loyalty programme score |
| `previous_purchases` | Historical purchase count |
| `income_bracket` | Low / Medium / High |
| `treatment` | 1 = received email, 0 = control |
| `conversion` | 1 = converted, 0 = did not |
| `spend` | Revenue generated (£) |

- **5,000 rows**, **no missing values**
- ~50/50 treatment split (confirmed by SRM test)

---

## Methodology

### 1. Data Quality & Randomisation Validation
- **Sample Ratio Mismatch (SRM) test** — chi-squared to confirm no randomisation failure
- **Covariate balance** — KS tests (continuous) and chi-squared (categorical) across all pre-treatment features
- Data quality assertions (binary labels, non-negative spend, etc.)

### 2. Exploratory Data Analysis
- Distribution checks by treatment arm (age, loyalty, income)
- Conversion rates across customer segments (age group, loyalty tier, income)
- Spend distribution analysis (log-scale, converter-only)

### 3. A/B Test — Hypothesis Testing
| Method | Purpose |
|--------|---------|
| Two-proportion z-test | Primary test for conversion rate difference |
| Chi-squared test | Cross-validation of z-test |
| Mann-Whitney U | Non-parametric test for spend (skewed distribution) |
| Wilson score CI | Confidence intervals for proportions (better than Wald at boundaries) |
| Bootstrap | Model-free uncertainty quantification of lift |
| Power analysis | Retrospective and prospective sample size planning |

**Subgroup Analysis:** Forest plot of absolute lift with 95% CIs across gender, income, age groups, and loyalty tiers.

### 4. Uplift Modelling — Individual Treatment Effects

We implement two **meta-learner** frameworks:

#### T-Learner (Two-Model)
Fit separate calibrated models for control ($\mu_0$) and treatment ($\mu_1$):
$$\hat{\tau}_i = \hat{\mu}_1(X_i) - \hat{\mu}_0(X_i)$$

Base models: Logistic Regression, Random Forest, Gradient Boosting Machine

#### S-Learner (Single-Model)
Single model with treatment as a feature:
$$\hat{\tau}_i = \hat{\mu}(X_i, T=1) - \hat{\mu}(X_i, T=0)$$

Base model: Gradient Boosting Machine

All models use **probability calibration** (isotonic regression, CV=3) to produce reliable propensity scores.

### 5. Uplift Model Evaluation

**Qini Curve:** Uplift equivalent of ROC curve — measures incremental conversions captured at each targeting fraction vs random baseline. Qini coefficient = normalised area between model curve and random line.

**Decile Table:** Actual observed uplift per predicted-score decile — tests whether high predicted scores correspond to high actual treatment effects.

**Uplift Score Distribution:** Checks that the model assigns a meaningful range of ITE estimates.

### 6. Business Targeting Analysis

- ROI simulation at each targeting threshold (cost = £0.50/contact, AOV = £50)
- Optimal targeting percentage identification
- Three-segment framework: **Persuadables / Borderline / Low-value**
- Feature importance (GBM) to understand which customer characteristics predict responsiveness

---

## Key Results

| Metric | Value |
|--------|-------|
| Control conversion rate | 7.80% |
| Treatment conversion rate | 13.25% |
| Absolute lift | **+5.45 pp** |
| Relative lift | **+69.9%** |
| p-value | **< 0.001** (highly significant) |
| Best uplift model | T-Learner (Logistic Regression) |
| Top decile actual uplift | ~17% |
| Bottom decile actual uplift | ~3.5% |

### Recommendations
1. **Roll out the campaign** — large, statistically significant effect
2. **Prioritise top 30% by uplift score** — 5× higher conversion than bottom 30%
3. **Avoid bottom 30%** — "Sure Things" and "Lost Causes" — wasted spend
4. **Target high-loyalty, older-age segments** — show above-average lift
5. **Future experiments:** 3pp MDE detectable at ~1,500 per arm

---

## Project Structure

```
ab_uplift_project/
├── AB_Uplift_Professional.ipynb   # Main analysis notebook
├── data.csv                       # Dataset (5,000 rows)
├── README.md                      # This file
└── outputs/
    ├── 01_eda_overview.png        # EDA dashboard
    ├── 02_ab_test_analysis.png    # A/B test results
    ├── 03_uplift_evaluation.png   # Qini curves & decile table
    ├── 04_business_targeting.png  # ROI & feature importance
    ├── uplift_scores.csv          # Model predictions on test set
    └── ab_results.json            # A/B test stats (machine-readable)
```

---

## How to Run

```bash
# Install dependencies
pip install numpy pandas matplotlib seaborn scipy scikit-learn

# Open the notebook
jupyter notebook AB_Uplift_Professional.ipynb
```

---

## Design Decisions

**Why Wilson score CIs instead of Wald?**
Wald intervals (p ± 1.96·SE) can have poor coverage near 0 or 1. Wilson score intervals have better frequentist properties and are the standard for proportion CIs at any sample size.

**Why calibrate the uplift models?**
Uplift models are used for ranking and scoring, not just classification. Calibrated probabilities ensure that ITE estimates (p1 − p0) are on a meaningful scale and not distorted by the model's internal sigmoid transformations.

**Why the T-Learner over S-Learner?**
The S-Learner risks regularising the treatment effect toward zero if the treatment indicator is seen as a weak predictor. The T-Learner allows each arm's model to capture the response surface independently, which is generally preferable when treatment heterogeneity is the primary interest.

**Why Mann-Whitney U for spend?**
Spend is heavily zero-inflated and right-skewed (most customers spend £0). A parametric t-test would violate normality assumptions. Mann-Whitney U is the appropriate non-parametric test for this distribution.

---

## Author Notes

This project demonstrates the full production pipeline from raw data to business recommendation. In a real-world deployment, additional steps would include:

- **CUPED** (Controlled-experiment Using Pre-Experiment Data) for variance reduction
- **Doubly Robust / DR-Learner** for more robust ITE estimation with propensity weighting
- **Cross-fitting (DML)** to avoid overfitting in the nuisance models
- **SHAP values** for local interpretability of uplift predictions
- **Online holdback** to validate the targeting strategy before full rollout

