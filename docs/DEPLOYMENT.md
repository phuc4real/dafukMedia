# Deployment Guide

## Overview

This guide covers various deployment scenarios for the dafukMedia application, from local development to production environments.

## Prerequisites

- .NET 8 Runtime
- SQL Server or SQL Server Express
- IIS (for Windows Server deployment) or reverse proxy (nginx/Apache)
- SSL certificate (for production)

## Configuration

### Environment-Specific Settings

Create environment-specific configuration files:

#### appsettings.Development.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=dafukMediaDb;Trusted_Connection=true;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft": "Information"
    }
  }
}
```

#### appsettings.Production.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=prod-server;Database=dafukMediaDb;User Id=app_user;Password=secure_password;TrustServerCertificate=true;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning"
    }
  },
  "AllowedHosts": "yourdomain.com"
}
```

## Local Development Deployment

### Using dotnet CLI

1. **Build the application**:
   ```bash
   dotnet build --configuration Release
   ```

2. **Run migrations**:
   ```bash
   dotnet ef database update --project src/dafukMedia.Model
   ```

3. **Start the application**:
   ```bash
   dotnet run --project src/dafukMedia.Api --configuration Release
   ```

### Using Visual Studio

1. Set `dafukMedia.Api` as startup project
2. Select `Release` configuration
3. Press F5 or Ctrl+F5 to run

## Docker Deployment

### Dockerfile

Create a `Dockerfile` in the root directory:

```dockerfile
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["src/dafukMedia.Api/dafukMedia.Api.csproj", "src/dafukMedia.Api/"]
COPY ["src/dafukMedia.Service/dafukMedia.Service.csproj", "src/dafukMedia.Service/"]
COPY ["src/dafukMedia.Model/dafukMedia.Data.csproj", "src/dafukMedia.Model/"]
COPY ["src/dafukMedia.DTO/dafukMedia.DTO.csproj", "src/dafukMedia.DTO/"]

RUN dotnet restore "src/dafukMedia.Api/dafukMedia.Api.csproj"

# Copy source code and build
COPY . .
WORKDIR "/src/src/dafukMedia.Api"
RUN dotnet build "dafukMedia.Api.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "dafukMedia.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=publish /app/publish .

# Create non-root user
RUN addgroup --system --gid 1001 dafukmedia && \
    adduser --system --uid 1001 --gid 1001 dafukmedia

# Change ownership of the app directory
RUN chown -R dafukmedia:dafukmedia /app
USER dafukmedia

EXPOSE 8080
EXPOSE 8081

ENTRYPOINT ["dotnet", "dafukMedia.Api.dll"]
```

### Docker Compose

Create `docker-compose.yml`:

```yaml
version: '3.8'

services:
  app:
    build: .
    ports:
      - "8080:8080"
      - "8081:8081"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ASPNETCORE_URLS=http://+:8080;https://+:8081
      - ConnectionStrings__DefaultConnection=Server=db;Database=dafukMediaDb;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=true;
    depends_on:
      - db
    volumes:
      - music-data:/app/music
    networks:
      - dafukmedia-network

  db:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=YourStrong@Passw0rd
      - MSSQL_PID=Express
    ports:
      - "1433:1433"
    volumes:
      - db-data:/var/opt/mssql
    networks:
      - dafukmedia-network

volumes:
  db-data:
  music-data:

networks:
  dafukmedia-network:
    driver: bridge
```

### Building and Running with Docker

```bash
# Build the image
docker build -t dafukmedia:latest .

# Run with docker-compose
docker-compose up -d

# View logs
docker-compose logs -f app

# Stop services
docker-compose down
```

## IIS Deployment (Windows Server)

### Prerequisites

1. Install IIS with ASP.NET Core Module
2. Install .NET 8 Hosting Bundle
3. Create application pool

### Deployment Steps

1. **Publish the application**:
   ```bash
   dotnet publish src/dafukMedia.Api/dafukMedia.Api.csproj -c Release -o C:\inetpub\wwwroot\dafukmedia
   ```

