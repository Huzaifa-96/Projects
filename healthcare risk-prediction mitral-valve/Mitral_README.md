# Predicting Patient Outcomes After Mitral Valve Surgery
### A Professional Healthcare Risk Prediction Project

---

## What This Project Is About

This project analyses data from **1,000 patients** who underwent mitral valve surgery,
with the goal of predicting whether each patient's heart valve function improved
12 months after the procedure.

It applies six machine learning models, evaluates them rigorously against clinical standards,
and delivers an honest interpretation of the results — including a clear explanation of
why the models cannot perform well, and what would be needed to build a genuinely useful
clinical risk prediction tool.

**This project is written to be understood by non-technical readers, including clinicians.**

---

## The Key Finding (Plain English)

> After testing six different machine learning models — including advanced techniques
> like Random Forest and Gradient Boosting — we found that the available patient
> information (surgery type, gender, smoking status, baseline valve status) **cannot
> reliably predict who will improve after surgery**.
>
> All models perform at approximately random chance (AUC ≈ 0.50).
> Statistical tests confirm that none of the available variables are significantly
> linked to the outcome.
>
> This is not a modelling failure — it is an important clinical insight: richer
> clinical data is needed before a prediction model can be built.

---

## Project Structure

```
mitral_project/
├── Mitral_Valve_Risk_Prediction.ipynb    ← Full Python analysis notebook
├── mitral_valve.csv                      ← Dataset (1,000 patients)
├── README.md                             ← This file
└── outputs/
    ├── Healthcare_Risk_Prediction_Report.html   ← Non-technical HTML report
    ├── 01_eda_overview.png               ← Patient data exploration
    ├── 02_model_performance.png          ← ROC curves, AUC, metrics
    └── 03_interpretability.png           ← Feature importance, thresholds
```

---

## The Dataset

| Variable | Description | Role |
|----------|-------------|------|
| Surgery Type | Mini or Conventional | Predictor |
| Gender | Male / Female | Predictor |
| Smoking | Yes / No | Predictor |
| Baseline Valve Status | Improved / Not Improved before surgery | Predictor |
| Post-Surgery Status | Improved / Not Improved at 12 months | **Target** |

- 1,000 patients, no missing data
- Near-balanced classes: 46.9% improved, 53.1% did not
- All variables are categorical — no continuous clinical measurements

---

## Models Tested

| Model | Type | Purpose |
|-------|------|---------|
| No-Skill Baseline | Always predicts majority class | Floor reference |
| Logistic Regression | Linear probability | Standard clinical baseline |
| KNN (k=15) | Similarity-based | From original project |
| SVM (RBF kernel) | Margin-based | From original project |
| Random Forest (n=200) | Tree ensemble | Industry standard for tabular data |
| Gradient Boosting (n=200) | Sequential ensemble | Best performer for structured clinical data |

---

## Results Summary

| Model | AUC | Sensitivity | Specificity | F1 | Verdict |
|-------|-----|-------------|-------------|-----|---------|
| No-Skill Baseline | 0.500 | 0.00 | 1.00 | 0.00 | Floor |
| Logistic Regression | 0.497 | 0.245 | 0.792 | 0.331 | Below chance |
| KNN | 0.499 | 0.585 | 0.387 | 0.514 | Random level |
| SVM (RBF) | 0.555 | 0.000 | 1.000 | 0.000 | Predicts nobody improves |
| Random Forest | 0.481 | 0.426 | 0.481 | 0.423 | Below chance |
| Gradient Boosting | 0.486 | 0.404 | 0.604 | 0.437 | Near random |

**AUC = 0.50 means random guessing. AUC = 1.00 means perfect prediction.**

---

## Why the Models Cannot Predict Well

Chi-squared statistical tests show that **none** of the five available variables are
significantly associated with the outcome (all p-values > 0.13). This means:

- The data contains no meaningful signal for the models to learn from
- Increasing model complexity (KNN → SVM → Random Forest → GBM) makes no difference
- The limitation is in the data, not the modelling approach

---

## What Would Make This Useful

A clinically meaningful cardiac outcome prediction model would require:

- **Echocardiography:** Ejection fraction, valve dimensions, regurgitation grade
- **Patient age and BMI**
- **Comorbidities:** Atrial fibrillation, heart failure, diabetes, renal disease
- **Symptom severity:** NYHA class, symptom duration
- **Surgical details:** Bypass time, repair vs replacement technique

With richer data, Random Forest and Gradient Boosting would be the recommended models.

---

## Methodology

- **Validation:** Stratified 80/20 train/test split + 10-fold cross-validation
- **Metrics:** AUC, Sensitivity, Specificity, Precision, F1, Brier score, Calibration
- **Calibration:** Probability calibration applied to SVM
- **Imbalance:** No correction needed (near-balanced: 47%/53%)
- **Feature engineering:** Binary encoding + baseline × surgery interaction term
- **Leakage check:** Post-surgery outcome confirmed not used as input

---

## How to Run

```bash
# Dependencies
pip install numpy pandas matplotlib seaborn scipy scikit-learn

# Open the notebook
jupyter notebook Mitral_Valve_Risk_Prediction.ipynb
```

---

## Ethical Statement

- This model is **not suitable for clinical use** in its current form
- Any clinical risk prediction model must be externally validated before deployment
- Gender and smoking are included as predictors for completeness; clinical justification
  should be established before including them in a real-world model
- Prediction does not equal causation

---

## Improvements Over the Original Project

| Aspect | Original | This Version |
|--------|---------|-------------|
| Language | Technical R/caret | Plain English + clinical context |
| Models | KNN + SVM only | 6 models including Random Forest, GBM |
| Statistical testing | Basic CM + AUC | Chi-squared, Bootstrap CIs, Calibration |
| Evaluation | Accuracy, basic AUC | AUC, Sensitivity, Specificity, PR AUC, Brier, F1 |
| Threshold analysis | Default (0.5) only | Full threshold sensitivity analysis |
| Interpretability | None | Feature importance (3 models), coefficients |
| Honest assessment | Partial | Full honest clinical interpretation |
| Non-technical report | None | Full HTML report for clinical audiences |
| Data leakage check | Implicit | Explicit assertion |
| Clinical recommendations | Minimal | Detailed, actionable |
