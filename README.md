# MaxFitness

A fitness tracking web application built with ASP.NET Core 8.0 MVC.

Track workouts, monitor muscle groups, compete on leaderboards, and connect with a fitness community.

## Features

- **Dashboard** - Stats overview, interactive body map, weekly progress, achievements
- **Workout Tracking** - Log exercises with sets, reps, and weights per muscle group
- **Body Map** - Interactive SVG showing muscle group training status (strong/moderate/needs work)
- **Leaderboard** - Real-time rankings by total volume lifted with weekly/monthly/all-time filters
- **Community** - Social feed with posts, workout sharing, photo uploads, likes, and comments
- **Challenges** - Join fitness challenges, track progress, compete with other users
- **Exercise Library** - Browse 40+ exercises across 8 muscle groups
- **Profile & Settings** - Training breakdown, preferences, achievements showcase
- **Admin Dashboard** - User management, post moderation, challenge creation, site analytics

## Tech Stack

- **Backend**: ASP.NET Core 8.0 MVC
- **Database**: SQL Server (via Docker)
- **ORM**: Entity Framework Core 8
- **Auth**: ASP.NET Core Identity with role-based authorization
- **Frontend**: Razor Views, Bootstrap 5, FontAwesome 6.5, custom CSS
- **Testing**: xUnit, Moq, EF Core InMemory
- **CI/CD**: GitHub Actions

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)

## Quick Setup (Windows)

### 1. Clone the repository

```bash
git clone https://github.com/MaxiMitov/Final-project---MaxFitness.git
cd Final-project---MaxFitness
```

### 2. Start SQL Server with Docker

Make sure Docker Desktop is running, then:

```bash
docker compose up -d
```

This starts a SQL Server 2022 container on port 1433.

### 3. Set up User Secrets

```bash
cd "Final project - MaxFitness"
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost,1433;Database=MaxFitnessDb;User Id=sa;Password=MaxFitness123!;TrustServerCertificate=True;"
```

### 4. Apply database migrations

```bash
dotnet ef database update
```

### 5. Run the application

```bash
dotnet run
```

The app will be available at **http://localhost:5105**

### 6. Login

- **Regular user**: Register a new account via the Sign Up page
- **Admin account**: Username `admin` / Password `admin123`

## Running Tests

```bash
cd MaxFitness.Tests
dotnet test
```

47 unit tests covering services, controllers, and model validation.

## Project Structure

```
Final project - MaxFitness/
├── Controllers/          # 5 controllers (Home, Workout, Admin, Exercise, Challenge)
├── Data/                 # EF Core DbContext with seed data
├── Models/               # 13+ entity and view models
├── Services/             # Business logic (MuscleService, WorkoutStatsService)
├── Views/                # 16+ Razor views
│   ├── Admin/            # Admin dashboard
│   ├── Exercise/         # Exercise library
│   ├── Home/             # Dashboard, Settings, Leaderboard, Community, etc.
│   ├── Shared/           # Layout, error pages
│   └── Workout/          # Workout logger
├── wwwroot/              # Static files (CSS, JS)
└── Migrations/           # EF Core migrations

MaxFitness.Tests/         # xUnit test project
├── Tests/
│   ├── Controllers/      # Controller tests with Moq
│   ├── Models/           # Model validation tests
│   └── Services/         # Service tests with InMemory DB
```

## Default Accounts

| Role | Username | Password |
|------|----------|----------|
| Admin | admin | admin123 |
