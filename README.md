# 🚗 Car Parking Management System

A **console-based Car Parking Management System** built with C# and .NET Framework. This project helps manage vehicle entries, exits, slot allocation, billing, and reports for a parking lot—suitable for college projects and learning object-oriented programming with file-based data storage.

---

## 📋 Table of Contents

- [Project Overview](#-project-overview--introduction)
- [Main Features](#-main-features-of-the-project)
- [Technologies Used](#-technologies-used)
- [Project Structure](#-project-structure)
- [How the System Works](#-how-the-system-works)
- [Data Storage](#-data-storage-explanation)
- [Key Modules](#-key-modules-or-components)
- [Presentation Summary](#-presentation-summary)

---

## 📌 Project Overview / Introduction

This is a **desktop console application** that simulates a real-world parking management system. Users (admins or attendants) log in, record vehicle entries and exits, allocate parking slots by vehicle type (4-wheeler or 2-wheeler), calculate parking fees based on duration, and view reports. All data is stored in **CSV files**, so no database setup is required - making it easy to run and demonstrate.

The system supports **role-based access**: Admins can manage slots, users, and view reports; Attendants can perform vehicle entry and exit; Security has limited access.

---

## ✨ Main Features of the Project

| Feature | Description |
|--------|-------------|
| **🔐 User Login** | Username and password authentication with role-based menus (Admin, Attendant, Security). |
| **🚙 Vehicle Entry** | Record new vehicles with vehicle number, owner name, and type (4-wheeler / 2-wheeler). System auto-assigns an available slot and prints an entry ticket. |
| **🚪 Vehicle Exit & Billing** | Enter vehicle number to exit; system calculates parking fee by duration and hourly rate, shows bill, and frees the slot on confirmation. |
| **📊 System Reports** | View currently parked vehicles and daily income report (total vehicles exited and total fee collected for the day). |
| **⚙️ Admin Functions** | Add or remove parking slots and manage user list (simplified in this version). |
| **💾 CSV Data Storage** | All users, slots, and parking records are saved in CSV files—no database needed. |
| **📱 Live Slot Status** | Main dashboard shows how many 4-wheeler and 2-wheeler slots are free vs total. |

---

## 🛠️ Technologies Used

- **Language:** C#
- **Framework:** .NET Framework 4.8
- **Type:** Console Application (Windows)
- **IDE:** Visual Studio (compatible with 2017+)
- **Data Storage:** CSV files (plain text)
- **Libraries:** Standard .NET libraries only (`System`, `System.IO`, `System.Linq`, etc.)—no external NuGet packages required.

---

## 📁 Project Structure

Important files and folders:

```
car-parking-management-system/
├── car-parking-management-system.slnx          # Solution file (open this in Visual Studio)
├── car-parking-management-system/              # Main project folder
│   ├── Program.cs                             # All application code (main, logic, data, UI)
│   ├── App.config                             # Application configuration (.NET runtime)
│   ├── car-parking-management-system.csproj   # Project file (references, build settings)
│   └── Properties/
│       └── AssemblyInfo.cs                     # Assembly metadata (version, title)
├── bin/Debug/                                 # Output folder (after build)
│   ├── users.csv                              # User accounts (created/updated at runtime)
│   ├── slots.csv                              # Parking slots (created/updated at runtime)
│   ├── parking_ledger.csv                     # All parking records (entries & exits)
│   └── car-parking-management-system.exe      # Executable after build
└── README.md                                  # This file
```

- **`Program.cs`** — Contains the entire application: login, menus, vehicle entry/exit, reports, admin, data load/save, and helper classes.
- **`App.config`** — Specifies the .NET runtime version.
- **CSV files** — Stored in `bin/Debug/` when you run the program; they are created automatically if missing.

---

## 🔄 How the System Works

### Step-by-step workflow

1. **Start the application**  
   Run the project from Visual Studio (F5) or run `car-parking-management-system.exe` from `bin\Debug\`. The console opens with the title *"Car Parking Management System (v2.0)"*.

2. **Load data**  
   The system reads `users.csv`, `slots.csv`, and `parking_ledger.csv` from the executable folder. If any file is missing, default users and slots are created.

3. **Login**  
   User enters **Username** and **Password**. If valid, the main menu is shown based on **role** (Admin / Attendant / Security).

4. **Main dashboard**  
   The screen shows:
   - Parking availability (e.g. *4-Wheeler: X Free / 20 Total*, *2-Wheeler: Y Free / 10 Total*).
   - Menu options: Vehicle Entry, Vehicle Exit & Billing, (for Admin) System Reports, Admin Functions, Logout, Save & Exit.

5. **Vehicle entry**  
   - User selects *Vehicle Entry*, then enters:
     - Vehicle number (e.g. GJ-03-AB-1234)
     - Owner name
     - Vehicle type (1 = 4-wheeler, 2 = 2-wheeler)
   - System finds a free slot of that type, creates a parking record, saves to `parking_ledger.csv`, and prints an **entry ticket** (ticket number, slot, entry time).

6. **Vehicle exit**  
   - User selects *Vehicle Exit & Billing*, enters the **vehicle number**.
   - System finds the active record (no exit time), calculates **duration** and **fee** (e.g. 4-wheeler $5/hour, 2-wheeler $2/hour), shows the **bill**.
   - User confirms with **y**; system sets exit time and total fee, saves ledger, and frees the slot.

7. **Reports (Admin only)**  
   - *Currently Parked Vehicles*: List of all vehicles still in the lot (slot, vehicle number, owner, entry time).
   - *Daily Income Report*: Vehicles exited today and total income for the day.

8. **Admin functions (Admin only)**  
   - *Manage Parking Slots*: Add new slot (e.g. C01, type 1 or 2) or remove a slot.
   - *Manage Users*: View current users (simplified in this version).

9. **Save & exit**  
   Choosing *Save & Exit* writes all data to CSV files and closes the application.

---

## 💾 Data Storage Explanation

The system uses **three CSV files** in the application folder (e.g. `bin\Debug\`). No database is used.

### 1. `users.csv`

Stores user accounts for login.

| Column    | Description                          |
|----------|--------------------------------------|
| Username | Login username                       |
| Password | Login password (stored in plain text)|
| FullName | Display name                         |
| Role     | 0 = Admin, 1 = Attendant, 2 = Security|

**Example:**
```csv
Username,Password,FullName,Role
admin,pass123,Manish Suthar,0
attendant1,pass123,Sandip Limbasiya,1
```

If the file does not exist, default users (e.g. `admin` / `attendant1`) are created.

---

### 2. `slots.csv`

Stores parking slots and their type.

| Column     | Description                    |
|-----------|--------------------------------|
| SlotNumber| Unique slot ID (e.g. A01, B02)|
| Type      | 0 = TwoWheeler, 1 = FourWheeler (other values reserved)|

**Example:**
```csv
SlotNumber,Type
A01,1
A02,1
B01,0
B02,0
```

Default: 20 slots A01 - A20 (4-wheeler), 10 slots B01–B10 (2-wheeler) if file is missing.

---

### 3. `parking_ledger.csv`

Stores every parking session (entry and, when exited, exit and fee).

| Column            | Description                                  |
|-------------------|----------------------------------------------|
| TicketNumber      | Unique ticket (e.g. T260313095518530)        |
| VehicleNumber     | Vehicle registration number                  |
| OwnerName         | Owner name                                   |
| Type              | Vehicle type (0/1 as in slots)               |
| AllocatedSlotNumber| Slot assigned                               |
| EntryTime         | Entry date and time (ISO format)             |
| ExitTime          | Exit date and time; empty if still parked    |
| TotalFee          | Amount charged; empty if not yet exited      |

**Example:**
```csv
TicketNumber,VehicleNumber,OwnerName,Type,AllocatedSlotNumber,EntryTime,ExitTime,TotalFee
T260313095518530,GJ-03 KC 4368,sandip,1,A01,2026-03-13T09:55:18,... ,5.0
```

Records with empty **ExitTime** represent vehicles currently in the parking lot.

---

## 🧩 Key Modules or Components

All logic is in **one file** (`Program.cs`), organized into regions:

| Component        | Purpose |
|------------------|--------|
| **Program (Main)** | Loads data, runs login loop, and main menu (Vehicle Entry, Exit, Reports, Admin, Logout, Save & Exit). |
| **Authentication** | `Login()` and `Logout()`; checks username/password against `users` list and sets `loggedInUser`. |
| **Entity classes** | `User`, `ParkingSlot`, `ParkingRecord`; enums `UserRole` (Admin, Attendant, Security) and `VehicleType` (TwoWheeler, FourWheeler, etc.). |
| **DataManager**  | Loads and saves all three CSV files; parses CSV lines into objects; provides default users and slots when files are missing. |
| **AdminManager** | Admin menu: manage slots (add/remove), manage users (view list); calls `DataManager.SaveAllData` after changes. |
| **ReportManager**| Report menu: currently parked vehicles, daily income; filters `parkingLedger` by exit time and date. |
| **UIHelper**     | Console UI: headers, menus, colored messages (success/error/info), password masking, pause, access denied message. |

Billing uses fixed hourly rates (e.g. 4-wheeler $5/hour, 2-wheeler $2/hour); minimum 1 hour is charged.

---

## Summary

> *"Our project is a **Car Parking Management System** developed in **C#** as a console application. It allows staff to **log in** with different roles - Admin and Attendant - and perform **vehicle entry** and **exit** with automatic **slot allocation** for 4-wheelers and 2-wheelers. When a vehicle exits, the system **calculates the parking fee** based on how long it was parked and generates a bill. Admins can **add or remove parking slots**, view **reports** such as currently parked vehicles and **daily income**, and manage users. All data is stored in **CSV files** so the system runs without any database. The project demonstrates **object-oriented design**, **file I/O**, **role-based access**, and **menu-driven console UI**."*

---

## 🚀 How to Run the Project

1. **Clone or download** the repository.
2. Open **Visual Studio** and open `car-parking-management-system.slnx` (or the `.sln` if present).
3. Build the solution (**Build → Build Solution** or Ctrl+Shift+B).
4. Run the application (**Debug → Start Debugging** or F5).
5. Use the default login:  
   - **Admin:** username `admin`, password `pass123`  
   - **Attendant:** username `attendant1`, password `pass123`

The CSV files will be created in `bin\Debug\` on first run. You can add or remove slots and park vehicles to see entries and reports in action.

---


*Developed by Manish and Sandip - Car Parking Management System *
