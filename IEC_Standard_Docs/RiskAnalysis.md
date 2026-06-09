# Software Risk Analysis

> **Document ID**: IEC-DOC-009  
> **Version**: 1.0  
> **Date**: 2026-04-09  
> **IEC 62304 Clause**: 7 (Software Risk Management)  
> **Related Standard**: ISO 14971:2019 — Application of Risk Management to Medical Devices  
> **Status**: Draft  
> **Approved by**: _Pending_

---

## 1. Purpose

This document identifies and evaluates software-specific hazards and risks for the
**Alcon Music Player**, and defines controls to reduce residual risk to an acceptable level.

This analysis is performed in accordance with **IEC 62304 §7** and uses the risk management
framework defined in **ISO 14971:2019**.

---

## 2. Software Safety Classification

| Field | Value |
|-------|-------|
| Classification | **Class A** |
| Basis | No injury to patient or operator is possible from any failure of this software |
| Implication | Class A requires only identification of software items contributing to hazardous situations — no detailed risk control measures are mandated |

---

## 3. Risk Evaluation Criteria

### 3.1 Probability of Occurrence

| Level | Label | Description |
|-------|-------|-------------|
| 1 | Negligible | Extremely unlikely in the software's lifetime |
| 2 | Remote | Could occur but unlikely |
| 3 | Occasional | May occur a few times |
| 4 | Probable | Likely to occur multiple times |
| 5 | Frequent | Expected to occur regularly |

### 3.2 Severity of Harm

| Level | Label | Description |
|-------|-------|-------------|
| 1 | Negligible | No impact on user or patient |
| 2 | Minor | Inconvenience; no clinical impact |
| 3 | Moderate | Temporary inconvenience; easily recoverable |
| 4 | Serious | Significant impact; not easily recoverable |
| 5 | Critical | Severe harm possible |

### 3.3 Risk Acceptability Matrix

|  | **Sev 1** | **Sev 2** | **Sev 3** | **Sev 4** | **Sev 5** |
|--|-----------|-----------|-----------|-----------|-----------|
| **Prob 5** | Low | Low | Medium | High | High |
| **Prob 4** | Low | Low | Medium | High | High |
| **Prob 3** | Low | Low | Medium | Medium | High |
| **Prob 2** | Low | Low | Low | Medium | Medium |
| **Prob 1** | Low | Low | Low | Low | Low |

- **Low**: Acceptable — no additional controls required
- **Medium**: ALARP — reduce if reasonably practicable
- **High**: Unacceptable — must be reduced before release

---

## 4. Hazard Identification and Risk Assessment

| Risk ID | Hazard | Cause | Affected CI | Prob | Sev | Rating | Controls | Residual Prob | Residual Sev | Residual Rating |
|---------|--------|-------|------------|------|-----|--------|----------|--------------|-------------|----------------|
| R-001 | Application crashes unexpectedly | Unhandled exception in ViewModel or use case | CI-001 Source Code | 3 | 2 | Low | (1) Global `DispatcherUnhandledException` handler in `App.xaml.cs` logs and shows friendly message. (2) All use cases throw typed exceptions caught by ViewModels. | 2 | 2 | Low |
| R-002 | User loses unsaved playlist data | Application crash before in-memory state is persisted | CI-001 Source Code | 3 | 2 | Low | No persistent storage is required — all data is seed data. Playlists exist in-memory only by design. Loss of playlist data is expected on exit. | 1 | 2 | Low |
| R-003 | Incorrect song displayed as "Now Playing" | ViewModel state synchronisation bug | CI-001 Source Code | 2 | 1 | Low | `NowPlayingViewModel` is a Singleton; updated only via explicit command. No asynchronous mutations. | 1 | 1 | Low |
| R-004 | Duplicate track added to playlist | Missing idempotency in `AddTrack` | CI-001 Source Code | 2 | 1 | Low | `Playlist.AddTrack()` is idempotent — silently ignores duplicate. Covered by UT-D-014. | 1 | 1 | Low |
| R-005 | Incorrect track removed from wrong playlist | Use case resolves wrong playlist ID | CI-001 Source Code | 1 | 2 | Low | `RemoveTrackFromPlaylistUseCase` validates playlist ID before mutation. Covered by UT-AS-016. | 1 | 1 | Low |
| R-006 | Log file grows without bound | Application restarts frequently; log not rotated | CI-001 Source Code | 2 | 1 | Low | Log file is append-only; for long-running use, manual rotation or a rotation provider can be added. Current Class A classification does not require automated rotation. | 2 | 1 | Low |
| R-007 | Sensitive data written to log | Developer accidentally logs track path or user input | CI-001 Source Code | 2 | 2 | Low | Logging style guide in `logging-guide.md` specifies logging business events only (names and IDs). No passwords or personal data exist in this system. | 1 | 1 | Low |
| R-008 | UI becomes unresponsive | Long-running operation on UI thread | CI-001 Source Code | 2 | 2 | Low | All data access is synchronous in-memory (< 1 ms). No I/O or network calls. No async risk. | 1 | 1 | Low |
| R-009 | Build produces incorrect output | Wrong dependency version resolved | CI-007, CI-008 | 1 | 2 | Low | NuGet lock files committed to version control ensure deterministic restore (SCM.md §7). | 1 | 1 | Low |

---

## 5. Software Items Contributing to Hazardous Situations

Per IEC 62304 §7.1.2 (Class A), the following software items are identified as those that could
*theoretically* contribute to a hazardous situation. For Class A, no hazardous situations are
identified — all residual risks are rated **Low**.

| Software Item | Potential Contribution | Notes |
|--------------|----------------------|-------|
| `CreatePlaylistUseCase` | Incorrect validation could create duplicate data | Mitigated by UT-AS-004/005 |
| `RemoveTrackFromPlaylistUseCase` | Wrong ID resolution could remove wrong item | Mitigated by UT-AS-016 |
| `App.xaml.cs` (DI composition) | Misconfigured DI could inject wrong implementation | Mitigated by integration tests |
| `FileLoggerProvider` | File write failure could suppress error evidence | Low probability; non-critical for Class A |

---

## 6. Risk Benefit Assessment

All identified risks are rated **Low** in residual state. The benefits of the software
(music browsing, playlist management for patient entertainment or wellness contexts) outweigh
the negligible residual risks. No unacceptable risks remain.

---

## 7. Risk Management Summary

| Total Risks Identified | 9 |
|------------------------|---|
| Pre-control High | 0 |
| Pre-control Medium | 0 |
| Pre-control Low | 9 |
| Post-control High | 0 |
| Post-control Medium | 0 |
| Post-control Low | 9 |
| Release decision | **Acceptable — no blocking risks** |

---

## 8. Revision History

| Version | Date | Author | Description |
|---------|------|--------|-------------|
| 1.0 | 2026-04-09 | Development Team | Initial risk analysis for Class A software |
