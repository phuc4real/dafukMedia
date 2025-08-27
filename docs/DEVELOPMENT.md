# Development Guide

## Getting Started

This guide will help you set up the development environment and understand the project structure for contributing to dafukMedia.

## Prerequisites

- **.NET 8 SDK** - Download from [Microsoft](https://dotnet.microsoft.com/download)
- **Visual Studio 2022** or **Visual Studio Code** with C# extension
- **SQL Server** (LocalDB is sufficient for development)
- **Git** for version control

## Project Structure

```
dafukMedia/
??? src/
?   ??? dafukMedia.Api/              # Web API project
?   ?   ??? Controllers/             # API controllers
?   ?   ??? AutofacModule.cs         # DI configuration
?   ?   ??? Program.cs               # Application entry point
?   ??? dafukMedia.Service/          # Business logic layer
?   ?   ??? Interfaces/              # Service contracts
?   ?   ??? Implements/              # Service implementations
?   ?   ??? Common/                  # Shared utilities
?   ??? dafukMedia.Data/             # Data layer
?   ?   ??? Models/                  # Entity models
?   ?   ??? DBContext/               # Database context
?   ??? dafukMedia.DTO/              # Data transfer objects
??? docs/                            # Documentation
??? README.md                        # Project overview
??? dafukMedia.sln                   # Solution file
```

## Setting Up Development Environment

### 1. Clone the Repository

```bash
git clone https://github.com/phuc4real/dafukMedia.git
cd dafukMedia
```

### 2. Restore Dependencies

```bash
dotnet restore
```

### 3. Configure Database

Update the connection string in `src/dafukMedia.Api/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=dafukMediaDb;Trusted_Connection=true;"
  }
}
```

### 4. Create Database

```bash
dotnet ef database update --project src/dafukMedia.Model
```

### 5. Build and Run

```bash
dotnet build
dotnet run --project src/dafukMedia.Api
```

## Architecture Overview

### Clean Architecture Principles

The project follows Clean Architecture principles with clear separation of concerns:

- **API Layer**: Controllers and HTTP-related code
- **Service Layer**: Business logic and orchestration
- **Data Layer**: Entity models and database context
- **DTO Layer**: Data contracts for API communication

### Dependency Injection

The project uses **Autofac** for dependency injection. All services are registered in `AutofacModule.cs`.

Example service registration:
```csharp
builder.RegisterType<ScannerService>()
    .As<IScannerService>()
    .InstancePerLifetimeScope();
```

### Key Services

#### IScannerService
Manages scan job operations including:
- Starting new scans
- Tracking progress
- Managing job lifecycle

#### IMetadataExtractionService
Handles audio file metadata extraction using TagLibSharp:
- Reads audio file properties
- Extracts artist, album, track information
- Determines file format and quality

#### IMusicLibraryService
Manages the music library:
- Organizes tracks by artist/album
- Handles duplicate detection
- Maintains library statistics

## Coding Standards

### C# Style Guidelines

1. **Naming Conventions**:
   - PascalCase for classes, methods, properties
   - camelCase for parameters and local variables
   - Prefix interfaces with 'I'

2. **File Organization**:
   - One class per file
   - Namespace matches folder structure
   - Use proper using statements

3. **Async/Await**:
   - Use async/await for all I/O operations
   - Suffix async methods with 'Async'
   - Always use ConfigureAwait(false) in libraries

### Example Code Style

```csharp
public class ScannerService : IScannerService
{
    private readonly IMusicLibraryService _musicLibraryService;
    private readonly ILogger<ScannerService> _logger;

    public ScannerService(
        IMusicLibraryService musicLibraryService,
        ILogger<ScannerService> logger)
    {
        _musicLibraryService = musicLibraryService;
        _logger = logger;
    }

    public async Task<ApiResponse<ScanJobDto>> StartScanAsync(string directoryPath)
    {
        try
        {
            // Implementation here
            return ApiResponse<ScanJobDto>.SuccessResult(scanJobDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start scan for directory: {DirectoryPath}", directoryPath);
            return ApiResponse<ScanJobDto>.ErrorResult("Scan failed to start");
        }
    }
}
```

## Database Development

### Entity Framework Migrations

When making model changes, create migrations:

```bash
# Add migration
dotnet ef migrations add MigrationName --project src/dafukMedia.Model

# Update database
dotnet ef database update --project src/dafukMedia.Model
```

### Model Conventions

1. All entities should have:
   - Primary key named `Id`
   - `CreatedAt` and `UpdatedAt` timestamps
   - Proper navigation properties

2. Use attributes for validation:
   ```csharp
   [Required]
   [MaxLength(255)]
   public string Title { get; set; }
   ```

## Testing

### Unit Testing Setup

Currently, the project doesn't have a test project. To add testing:

1. Create test project:
   ```bash
   dotnet new xunit -n dafukMedia.Tests
   dotnet sln add src/dafukMedia.Tests
   ```

2. Add test dependencies:
   ```bash
   dotnet add package Microsoft.EntityFrameworkCore.InMemory
   dotnet add package Moq
   ```

### Testing Guidelines

- Write unit tests for all service methods
- Use in-memory database for testing
- Mock external dependencies
- Test both success and failure scenarios

## Debugging

### Logging

The application uses built-in .NET logging. Configure log levels in `appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "dafukMedia": "Debug"
    }
  }
}
```

### Common Issues

1. **Database Connection Issues**:
   - Verify SQL Server is running
   - Check connection string format
   - Ensure database exists

2. **File Access Issues**:
   - Check directory permissions
   - Verify file paths are accessible
   - Handle long path names on Windows

## Contributing Workflow

### 1. Create Feature Branch

```bash
git checkout -b feature/your-feature-name
```

### 2. Make Changes

- Follow coding standards
- Add appropriate logging
- Handle errors gracefully

### 3. Test Changes

```bash
dotnet build
dotnet test
```

### 4. Commit Changes

```bash
git add .
git commit -m "Add feature: your feature description"
```

### 5. Create Pull Request

- Push to your fork
- Create PR with clear description
- Ensure all checks pass

## Performance Considerations

### Async Best Practices

- Use `ConfigureAwait(false)` in library code
- Avoid blocking on async code
- Use cancellation tokens for long-running operations

### Database Optimization

- Use appropriate indexes
- Implement pagination for large datasets
- Consider connection pooling

### File Processing

- Process files in batches
- Implement progress reporting
- Handle large directories efficiently

## Deployment

### Development Deployment

The application can be run locally with:

```bash
dotnet run --project src/dafukMedia.Api
```

### Production Considerations

- Use production database
- Configure proper logging
- Set up health checks
- Consider containerization with Docker

## Resources

- [.NET 8 Documentation](https://docs.microsoft.com/en-us/dotnet/)
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)
- [TagLibSharp Documentation](https://github.com/mono/taglib-sharp)
- [Autofac Documentation](https://autofac.readthedocs.io/)