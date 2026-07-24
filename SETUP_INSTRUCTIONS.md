# Setup Instructions - X:\BMS\Programs Installation

## Overview

This setup stores the VAR application files centrally at `X:\BMS\Programs\VAR\` and creates shortcuts in each project's Variations folder. This keeps your project folders clean and organized.

## File Structure

```
X:\BMS\Programs\
├── PDC\                    ← PDC application files (~200 files, 65MB)
│   └── PDC.exe
└── VAR\                    ← VAR application files (~200 files, 65MB)
    └── VAR.exe

X:\Projects\                ← Your projects folder (can be anywhere)
├── [Project 1]\
│   └── Variations\
│       ├── VAR.lnk         ← Shortcut to X:\BMS\Programs\VAR\VAR.exe
│       └── project.db      ← Project data
├── [Project 2]\
│   └── Variations\
│       ├── VAR.lnk
│       └── project.db
└── ...
```

## Installation Steps

### Step 1: Deploy Applications to X:\BMS\Programs

From your development folder, run:

```batch
deploy-to-network.bat
```

This will:
- Create `X:\BMS\Programs\PDC\` and copy all PDC files
- Create `X:\BMS\Programs\VAR\` and copy all VAR files

**Result:**
```
X:\BMS\Programs\PDC\PDC.exe
X:\BMS\Programs\VAR\VAR.exe
```

### Step 2: Set Up PDC in Your Projects Folder

You have two options:

#### Option A: Copy PDC.exe (Recommended)
```batch
# Copy PDC to your main projects folder
xcopy "X:\BMS\Programs\PDC\*.*" "X:\Projects\" /Y /E /I
```

Now you can run `X:\Projects\PDC.exe` to create projects.

#### Option B: Create a Shortcut to PDC
1. Right-click in `X:\Projects\`
2. New → Shortcut
3. Location: `X:\BMS\Programs\PDC\PDC.exe`
4. Name: `PDC`

### Step 3: Create Projects

1. Navigate to your projects folder (e.g., `X:\Projects\`)
2. Run `PDC.exe` (or double-click the shortcut)
3. Enter project details
4. Click "Create Project"

**PDC will automatically:**
- Create the project folder structure
- Create `project.db` in the Variations folder
- Create a shortcut `VAR.lnk` that points to `X:\BMS\Programs\VAR\VAR.exe`

### Step 4: Use VAR

For each project:
1. Navigate to `[Project]\Variations\`
2. Double-click `VAR.lnk`
3. VAR opens and reads `project.db` from the same folder

## Advantages of This Setup

### Clean Project Folders
Each Variations folder contains only:
- `VAR.lnk` (1 KB shortcut)
- `project.db` (your data)

**Not cluttered with:**
- ❌ VAR.exe
- ❌ VAR.dll
- ❌ System.Data.SQLite.dll
- ❌ 195+ other runtime files
- ❌ runtimes folder

### Easy Updates
To update VAR for all projects:
1. Build new version: `dotnet publish VAR/VAR.csproj -c Release -r win-x64 --self-contained true`
2. Copy to: `X:\BMS\Programs\VAR\`
3. Done! All projects automatically use the new version

No need to update each project individually.

### Network Efficiency

**Before (copying files to each project):**
- 10 projects = 2000 files, 650 MB
- 50 projects = 10,000 files, 3.25 GB

**After (using shortcuts):**
- 10 projects = 220 files, 65.5 MB
- 50 projects = 300 files, 67.5 MB

**Savings for 50 projects: ~9,700 files and ~3.2 GB!**

### Centralized Management
- All application files in one known location: `X:\BMS\Programs\`
- Easy to backup
- Easy to find
- Professional organization

## Migrating Existing Projects

If you have existing projects with VAR files already copied (like your ACME INC 3223 project):

### Option 1: Manual Migration
1. Navigate to the project's Variations folder
2. Delete everything EXCEPT `project.db`
3. Create a shortcut:
   - Right-click → New → Shortcut
   - Target: `X:\BMS\Programs\VAR\VAR.exe`
   - Start in: [The Variations folder path]
   - Name: `VAR.lnk`

### Option 2: Use Migration Script
```batch
migrate-to-centralized.bat
```

When prompted:
- Project Variations folder: `C:\Users\Cameron\Desktop\Deploy\ACME INC 3223\Variations`
- Central VAR path: `X:\BMS\Programs\VAR`

This will:
- Backup `project.db`
- Delete all VAR runtime files
- Create `VAR.lnk` pointing to `X:\BMS\Programs\VAR\VAR.exe`

## Folder Permissions

Make sure users have:
- **Read & Execute** permissions on `X:\BMS\Programs\`
- **Read, Write, Modify** permissions on their project folders

Users only need to **read** from `X:\BMS\Programs\` and **write** to their own project databases.

## Troubleshooting

### "X: drive not found"
- **Cause**: Network drive not connected
- **Fix**: Connect to the network drive first

### "VAR.exe not found" when clicking shortcut
- **Cause**: Files not deployed to `X:\BMS\Programs\VAR\`
- **Fix**: Run `deploy-to-network.bat`

### "Database not found" when running VAR
- **Cause**: Shortcut's working directory is wrong
- **Fix**:
  1. Right-click `VAR.lnk` → Properties
  2. Ensure "Start in:" points to the Variations folder containing `project.db`

### VAR shows .NET installation error
- **Cause**: Using non-standalone build
- **Fix**: Make sure you published with `--self-contained true` flag

## Complete Deployment Checklist

- [ ] Run `deploy-to-network.bat` to copy apps to `X:\BMS\Programs\`
- [ ] Verify `X:\BMS\Programs\PDC\PDC.exe` exists
- [ ] Verify `X:\BMS\Programs\VAR\VAR.exe` exists
- [ ] Copy PDC to your projects folder (or create shortcut)
- [ ] Test: Run PDC and create a test project
- [ ] Test: Open VAR from the test project's Variations folder
- [ ] Test: Create a variation to ensure database works
- [ ] (Optional) Migrate existing projects using migration script
- [ ] Document the `X:\BMS\Programs\` location for your team

## Summary

**One-time setup:**
1. `deploy-to-network.bat` → Copies apps to `X:\BMS\Programs\`
2. Copy PDC to projects folder

**For each new project:**
1. Run PDC
2. PDC creates project with VAR shortcut
3. Double-click VAR.lnk to manage variations

**Clean, efficient, easy to maintain!**

## File Locations Quick Reference

| Item | Location | Size |
|------|----------|------|
| PDC Application | `X:\BMS\Programs\PDC\` | ~65 MB |
| VAR Application | `X:\BMS\Programs\VAR\` | ~65 MB |
| PDC Launcher | `X:\Projects\PDC.exe` (copy or shortcut) | ~65 MB or 1 KB |
| Project Database | `X:\Projects\[Project]\Variations\project.db` | Varies |
| VAR Shortcut | `X:\Projects\[Project]\Variations\VAR.lnk` | 1 KB |

**Total centralized storage:** ~130 MB (one-time)
**Per-project overhead:** ~1 KB (shortcut only)
