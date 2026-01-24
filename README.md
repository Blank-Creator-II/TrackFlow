# TrackFlow

A C# WinForms application for tracking, planning, and scheduling expenses and business tasks. Built with the MaterialSkin UI framework for a modern, responsive desktop interface.

---

## Table of Contents

* [About](#about)
* [Features](#features)
* [Project layout](#project-layout)
* [Prerequisites](#prerequisites)
* [Getting started](#getting-started)

  * [Method 1 — Download release (recommended)](#method-1---download-release-recommended)
  * [Method 2 — Build from source](#method-2---build-from-source)
* [Usage](#usage)
* [Testing & sample data](#testing--sample-data)
* [Known issues & TODOs](#known-issues--todos)
* [Contributing](#contributing-fork-yeah)
* [License](#license)
* [Author](#author)

---

## About

**TrackFlow** is a lightweight desktop application implemented in C# using WinForms and the MaterialSkin UI framework. It combines expense tracking, a planner/flow board (to-dos, reminders, notes), and handy utility tools (calculator, converters, timer) into a single, offline-capable app designed for personal and small-business use.

## Features

### Core features

* **Expense Tracker (Expenses page)**

  * Record, plan, and budget monthly and annual expenses
  * Smart recommendations for budgeting
  * Apply coupons and discounts when planning purchases
  * Expense splitting between users (bank account linking planned)

* **Planner (Planner page)**

  * To-do lists
  * Reminders with simple scheduling
  * Notes and lightweight note management

* **General Tools (Tools page)**

  * Basic calculator
  * Currency converter
  * Unit converter
  * Timer

### Extra

* Local file-based storage (plain-text test data included for quick testing)
* Themes and icon assets organized per feature
* Designed for easy extension and maintenance (service layer under UI)

---

## Project layout

```
TrackFlow/
├── Assets
│   ├── App.ico
│   └── Icons
│       ├── Expenses
│       │   ├── entertainment.svg
│       │   ├── grocery.svg
│       │   └── ...
│       ├── Misc
│       │   ├── add.svg
│       │   └── ...
│       ├── Planner
│       │   ├── add_note.svg
│       │   └── ...
│       └── Sidebar
│           ├── expense.svg
│           └── ...
├── Directory.Build.props
├── LICENSE
├── MainForm.cs
├── Models
│   ├── Expense.cs
│   ├── Note.cs
│   └── ...
├── Program.cs
├── README.md
├── Services
│   ├── ExpenseService.cs
│   ├── NoteService.cs
│   └── ...
├── Test
│   └── Data
│       ├── Expense
│       ├── Note
│       └── Todo
├── TrackFlow.csproj
├── TrackFlow.csproj.user
├── UI
│   ├── ExpenseEvents.cs
│   ├── ExpensesPage.cs
│   └── PlannerPage.cs
└── Utils
    ├── DateHelper.cs
    ├── FileHelper.cs
    └── LoadIcons.cs

(17 directories, ~187 files including test files)
```

---

## Prerequisites

* **Windows 10 / 11 (recommended)** — Yes WinForms apps target only Windows desktop. But the project is/was created on Linux/Arch, the final UI runs on Windows but there is no VS designer to help for developers.
* **.NET SDK** (6.0 or later recommended). Install the SDK matching the project target.
* An IDE such as **Visual Studio 2022/2023** (not recommended) or **Visual Studio Code** with the C# extensions (that's what I used).

> Note: If you develop on a non-Windows host (for example Linux/Arch) which I did, use a Windows VM (Tiny10/Windows 10) for testing and running the compiled WinForms executable.

---

## Getting started

### Method 1 - Download release (recommended)

1. Download the installer or ZIP package from the repository Releases.
2. Run the installer or extract and double-click `TrackFlow.exe` in the extracted folder.

### Method 2 - Build from source

1. Clone the repository

```bash
git clone https://github.com/Blank-Creator-II/TrackFlow.git
cd TrackFlow
```

2. Restore and build with the dotnet CLI or open the project in Visual Studio

```bash
# restore dependencies
dotnet restore TrackFlow.csproj

# build
dotnet build TrackFlow.csproj -c Release

# run (useful for development inside a Windows environment)
dotnet run --project TrackFlow.csproj
```

3. Publish the app for release (example for Windows x64)

```bash
dotnet publish TrackFlow.csproj -c Release -r win-x64 --self-contained false
```

> If you want a self-contained single-file executable, add `-p:PublishSingleFile=true --self-contained true` and choose the appropriate runtime identifier.

---

## Usage

* **Expenses**: Open the Expenses page from the sidebar to add, edit, or remove expense entries. Use categories (Entertainment, Grocery, Medicine, Travel, Utilities, etc.) to organize spending. Use the budgeting tools to set monthly or yearly targets and view recommendations.

* **Planner**: Use the Flow board / Planner page to add To-dos, Reminders, and Notes. Some test items are saved as plain text records in the `Test/Data` folder for help during development; production builds use the configured app data path.

* **Tools**: Quick access to the Calculator, Currency Converter, Unit Converter, and Timer from the Tools page.

---

## Testing & sample data

A `Test/Data` folder is included with sample files for Expenses, Notes, Reminders, and To-dos. Use these files for unit tests or for manual QA during development. The project contains `ServiceTester.cs` for running lightweight service tests.

---

## Known issues & TODOs

* Expense splitting requires secure bank integration (planned).
* Notifications and advanced reminder recurrence are basic and may not persist across reboots.
* UI polish: accessibility improvements and additional themes.
* Tools group: add more converters and support for offline exchange rate updates.

---

## Contributing (Fork yeah!)

Contributions, bug reports, and pull requests are welcome. If you plan a larger feature, open an issue first to discuss the design. Keep these points in mind:

* Follow the existing service-oriented structure: keep business logic in `Services/` and UI code in `UI/`.
* Add tests where applicable and update the `Test/Data` folder with representative samples.
* Use clear commit messages and sign off your PR with a description of the change.

---

## License

This project is released under the **Apache-2.0** license. See [LICENSE](LICENSE) for details.

---

## Author

**By (^▽^＠)ノ Nahom Ziyin (Blank-Creator-II)**
