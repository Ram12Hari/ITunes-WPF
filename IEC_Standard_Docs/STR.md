# Software Test Report (STR)

> **Document ID**: IEC-DOC-006  
> **Version**: 1.0  
> **Date**: _Fill on test execution_  
> **IEC 62304 Clause**: 5.7.5  
> **Status**: Template — populate after each test run  
> **Approved by**: _Pending_

---

## 1. Purpose

This document records the results of all test activities defined in [STP.md](STP.md).
A new revision of this document must be created for each software release.

---

## 2. Test Execution Summary

| Field | Value |
|-------|-------|
| Test Run Date | _yyyy-MM-dd_ |
| Software Version | _x.y.z_ |
| Build Configuration | Debug / Release |
| Tester | _Name_ |
| Environment | Windows 10/11, .NET 8 SDK |
| Test Tool | `dotnet test` + manual |

---

## 3. Automated Test Results

Run command:
```bash
dotnet test --logger "console;verbosity=detailed"
```

### 3.1 Summary

| Project | Total Tests | Passed | Failed | Skipped |
|---------|------------|--------|--------|---------|
| `AlconMusicPlayer.Domain.Tests` | — | — | — | — |
| `AlconMusicPlayer.ApplicationService.Tests` | — | — | — | — |
| `AlconMusicPlayer.Infra.Tests` | — | — | — | — |
| **Total** | — | — | — | — |

### 3.2 Failed Tests

> _List any failed tests here. If none, write "None"._

| Test ID | Test Name | Failure Message | Resolution |
|---------|-----------|----------------|-----------|
| — | — | — | — |

---

## 4. System Test Results

| Test ID | Description | Result | Notes |
|---------|------------|--------|-------|
| ST-001 | Application startup < 3 sec | Pass / Fail | |
| ST-002 | Log file created on startup | Pass / Fail | |
| ST-010 | Songs DataGrid displays ≥35 tracks | Pass / Fail | |
| ST-011 | Real-time search filter | Pass / Fail | |
| ST-012 | Clear search | Pass / Fail | |
| ST-013 | Selected row highlighted | Pass / Fail | |
| ST-020 | Artists list displayed | Pass / Fail | |
| ST-021 | Artist → albums drill-down | Pass / Fail | |
| ST-022 | Artist+album → tracks drill-down | Pass / Fail | |
| ST-023 | Albums list displayed | Pass / Fail | |
| ST-024 | Album → tracks | Pass / Fail | |
| ST-030 | Playlists view loads | Pass / Fail | |
| ST-031 | Create playlist | Pass / Fail | |
| ST-032 | Empty name rejected | Pass / Fail | |
| ST-033 | Duplicate name rejected | Pass / Fail | |
| ST-034 | Playlist detail view | Pass / Fail | |
| ST-040 | Context menu appears on right-click | Pass / Fail | |
| ST-041 | Add to Playlist submenu | Pass / Fail | |
| ST-042 | Track added to playlist | Pass / Fail | |
| ST-050 | Log contains playlist created entry | Pass / Fail | |
| ST-051 | Log contains duplicate warning entry | Pass / Fail | |
| ST-052 | Log contains shutdown entry | Pass / Fail | |

---

## 5. Overall Result

| Field | Value |
|-------|-------|
| Automated tests | Pass / Fail |
| System tests | Pass / Fail |
| Open severity-1/2 problem reports | _Count_ |
| **Overall** | **Pass / Fail** |

---

## 6. Outstanding Issues

> _List any issues found during this test run that require a problem report in [SPRP.md](SPRP.md)._

| Issue | Severity | Problem Report ID | Status |
|-------|---------|-----------------|--------|
| — | — | — | — |

---

## 7. Sign-off

| Role | Name | Signature | Date |
|------|------|-----------|------|
| Tester | | | |
| Reviewer | | | |
| Approver | | | |

---

## 8. Revision History

| Version | Date | Author | Description |
|---------|------|--------|-------------|
| 1.0 | 2026-04-09 | Development Team | Template created |
