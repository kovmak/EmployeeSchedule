# 📅 Employee Shift Scheduling App

This application is designed for easy and flexible scheduling of employee shifts. It's ideal for small teams that need to manage shifts, track working days, and calculate salaries based on shift types.

## ⚙️ Main Features

- **Shift Types**:
  - Morning Shift
  - Evening Shift
  - Full Day
  - Day Off
  - Empty Day (no shift assigned)

- **Shift Scheduling**:
  - Each day is displayed in a calendar-style table
  - Each employee has their own row
  - Shifts are entered manually using numeric values

- **Shift Counting**:
  - Automatically calculates the total number of shifts per employee
  - Ability to export the full schedule to Excel with preserved colors and shift values

- **Salary Calculation**:
  - Each shift has a weight:
    - Morning → 1  
    - Evening → 1  
    - Full Day → 1.2  
    - Day Off → 0
  - Each job position has its own editable wage rate
  - Salary is calculated based on shift counts and wage rates

- **Admin Panel**:
  - Add/edit employees
  - Assign job positions
  - View total shift count and salary per employee
  - Edit salary rates by position

- **Additional Features**:
  - Admin login (authorization required)
  - Data persistence using XML files
  - Month selection (dynamically changes the number of days in the schedule)

## 💾 Data Storage

- All data is saved in XML format:
  - Employee schedule: `schedule.xml`
  - Salary rates by position: `salary_rates.xml`

## 💡 Technologies Used

- C#
- WPF (Windows Presentation Foundation)
- DataGrid for schedule display
- XML for data storage

## 🏁 Getting Started

1. Open the project in Visual Studio
2. Run `MainWindow.xaml`
3. Log in as an admin to access the Admin Panel
4. Use the main window to edit and manage the work schedule

## 📦 Export to Excel

- Supports exporting the schedule to Excel with:
  - Color formatting
  - Proper shift value mapping

### Excel Format:
| Shift Type   | Value |
|--------------|-------|
| Morning      | 1     |
| Evening      | 1     |
| Full Day     | 1.2   |
| Day Off      | 0     |

> 🔒 This app is intended for internal use. Admin login is required for accessing administrative features.
