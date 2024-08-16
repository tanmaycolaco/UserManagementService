# User Management Service - README

This service provides user registration and login functionality, integrating with Auth0 for authentication and authorization. It uses a PostgreSQL database for data storage and implements best practices in code structure, design patterns, and security.

## Prerequisites

### .NET 8 SDK

Download and install the latest .NET 8 SDK from the official website: [Download .NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0).

### PostgreSQL

Download and install PostgreSQL from the official website: [Download PostgreSQL](https://www.postgresql.org/download/).  
Make sure you have the necessary tools to manage your PostgreSQL database (e.g., `psql`, `pgAdmin`).

## Installation and Setup

### Clone the Repository

git clone <repository-url>
cd UserManagementService
Create the Database
Use your preferred PostgreSQL tool (e.g., psql, pgAdmin) to create a new database.

### Create the Database

```bash
psql -U <username> -d <database_name> -f SqlScripts/initialize.sql
Execute the SQL scripts provided in the SqlScripts folder to create the necessary tables.
```

### Configure the Connection String

Open the appsettings.json file in the root of the project.
Update the ConnectionStrings:DefaultConnection value with your actual PostgreSQL connection string.

```json
"ConnectionStrings": {
    "DefaultConnection": "Host=<hostname>;Database=<database>;Username=<username>;Password=<password>"
}
```

### Configure Auth0 Settings

The Auth0 settings in the `appsettings.json` file currently contain dummy values.  
Contact the author of this service to obtain the actual Auth0 credentials (Domain, Client ID, Client Secret), or use your own Auth0 tenant and configure the settings accordingly.

```json
"Auth0": {
    "Domain": "your-auth0-domain",
    "ClientId": "your-auth0-client-id",
    "ClientSecret": "your-auth0-client-secret"
}
```

### Restore Dependencies and Build

#### Bash (Linux)

```bash
dotnet restore
dotnet build
```

PowerShell (Windows)

```powershell
dotnet restore
dotnet build
```

## Run the Service

Linux

```bash
dotnet run --project ./UserManagementService/
```

Windows

```powershell
dotnet run --project .\UserManagementService\
```

The service will start running and listen on the specified port.

## API Endpoints

### POST /api/v1/user/register

Registers a new user.  
**Request body:** `RegisterUserRequest` (see `Shared.Models.Request`).

### POST /api/v1/user/login

Authenticates a user and returns an Auth0 token.  
**Request body:** `LoginRequest` (see `Shared.Models.Request`).

## Important Notes

### Auth0

- Make sure you have a valid Auth0 tenant and application set up.
- Configure the Auth0 settings in `appsettings.json` accordingly.

### Security

- Never store sensitive information like database passwords or Auth0 secrets in plain text in your configuration files, especially in production. Consider using environment variables or other secure configuration providers.
- Use HTTPS in production to protect data transmission.

### Error Handling

- The service includes a global exception handler middleware to gracefully handle errors and provide informative responses.

### Logging

- The service uses Serilog for logging. You can customize the logging configuration in `Program.cs`.

### Contact

- For any questions or issues, please contact the author of this service.

### Additional Information

- The service follows best practices in code structure, design patterns (dependency injection, repository pattern), and security (password hashing, token-based authentication).
- Unit tests are included in a separate project to ensure code quality and correctness.
- Feel free to enhance this README with additional sections or details specific to your project or deployment environment.
