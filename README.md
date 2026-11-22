# Sany3y (صنـايعـي)

<div align="center">

![Sany3y Banner](docs/sany3y_banner4.png)

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=flat-square&logo=dotnet)](https://dotnet.microsoft.com/)
[![Next.js](https://img.shields.io/badge/Next.js-15-000000?style=flat-square&logo=next.js)](https://nextjs.org/)
[![React](https://img.shields.io/badge/React-19-61DAFB?style=flat-square&logo=react&logoColor=black)](https://react.dev/)
[![TypeScript](https://img.shields.io/badge/TypeScript-5.7-3178C6?style=flat-square&logo=typescript&logoColor=white)](https://www.typescriptlang.org/)
[![FastAPI](https://img.shields.io/badge/FastAPI-YOLOv8_OCR-009688?style=flat-square&logo=fastapi&logoColor=white)](https://fastapi.tiangolo.com/)
[![SQL Server](https://img.shields.io/badge/Database-SQL_Server-CC2927?style=flat-square&logo=microsoftsqlserver&logoColor=white)](https://www.microsoft.com/sql-server)
[![License](https://img.shields.io/badge/License-GPL--3.0-2ea44f?style=flat-square)](LICENSE)

**A full-stack, decoupled service marketplace connecting clients with verified technicians and maintenance shops across Egypt.**

[Overview](#overview) | [System Architecture](#system-architecture) | [Key Capabilities](#key-capabilities) | [Technology Stack](#technology-stack) | [API & Real-Time Reference](#api--real-time-reference) | [Database Design](#database-design) | [Getting Started](#getting-started) | [Documentation](#documentation) | [Contributors](#contributors)

</div>

---

## Overview

**Sany3y** is a decoupled service and maintenance marketplace platform engineered with a **Next.js 15 (App Router + TypeScript)** client application and an **ASP.NET Core 8 Web API** backend. It streamlines the discovery, verification, scheduling, communication, and payment workflows between clients seeking residential or commercial maintenance, independent technicians, and local hardware/supply shops across Egyptian governorates and cities.

### Core Engineering Highlights

- **Decoupled Client-Server Architecture**: Separation between the Next.js 15 App Router frontend (`sany3y-web`) and the .NET 8 REST/SignalR backend (`Sany3y.API`), backed by a dedicated Entity Framework Core data access layer (`Sany3y.Infrastructure`).
- **Identity & Role-Based Access Control (RBAC)**: Built on ASP.NET Core Identity with stateless JWT Bearer authentication, Google OAuth integration, email confirmation workflows, and account lockout policies across three primary roles (`Admin`, `Technician`, `Client`) and shop entities (`IsShop`).
- **Computer Vision Identity Verification**: Embedded Python FastAPI microservice executing a two-stage **Ultralytics YOLOv8** inference pipeline (`detect_id_card.pt` and `detect_arabic_numbers.pt`) to extract and verify 14-digit Egyptian National ID numbers from uploaded ID card images during registration.
- **Real-Time Bidirectional Communication**: SignalR WebSocket hubs (`/chatHub` and `/userStatusHub`) authenticated via JWT tokens, providing instant private messaging, typing indicators, and live online/offline presence tracking.
- **Integrated Payment Processing**: Stripe Checkout and Webhook integration supporting online payments in Egyptian Pounds (EGP) alongside Cash-on-Service settlement.
- **Automated Reporting & Arabic RTL Exports**: Server-side document generation using iTextSharp for right-to-left Arabic PDF reports and UTF-8 CSV exports, paired with interactive Chart.js administrative analytics.

---

## System Architecture

```text
Sany3y/
├── sany3y-web/                        # Next.js 15 Frontend Application (TypeScript)
│   ├── public/                        # Static assets (backgrounds, team portraits, favicon)
│   ├── src/
│   │   ├── app/                       # Next.js App Router pages and layouts
│   │   │   ├── admin/                 # Administrative dashboard, user/shop/category management
│   │   │   ├── chat/[id]/             # Full-page real-time SignalR messaging interface
│   │   │   ├── dashboard/             # Role-specific dashboards (Customer, Worker, Shop)
│   │   │   ├── payment/               # Stripe Checkout verification and cancellation callbacks
│   │   │   ├── services/              # Technician directory, search filters, and booking views
│   │   │   ├── shops/                 # Maintenance shop directory and detail views
│   │   │   ├── login/ & register/     # Authentication, shop onboarding, and ID OCR upload
│   │   │   └── profile/               # Account and service profile management
│   │   ├── components/                # Reusable UI modules (Navbar, Footer, FloatingChatWidget)
│   │   ├── context/                   # React contexts for JWT Auth and SignalR connections
│   │   ├── lib/                       # Typed HTTP client and authenticated file download utilities
│   │   └── types/                     # TypeScript domain contracts and DTO interfaces
│   └── package.json
│
├── Sany3y.API/                        # ASP.NET Core 8 Web API & SignalR Host
│   ├── Controllers/                   # 18 RESTful API controllers
│   ├── Hubs/                          # SignalR hubs (ChatHub, UserStatusHub)
│   ├── Services/                      # JWT issuance, email dispatch, OCR proxy, location services
│   ├── DTOs/                          # Data Transfer Objects for dashboards and API contracts
│   ├── Extensions/                    # Service registration, Identity, JWT, CORS, and Swagger
│   ├── py/                            # Python FastAPI + YOLOv8 OCR Microservice
│   │   ├── app.py                     # FastAPI inference server (/predict/)
│   │   ├── detect_id_card.pt          # YOLOv8 model for national ID bounding-box detection
│   │   ├── detect_arabic_numbers.pt   # YOLOv8 model for Arabic numeral recognition
│   │   └── requirements.txt
│   └── Program.cs
│
├── Sany3y.Infrastructure/             # Persistence & Infrastructure Layer
│   ├── Models/                        # 16 EF Core domain entities and AppDbContext
│   ├── Repositories/                  # Generic repository contract and 9 concrete repositories
│   ├── Services/                      # Database seeding, Arabic PDF/CSV exporter, SMTP client
│   ├── DTOs/ & ViewModels/            # Shared data transfer and validation models
│   ├── Migrations/                    # EF Core SQL Server schema migrations
│   └── Queries/                       # SQL seed scripts for Egyptian administrative divisions
│
└── docs/                              # Technical & Business Documentation
    ├── Business Requirements Specification.pdf
    ├── Database Design.pdf
    ├── Project Planning.pdf
    ├── Stakeholder Analysis.pdf
    └── ERDs/ERD.png
```

---

## Key Capabilities

### Client Capabilities
- **Discovery & Filtering**: Search technicians and shops by category, governorate, city, name, price range, and rating, including server-side Arabic character normalization.
- **Profile Inspection**: Review technician credentials, verification status, years of experience, inspection fee, service coverage area, and verified customer ratings.
- **Appointment Scheduling**: Inspect real-time availability slots and submit booking requests with preferred payment methods (Cash or Online via Stripe).
- **Booking Lifecycle Management**: Track appointment states (`Pending`, `Accepted`, `In Progress`, `Paid`, `Completed`, `Cancelled`), trigger online checkout once accepted, or cancel pending requests.
- **Direct Messaging & Reviews**: Communicate with service providers in real time and submit post-service ratings and reviews.

### Technician Capabilities
- **Identity-Verified Onboarding**: Automated validation matching the user's 14-digit National ID against the uploaded ID card image via YOLOv8 OCR.
- **Service Configuration**: Manage specialty category, inspection pricing, experience metrics, biography, and profile imagery.
- **Availability Management**: Create and remove unbooked schedule slots directly from the Worker Dashboard.
- **Request Fulfillment**: Accept or reject incoming client requests (automatically releasing reserved schedule slots upon rejection) and monitor cumulative earnings.

### Shop Capabilities
- **Commercial Onboarding**: Dedicated registration flow capturing shop name, commercial category, establishment date, address, and owner identity verification.
- **Storefront Presence**: Public listing in the Shops directory with live online presence indicators and direct customer messaging.

### Administrative Capabilities
- **Platform Telemetry**: Real-time KPI summary cards and Chart.js visualizations for monthly user acquisition and demographic distribution, with client-side PNG chart export.
- **Entity Management**: Full CRUD administration over users, shops, roles, and service categories, integrated with live SignalR user status updates.
- **Document Exports**: On-demand generation and download of formatted Arabic RTL PDF and UTF-8 CSV reports for users, stores, and categories.

---

## Technology Stack

| Layer | Technologies |
| :--- | :--- |
| **Frontend (`sany3y-web`)** | Next.js 15 (App Router), React 19, TypeScript 5.7, Bootstrap 5 RTL, Font Awesome 6, `@microsoft/signalr`, Chart.js, `react-chartjs-2`, Sonner, EmailJS |
| **Backend (`Sany3y.API`)** | .NET 8, ASP.NET Core Web API, ASP.NET Core Identity, JWT Bearer Authentication, SignalR, Stripe.net (v50), Swashbuckle (OpenAPI/Swagger) |
| **Data & Infrastructure (`Sany3y.Infrastructure`)** | Entity Framework Core 8, Microsoft SQL Server, Repository Pattern, iTextSharp (PDF generation), SMTP Mail Client |
| **Computer Vision (`Sany3y.API/py`)** | Python 3.10+, FastAPI, Uvicorn, Ultralytics YOLOv8 (PyTorch), Pillow |
| **CI/CD** | GitHub Actions ([`.github/workflows/dotnet.yml`](.github/workflows/dotnet.yml)) validating both the .NET 8 solution and Next.js production build |

---

## API & Real-Time Reference

### Primary REST Endpoints (`Sany3y.API`)

| Module | Endpoint | Method | Description |
| :--- | :--- | :--- | :--- |
| **Authentication** | `/api/Auth/login` | `POST` | Validate credentials, update online status, and issue a signed JWT |
| **Authentication** | `/api/Auth/register` | `POST` (`multipart/form-data`) | Validate National ID via OCR, persist user and address, assign role, and send confirmation email |
| **Authentication** | `/api/Auth/google` | `POST` | Authenticate existing Google OAuth users or return prefill metadata for profile completion |
| **Authentication** | `/api/Auth/complete-profile` | `POST` (`multipart/form-data`) | Finalize registration for external OAuth users with National ID OCR verification |
| **Authentication** | `/api/Auth/confirm-email` | `GET` | Confirm user email address using Identity confirmation token |
| **Authentication** | `/api/Auth/forgot-password` | `POST` | Issue a password reset token and dispatch reset instructions via email |
| **Authentication** | `/api/Auth/reset-password` | `POST` | Reset user password using token verification |
| **Authentication** | `/api/Auth/profile` | `GET` / `PUT` | Retrieve or update authenticated user profile, address, and base64 avatar |
| **Marketplace** | `/api/Technician/Search` | `GET` | Query technicians or shops with filters (`isShop`, `categoryId`, `governorate`, `city`, `name`, `serviceType`, `sortBy`) |
| **Marketplace** | `/api/Technician/Details/{id}` | `GET` | Retrieve full technician or shop profile including address, category, ratings, and aggregates |
| **Scheduling** | `/api/TechnicianSchedule/GetSchedule/{id}` | `GET` | List available (unbooked) schedule slots for a specific technician |
| **Scheduling** | `/api/TechnicianSchedule/GetMySchedule/{id}` | `GET` | List all schedule slots (booked and available) for technician dashboard management |
| **Scheduling** | `/api/TechnicianSchedule/AddSlot` | `POST` | Create a new availability time slot |
| **Scheduling** | `/api/TechnicianSchedule/BookSlot` | `POST` | Reserve an available time slot and initialize a `Pending` service task |
| **Scheduling** | `/api/TechnicianSchedule/AcceptRequest/{taskId}` | `POST` | Transition a booking task from `Pending` to `Accepted` |
| **Scheduling** | `/api/TechnicianSchedule/RejectRequest/{taskId}` | `POST` | Reject a pending task and release the associated schedule slot |
| **Scheduling** | `/api/TechnicianSchedule/CancelBooking/{taskId}` | `POST` | Cancel a pending task and release the associated schedule slot |
| **Dashboards** | `/api/Dashboard/customer/{userId}` | `GET` | Aggregate customer profile, upcoming appointments, and historical bookings |
| **Dashboards** | `/api/Dashboard/worker/{userId}` | `GET` | Aggregate technician/shop profile, pending requests, active jobs, and total earnings |
| **Payments** | `/api/Payment/create-checkout-session` | `POST` | Create a Stripe Checkout session in EGP for an accepted online-payment task |
| **Payments** | `/api/Payment/verify-payment/{sessionId}` | `GET` | Verify Stripe session completion and persist the payment transaction |
| **Messaging** | `/api/Message/GetConversation/{u1}/{u2}` | `GET` | Retrieve chronological message history between two users |
| **Messaging** | `/api/Message/GetChatPartners/{userId}` | `GET` | Retrieve distinct users with whom the specified user has exchanged messages |
| **Locations** | `/api/CountryServices/GetAllGovernorates` | `GET` | Retrieve all Egyptian governorates |
| **Locations** | `/api/CountryServices/GetCitiesByGovernorateId/{id}` | `GET` | Retrieve cities belonging to a specific governorate |
| **Administration** | `/api/Admin/stats` | `GET` | Retrieve platform-wide user, technician, shop, and category counts (`Admin` role required) |
| **Administration** | `/api/Admin/charts/monthly-users` | `GET` | Retrieve monthly user registration time series for the current year |
| **Administration** | `/api/Admin/charts/gender-distribution` | `GET` | Retrieve demographic distribution across non-admin accounts |
| **Administration** | `/api/Admin/users` | `GET` / `POST` | List or create user and shop accounts with role assignment |
| **Administration** | `/api/Admin/users/{id}` | `GET` / `PUT` | Retrieve or update a specific user account, address, and role |
| **Administration** | `/api/Admin/export/{target}/{format}` | `GET` | Stream PDF or CSV exports for `users`, `stores`, or `categories` |

### SignalR WebSocket Hubs

| Hub Endpoint | Client-Invoked Methods | Server-Emitted Events |
| :--- | :--- | :--- |
| **`/chatHub`** | `SendPrivateMessage(receiverUserName, message)`, `Typing(receiverUserName)`, `MarkAsSeen(senderUserName)` | `ReceiveMessage(senderUserName, message, time)`, `MessageSent(receiverUserName, message, time)`, `TypingIndicator(senderUserName)`, `MessageSeen` |
| **`/userStatusHub`** | Managed automatically on connection lifecycle via JWT `access_token` | `ReceiveUserStatus(userId, isOnline)`, `UserStatusChanged(userName, isOnline)` |

---

## Database Design

| Entity | Responsibility |
| :--- | :--- |
| **`User`** | Extends `IdentityUser<long>` with `NationalId`, `FirstName`, `LastName`, `BirthDate`, `Gender`, `Bio`, `IsOnline`, `ExperienceYears`, `Price`, `Rating`, `CategoryID`, `IsShop`, `ShopName`, `AddressId`, and `ProfilePictureId` |
| **`Role`** | Extends `IdentityRole<long>` (`Admin`, `Technician`, `Client`) |
| **`Category`** | Service categories (seeded with 16 standard maintenance and construction specialties) |
| **`Task`** | Service bookings connecting `ClientId`, `TaskerId`, `CategoryId`, `ScheduleId`, `PaymentMethodId`, and `Status` |
| **`TechnicianSchedule`** | Time slot availability records (`TechnicianId`, `Date`, `StartTime`, `EndTime`, `IsBooked`) |
| **`Payment` & `PaymentMethod`** | Financial records tracking agreed amount, paid amount, payment date, status, and method (`Cash` or `Online`) |
| **`Rating`** | Customer evaluations (`Score` 1–5, `Comment`, `ClientId`, `TaskerId`, `CreatedAt`) |
| **`Message` & `Notification`** | Persisted chat messages and system notifications |
| **`Address`, `Province`, `Governorate`, `City`** | Normalized Egyptian geographic hierarchy |

---

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 20+](https://nodejs.org/) and npm
- [Microsoft SQL Server](https://www.microsoft.com/sql-server) (LocalDB, Express, or Developer Edition)
- [Python 3.10+](https://www.python.org/) (required for YOLOv8 National ID OCR verification)

### 1. Clone the Repository

```bash
git clone https://github.com/Mohammed-3tef/Sany3y.git
cd Sany3y
```

### 2. Backend Setup (`Sany3y.API`)

1. Configure your database connection string, JWT secret, SMTP credentials, Stripe keys, and `FrontendUrl` in `Sany3y.API/appsettings.json`:
   ```json
   {
     "FrontendUrl": "http://localhost:3000",
     "ConnectionStrings": {
       "MainDB": "Data Source=.;Initial Catalog=Sany3yDB;Integrated Security=True;Trust Server Certificate=True"
     }
   }
   ```
2. Apply Entity Framework Core database migrations:
   ```bash
   dotnet ef database update --project Sany3y.Infrastructure --startup-project Sany3y.API
   ```
3. (Optional) Install Python dependencies for the YOLOv8 OCR microservice (automatically launched on port `8000` when `Sany3y.API` starts):
   ```bash
   cd Sany3y.API/py
   pip install -r requirements.txt
   cd ../..
   ```
4. Run the API project:
   ```bash
   cd Sany3y.API
   dotnet run
   ```
   - **API Base URL**: `https://localhost:7178`
   - **Swagger Documentation**: `https://localhost:7178/swagger`
   - **Default Seeded Administrator**: `sany3y.admin@gmail.com` / `Admin@123` (Username: `Admin`)

### 3. Frontend Setup (`sany3y-web`)

1. Install Node.js dependencies and start the Next.js development server:
   ```bash
   cd sany3y-web
   npm install
   npm run dev
   ```
2. Navigate to **`http://localhost:3000`**.
   *(To point the client to a custom API host, set `NEXT_PUBLIC_API_URL` in `sany3y-web/.env.local`.)*

---

## Documentation

- [Physical Entity-Relationship Diagram (ERD)](./docs/ERDs/ERD.png)
- [Comprehensive System Documentation (PDF)](./docs/Sany3y.pdf)
- [Business Requirements Specification](./docs/Business%20Requirements%20Specification.pdf)
- [Database Design Specification](./docs/Database%20Design.pdf)
- [Project Planning Document](./docs/Project%20Planning.pdf)
- [Stakeholder Analysis](./docs/Stakeholder%20Analysis.pdf)

---

## Contributors

<table width="100%">
  <tr>
    <td align="center">
      <a href="https://github.com/Mohammed-3tef">
        <img src="https://github.com/Mohammed-3tef.png" width="100px;" alt="Mohammed Atef"/><br />
        <sub><b>Mohammed Atef</b></sub>
      </a><br />
      <a href="https://www.linkedin.com/in/mohammed-atef-abd-elkader/">LinkedIn</a>
    </td>
    <td align="center">
      <a href="https://github.com/Steven-Amin02">
        <img src="https://github.com/Steven-Amin02.png" width="100px;" alt="Steven Amin"/><br />
        <sub><b>Steven Amin</b></sub>
      </a><br />
      <a href="https://www.linkedin.com/in/steven-amin02/">LinkedIn</a>
    </td>
    <td align="center">
      <a href="https://github.com/Omniaashraf10">
        <img src="https://github.com/Omniaashraf10.png" width="100px;" alt="Omniaa Ashraf"/><br />
        <sub><b>Omniaa Ashraf</b></sub>
      </a><br />
      <a href="https://www.linkedin.com/in/omniaashraf2/">LinkedIn</a>
    </td>
    <td align="center">
      <a href="https://github.com/kenzi-shibl">
        <img src="https://github.com/kenzi-shibl.png" width="100px;" alt="Kenzi Shibl"/><br />
        <sub><b>Kenzi Shibl</b></sub>
      </a><br />
      <a href="https://www.linkedin.com/in/kenzi-shibl-2832aa357/">LinkedIn</a>
    </td>
  </tr>
</table>

---

## License

This project is licensed under the **GNU General Public License v3.0**. See the [LICENSE](LICENSE) file for full license text.
