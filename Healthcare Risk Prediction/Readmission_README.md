# Hospital Readmission Risk Prediction
### Expert-Level Healthcare Machine Learning — Diabetic Patient Cohort

---

## What This Project Does

This project builds a clinical risk prediction pipeline that scores every diabetic patient
at discharge with a 30-day readmission risk — enabling care teams to prioritise discharge
planning for the patients most likely to return to hospital.

---

## The Clinical Problem

30-day hospital readmission is a key quality indicator in healthcare. For diabetic patients,
many readmissions are preventable with better discharge planning, medication management,
and follow-up care. Identifying high-risk patients *before* discharge gives clinical teams
a critical window to intervene.

---

## Dataset

| Property | Value |
|----------|-------|
| Source | UCI Diabetes 130-US Hospitals (1999–2008) |
| Original records | 101,766 patient encounters |
| After cleaning | 69,990 unique patients |
| 30-day readmission rate | 8.98% |
| Class balance | ~10:1 negative-to-positive |

**Key cleaning steps:**
- Removed patients discharged to hospice/died (discharge codes 11, 13, 14, 19, 20, 21)
- Restricted to first encounter per patient (prevents leakage across admissions)
- Replaced '?' placeholders with NaN and imputed appropriately

---

## Features Used

| Feature | Clinical Relevance |
|---------|-------------------|
| Prior inpatient admissions | Strongest predictor — frequent attenders |
| Length of stay | Sicker patients stay longer |
| Total prior visits | Overall utilisation pattern |
| Number of medications | Medication burden = disease complexity |
| Number of diagnoses | Comorbidity burden |
| Discharge destination | Home vs SNF/rehab vs other |
| Primary diagnosis (ICD-9 group) | Circulatory highest readmission |
| Admission type | Emergency vs elective |
| A1C result | Glycaemic control |
| Age, race, gender | Demographics |

**Engineered features:** n_meds_active, total_prior_visits, high_utiliser flag, emergency_hx flag, has_primary_diabetes

---

## Models

| Model | AUC | Sensitivity (t=0.15) | Specificity | F1 |
|-------|-----|---------------------|------------|-----|
| Logistic Regression | 0.644 | 0.22 | 0.94 | 0.24 |
| Random Forest | 0.640 | 0.22 | 0.94 | 0.24 |
| **Gradient Boosting** | **0.651** | **0.22** | **0.94** | **0.24** |

**Honest note:** AUC ~0.65 reflects the genuine difficulty of predicting readmission from
administrative data alone. Richer clinical data (labs, vitals, medication adherence) would
substantially improve performance. The risk tiering (4.3% vs 15.4% readmission rate) still
provides meaningful clinical value.

---

## Risk Tiers

| Tier | Patients | Actual Readmission Rate | Action |
|------|---------|------------------------|--------|
| 🔴 High Risk | 23,097 (33%) | ~15.4% | Priority discharge planning |
| 🟡 Medium Risk | 23,796 (34%) | ~7.3% | Enhanced standard care |
| 🟢 Low Risk | 23,097 (33%) | ~4.3% | Routine discharge |

The 3.6× difference between high and low risk tiers confirms the model's clinical utility.

---

## Project Structure

```
readmission_project/
├── Healthcare_Readmission_Prediction.ipynb    ← Full analysis notebook
├── diabetic_data.csv                          ← Raw dataset
├── IDS_mapping.csv                            ← Admission/discharge code mapping
├── README.md                                  ← This file
└── outputs/
    ├── Healthcare_Risk_Prediction_Report.html ← Non-technical HTML report
    ├── 01_eda_overview.png                    ← Patient data exploration
    ├── 02_model_performance.png               ← ROC, PR, threshold, calibration
    ├── 03_risk_stratification.png             ← Risk tiers & clinical drivers
    ├── cleaned_data.csv                       ← Preprocessed dataset
    ├── patients_with_risk.csv                 ← All patients with risk scores
    ├── feature_importance.csv                 ← RF feature importances
    └── model_results.json                     ← Model metrics (machine-readable)
```

---

## How to Run

```bash
pip install numpy pandas matplotlib seaborn scipy scikit-learn
jupyter notebook Healthcare_Readmission_Prediction.ipynb
```

---

## Ethical Considerations

- **Fairness audit required** before deployment — racial disparities in readmission rates exist
- **Data is historical** (1999–2008) — retrain on current data before clinical use
- **Model supports, not replaces, clinical judgement** — risk scores are decision aids
- **Social determinants missing** — model will underestimate risk for socially vulnerable patients
- **Transparency** — Logistic Regression provides fully explainable coefficients if required

---

## Improvements Over Original Notebook

| Aspect | Original | This Version |
|--------|---------|-------------|
| Leakage prevention | Partial | Explicit (hospice/death removal, first-encounter-only) |
| Class imbalance | Not addressed | class_weight='balanced' |
| Models | LR + RF only | LR + RF + GBM with full comparison |
| Evaluation | Basic AUC | AUC, AP, Sensitivity, Specificity, F1, Calibration, Threshold |
| Risk tiering | None | 3-tier scoring for all 70K patients |
| Feature engineering | Basic | + utilisation flags, ICD-9 grouping, med change, A1C flags |
| EDA | Minimal | 8-panel clinical dashboard with equity check |
| Clinical framing | Academic | Full clinical context, intervention recommendations |
| HTML report | None | Full non-technical report for healthcare managers |
