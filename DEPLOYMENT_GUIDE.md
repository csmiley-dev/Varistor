# Deployment Guide - Clean Network Drive Setup

## Problem
The standalone .NET applications include ~200+ runtime files totaling ~65MB each. Copying these to every project creates clutter on shared network drives.

## Solution: Centralized Installation

Store applications in **one hidden central location** and use shortcuts in project folders.

## Installation Steps

### Step 1: Run the Centralized Deployment Script

```batch
deploy-standalone.bat
```

When prompted, enter your projects path (e.g., `X:\Projects`)

This creates:
```
X:\Projects\
├── _Apps\              ← Hidden folder with applications
│   ├── PDC\            ← ~200 files, 65MB
│   └── VAR\            ← ~200 files, 65MB
└── PDC.lnk             ← Shortcut (1KB) to run PDC
```

### Step 2: Create Your First Project

1. Double-click `PDC.lnk` in X:\Projects
2. Enter project details
3. Click "Create Project"

PDC creates:
```
X:\Projects\ProjectName ProjectNumber\
├── [All your folders...]
└── Variations\
    ├── VAR.lnk      ← Shortcut (1KB) to VAR
    └── project.db   ← Project database
```

### Step 3: Use VAR

1. Navigate to project's Variations folder
2. Double-click `VAR.lnk`
3. Create and manage variations

## What You Get

**Clean Project Folders:**
- Variations folder contains only:
  - VAR.lnk (1KB shortcut)
  - project.db (database file, grows with use)
- No clutter, easy to backup databases

**Easy Updates:**
- Update files in `_Apps` folder only
- All projects automatically use updated version
- No need to update each project individually

**Network Efficiency:**
- One copy of runtime files instead of N copies
- Saves ~65MB × (number of projects)
- For 20 projects: Saves ~1.3GB!

## File Structure Comparison

### ❌ Old Method (Copied Files)
```
Project 1\Variations\    200 files, 65MB
Project 2\Variations\    200 files, 65MB
Project 3\Variations\    200 files, 65MB
...
Total for 10 projects:   2000 files, 650MB
```

### ✅ New Method (Shortcuts)
```
_Apps\VAR\               200 files, 65MB (once)
Project 1\Variations\    2 files, ~50KB
Project 2\Variations\    2 files, ~50KB
Project 3\Variations\    2 files, ~50KB
...
Total for 10 projects:   220 files, ~65.5MB
```

## Showing/Hiding the _Apps Folder

### To Hide (Recommended for Network Drive)
```batch
attrib +h "X:\Projects\_Apps"
```

### To Show (for maintenance)
```batch
attrib -h "X:\Projects\_Apps"
```

Or in Windows Explorer: View → Show → Hidden items

## Updating Applications

### To Update PDC:
1. Build new version: `dotnet publish PDC/PDC.csproj -c Release -r win-x64 --self-contained true`
2. Copy to: `X:\Projects\_Apps\PDC\`
3. All users get the update immediately

### To Update VAR:
1. Build new version: `dotnet publish VAR/VAR.csproj -c Release -r win-x64 --self-contained true`
2. Copy to: `X:\Projects\_Apps\VAR\`
3. All existing and new projects use the updated version

## Migrating Existing Projects

If you have projects with VAR files already copied (before the shortcut method):

### Option 1: Manual Migration
1. Delete all VAR files except `project.db` from Variations folder
2. Create shortcut: Right-click → New → Shortcut
3. Target: `X:\Projects\_Apps\VAR\VAR.exe`
4. Start in: [Your project's Variations folder]
5. Name it: `VAR.lnk`

### Option 2: Use Migration Script
```batch
migrate-to-shortcuts.bat
```
(Automatically cleans up old files and creates shortcuts)

## Troubleshooting

### Shortcut doesn't work
- **Check**: Is `_Apps\VAR\VAR.exe` present?
- **Check**: Shortcut's "Start in" folder should be the Variations folder
- **Fix**: Right-click shortcut → Properties → Check paths

### "Database not found" error
- **Cause**: Shortcut's working directory is wrong
- **Fix**: Right-click VAR.lnk → Properties → "Start in" should be the Variations folder containing project.db

### Can't see _Apps folder
- Enable "Show hidden items" in File Explorer
- Or run: `attrib -h "X:\Projects\_Apps"`

### Want to move _Apps to different location
1. Move `_Apps` folder to new location
2. Update all shortcuts to point to new location
3. Or run deployment script again with new path

## Alternative: Single-File Executables

If shortcuts don't work in your environment, you can create single-file executables:

```batch
dotnet publish PDC/PDC.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
dotnet publish VAR/VAR.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

This creates single `.exe` files (~55MB each) that can be copied directly, but still recommended to use centralized location.

## Recommendations

**For Network Drives (X:\):**
- ✅ Use centralized installation with shortcuts
- ✅ Hide _Apps folder
- ✅ Keep only databases in project folders

**For Local Testing:**
- Use regular build (not standalone)
- Requires .NET 8.0 Desktop Runtime installed
- Smaller files, faster builds

**For Distribution to Other Locations:**
- Use standalone builds
- Users don't need .NET installed
- Can work offline

## Summary

The centralized installation approach gives you:
- **Clean folders**: Only shortcuts and databases in projects
- **Easy maintenance**: Update once, applies everywhere
- **Network efficiency**: One copy instead of many
- **User-friendly**: Double-click shortcuts just like regular programs
- **Professional**: Hidden system folder, clean workspace
