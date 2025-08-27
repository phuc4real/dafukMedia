# dafukMedia - Music Library Management System

A .NET 8 Web API for organizing and managing music collections with automatic metadata extraction, metadata editing, and duplicate detection.

## 🎵 Overview

dafukMedia is a comprehensive music library management system that automatically scans, organizes, and catalogs music files from local directories. It extracts metadata from audio files and organizes them by Artist, Album, and Genre with support for both lossy and lossless audio formats. The system also provides comprehensive metadata editing capabilities to correct and enhance your music library information.

## ✨ Features

### Music File Organization
- **Automatic Folder Scanning**: Scan local directories recursively for music files
- **Metadata Extraction**: Extract comprehensive metadata using TagLibSharp
- **Smart Categorization**: Automatically organize by Artist, Album, and Genre
- **Duplicate Detection**: Prevent reimporting existing files using file hash comparison
- **Format Support**: 
  - **Lossy**: MP3, AAC, OGG, WMA
  - **Lossless**: FLAC, ALAC, WAV

### Metadata Editing & Management
- **Single Track Editing**: Edit individual track metadata fields (title, artist, album, genre, year, etc.)
- **Batch Editing**: Update metadata for multiple tracks simultaneously
- **Metadata Validation**: Detect and highlight missing or invalid metadata fields
- **File Tag Updates**: Save changes directly to audio file tags using TagLibSharp
- **Revert Functionality**: Restore metadata from original audio files when needed
- **Advanced Search**: Find tracks by various criteria including tracks with missing metadata

### Real-time Scanning
- Background scan job processing with cancellation support
- Real-time progress tracking with percentage completion
- Comprehensive scan logging and error reporting
- Batch import functionality for large music libraries

### API Features
- RESTful API with comprehensive endpoint coverage
- Real-time scan progress monitoring
- Detailed error handling and validation
- Swagger/OpenAPI documentation

## 🏗️ Architecture

The project follows a clean architecture pattern with clear separation of concerns:

```
dafukMedia/
├── src/
│   ├── dafukMedia.Api/          # Web API layer
│   ├── dafukMedia.Service/      # Business logic layer
│   ├── dafukMedia.Data/         # Data access layer (Models)
│   └── dafukMedia.DTO/          # Data transfer objects
```

### Technology Stack
- **.NET 8**: Latest LTS version with minimal APIs support
- **Entity Framework Core**: Data access with SQL Server support
- **TagLibSharp**: Audio metadata extraction and editing library
- **Autofac**: Dependency injection container
- **Swagger/OpenAPI**: Auto-generated API documentation

### Project Structure

#### dafukMedia.Api
- **Controllers**: API endpoints for scan management, metadata editing, tracks, and media providers
- **Program.cs**: Application startup and configuration
- **AutofacModule.cs**: Dependency injection configuration

#### dafukMedia.Service
- **Interfaces**: Service contracts and abstractions
- **Implements**: Business logic implementations including metadata editing services
- **Providers**: File system and media provider implementations
- **Common**: Shared constants and utilities

#### dafukMedia.Data
- **Models**: Entity framework models (Track, Artist, Album, Genre, etc.)
- **DBContext**: Database context configuration

#### dafukMedia.DTO
- **DTOs**: Data transfer objects for API communication including metadata editing DTOs
- **ApiResponse**: Standardized API response wrapper

## 🚀 Getting Started

### Prerequisites
- .NET 8 SDK
- SQL Server (LocalDB or full instance)
- Visual Studio 2022 or VS Code

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/phuc4real/dafukMedia.git
   cd dafukMedia
   ```

2. **Restore NuGet packages**
   ```bash
   dotnet restore
   ```

3. **Update database connection string**
   
   Update `appsettings.json` in the API project:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=dafukMediaDb;Trusted_Connection=true;"
     }
   }
   ```

4. **Run database migrations**
   ```bash
   dotnet ef database update --project src/dafukMedia.Model
   ```

5. **Run the application**
   ```bash
   dotnet run --project src/dafukMedia.Api
   ```

6. **Access the API**
   - API: `https://localhost:7071`
   - Swagger UI: `https://localhost:7071/swagger`

## 📖 API Documentation

### Scan Management

#### Start a Scan Job
```http
POST /api/scan/start
Content-Type: application/json

{
  "directoryPath": "C:\\Music\\MyCollection"
}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "id": 1,
    "directoryPath": "C:\\Music\\MyCollection",
    "status": "Running",
    "startTime": "2024-01-15T10:30:00Z",
    "filesScanned": 0,
    "filesAdded": 0,
    "duplicatesFound": 0,
    "errorsCount": 0
  },
  "message": "Scan job started successfully"
}
```

#### Get Scan Progress
```http
GET /api/scan/{id}/progress
```

