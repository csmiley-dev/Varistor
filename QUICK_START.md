# Quick Start Guide

## Building the Applications

### Option 1: Using the build script (Recommended)
```bash
build.bat
```

### Option 2: Using Visual Studio
1. Open `Varistor.sln`
2. Press F6 to build

### Option 3: Create standalone executables
```bash
publish.bat
```
This creates executables that don't require .NET installation.

## Initial Setup (First Time Only)

1. Copy the contents of the `Deploy` folder (or build output) to your main projects folder
2. Structure should be:
   ```
   X:\Projects\
   ├── PDC.exe
   └── zz\
       └── VAR.exe
   ```

## Creating a New Project

1. Run `PDC.exe` from your projects folder
2. Fill in:
   - Project Number
   - Project Name
   - Client Name (or use custom)
3. Click "Create Project"

Done! A new project folder with VAR installed is ready.

## Managing Variations

1. Go to `[ProjectName ProjectNumber]\Variations\`
2. Run `VAR.exe`
3. Click "New Variation" to create variations
4. Double-click to edit existing variations
5. Use the "Approve" button to approve variations

## Key Features

- **Auto-save**: Use Ctrl+S while editing variations
- **Auto-calculations**: All totals calculate automatically
- **Validation**: Duplicate names/numbers are prevented
- **Visual cues**: Empty material quantities highlighted in red
- **Single instance**: Can't run multiple copies of the same program

## Customization

### Add/Edit Clients
1. Open `X:\Projects\zz\clients.db` in a SQLite browser
2. Edit the `Clients` and `ClientContacts` tables
3. Save and restart PDC

### Modify Hourly Rates
1. Open `[Project]\Variations\project.db` in a SQLite browser
2. Edit the `HourlyRates` table
3. Save and restart VAR

### Change Folder Structure
Edit the `dirPaths` list in `PDC\MainForm.cs` and rebuild.

## Troubleshooting

**Program won't start**: Install .NET 8.0 Runtime or use the published version

**"Already running" message**: Close existing instances or check Task Manager

**Database errors**: Ensure you have write permissions to the folder

**Changes not saving**: Check for error messages and ensure database isn't locked
