# Sany3y - صنـايعـي

<div align="center">

![Sany3y Banner](docs/sany3y_banner4.png)

![Sany3y Logo](https://img.shields.io/badge/Sany3y-Service_Marketplace-004d00?style=for-the-badge)
[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet)](https://dotnet.microsoft.com/)
[![Next.js](https://img.shields.io/badge/Next.js-15-000000?style=for-the-badge&logo=next.js)](https://nextjs.org/)
[![React](https://img.shields.io/badge/React-19-61DAFB?style=for-the-badge&logo=react&logoColor=black)](https://react.dev/)
[![TypeScript](https://img.shields.io/badge/TypeScript-5.7-3178C6?style=for-the-badge&logo=typescript&logoColor=white)](https://www.typescriptlang.org/)
[![FastAPI](https://img.shields.io/badge/FastAPI-YOLOv8_OCR-009688?style=for-the-badge&logo=fastapi&logoColor=white)](https://fastapi.tiangolo.com/)
[![License](https://img.shields.io/badge/License-GPL--3.0-green?style=for-the-badge)](LICENSE)

**A modern, decoupled Arabic RTL service marketplace connecting clients with trusted technicians and local maintenance shops across Egypt**

[Overview](#-overview) • [Features](#-features) • [Architecture](#️-architecture) • [Tech Stack](#-technology-stack) • [API & SignalR Reference](#-api--signalr-reference) • [Getting Started](#-getting-started) • [Documentation](#-documentation) • [Team](#-team-members)

</div>

---

## 📌 Overview

**Sany3y** (صنـايعـي) is a full-stack service and maintenance marketplace platform built with a decoupled **Next.js 15 (App Router + TypeScript)** frontend and an **ASP.NET Core 8 Web API** backend. It bridges the gap between clients seeking home or commercial maintenance services, skilled independent technicians, and local hardware/service shops across all Egyptian governorates and cities.

### 🎯 Key Highlights

- ⚡ **Decoupled Modern Frontend** — Next.js 15 App Router with React 19, TypeScript, Arabic RTL (`dir="rtl"`) layout, and responsive Bootstrap 5 UI
- 🔐 **Unified JWT & Identity Authentication** — ASP.NET Core Identity with Role-Based Access Control (`Admin`, `Technician`, `Client`), Google OAuth, email confirmation, and account lockout protection
- 🤖 **AI-Powered National ID Verification** — Embedded Python FastAPI microservice running dual **Ultralytics YOLOv8** computer vision models (`detect_id_card.pt` & `detect_arabic_numbers.pt`) to verify 14-digit Egyptian National ID cards during registration
- 💬 **Real-Time Chat & Online Presence** — SignalR WebSocket hubs (`/chatHub` and `/userStatusHub`) with JWT authentication, live typing indicators, and instant online/offline status badges
- 💳 **Flexible Payments** — Stripe Checkout & Webhook integration for online payments in EGP alongside Cash-on-Service booking
- 📅 **Smart Slot Scheduling** — Real-time technician availability management and booking lifecycle (`Pending` → `Accepted` → `Paid` / `Completed` / `Cancelled`)
- 📊 **Admin Analytics & Arabic RTL Exports** — Interactive Chart.js analytics with one-click PNG export, plus server-side Arabic RTL PDF (iTextSharp) and UTF-8 CSV report generation

---

## ✨ Features

### 👤 For Clients
- ✅ Search and filter technicians and shops by specialty category, governorate, city, name, price, and rating (with smart Arabic text normalization)
- ✅ View detailed technician and shop profiles with verified badges, experience years, inspection fee, bio, and customer reviews
- ✅ Browse available schedule time slots and book appointments with Cash or Online (Stripe) payment selection
- ✅ Manage upcoming appointments and past bookings from a dedicated Customer Dashboard (`/dashboard/customer`)
- ✅ Initiate instant real-time chat via the floating chat widget or full-page chat interface (`/chat/[id]`)
- ✅ Rate and review technicians after service completion

### 🔧 For Technicians
- ✅ Register with AI-verified Egyptian National ID card OCR validation
- ✅ Customize specialty category, years of experience, inspection price, bio, and profile picture
- ✅ Add and delete availability time slots in real time from the Worker Dashboard (`/dashboard/worker`)
- ✅ Accept or reject incoming booking requests (automatically releasing time slots on rejection/cancellation)
- ✅ Track total completed earnings and communicate directly with clients via SignalR chat

### 🏪 For Maintenance & Supply Shops
- ✅ Dedicated shop registration workflow (`/register-shop`) with shop name, business activity category, establishment date, and owner ID verification
- ✅ Public shop directory (`/shops`) with live online availability indicator and shop profile pages (`/shops/[id]`)
- ✅ Dedicated Shop Dashboard (`/dashboard/shop`) for managing shop details and customer inquiries

### 👨‍💼 For Administrators
- ✅ Role-protected Admin Dashboard (`/admin`) with KPI cards (Total Clients, Technicians, Shops, Categories, and Top Requested Categories)
- ✅ Interactive Chart.js visualizations for **Monthly User Growth** and **Gender Distribution** with PNG download support
- ✅ Full CRUD management for **Users** (`/admin/users`), **Shops** (`/admin/shops`), and **Categories** (`/admin/categories`) with live SignalR online status indicators
- ✅ Server-side **Arabic RTL PDF** and **CSV** data exports for Users, Stores, and Categories

---

## 🏗️ Architecture

The system follows a clean, decoupled client-server architecture:

```text
Sany3y/
├── sany3y-web/                        # Next.js 15 Frontend (App Router + TypeScript)
│   ├── public/                        # Static assets (hero backgrounds, team photos, favicon)
│   ├── src/
│   │   ├── app/                       # Next.js App Router pages & layouts (31 routes)
│   │   │   ├── admin/                 # Admin dashboard, users, shops, categories CRUD
│   │   │   ├── chat/[id]/             # Full-page real-time SignalR chat
│   │   │   ├── dashboard/             # Customer, Worker, and Shop dashboards
│   │   │   ├── payment/               # Stripe Checkout success & cancel callback pages
│   │   │   ├── services/              # Technicians directory & profile/booking pages
│   │   │   ├── shops/                 # Shops directory & shop profile pages
│   │   │   ├── login/ & register/     # Auth, shop registration, OCR upload, password reset
│   │   │   └── profile/               # User/Technician/Shop profile editor
│   │   ├── components/                # Shared UI components (Navbar, Footer, FloatingChat, etc.)
│   │   ├── context/                   # AuthContext (JWT session) & SignalRContext (WebSockets)
│   │   ├── lib/                       # Typed API client & file download helpers
│   │   └── types/                     # TypeScript interfaces matching backend DTOs
│   └── package.json
│
├── Sany3y.API/                        # ASP.NET Core 8 Web API & SignalR Backend
│   ├── Controllers/                   # 18 REST API Controllers (Auth, Admin, Technician, Payment, etc.)
│   ├── Hubs/                          # SignalR Hubs (ChatHub, UserStatusHub)
│   ├── Services/                      # ApiJwtTokenService, ApiEmailService, ApiOcrService, CountryServices
│   ├── DTOs/                          # Dashboard & API Data Transfer Objects
│   ├── Extensions/                    # DI, EF Core, Identity, CORS, JWT, and Swagger configurations
│   ├── py/                            # Python FastAPI + YOLOv8 National ID OCR microservice
│   │   ├── app.py                     # FastAPI /predict/ endpoint
│   │   ├── detect_id_card.pt          # YOLOv8 ID card bounding-box detector
│   │   ├── detect_arabic_numbers.pt   # YOLOv8 Arabic digit recognizer
│   │   └── requirements.txt
│   └── Program.cs
│
├── Sany3y.Infrastructure/             # Data Access & Domain Layer
│   ├── Models/                        # 16 Entity Models & AppDbContext (IdentityDbContext)
│   ├── Repositories/                  # Generic IRepository<T> & 9 concrete repositories
│   ├── Services/                      # SeedService, TableExporter (Arabic PDF/CSV), EmailConfirm
│   ├── DTOs/ & ViewModels/            # Shared request/response models
│   ├── Migrations/                    # Entity Framework Core SQL Server migrations
│   └── Queries/                       # Egyptian Provinces, Governorates, and Cities SQL seed scripts
│
└── docs/                              # Project Documentation & ERDs
    ├── Business Requirements Specification.pdf
    ├── Database Design.pdf
    ├── Project Planning.pdf
    ├── Stakeholder Analysis.pdf
    └── ERDs/ERD.png
```

---

## 🔧 Technology Stack

| Layer | Technologies |
| :--- | :--- |
| **Frontend (`sany3y-web`)** | Next.js 15 (App Router), React 19, TypeScript 5.7, Bootstrap 5 RTL, Font Awesome 6, `@microsoft/signalr`, Chart.js + `react-chartjs-2`, Sonner (RTL Toasts), EmailJS |
| **Backend (`Sany3y.API`)** | .NET 8, ASP.NET Core Web API, ASP.NET Core Identity, JWT Bearer Authentication, SignalR, Stripe.net (v50), Swashbuckle (Swagger/OpenAPI) |
| **Data & Infrastructure (`Sany3y.Infrastructure`)** | Entity Framework Core 8, SQL Server, Repository Pattern, iTextSharp (Arabic RTL PDF generation), SMTP Email Sender |
| **AI / Computer Vision (`Sany3y.API/py`)** | Python 3.10+, FastAPI, Uvicorn, Ultralytics YOLOv8 (PyTorch), Pillow |
| **DevOps & CI/CD** | GitHub Actions ([`.github/workflows/dotnet.yml`](.github/workflows/dotnet.yml)) building both `.NET 8` backend and `Next.js` frontend |

---

## 📡 API & SignalR Reference

### Core REST Endpoints (`Sany3y.API`)

| Area | Endpoint | Method | Description |
| :--- | :--- | :--- | :--- |
| **Auth** | `/api/Auth/login` | `POST` | Authenticate user, mark online, and issue JWT |
| **Auth** | `/api/Auth/register` | `POST` (`multipart/form-data`) | Verify National ID via YOLOv8 OCR, create user/shop, send confirmation email |
| **Auth** | `/api/Auth/google` | `POST` | Authenticate or prefill registration for Google OAuth users |
| **Auth** | `/api/Auth/profile` | `GET` / `PUT` | Fetch or update authenticated user's profile, address, and avatar |
| **Marketplace** | `/api/Technician/Search` | `GET` | Search/filter technicians or shops (`isShop`, `categoryId`, `governorate`, `city`, `name`, `serviceType`, `sortBy`) |
| **Marketplace** | `/api/Technician/Details/{id}` | `GET` | Full technician/shop profile with address, category, avatar, and reviews |
| **Scheduling** | `/api/TechnicianSchedule/GetSchedule/{id}` | `GET` | Get available (unbooked) slots for a technician |
| **Scheduling** | `/api/TechnicianSchedule/BookSlot` | `POST` | Reserve a slot and create a `Pending` service task |
| **Scheduling** | `/api/TechnicianSchedule/AcceptRequest/{taskId}` | `POST` | Technician accepts a pending booking request |
| **Scheduling** | `/api/TechnicianSchedule/RejectRequest/{taskId}` | `POST` | Technician rejects request and frees the reserved slot |
| **Dashboards** | `/api/Dashboard/customer/{userId}` | `GET` | Customer profile, upcoming appointments, and past bookings |
| **Dashboards** | `/api/Dashboard/worker/{userId}` | `GET` | Worker/Shop profile, incoming requests, active jobs, and total earnings |
| **Payments** | `/api/Payment/create-checkout-session` | `POST` | Create a Stripe Checkout session in EGP |
| **Payments** | `/api/Payment/verify-payment/{sessionId}` | `GET` | Verify Stripe session status and record payment |
| **Chat** | `/api/Message/GetConversation/{u1}/{u2}` | `GET` | Load message history between two users |
| **Chat** | `/api/Message/GetChatPartners/{userId}` | `GET` | List distinct users the current user has chatted with |
| **Locations** | `/api/CountryServices/GetAllGovernorates` | `GET` | List all Egyptian governorates |
| **Locations** | `/api/CountryServices/GetCitiesByGovernorateId/{id}` | `GET` | List cities within a specific governorate |
| **Admin** | `/api/Admin/stats` | `GET` | Platform KPIs and top categories (`[Authorize(Roles = "Admin")]`) |
| **Admin** | `/api/Admin/charts/monthly-users` | `GET` | Monthly user registration counts for Chart.js |
| **Admin** | `/api/Admin/charts/gender-distribution` | `GET` | User gender distribution for Chart.js |
| **Admin** | `/api/Admin/export/{target}/{format}` | `GET` | Download Arabic RTL PDF or CSV (`users`, `stores`, `categories`) |

### SignalR WebSocket Hubs

| Hub Path | Client → Server Methods | Server → Client Events |
| :--- | :--- | :--- |
| **`/chatHub`** | `SendPrivateMessage(receiverUserName, message)`, `Typing(receiverUserName)`, `MarkAsSeen(senderUserName)` | `ReceiveMessage(senderUserName, message, time)`, `MessageSent(receiverUserName, message, time)`, `TypingIndicator(senderUserName)`, `MessageSeen` |
| **`/userStatusHub`** | Automatic on connection / disconnection (JWT `access_token`) | `ReceiveUserStatus(userId, isOnline)`, `UserStatusChanged(userName, isOnline)` |

---

## 📊 Database Schema

| Entity | Description |
| :--- | :--- |
| **`User`** | Extended `IdentityUser<long>` with `NationalId` (14 digits), `FirstName`, `LastName`, `BirthDate`, `Gender`, `Bio`, `IsOnline`, `ExperienceYears`, `Price`, `Rating`, `CategoryID`, `IsShop`, `ShopName`, `AddressId`, and `ProfilePictureId` |
| **`Role`** | Extended `IdentityRole<long>` (`Admin`, `Technician`, `Client`) |
| **`Category`** | 16 seeded Arabic maintenance and craft categories (سباكة، كهرباء، نجارة، نقاشة، تكييف وتبريد، إلخ) |
| **`Task`** | Service bookings linking `ClientId`, `TaskerId`, `CategoryId`, `ScheduleId`, `PaymentMethodId`, and `Status` |
| **`TechnicianSchedule`** | Technician availability slots (`Date`, `StartTime`, `EndTime`, `IsBooked`) |
| **`Payment` & `PaymentMethod`** | Payment transaction records (`Cash` = 1, `Online/Stripe` = 2) |
| **`Rating`** | Client reviews and 1–5 star scores for technicians and shops |
| **`Message` & `Notification`** | Real-time chat messages and user notifications |
| **`Address`, `Province`, `Governorate`, `City`** | Full Egyptian administrative location hierarchy |

---

## 🚀 Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 20+](https://nodejs.org/) & npm
- [SQL Server](https://www.microsoft.com/sql-server) (LocalDB or Express)
- [Python 3.10+](https://www.python.org/) (for YOLOv8 National ID OCR verification)

### 1. Clone the Repository

```bash
git clone https://github.com/Mohammed-3tef/Sany3y.git
cd Sany3y
```

### 2. Configure & Run the Backend (`Sany3y.API`)

1. Update the SQL Server connection string, JWT settings, Stripe keys, and `FrontendUrl` in `Sany3y.API/appsettings.json`:
   ```json
   {
     "FrontendUrl": "http://localhost:3000",
     "ConnectionStrings": {
       "MainDB": "Data Source=.;Initial Catalog=Sany3yDB;Integrated Security=True;Trust Server Certificate=True"
     }
   }
   ```
2. Apply Entity Framework Core migrations:
   ```bash
   dotnet ef database update --project Sany3y.Infrastructure --startup-project Sany3y.API
   ```
3. (Optional) Install Python dependencies for the YOLOv8 National ID OCR microservice (auto-started by `Sany3y.API` on port `8000`):
   ```bash
   cd Sany3y.API/py
   pip install -r requirements.txt
   cd ../..
   ```
4. Start the `.NET 8` API server:
   ```bash
   cd Sany3y.API
   dotnet run
   ```
   - **API Base URL**: `https://localhost:7178`
   - **Swagger UI**: `https://localhost:7178/swagger`
   - **Default Seeded Admin**: `sany3y.admin@gmail.com` / `Admin@123` (Username: `Admin`)

### 3. Configure & Run the Next.js Frontend (`sany3y-web`)

1. Install dependencies and start the Next.js development server:
   ```bash
   cd sany3y-web
   npm install
   npm run dev
   ```
2. Open **`http://localhost:3000`** in your browser.
   *(To point the frontend to a different API URL, update `NEXT_PUBLIC_API_URL` in `sany3y-web/.env.local`.)*

---

## 📖 Documentation

- 🗺️ [Physical ERD Diagram](./docs/ERDs/ERD.png)
- 📄 [Full Project Documentation (PDF)](./docs/Sany3y.pdf)
- 📋 [Business Requirements Specification](./docs/Business%20Requirements%20Specification.pdf)
- 🗄️ [Database Design Specification](./docs/Database%20Design.pdf)
- 📈 [Project Planning](./docs/Project%20Planning.pdf)
- 👥 [Stakeholder Analysis](./docs/Stakeholder%20Analysis.pdf)

---

## 👥 Team Members

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

## 📝 License

This project is licensed under the **GNU General Public License v3.0** — see the [LICENSE](LICENSE) file for details.

<div align="center">

**Built with ❤️ by the Sany3y Team**

⭐ Star us on GitHub if you find this project useful!

</div>
