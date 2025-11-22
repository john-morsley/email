# Health Checks for Morsley.UK.Email.API

This directory contains health check implementations for monitoring the Email API application.

## Available Health Checks

### 1. StartupHealthCheck
- **Purpose**: Verifies the application has completed startup initialization
- **Tag**: `startup`
- **Usage**: Kubernetes liveness probes

### 2. CosmosDbHealthCheck
- **Purpose**: Verifies CosmosDB connectivity and container availability
- **Tags**: `ready`, `db`
- **Checks**:
  - Database accessibility
  - Sent emails container availability
  - Received emails container availability
- **Usage**: Kubernetes readiness probes

## Health Check Endpoints

### 1. `/health`
- **Description**: Overall health status of all checks
- **Response**: JSON with detailed status for each check
- **Status Codes**:
  - `200 OK`: All checks passed
  - `503 Service Unavailable`: One or more checks failed

### 2. `/health/live`
- **Description**: Liveness probe (startup check only)
- **Response**: Simple alive/starting status
- **Status Codes**:
  - `200 OK`: Application is running
  - `503 Service Unavailable`: Application still starting

### 3. `/health/ready`
- **Description**: Readiness probe (checks with `ready` tag)
- **Response**: JSON with readiness check results
- **Status Codes**:
  - `200 OK`: Application ready for traffic
  - `503 Service Unavailable`: Dependencies not ready

### 4. `/api/health` (Controller)
- **Description**: REST API endpoint for health status
- **Response**: Detailed JSON report
- **Additional Endpoints**:
  - `/api/health/live`: Liveness check
  - `/api/health/ready`: Readiness check

## Example Responses

### Healthy Response (`/health`)
```json
{
  "status": "Healthy",
  "totalDuration": 145.23,
  "entries": {
    "startup": {
      "data": {},
      "description": "Application startup completed",
      "duration": "00:00:00.0012345",
      "status": "Healthy"
    },
    "cosmosdb": {
      "data": {
        "endpoint": "https://your-cosmos.documents.azure.com:443/",
        "database": "EmailDatabase",
        "sentEmailsContainer": "SentEmails",
        "receivedEmailsContainer": "ReceivedEmails"
      },
      "description": "CosmosDB is accessible and containers are available",
      "duration": "00:00:00.1439878",
      "status": "Healthy"
    }
  }
}
```

### Unhealthy Response
```json
{
  "status": "Unhealthy",
  "totalDuration": 305.67,
  "entries": {
    "cosmosdb": {
      "data": {},
      "description": "CosmosDB is not accessible",
      "duration": "00:00:00.3056789",
      "status": "Unhealthy",
      "exception": "Connection timeout..."
    }
  }
}
```

## Kubernetes Configuration

### Liveness Probe
```yaml
livenessProbe:
  httpGet:
    path: /health/live
    port: 80
  initialDelaySeconds: 10
  periodSeconds: 10
  timeoutSeconds: 5
  failureThreshold: 3
```

### Readiness Probe
```yaml
readinessProbe:
  httpGet:
    path: /health/ready
    port: 80
  initialDelaySeconds: 15
  periodSeconds: 10
  timeoutSeconds: 5
  failureThreshold: 3
```

## Azure App Service Configuration

For Azure App Service, configure health check monitoring:

1. Navigate to your App Service
2. Go to **Monitoring** → **Health check**
3. Enable health check
4. Set path to `/health/ready`
5. Configure load balancing to remove unhealthy instances

## Adding Additional Health Checks

To add a new health check:

1. Create a class implementing `IHealthCheck` in this directory
2. Register it in `Program.cs`:
   ```csharp
   builder.Services.AddHealthChecks()
       .AddCheck<YourHealthCheck>("name", tags: new[] { "tag1", "tag2" });
   ```

## NuGet Packages Used

- `Microsoft.Extensions.Diagnostics.HealthChecks` - Core health check framework
- `AspNetCore.HealthChecks.CosmosDb` - CosmosDB health check
- `AspNetCore.HealthChecks.UI.Client` - JSON response writer
