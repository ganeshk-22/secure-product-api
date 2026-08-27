# SecureProductApi

A secure Web API built with **ASP.NET Core 8**, implementing JWT authentication, role-based authorization, refresh tokens, and email verification — with a lightweight HTML/CSS/JS frontend consuming it. Built using **Clean Architecture** (Domain / Application / Infrastructure / API).

## Features

- **JWT Authentication** — short-lived access tokens (15 min)
- **Refresh Tokens** — rotated on each use, stored server-side, 7-day expiry
- **Email Verification** — required before login (dev-mode: verification link is logged to console instead of sent via SMTP)
- **Role-Based Authorization** — `Admin` (full Product CRUD) and `User` (read-only)
- **Product CRUD** — with input validation (name, description, price, quantity)
- **Frontend** — plain HTML/CSS/JS consuming the API, with role-based UI (Admin sees management controls, User sees read-only view)

## Tech Stack

- ASP.NET Core 8 Web API
- Entity Framework Core + SQL Server
- ASP.NET Core Identity
- JWT Bearer Authentication
- Clean Architecture (4 layers)
- Vanilla HTML/CSS/JS frontend

## Project Structure

```
SecureProductApi.sln
├── SecureProductApi.Domain          # Entities: ApplicationUser, RefreshToken, Product
├── SecureProductApi.Application     # Interfaces, DTOs
├── SecureProductApi.Infrastructure  # EF Core, Identity, JWT, Email, Repositories
├── SecureProductApi.API             # Controllers, Program.cs, Swagger
└── SecureProductApi.Web             # Static frontend (login/register/products)
```

## Prerequisites

- .NET 8 SDK
- SQL Server (LocalDB or full instance)
- VS Code or Visual Studio
- VS Code **Live Server** extension (for running the frontend)

## Setup

### 1. Clone and restore

```bash
git clone <your-repo-url>
cd SecureProductApi
dotnet restore
```

### 2. Configure User Secrets (API project)

```bash
cd SecureProductApi.API
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=.;Database=SecureProductApiDb;Trusted_Connection=True;TrustServerCertificate=True"
dotnet user-secrets set "Jwt:Key" "your-own-long-random-secret-key-min-32-chars"
```

`appsettings.json` already contains non-secret values (`Jwt:Issuer`, `Jwt:Audience`) — only the connection string and signing key are kept in User Secrets.

### 3. Apply migrations

```bash
dotnet ef database update --project SecureProductApi.Infrastructure --startup-project SecureProductApi.API
```

This creates all Identity tables plus `Products` and `RefreshTokens`. `Admin` and `User` roles are seeded automatically on first run.

### 4. Run the API

```bash
cd SecureProductApi.API
dotnet run
```

Swagger UI: `https://localhost:<port>/swagger`

> Visit the Swagger URL once in your browser first, to accept the self-signed dev certificate — otherwise the frontend's fetch calls to `https://localhost` will fail silently.

### 5. Run the frontend

Open `SecureProductApi.Web/login.html` with VS Code's **Live Server** (right-click → "Open with Live Server").

In `SecureProductApi.Web/js/api.js`, confirm `BASE_URL` matches your API's actual port:
```javascript
const BASE_URL = "https://localhost:<your-port>/api";
```

## Usage Flow

1. **Register** at `register.html` — creates a `User`-role account
2. **Verify email** — check the API console output for the logged verification link (dev-mode stub, no real email sent), and open it in the browser
3. **Login** at `login.html` — issues access + refresh tokens, stored in `localStorage`
4. **View products** at `products.html` — all logged-in users can view; only `Admin` accounts see Add/Edit/Delete controls

### Promoting a user to Admin

No open Admin registration by design. To test Admin functionality, manually assign the role in the database:
```sql
INSERT INTO AspNetUserRoles (UserId, RoleId)
VALUES ('<user-id-from-AspNetUsers>', '<admin-role-id-from-AspNetRoles>')
```
Re-login afterward to get a fresh token with the updated role claim.

## API Endpoints

| Method | Endpoint | Access |
|---|---|---|
| POST | `/api/auth/register` | Public |
| GET | `/api/auth/confirm-email` | Public |
| POST | `/api/auth/login` | Public |
| POST | `/api/auth/refresh-token` | Public |
| GET | `/api/products` | Any authenticated user |
| GET | `/api/products/{id}` | Any authenticated user |
| POST | `/api/products` | Admin only |
| PUT | `/api/products/{id}` | Admin only |
| DELETE | `/api/products/{id}` | Admin only |

## Notes

- Email sending is a dev-mode stub (`EmailService` logs to console) — swap in a real provider (e.g., MailKit + SMTP) for production use
- JWT signing key and connection string are never committed — see User Secrets setup above
- Token storage uses `localStorage` for simplicity; for production, consider httpOnly cookies for the refresh token