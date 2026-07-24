# Clean Network Drive Installation

## TL;DR - Quick Setup

```batch
# 1. Run this script
deploy-standalone.bat

# 2. Enter your path when prompted (e.g., X:\Projects)

# 3. Done! Use PDC.lnk to create projects
```

## What This Does

Creates a **clean, professional** installation:

```
X:\Projects\
├── _Apps\              ← Hidden folder (you won't see this normally)
│   ├── PDC\            ← 200 files, 65MB - PDC application
│   └── VAR\            ← 200 files, 65MB - VAR application
│
├── PDC.lnk             ← Click this to create projects (1KB)
│
└── ProjectName 001\    ← Created by PDC
    └── Variations\
        ├── VAR.lnk     ← Click this to manage variations (1KB)
        └── project.db  ← Your data (grows as you add variations)
```

## Benefits

### ✅ Clean Folders
- No DLL files cluttering your projects
- Only shortcuts (1KB) and databases
- Professional, organized workspace

### ✅ Easy Maintenance
- Update apps once in `_Apps` folder
- All projects automatically use new version
- No need to update 50+ projects individually

### ✅ Network Efficient
- **Before**: 10 projects = 2000 files, 650MB
- **After**: 10 projects = 220 files, 65.5MB
- **Savings**: ~10x less files, ~10x less space

### ✅ User Friendly
- Shortcuts work like normal programs
- Double-click to run (no difference for users)
- Hidden system folder keeps workspace clean

## For Your Existing Project

Your project at `C:\Users\Cameron\Desktop\Deploy\ACME INC 3223\Variations` has all the files copied. To clean it up:

```batch
migrate-existing-project.bat
```

This will:
1. Remove all 200+ VAR files
2. Keep your project.db intact
3. Create a shortcut to centralized VAR

## Comparison

### Old Method (What you have now)
```
ACME INC 3223\Variations\
├── VAR.exe
├── VAR.dll
├── System.Data.SQLite.dll
├── [195 more DLL files...]
├── runtimes\
│   └── [50+ more files...]
└── project.db
```
**Total: ~200 files, 65MB per project**

### New Method (Recommended)
```
ACME INC 3223\Variations\
├── VAR.lnk              ← Shortcut (1KB)
└── project.db           ← Your data
```
**Total: 2 files, ~50KB**

## Installation Steps

### Step 1: Build and Deploy
```batch
# Make sure applications are built
dotnet publish PDC/PDC.csproj -c Release -r win-x64 --self-contained true
dotnet publish VAR/VAR.csproj -c Release -r win-x64 --self-contained true

# Deploy to network location
deploy-standalone.bat
```

Enter your path: `X:\Projects` (or wherever your projects are)

### Step 2: Create Projects
1. Go to X:\Projects\
2. Double-click `PDC.lnk`
3. Enter project details
4. Click Create

PDC automatically creates VAR shortcut in the new project.

### Step 3: Clean Up Existing Projects (Optional)
For each existing project with copied VAR files:

```batch
migrate-existing-project.bat
```

Enter:
- Project's Variations folder path
- Path to _Apps folder

## Does It Work?

**Yes!** Shortcuts work exactly like the real programs:
- Double-click VAR.lnk → Opens VAR
- VAR reads project.db from the same folder
- Everything functions normally
- Users won't notice any difference

The "Start in" (working directory) of the shortcut is set to the Variations folder, so VAR finds project.db correctly.

## Advantages Over Copying

| Aspect | Copied Files | Shortcuts |
|--------|-------------|-----------|
| Files per project | ~200 | 2 |
| Size per project | ~65MB | ~50KB |
| Update process | Update each project | Update once |
| Network traffic | High | Minimal |
| Backup size | Large | Small |
| Clutter | High | None |

## Summary

**Recommended for your X:\ network drive:**

1. Run `deploy-standalone.bat` → Creates `X:\Projects\_Apps\` with all application files
2. Use `PDC.lnk` → Creates projects with VAR shortcuts
3. Optional: Run `migrate-existing-project.bat` → Cleans up existing projects

**Result:**
- Clean, professional workspace
- Easy to maintain
- Efficient use of network storage
- No user-visible changes (shortcuts work the same)
