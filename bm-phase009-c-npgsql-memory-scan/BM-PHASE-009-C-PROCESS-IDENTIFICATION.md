# BM-PHASE-009-C — Process Identification

## CHIFA-OFFICINE Process

### Basic Information

| Field | Value |
|-------|-------|
| Executable | `CHIFA_OFFICINE.exe` |
| PID | 16376 |
| Architecture | 32-bit (WOW64 on 64-bit OS) |
| Owner | SALON-AMD\pc |
| Start Time | 2026-07-28 17:51:03 |
| Process Status | Continuously running (not restarted during investigation) |

### Assembly Identity

The running EXE reports its assembly name as:

```
CHIFA_OFFICINE_INSECURE
```

This differs from the expected `CHIFA_OFFICINE`, suggesting a debug or unprotected build variant. The file on disk remains `CHIFA_OFFICINE.exe`.

### Obfuscation

| Tool | Version | Impact |
|------|---------|--------|
| ConfuserEx | 1.0.0 | Applied to main EXE |
| Effect | Prevents ildasm/decompilation | |
| Non-obfuscated assemblies | Npgsql.dll (2.0.14.3), ConfuserEx itself | Successfully IL-disassembled |

### Memory Scan Compatibility

The 32-bit WOW64 process was fully readable using standard `OpenProcess` + `VirtualQueryEx` + `ReadProcessMemory` API calls. All committed memory regions (MEM_COMMIT with PAGE_READWRITE) were accessible.

### Process Lifetime

The process was started at 17:51:03 on 2026-07-28 and remained active throughout both scans (Scan A and Scan B), with no restarts between scans.

### Confirmation

CHIFA process identification is **CONFIRMED**.
