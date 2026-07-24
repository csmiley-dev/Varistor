# Varistor Project Management Suite

This solution contains two Windows applications for managing construction projects and variations:

1. **PDC (Project Directory Creator)** - Creates new project folders with standardized directory structure
2. **VAR (Varistor)** - Manages project variations, costs, and approvals

## Requirements

- Windows 10 or later
- .NET 8.0 Runtime or SDK
- Visual Studio 2022 (for building from source)

## Building the Applications

### Using Visual Studio

1. Open `Varistor.sln` in Visual Studio 2022
2. Right-click on the solution and select "Restore NuGet Packages"
3. Build the solution (F6 or Build > Build Solution)
4. The executables will be in:
   - `PDC\bin\Release\net8.0-windows\PDC.exe`
   - `VAR\bin\Release\net8.0-windows\VAR.exe`

### Using Command Line

```bash
# Restore packages
dotnet restore

# Build PDC
dotnet build PDC\PDC.csproj -c Release

# Build VAR
dotnet build VAR\VAR.csproj -c Release
```

## Publishing as Self-Contained Executables

To create standalone executables that don't require .NET runtime installation:

```bash
# Publish PDC
dotnet publish PDC\PDC.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true

# Publish VAR
dotnet publish VAR\VAR.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

The published files will be in:
- `PDC\bin\Release\net8.0-windows\win-x64\publish\`
- `VAR\bin\Release\net8.0-windows\win-x64\publish\`

## Installation and Setup

### Initial Setup

1. Create your main projects folder (e.g., `X:\Projects`)
2. Copy `PDC.exe` to this folder
3. Create a `zz` subfolder in the main projects folder
4. Copy `VAR.exe` to the `zz` folder

Your structure should look like:
```
X:\Projects\
├── PDC.exe
└── zz\
    └── VAR.exe
```

### First Run

When you first run PDC, it will automatically create:
- A `clients.db` file in the `zz` folder with sample clients
- The directory structure for any new projects you create

## Using PDC (Project Directory Creator)

### Creating a New Project

1. Navigate to your main projects folder (e.g., `X:\Projects`)
2. Run `PDC.exe`
3. Enter the project details:
   - **Project Number**: Unique identifier for the project
   - **Project Name**: Name of the project
   - **Client Name**: Select from dropdown or check "Enter custom client name"
4. Click "Create Project"

### What PDC Does

PDC creates a new folder named `[ProjectName ProjectNumber]` with the following structure:

```
[ProjectName ProjectNumber]\
├── Tender\
│   └── Superseded\
├── For Approval\
│   └── Superseded\
├── Construction\
│   ├── Architectural\
│   │   └── Superseded\
│   ├── Electrical\
│   │   └── Superseded\
│   ├── Hydraulic\
│   │   └── Superseded\
│   ├── Mechanical--[ClientName]\
│   │   └── Superseded\
│   └── Mechanical--Consultant\
│       └── Superseded\
├── Tech Data\
│   └── [ClientName] Tech Data\
│       └── Superseded\
├── As Installed\
├── Site Pics\
├── BMS\
│   └── Superseded\
├── Site Start\
├── Quotes and POs\
│   └── Superseded\
├── Switchboard Photos\
├── Variations\             ← VAR.exe is copied here
│   ├── VAR.exe
│   └── project.db          ← Project-specific database
├── Commissioning\
│   └── Superseded\
└── Finance\
```

PDC also:
- Copies `VAR.exe` from the `zz` folder to the `Variations` folder
- Creates a `project.db` database seeded with project information
- Copies client contacts to the project database

### Managing Client List

To add or modify clients:

1. Navigate to `X:\Projects\zz\`
2. Open `clients.db` using a SQLite database browser (e.g., DB Browser for SQLite)
3. Edit the `Clients` table to add/remove clients
4. Edit the `ClientContacts` table to add/remove client contacts

## Using VAR (Varistor)

### Accessing VAR

1. Navigate to the project's `Variations` folder
2. Run `VAR.exe`
3. The summary page will display project information and all variations

### Summary Page Features

The summary page shows:
- Project name, number, and client
- Current date
- List of all variations with:
  - Variation number and name
  - Date
  - Type (Addition, Credit, or Nil-Cost)
  - Total value
  - Approval status
- Two summary sections:
  - **All Variations**: Total additions, credits, and net value
  - **Approved Variations**: Approved additions, credits, and net value

### Creating a New Variation

1. Click "New Variation"
2. Enter variation details:
   - **Variation Number**: Defaults to next sequential number (e.g., VAR#1, VAR#2)
   - **Variation Name**: Unique name for the variation
   - **Date**: Defaults to current date
   - **Client Contact**: Select from dropdown or enter manually
3. Add line items (8 rows are shown by default):
   - **Item Number**: Auto-numbered, can be overridden
   - **Item Description**: Description of the work
   - **Type**: "Cost" (addition) or "Refund" (credit)
   - **Material Qty**: Quantity (highlighted red if empty)
   - **Material Cost**: Cost per unit
   - **Material Total**: Auto-calculated (Qty × Cost)
   - **Hours**: Number of hours
   - **Hourly Rate**: Select preset rate or choose "Custom" to enter manually
   - **Labour Total**: Auto-calculated (Hours × Rate)
   - **Line Total**: Auto-calculated (Material + Labour)
4. Click "Add Row" to add more line items if needed
5. Save using "Save" button or Ctrl+S

### Editing a Variation

1. Double-click a variation in the summary list, or
2. Select a variation and click "Edit Variation"
3. Make changes and save

### Approving a Variation

1. In the summary page, click the "Approve" button for a variation
2. Enter your name when prompted
3. The variation will be marked as approved with timestamp

To unapprove, click the "Unapprove" button.

### Deleting a Variation

1. Select a variation in the summary list
2. Click "Delete Variation"
3. Confirm the deletion

### Managing Hourly Rates

Hourly rates are stored in the project database. To modify them:

1. Navigate to the project's `Variations` folder
2. Open `project.db` using a SQLite database browser
3. Edit the `HourlyRates` table
4. Restart VAR to see the changes

Default rates:
- Standard: $100.00
- Senior: $150.00
- Specialist: $200.00

## Features

### Single Instance Enforcement

Both PDC and VAR prevent multiple instances from running simultaneously. If you try to launch a second instance, you'll see a warning message.

### Data Validation

- **PDC**: Prevents duplicate project folders
- **VAR**:
  - Prevents duplicate variation numbers and names
  - Validates required fields
  - Highlights empty material quantities in red
  - Warns about unsaved changes

### Auto-Calculations

VAR automatically calculates:
- Material totals (Qty × Cost)
- Labour totals (Hours × Rate)
- Line totals (Material + Labour)
- Variation subtotals and grand totals
- Summary statistics

### Keyboard Shortcuts

- **Ctrl+S**: Save variation (in editor)

## Database Schema

### clients.db (in zz folder)

```sql
Clients
- Id (INTEGER PRIMARY KEY)
- ClientName (TEXT UNIQUE)