**Response:**
```json
{
  "success": true,
  "data": {
    "jobId": 1,
    "status": "Running",
    "filesScanned": 150,
    "filesAdded": 145,
    "duplicatesFound": 3,
    "errorsCount": 2,
    "currentFile": "artist - song.mp3",
    "progressPercentage": 75.5
  }
}
```

#### Cancel Scan Job
```http
POST /api/scan/{id}/cancel
```

#### Get All Scan Jobs
```http
GET /api/scan
```

#### Delete Scan Job
```http
DELETE /api/scan/{id}
```

### Metadata Editing

#### Get Track Metadata
```http
GET /api/metadata/track/{id}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "id": 1,
    "title": "Song Title",
    "artist": "Artist Name",
    "album": "Album Title",
    "genre": "Rock",
    "year": 2024,
    "trackNumber": 1,
    "filePath": "C:\\Music\\Artist\\song.mp3",
    "additionalMetadata": {}
  }
}
```

#### Edit Single Track Metadata
```http
PUT /api/metadata/track/{id}
Content-Type: application/json

{
  "title": "Updated Title",
  "artist": "Updated Artist",
  "album": "Updated Album",
  "genre": "Rock",
  "year": 2024,
  "trackNumber": 1
}
```

#### Batch Edit Metadata
```http
PUT /api/metadata/batch
Content-Type: application/json

{
  "trackIds": [1, 2, 3],
  "metadata": {
    "album": "Compilation Album",
    "year": 2024
  },
  "overwriteExisting": false
}
```

#### Validate Track Metadata
```http
GET /api/metadata/track/{id}/validate
```

**Response:**
```json
{
  "success": true,
  "data": {
    "trackId": 1,
    "filePath": "C:\\Music\\song.mp3",
    "missingFields": ["genre"],
    "invalidFields": [],
    "warnings": ["Year not specified"]
  }
}
```

#### Get Tracks with Metadata Issues
```http
GET /api/metadata/issues
```

#### Revert Track Metadata
```http
POST /api/metadata/track/{id}/revert
```

### Track Management

#### Get All Tracks
```http
GET /api/tracks?page=1&pageSize=50&search=rock
```

#### Search Tracks
```http
POST /api/tracks/search
Content-Type: application/json

{
  "hasMissingMetadata": true,
  "genre": "Rock",
  "maxResults": 100
}
```

#### Get Artists/Albums/Genres
```http
GET /api/tracks/artists
GET /api/tracks/albums
GET /api/tracks/genres
```

### Provider Management

#### Get Available Providers
```http
GET /api/provider
```

## 🗃️ Database Schema

### Core Entities

- **Track**: Individual music files with metadata
- **Artist**: Music artists/performers
- **Album**: Music albums/collections
- **Genre**: Music genres
- **Playlist**: User-created playlists
- **ScanJob**: Scan operation tracking
- **Metadata**: Additional metadata key-value pairs

### Relationships

- Track → Artist (Many-to-One)
- Track → Album (Many-to-One)
- Track → Genre (Many-to-One)
- Track → Metadata (One-to-Many)

## 🔧 Configuration

### Supported File Extensions

The system supports various audio formats defined in `FileExtensionConstants.cs`:

- **MP3**: .mp3
- **FLAC**: .flac
- **WAV**: .wav
- **M4A/AAC**: .m4a, .aac
- **OGG**: .ogg
- **WMA**: .wma

### Service Configuration

Services are configured using Autofac in `AutofacModule.cs`:

- `IScannerService`: Handles scan job management
- `IMetadataExtractionService`: Extracts metadata from audio files
- `IMetadataEditService`: Handles metadata editing operations
- `IMusicLibraryService`: Manages music library operations
- `ILocalFileProvider`: Handles local file system operations

## 🛠️ Development

### Building the Project
```bash
dotnet build
```

### Running Tests
```bash
dotnet test
```

### Running with Docker
```bash
docker build -t dafukmedia .
docker run -p 8080:8080 dafukmedia
```

### Code Structure Guidelines

- Follow Clean Architecture principles
- Use dependency injection for all services
- Implement proper error handling with `ApiResponse<T>`
- Use async/await for all I/O operations
- Follow C# naming conventions

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🐛 Issues & Support

If you encounter any issues or have questions:

1. Check the [Issues](https://github.com/phuc4real/dafukMedia/issues) page
2. Create a new issue with detailed information
3. Include logs and error messages when applicable

## 🚧 Roadmap

- [x] Music library scanning and organization
- [x] Metadata editing with TagLibSharp integration
- [x] Batch metadata operations
- [x] Metadata validation and issue detection
- [ ] Web UI for library management
- [ ] Advanced search and filtering
- [ ] Playlist management
- [ ] Audio streaming capabilities
- [ ] Multi-user support
- [ ] Cloud storage provider integration
- [ ] Mobile app support

---

**dafukMedia** - Making music library management simple and efficient! 🎵