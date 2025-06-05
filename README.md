# Vehicle Fleet Management System for Mining Operations

A comprehensive vehicle management application designed for nickel mining operations with multiple locations. This application allows for monitoring vehicle usage, fuel consumption, maintenance schedules, and reservation workflows with multi-level approval processes.

## System Requirements

- .NET version: .NET 9.0
- Database: Microsoft SQL Server 2019 or newer
- Web Browser: Modern browsers (Chrome, Firefox, Edge, Safari)

## Application Features

- Dashboard with vehicle usage analytics
- Vehicle reservation system with multi-level approval workflows
- Vehicle and driver management
- Fuel consumption tracking
- Maintenance scheduling
- Activity logging for all operations
- Excel report export functionality
- Responsive UI for desktop and mobile devices

## Default User Accounts

The system comes pre-configured with the following user accounts:

| Username  | Password    | Role     | Description                  |
|-----------|-------------|----------|------------------------------|
| admin     | Admin123!   | Admin    | System administrator         |
| approver1 | Approve123! | Approver | First level approval (Operations Manager) |
| approver2 | Approve123! | Approver | Second level approval (Fleet Director) |

## Application Architecture

- **Backend**: ASP.NET Core MVC 9.0
- **Database**: SQL Server with Entity Framework Core
- **Frontend**: Bootstrap 5, jQuery, Chart.js

## Database Schema

The application uses the following key entities:
- Regions and Locations
- Vehicles (owned and rented)
- Drivers
- Vehicle Reservations
- Maintenance Records
- Fuel Consumption Records
- Activity Logs

## User Guide

### Login

1. Launch the application in your web browser
2. Enter your username and password
3. Click "Login"

### Dashboard

The home page displays:
- A chart showing vehicle usage over time
- Summary of pending and partially approved reservations
- Quick-access list of reservations requiring your approval (for approvers)

### Making a Reservation (Admin)

1. Navigate to "Reservations" in the menu
2. Click "Create Reservation"
3. Fill in the required details:
   - Purpose of the trip
   - Start and end dates/times
   - Select a vehicle
   - Select a driver
   - Specify origin and destination
   - Add any relevant notes
4. Select at least two approvers in hierarchical order
5. Click "Create Reservation"

### Approving Reservations (Approvers)

1. Navigate to "Approvals" in the menu or access them from the dashboard
2. Click "Review" on a reservation awaiting your approval
3. Review the reservation details
4. Select "Approve" or "Reject"
5. Add comments if necessary
6. Click "Submit Decision"

### Viewing Reports (Admin)

1. Navigate to "Reports" in the menu
2. Select a date range
3. Optionally select a specific reservation status
4. Click "Generate Excel Report"
5. Save the downloaded Excel file

## Approval Process

The application enforces a hierarchical approval process:
1. Admin creates a reservation and assigns approvers
2. First-level approver reviews the request
3. If approved, the request goes to the second-level approver
4. Once all approvals are received, the reservation status changes to "Approved"

## Technical Support

For technical support, please contact:
- Email: support@miningtransport.com
- Phone: +62 21 1234567
