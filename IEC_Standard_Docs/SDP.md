# Software Development Plan (SDP)

> **Document ID**: IEC-DOC-001  
> **Version**: 1.0  
> **Date**: 2026-04-09  
> **IEC 62304 Clause**: 5.1  
> **Status**: Draft  
> **Approved by**: _Pending_

---

## 1. Purpose and Scope

This Software Development Plan (SDP) defines the lifecycle processes, methods, tools, and standards
used to develop the **Alcon Music Player** software. It establishes how the development team will
plan, implement, verify, and maintain the software in accordance with **IEC 62304:2006+AMD1:2015**.

---

## 2. Software Safety Classification

| Field | Value |
|-------|-------|
| Classification | **Class A** |
| Justification | No injury to patient or operator is possible from a software failure. The software has no control over physical devices, no patient data, and no life-critical functionality. |
| Review required if | Software is integrated into a medical device system, or if data processed could influence clinical decisions |

---

## 3. Development Lifecycle Model

The project follows an iterative development model with the following phases:

```
Planning → Requirements → Architecture → Detailed Design
                                              │
                                    Implementation + Unit Testing
                                              │
                                    Integration Testing
                                              │
                                    System Testing → Release
```

Each phase produces one or more documents listed in [README.md](README.md).

---

## 4. Roles and Responsibilities

| Role | Responsibility |
|------|---------------|
| Software Developer | Design, implement, unit test, and document all software items |
| Software Reviewer | Review code, test results, and documentation for correctness |
| Configuration Manager | Maintain version control, change records, and release artifacts |
| Risk Manager | Identify and evaluate software-related risks per ISO 14971 |

> For a small team, one individual may hold multiple roles provided reviews are not self-approved.

---

## 5. Development Standards and Conventions

### 5.1 Language and Framework
- Language: C# (latest stable features for .NET 8)
- Framework: WPF (.NET 8 LTS — `net8.0-windows`)
- Build system: `dotnet` CLI / Visual Studio 2022+

### 5.2 Architecture Pattern
- Clean Architecture (4-project solution)
- MVVM pattern in the WPF presentation layer
- Refer to [SAD.md](SAD.md) for full architectural breakdown

### 5.3 Coding Conventions
- PascalCase for public members; `_camelCase` for private fields
- XML doc comments (`///`) on all public interfaces and service contracts
- ADR prefix (`// ADR-NNN:`) for comments that reference architecture decisions
- No magic strings or inline colours in XAML — use `{StaticResource}` only
- No code-behind in Views — all logic in ViewModels

### 5.4 Dependency Management
- Permitted NuGet packages:
  - `Microsoft.Extensions.DependencyInjection` (WPF project)
  - `Microsoft.Extensions.Logging.Abstractions` (ApplicationService project)
  - `xunit` + `xunit.runner.visualstudio` (test projects only)
- No additional packages without a corresponding ADR entry in `decisions.md`

### 5.5 Version Control
- All source code and documentation maintained in Git
- Branching strategy: feature branches → main
- Each IEC document change must include a meaningful commit message referencing the document ID
- Refer to [SCM.md](SCM.md) for full configuration management rules

---

## 6. Planning Estimates

| Phase | Deliverable | Status |
|-------|------------|--------|
| Requirements | SRS.md complete | Draft |
| Architecture | SAD.md complete | Draft |
| Detailed Design | SDD.md complete | Draft |
| Implementation | All 4 projects compile and run | Complete |
| Unit Testing | Domain + ApplicationService tests passing | Partial |
| Integration Testing | End-to-end use case tests | Planned |
| System Testing | All SRS requirements verified | Planned |
| Documentation | All IEC docs approved | Draft |

---

## 7. Tools

| Tool | Purpose | Version |
|------|---------|---------|
| Visual Studio 2022 / VS Code | IDE | Latest |
| dotnet CLI | Build and run | .NET 8 SDK |
| xUnit | Unit and integration testing | 2.9+ |
| Git | Version control | Latest |
| Markdown | Documentation format | — |

---

## 8. Entry and Exit Criteria

### 8.1 Entry Criteria (start of development phase)
- SRS is approved
- SAD is approved
- Development environment is configured and verified

### 8.2 Exit Criteria (software release)
- All SRS requirements have at least one passing test (traced in STP.md)
- All `[ERR]` and `[CRT]` log entries from system test runs have been investigated and resolved
- Risk Analysis (RiskAnalysis.md) shows no unacceptable residual risks
- STR.md is complete and approved
- No open problem reports with severity 1 or 2 (see SPRP.md)

---

## 9. Revision History

| Version | Date | Author | Description |
|---------|------|--------|-------------|
| 1.0 | 2026-04-09 | Development Team | Initial draft |
