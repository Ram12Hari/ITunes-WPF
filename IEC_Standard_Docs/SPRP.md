# Software Problem Resolution Process (SPRP)

> **Document ID**: IEC-DOC-008  
> **Version**: 1.0  
> **Date**: 2026-04-09  
> **IEC 62304 Clause**: 9  
> **Status**: Draft  
> **Approved by**: _Pending_

---

## 1. Purpose

This document defines the process for identifying, evaluating, resolving, and tracking software
problems (defects, anomalies, and failures) in the **Alcon Music Player** in accordance with
**IEC 62304 §9 — Software Problem Resolution**.

---

## 2. Applicability

This process applies to:
- Problems found during development testing
- Problems found during system testing (recorded in [STR.md](STR.md))
- Problems reported after release by users or stakeholders

---

## 3. Problem Severity Classification

| Severity | Label | Definition | Example |
|---------|-------|-----------|---------|
| 1 | Critical | Application crash or data loss; blocks all use | Unhandled exception on startup |
| 2 | Major | Core feature unusable; no workaround | Cannot create playlist at all |
| 3 | Moderate | Feature partially broken; workaround exists | Search filter returns extra results |
| 4 | Minor | UI defect; functional impact negligible | Incorrect colour on hover state |
| 5 | Cosmetic | Typo, layout issue, log message wording | Incorrect log message format |

> **Release gate**: No severity-1 or severity-2 problems may remain open at time of release.

---

## 4. Problem Report Template

Each problem is recorded as a numbered entry below in §6 using the following fields:

| Field | Description |
|-------|-------------|
| PR-ID | Unique ID: `PR-YYYY-NNN` (e.g., PR-2026-001) |
| Date Reported | Date the problem was first identified |
| Reported By | Person or role who found the problem |
| Severity | 1–5 (see §3) |
| Description | Clear description of the issue and how to reproduce it |
| Affected CI | Which Configuration Item (from SCM.md) is affected |
| Root Cause | Analysis of why the problem occurred |
| Resolution | What was changed to fix it |
| Verification | How the fix was verified (test ID or manual check) |
| Status | Open / In Progress / Resolved / Closed |
| Closed Date | Date closed (blank if open) |

---

## 5. Problem Lifecycle

```
Reported (Open)
      │
      ▼
Triaged — severity assigned
      │
      ▼
Assigned to developer
      │
      ▼
Fix developed on fix/<pr-id> branch
      │
      ▼
Fix verified by test (references test ID)
      │
      ▼
Pull request merged to main
      │
      ▼
Problem Report marked Closed
```

---

## 6. Problem Register

> _Add entries here as problems are discovered. Template entry shown below._

---

### PR-2026-000 _(Example / Template)_

| Field | Value |
|-------|-------|
| PR-ID | PR-2026-000 |
| Date Reported | 2026-04-09 |
| Reported By | Development Team |
| Severity | 5 — Cosmetic |
| Description | This is a template entry. Replace with real problem description. Steps to reproduce: (1) ... (2) ... |
| Affected CI | CI-001 Source Code |
| Root Cause | _Pending investigation_ |
| Resolution | _Pending_ |
| Verification | _Pending_ |
| Status | Open |
| Closed Date | — |

---

## 7. Trend Analysis

After each release, review the problem register to identify:
- Recurring problem types (e.g., multiple validation issues → improve input validation standards)
- Severity distribution (e.g., too many severity-3 issues → increase system test coverage)
- Average time to close by severity

Findings should be recorded in the next revision of this document.

---

## 8. Revision History

| Version | Date | Author | Description |
|---------|------|--------|-------------|
| 1.0 | 2026-04-09 | Development Team | Initial process definition and empty problem register |
