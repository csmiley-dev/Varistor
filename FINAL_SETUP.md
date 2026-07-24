# Final Setup Guide

## Simple, Clean Installation

Everything is configured for your exact setup:
- PDC and VAR stored in **X:\BMS\Programs\**
- Projects created in **X:\Projects\**
- Only shortcuts in project folders

## One-Command Setup

```batch
deploy-complete.bat
```

That's it! This creates:

```
X:\BMS\Programs\
├── PDC\
│   ├── PDC.exe
│   ├── clients.db (client database)
│   └── [~200 runtime files]
└── VAR\
    ├── VAR.exe
    └── [~200 runtime files]

X:\Projects\
└── PDC.lnk  ← Double-click this to create projects
```

## How to Use

### Create a Project
1. Open `X:\Projects\`
2. Double-click `PDC.lnk`
3. Enter project details
4. Click "Create Project"

PDC creates:
```
X:\Projects\MyProject 001\
├── [All your folder structure]
└── Variations\
    ├── VAR.lnk     ← Shortcut to X:\BMS\Programs\VAR\
    └── project.db  ← Project database
```

### Manage Variations
1. Navigate to `X:\Projects\[ProjectName]\Variations\`
2. Double-click `VAR.lnk`
3. Create and manage variations

## What's Different

**Before (messy):**
```
Project\Variations\
├── VAR.exe
├── VAR.dll
├── System.Data.SQLite.dll
├── [195 more files...]
├── runtimes\
│   └── [50 more files...]
└── project.db

Total: 200+ files, 65MB per project
```

**After (clean):**
```
Project\Variations\
├── VAR.lnk     ← 1KB shortcut
└── project.db  ← Your data

Total: 2 files, ~50KB
```

## Benefits

✅ **Clean folders** - No DLL clutter in projects
✅ **Easy updates** - Update X:\BMS\Programs\ once, all projects get it
✅ **Network efficient** - Save 65MB × number of projects
✅ **Simple** - One deployment script does everything

## Updating Applications

### Update PDC:
```batch
# Build new version
dotnet publish PDC/PDC.csproj -c Release -r win-x64 --self-contained true

# Copy to network
xcopy "PDC\bin\Release\net8.0-windows\win-x64\publish\*.*" "X:\BMS\Programs\PDC\" /Y /E /I
```

### Update VAR:
```batch
# Build new version
dotnet publish VAR/VAR.csproj -c Release -r win-x64 --self-contained true

# Copy to network
xcopy "VAR\bin\Release\net8.0-windows\win-x64\publish\*.*" "X:\BMS\Programs\VAR\" /Y /E /I
```

All existing projects automatically use the updated version!

## File Locations

| What | Where | Size |
|------|-------|------|
| PDC Application | X:\BMS\Programs\PDC\ | ~65MB |
| VAR Application | X:\BMS\Programs\VAR\ | ~65MB |
| Client Database | X:\BMS\Programs\PDC\clients.db | ~50KB |
| PDC Shortcut | X:\Projects\PDC.lnk | 1KB |
| Projects | X:\Projects\[ProjectName]\ | Varies |
| VAR Shortcut | X:\Projects\[Project]\Variations\VAR.lnk | 1KB |
| Project Database | X:\Projects\[Project]\Variations\project.db | Varies |

## Troubleshooting

**"X: drive not found"**
- Connect to the network drive first

**"Cannot create project"**
- Check you have write permissions to X:\Projects\

**"Database not found" in VAR**
- Right-click VAR.lnk → Properties
- "Start in" should be the Variations folder path

**Shortcut doesn't work**
- Check X:\BMS\Programs\VAR\VAR.exe exists
- Run deploy-complete.bat again if needed

## Summary

**One-time setup:**
```batch
deploy-complete.bat
```

**Daily use:**
1. Double-click `X:\Projects\PDC.lnk`
2. Create projects
3. Use `VAR.lnk` in each project

**Clean, simple, efficient!**
