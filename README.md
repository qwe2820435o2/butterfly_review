# Butterfly Tracking 🦋

A full-stack web application for tracking butterfly release and sighting records across New Zealand. Volunteers can search their tagged butterflies by email, visualize flight trajectories on an interactive map, and explore annual statistics reports.

[![Frontend](https://img.shields.io/badge/Frontend-Next.js%2015-black?style=for-the-badge&logo=next.js)](https://nextjs.org/)
[![Backend](https://img.shields.io/badge/Backend-.NET%208-blue?style=for-the-badge&logo=.net)](https://dotnet.microsoft.com/)
[![Database](https://img.shields.io/badge/Database-MongoDB-green?style=for-the-badge&logo=mongodb)](https://www.mongodb.com/)
[![Deployment](https://img.shields.io/badge/Deployment-Railway%20%2F%20Vercel-black?style=for-the-badge)](https://railway.app/)

---

## 📖 Project Overview

Butterfly Tracking is built for the [NZ Butterflies](https://nzbutterflies.org.nz) volunteer programme. Participants release tagged butterflies and submit records via [Jotform](https://jotform.com/); this platform aggregates those submissions to let volunteers:

- **Search their tags** — enter an email address or tag number to retrieve all associated release and sighting records.
- **View trajectories** — see release points and sighting points plotted on an interactive map, with flight paths connecting them.
- **Explore the Year in Review** — scroll through a Spotify Wrapped–style annual report of project-wide statistics, geographic distributions, and volunteer contributions.

---

## ✨ Key Features

### Tag Search
- Search by **email address** or **tag number**
- Each result card shows release date, latest sighting date, survival days, sighting count, and alive/dead/unknown status
- Click any card to jump straight to the trajectory map

### Trajectory Map
- Interactive **Leaflet** map showing every butterfly's release point (red) and sighting points (blue)
- All trajectories overlaid on a single overview map, each with a unique colour
- Coordinate auto-correction — detects swapped lat/lng values and validates points are within New Zealand bounds
- Per-tag detail map accessible from the search results

### Year in Review
- Annual overview: total releases, total sightings, unique volunteers, regions covered
- Average flight distance (Haversine calculation) and average days to first sighting
- Geographic distribution map with most-active release and sighting locations
- Animated number reveals and scroll-triggered transitions

### Data Pipeline
- **Jotform webhook** receivers ingest release and sighting submissions in real time
- **Periodic sync task** polls the Jotform API to backfill any missed submissions
- **Tag number normalisation task** standardises tag formats in the background
- **Release confirmation email** sent to submitters via Gmail OAuth2

---

## 🛠️ Technology Stack

### Frontend (`butterfly-review-frontend`)
| Layer | Technology |
|---|---|
| Framework | Next.js 15 (App Router, Turbopack) |
| UI Library | React 19 + TypeScript 5 |
| Styling | Tailwind CSS 4 + shadcn/ui (Radix UI) |
| Maps | Leaflet 1.9 + React-Leaflet 4 + `@react-google-maps/api` |
| State | Redux Toolkit 2 + React-Redux 9 |
| HTTP | Axios 1 |
| Notifications | Sonner 2 |
| Themes | next-themes |

### Backend (`butterfly-review-api`)
| Layer | Technology |
|---|---|
| Framework | ASP.NET Core 8 (.NET 8) |
| Language | C# 12 |
| Database | MongoDB (MongoDB.Driver 2.26) |
| Auth | JWT Bearer (BCrypt password hashing) |
| Mapping | AutoMapper 12 |
| Logging | Serilog 9 (Console + rolling file) |
| API Docs | Swagger / OpenAPI (Swashbuckle 6) |
| Email | Gmail API via OAuth2 |

### Infrastructure
| Concern | Service |
|---|---|
| Backend hosting | Railway |
| Frontend hosting | Vercel |
| Database | MongoDB Atlas (or self-hosted) |
| Form submissions | Jotform |

---

## 🏗️ Project Structure

```
butterfly_review/
├── butterfly-review-frontend/         # Next.js 15 frontend
│   └── src/
│       ├── app/
│       │   ├── page.tsx               # Home page (feature navigation)
│       │   ├── search/                # Email / tag number search
│       │   ├── map/
│       │   │   ├── overview/          # All trajectories overview map
│       │   │   └── [tagNumber]/       # Per-tag trajectory map
│       │   ├── year-in-review/[year]/ # Annual report
│       │   └── users/                 # User management (admin)
│       ├── components/
│       │   ├── butterfly/             # Domain components (maps, search, cards)
│       │   ├── common/                # Shared components (Avatar, Loading, ThemeToggle)
│       │   ├── layout/                # Header
│       │   └── ui/                    # shadcn/ui primitives
│       ├── services/
│       │   ├── butterflyService.ts    # All butterfly API calls
│       │   └── userService.ts         # User management API calls
│       ├── store/                     # Redux store + loadingSlice
│       └── types/                     # TypeScript interfaces
│
└── butterfly-review-api/              # ASP.NET Core 8 backend
    ├── Controllers/
    │   ├── ReleaseSubmissionsController.cs   # CRUD for release records
    │   ├── SightingSubmissionsController.cs  # CRUD for sighting records
    │   ├── TrajectoriesController.cs         # Trajectory aggregation
    │   ├── YearInReviewController.cs         # Annual statistics
    │   ├── AuthController.cs                 # Register / Login
    │   ├── UserController.cs                 # User management
    │   ├── JotformSyncController.cs          # Manual sync trigger
    │   ├── ReleaseWebhookController.cs       # Jotform release webhook
    │   └── SightWebhookController.cs         # Jotform sighting webhook
    ├── Services/
    │   ├── JotformApiService.cs              # Jotform REST API client
    │   ├── JotformSyncService.cs             # Sync orchestration
    │   ├── WebhookProcessingService.cs       # Webhook payload parsing
    │   ├── ReleaseConfirmationEmailService.cs# Confirmation emails
    │   ├── TagNumberNormalizationService.cs  # Tag format normalisation
    │   ├── AuthService.cs                    # JWT auth logic
    │   └── GmailService.cs                   # Gmail OAuth2 sender
    ├── Tasks/
    │   ├── JotformSubmissionSyncTask.cs      # Background sync (IHostedService)
    │   └── TagNumberNormalizationTask.cs     # Background normalisation
    ├── Data/
    │   ├── MongoDbHelper.cs
    │   ├── ReleaseSubmissionRepository.cs
    │   ├── SightingSubmissionRepository.cs
    │   └── UserRepository.cs
    ├── Models/
    │   ├── Entities/                         # ReleaseSubmission, SightingSubmission, User
    │   └── DTOs/                             # Request / response contracts
    ├── Helpers/
    │   ├── ApiResponseHelper.cs              # Uniform { code, message, data } envelope
    │   └── JotformMappingHelper.cs           # Jotform answer → entity mapping
    ├── Extensions/                           # DI registration, middleware, AutoMapper
    ├── Program.cs                            # App bootstrap
    └── appsettings.*.json                    # Environment configs
```

---

## 🚀 Getting Started

### Prerequisites

- **Node.js** 18+
- **.NET 8 SDK**
- **MongoDB** (local instance or Atlas connection string)
- (Optional) Jotform API key and form IDs

### Backend Setup

```bash
cd butterfly-review-api

# Restore dependencies
dotnet restore

# Configure environment — copy and edit the template
cp appsettings.Development.json appsettings.Local.json
```

Minimum config needed in `appsettings.Development.json` (or via environment variables):

```json
{
  "MongoDb": {
    "ConnectionString": "mongodb://localhost:27017",
    "DatabaseName": "butterfly_tracking"
  },
  "JwtSettings": {
    "SecretKey": "<at-least-32-char-secret>",
    "Issuer": "butterfly-api",
    "Audience": "butterfly-client",
    "ExpiryInMinutes": 60
  },
  "Jotform": {
    "ApiKey": "<your-jotform-api-key>",
    "ReleaseFormId": "<form-id>",
    "SightingFormId": "<form-id>"
  }
}
```

```bash
# Run the API (Swagger UI available at /swagger in Development)
dotnet run
```

The API defaults to `https://localhost:5001`. MongoDB indexes are created automatically on startup.

### Frontend Setup

```bash
cd butterfly-review-frontend

npm install

# Point the frontend at the local API
# Edit src/lib/config.ts or set NEXT_PUBLIC_API_BASE_URL
```

```bash
npm run dev   # http://localhost:3000
```

---

## 🌐 API Reference

All responses use a uniform envelope:

```json
{ "code": 0, "message": "...", "data": <payload> }
```

`code: 0` = success; non-zero = error.

### Release Submissions

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/api/ReleaseSubmissions?email=<email>` | Get releases by email |
| `GET` | `/api/ReleaseSubmissions?tagNumber=<tag>` | Get releases by tag number |

### Sighting Submissions

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/api/SightingSubmissions?email=<email>` | Get sightings by email |
| `GET` | `/api/SightingSubmissions?tagNumber=<tag>` | Get sightings by tag number |

### Trajectories

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/api/Trajectories/all` | All trajectory points (all years) |
| `GET` | `/api/Trajectories/all?year=2024` | Trajectory points for a specific year |
| `GET` | `/api/Trajectories/tagNumbers` | All distinct tag numbers that have coordinates |

### Year in Review

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/api/YearInReview/{year}` | Annual statistics for the given year |

### Authentication

| Method | Endpoint | Description |
|--------|----------|-------------|
| `POST` | `/api/Auth/register` | Register a new user |
| `POST` | `/api/Auth/login` | Login and receive a JWT |

### Jotform Integration

| Method | Endpoint | Description |
|--------|----------|-------------|
| `POST` | `/api/ReleaseWebhook` | Jotform release form webhook receiver |
| `POST` | `/api/SightWebhook` | Jotform sighting form webhook receiver |
| `POST` | `/api/JotformSync/sync` | Manually trigger a Jotform backfill sync |

---

## ⚙️ Configuration Reference

| Key | Environment Variable | Description |
|-----|----------------------|-------------|
| `MongoDb:ConnectionString` | `MongoDb__ConnectionString` | MongoDB connection string |
| `MongoDb:DatabaseName` | — | Target database name |
| `JwtSettings:SecretKey` | `JWT_SECRET_KEY` | JWT signing key (≥32 chars) |
| `JwtSettings:Issuer` | `JWT_ISSUER` | JWT issuer claim |
| `JwtSettings:Audience` | `JWT_AUDIENCE` | JWT audience claim |
| `Jotform:ApiKey` | — | Jotform API key |
| `Jotform:ReleaseFormId` | — | Jotform release form ID |
| `Jotform:SightingFormId` | — | Jotform sighting form ID |
| `Jotform:GmailClientId` | — | Gmail OAuth2 client ID |
| `Jotform:GmailClientSecret` | — | Gmail OAuth2 client secret |
| `Jotform:GmailRefreshToken` | — | Gmail OAuth2 refresh token |
| `PORT` | `PORT` | HTTP port (set automatically by Railway) |

---

## 🐳 Docker Deployment

A `Dockerfile` is provided for the backend:

```bash
cd butterfly-review-api

docker build -f Dockerfile.txt -t butterfly-api .

docker run -p 8080:80 \
  -e MongoDb__ConnectionString="mongodb://..." \
  -e JWT_SECRET_KEY="your-secret" \
  butterfly-api
```

The frontend can be deployed to Vercel with zero configuration — `vercel.json` is already present.

---

## 🗺️ Coordinate Validation

All geographic data is validated against New Zealand bounds before storage and display:

- **Latitude**: −47.5 to −33.5
- **Longitude**: 166.0 to 179.0
- Points within Australian bounds (a common data-entry error) are automatically rejected.
- Swapped lat/lng values are detected and corrected automatically.

---

## 🧪 Testing

```bash
# Backend unit tests
cd butterfly-review-api.Tests
dotnet test

# Frontend lint
cd butterfly-review-frontend
npm run lint
```

---

## 📄 License

This project is licensed under the MIT License.

## 👨‍💻 Author

**Travis Nong**
- GitHub: [qwe2820435o2](https://github.com/qwe2820435o2)
- LinkedIn: [Travis Nong](https://linkedin.com/in/travis-nong)

---

*Built for the NZ Butterflies volunteer tracking programme.*