ClientContacts
- Id (INTEGER PRIMARY KEY)
- ClientName (TEXT)
- ContactName (TEXT)
```

### project.db (in each project's Variations folder)

```sql
Project
- Id (INTEGER PRIMARY KEY)
- ProjectName (TEXT)
- ProjectNumber (TEXT)
- ClientName (TEXT)

Variations
- Id (INTEGER PRIMARY KEY)
- VariationNumber (TEXT UNIQUE)
- VariationName (TEXT UNIQUE)
- VariationDate (TEXT)
- ClientContact (TEXT)
- IsApproved (INTEGER)
- ApprovedBy (TEXT)
- ApprovedDate (TEXT)
- TotalValue (REAL)

LineItems
- Id (INTEGER PRIMARY KEY)
- VariationId (INTEGER)
- ItemNumber (INTEGER)
- ItemDescription (TEXT)
- ItemType (TEXT)
- MaterialQty (REAL)
- MaterialCost (REAL)
- MaterialTotal (REAL)
- HourlyQty (REAL)
- HourlyRate (REAL)
- LabourTotal (REAL)
- LineTotal (REAL)

HourlyRates
- Id (INTEGER PRIMARY KEY)
- RateName (TEXT UNIQUE)
- RateValue (REAL)

ClientContacts
- Id (INTEGER PRIMARY KEY)
- ContactName (TEXT UNIQUE)
```

## Customization

### Modifying Directory Structure

To change the folder structure created by PDC:

1. Open `PDC\MainForm.cs` in a text editor
2. Find the `CreateProjectStructure` method
3. Modify the `dirPaths` list
4. Rebuild the application

### Modifying Default Clients

Edit the `SeedDefaultClients` method in `PDC\DatabaseHelper.cs` before first run, or edit `clients.db` directly.

### Modifying Default Hourly Rates

Edit the `SeedHourlyRates` method in `PDC\DatabaseHelper.cs` before first run, or edit the `HourlyRates` table in project databases.

## Troubleshooting

### "Another instance is already running"

- Close any open instances of PDC or VAR
- Check Task Manager for hung processes
- If the issue persists, restart your computer

### "Project database not found"

- Ensure VAR.exe is run from a project's Variations folder
- Check that project.db exists in the same folder as VAR.exe

### SQLite errors

- Ensure you have write permissions to the folder
- Check that database files aren't locked by another application
- Try closing and reopening the application

### Grid not updating

- Click "Refresh" button to reload data
- Ensure you saved changes before closing the editor

## Future Enhancements

Potential improvements:
- Export variations to PDF/Excel
- Email variations to clients
- Import line items from templates
- Advanced reporting and analytics
- Cloud backup integration
- Multi-user support with conflict resolution

## License

This software is provided as-is for internal use.

## Support

For issues or feature requests, contact your system administrator.