2. **Configure IIS**:
   - Create new site in IIS Manager
   - Set physical path to published folder
   - Configure application pool to use "No Managed Code"
   - Set appropriate permissions for IIS_IUSRS

3. **Configure SSL**:
   - Install SSL certificate
   - Bind HTTPS to the site
   - Redirect HTTP to HTTPS

### web.config

IIS requires a `web.config` file (auto-generated during publish):

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <location path="." inheritInChildApplications="false">
    <system.webServer>
      <handlers>
        <add name="aspNetCore" path="*" verb="*" modules="AspNetCoreModuleV2" resourceType="Unspecified" />
      </handlers>
      <aspNetCore processPath="dotnet" 
                  arguments=".\dafukMedia.Api.dll" 
                  stdoutLogEnabled="false" 
                  stdoutLogFile=".\logs\stdout" 
                  hostingModel="inprocess" />
    </system.webServer>
  </location>
</configuration>
```

## Linux Server Deployment

### Prerequisites

1. Install .NET 8 Runtime
2. Install nginx (reverse proxy)
3. Configure systemd service

### Installation Steps

1. **Install .NET 8**:
   ```bash
   wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
   sudo dpkg -i packages-microsoft-prod.deb
   sudo apt update
   sudo apt install -y aspnetcore-runtime-8.0
   ```

2. **Deploy application**:
   ```bash
   # Create application directory
   sudo mkdir -p /var/www/dafukmedia
   
   # Copy published files
   sudo cp -r ./publish/* /var/www/dafukmedia/
   
   # Set permissions
   sudo chown -R www-data:www-data /var/www/dafukmedia
   sudo chmod -R 755 /var/www/dafukmedia
   ```

3. **Create systemd service**:

   Create `/etc/systemd/system/dafukmedia.service`:
   ```ini
   [Unit]
   Description=dafukMedia Music Library API
   After=network.target
   
   [Service]
   Type=notify
   ExecStart=/usr/bin/dotnet /var/www/dafukmedia/dafukMedia.Api.dll
   Restart=always
   RestartSec=5
   TimeoutStopSec=90
   KillMode=mixed
   SyslogIdentifier=dafukmedia
   User=www-data
   Environment=ASPNETCORE_ENVIRONMENT=Production
   Environment=ASPNETCORE_URLS=http://localhost:5000
   WorkingDirectory=/var/www/dafukmedia
   
   [Install]
   WantedBy=multi-user.target
   ```

4. **Configure nginx**:

   Create `/etc/nginx/sites-available/dafukmedia`:
   ```nginx
   server {
       listen 80;
       server_name yourdomain.com;
       return 301 https://$server_name$request_uri;
   }
   
   server {
       listen 443 ssl http2;
       server_name yourdomain.com;
       
       ssl_certificate /etc/ssl/certs/yourdomain.com.crt;
       ssl_certificate_key /etc/ssl/private/yourdomain.com.key;
       
       location / {
           proxy_pass http://localhost:5000;
           proxy_http_version 1.1;
           proxy_set_header Upgrade $http_upgrade;
           proxy_set_header Connection keep-alive;
           proxy_set_header Host $host;
           proxy_set_header X-Real-IP $remote_addr;
           proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
           proxy_set_header X-Forwarded-Proto $scheme;
           proxy_cache_bypass $http_upgrade;
       }
       
       # Increase upload size for large music files
       client_max_body_size 100M;
   }
   ```

5. **Enable and start services**:
   ```bash
   # Enable nginx site
   sudo ln -s /etc/nginx/sites-available/dafukmedia /etc/nginx/sites-enabled/
   sudo nginx -t
   sudo systemctl reload nginx
   
   # Start application service
   sudo systemctl enable dafukmedia.service
   sudo systemctl start dafukmedia.service
   sudo systemctl status dafukmedia.service
   ```

## Azure App Service Deployment

### Using Azure CLI

1. **Create resource group**:
   ```bash
   az group create --name dafukmedia-rg --location "East US"
   ```

2. **Create App Service plan**:
   ```bash
   az appservice plan create --name dafukmedia-plan --resource-group dafukmedia-rg --sku B1 --is-linux
   ```

3. **Create web app**:
   ```bash
   az webapp create --resource-group dafukmedia-rg --plan dafukmedia-plan --name dafukmedia-app --runtime "DOTNETCORE:8.0"
   ```

4. **Deploy application**:
   ```bash
   # Publish to zip
   dotnet publish src/dafukMedia.Api/dafukMedia.Api.csproj -c Release -o ./publish
   cd publish && zip -r ../app.zip . && cd ..
   
   # Deploy zip
   az webapp deployment source config-zip --resource-group dafukmedia-rg --name dafukmedia-app --src app.zip
   ```

5. **Configure connection string**:
   ```bash
   az webapp config connection-string set --resource-group dafukmedia-rg --name dafukmedia-app --connection-string-type SQLServer --settings DefaultConnection="Server=tcp:server.database.windows.net,1433;Database=dafukMediaDb;User ID=username;Password=password;Encrypt=true;"
   ```

## Database Setup

### SQL Server Setup

1. **Create database**:
   ```sql
   CREATE DATABASE dafukMediaDb;
   GO
   
   CREATE LOGIN dafuk_user WITH PASSWORD = 'SecurePassword123!';
   GO
   
   USE dafukMediaDb;
   GO
   
   CREATE USER dafuk_user FOR LOGIN dafuk_user;
   GO
   
   ALTER ROLE db_datareader ADD MEMBER dafuk_user;
   ALTER ROLE db_datawriter ADD MEMBER dafuk_user;
   ALTER ROLE db_ddladmin ADD MEMBER dafuk_user;
   GO
   ```

2. **Run migrations**:
   ```bash
   dotnet ef database update --project src/dafukMedia.Model --connection "Server=server;Database=dafukMediaDb;User Id=dafuk_user;Password=SecurePassword123!;"
   ```

## Monitoring and Logging

### Application Insights (Azure)

Add to `Program.cs`:
```csharp
builder.Services.AddApplicationInsightsTelemetry();
```

### Structured Logging

Configure in `appsettings.json`:
```json
{
  "Serilog": {
    "Using": ["Serilog.Sinks.File", "Serilog.Sinks.Console"],
    "MinimumLevel": "Information",
    "WriteTo": [
      {
        "Name": "File",
        "Args": {
          "path": "logs/dafukmedia-.log",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 30
        }
      }
    ]
  }
}
```

## Health Checks

Add health checks to `Program.cs`:
```csharp
builder.Services.AddHealthChecks()
    .AddSqlServer(connectionString)
    .AddCheck("file-system", () => Directory.Exists("/music") ? HealthCheckResult.Healthy() : HealthCheckResult.Unhealthy());

app.MapHealthChecks("/health");
```

## Performance Optimization

### Production Configuration

```csharp
// In Program.cs for production
if (app.Environment.IsProduction())
{
    app.UseHsts();
    app.UseHttpsRedirection();
    app.UseResponseCompression();
    app.UseResponseCaching();
}
```

### Caching

Add memory caching:
```csharp
builder.Services.AddMemoryCache();
builder.Services.AddResponseCaching();
```

## Security Considerations

### HTTPS Configuration

```csharp
builder.Services.AddHsts(options =>
{
    options.Preload = true;
    options.IncludeSubDomains = true;
    options.MaxAge = TimeSpan.FromDays(365);
});
```

### CORS Configuration

```csharp
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("https://yourdomain.com")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});
```

## Troubleshooting

### Common Issues

1. **Connection String Issues**:
   - Verify server accessibility
   - Check credentials
   - Validate connection string format

2. **File Permission Issues**:
   - Ensure app has read access to music directories
   - Check write permissions for logs and temp files

3. **Port Conflicts**:
   - Verify ports are available
   - Check firewall settings
   - Ensure no other services using same ports

### Logs Location

- **Windows**: `%PROGRAMDATA%\dafukMedia\logs`
- **Linux**: `/var/log/dafukmedia/`
- **Docker**: Use `docker logs` command

### Performance Monitoring

Monitor these metrics:
- Response times
- Memory usage
- Database connection pool
- File scan performance
- Error rates