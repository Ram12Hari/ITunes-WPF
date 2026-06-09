# Software Configuration Management Plan (SCM)

> **Document ID**: IEC-DOC-007  
> **Version**: 1.0  
> **Date**: 2026-04-09  
> **IEC 62304 Clause**: 8  
> **Status**: Draft  
> **Approved by**: _Pending_

---

## 1. Purpose

This document defines how the Alcon Music Player software and its associated documents are
versioned, stored, changed, and released in accordance with **IEC 62304 §8 — Software Configuration
Management**.

---

## 2. Configuration Items

A **Configuration Item (CI)** is any work product that must be controlled and tracked.

| CI ID | Item | Location | Comments |
|-------|------|----------|---------|
| CI-001 | Source Code | `src/` | All `.cs`, `.xaml`, `.csproj` files |
| CI-002 | Test Code | `tests/` | All xUnit test projects |
| CI-003 | IEC Documents | `IEC_Standard_Docs/` | This folder |
| CI-004 | Requirements | `Requirement/` | Source requirements |
| CI-005 | Architecture Decision Records | `decisions.md` | ADR log |
| CI-006 | Logging Guide | `logging-guide.md` | Technical implementation guide |
| CI-007 | Solution File | `AlconMusicPlayer.sln` | Project references |
| CI-008 | NuGet Lock Files | `*/obj/*.json` | Package restore state |

---

## 3. Version Control System

| Field | Value |
|-------|-------|
| System | Git |
| Repository | Local (migrate to remote for team use) |
| Default Branch | `main` |
| Protected Branches | `main` — no direct commits; merge via pull request |

---

## 4. Branching Strategy

```
main (protected)
  └── feature/<short-description>    ← development work
  └── fix/<issue-id>-<description>   ← bug fixes
  └── docs/<document-id>             ← documentation updates
```

**Rules:**
- All changes go through a feature/fix branch
- Branches are merged to `main` via pull request with at least one reviewer sign-off
- Branch must be up-to-date with `main` before merge
- Delete branch after merge

---

## 5. Labelling and Version Identification

### 5.1 Software Version

Semantic versioning: `MAJOR.MINOR.PATCH`

| Part | Increment When |
|------|---------------|
| `MAJOR` | Breaking change to architecture or public API |
| `MINOR` | New feature added (backward-compatible) |
| `PATCH` | Bug fix or documentation-only change |

Version is set in the `.csproj` `<Version>` property and reflected in the assembly via
`typeof(App).Assembly.GetName().Version` (used in startup log entry).

### 5.2 Document Version

All IEC documents use `MAJOR.MINOR`:
- `MAJOR` increments when content changes require re-approval
- `MINOR` increments for minor corrections that do not change meaning

Each document contains a **Revision History** table at the bottom.

### 5.3 Git Tags

Each release is tagged in Git:
```
v1.0.0    ← initial release
v1.0.1    ← patch
v1.1.0    ← new feature
```

---

## 6. Change Control Process

```
1. Developer identifies change needed
       │
       ▼
2. Open Problem Report (if defect) — see SPRP.md
   OR initiate new feature/doc change
       │
       ▼
3. Create feature/fix branch from main
       │
       ▼
4. Make changes; update Revision History in affected documents
       │
       ▼
5. Run dotnet build + dotnet test — must pass
       │
       ▼
6. Create pull request; assign reviewer
       │
       ▼
7. Reviewer approves or requests changes
       │
       ▼
8. Merge to main; update version; tag if release
```

---

## 7. Build Reproducibility

To reproduce any released build:
1. Check out the Git tag for the release: `git checkout v1.0.0`
2. Run: `dotnet restore && dotnet build --configuration Release`
3. Output is in `src/AlconMusicPlayer.WPF/bin/Release/net8.0-windows/`

> The NuGet lock files (`project.assets.json`) are committed — restore is deterministic.

---

## 8. Backup and Archival

| Requirement | Action |
|------------|--------|
| Source code backup | Push repository to a remote server (GitHub, Azure DevOps, etc.) |
| IEC document backup | Include `IEC_Standard_Docs/` in the same repository |
| Release archive | Tag + GitHub Release with the compiled `.exe` as an artifact |

---

## 9. Revision History

| Version | Date | Author | Description |
|---------|------|--------|-------------|
| 1.0 | 2026-04-09 | Development Team | Initial SCM plan |
