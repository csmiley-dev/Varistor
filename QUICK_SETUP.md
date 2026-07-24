# Quick Setup Guide

## For X:\BMS\Programs Installation

### Step 1: Deploy to Network
```batch
deploy-to-network.bat
```
This copies PDC and VAR to `X:\BMS\Programs\`

### Step 2: Copy PDC to Your Projects Folder
```batch
xcopy "X:\BMS\Programs\PDC\*.*" "X:\Projects\" /Y /E /I
```
(Replace `X:\Projects\` with your actual projects folder)

### Step 3: Create Projects
1. Run `X:\Projects\PDC.exe`
2. Enter project details
3. PDC creates project with VAR shortcut

### Step 4: Use VAR
1. Go to `[Project]\Variations\`
2. Double-click `VAR.lnk`
3. Manage variations

## Clean Up Existing Project

For your existing project at `C:\Users\Cameron\Desktop\Deploy\ACME INC 3223\Variations`:

```batch
migrate-to-centralized.bat
```

Enter: `C:\Users\Cameron\Desktop\Deploy\ACME INC 3223\Variations`

This removes all VAR files and creates a shortcut to `X:\BMS\Programs\VAR\`

## What You Get

**X:\BMS\Programs\** (130 MB, one-time)
- All application files stored here

**Each Project:** (1 KB + database)
- Just `VAR.lnk` and `project.db`
- Clean, organized, efficient

**Benefits:**
- 99% less files per project
- Easy to update (one location)
- Professional organization
