# API Documentation

## Overview

The dafukMedia API provides endpoints for managing music library scanning, metadata editing, and media provider operations. All endpoints return responses in a standardized `ApiResponse<T>` format.

## Base URL

```
https://localhost:7071/api
```

## Response Format

All API responses follow this standardized format:

```json
{
  "success": boolean,
  "data": T,
  "message": "string",
  "errors": ["string"]
}
```

## Authentication

Currently, the API does not require authentication. This may change in future versions.

## Endpoints

### Scan Controller

#### POST /scan/start

Starts a new scan job for the specified directory.

**Request Body:**
```json
{
  "directoryPath": "string"
}
```

**Responses:**
- `200 OK`: Scan job started successfully
- `400 Bad Request`: Invalid directory path or other validation error

**Example:**
```bash
curl -X POST https://localhost:7071/api/scan/start \
  -H "Content-Type: application/json" \
  -d '{"directoryPath": "C:\\Music\\MyCollection"}'
```

#### GET /scan

Gets all scan jobs.

**Responses:**
- `200 OK`: List of all scan jobs

**Example:**
```bash
curl https://localhost:7071/api/scan
```

#### GET /scan/{id}

Gets a specific scan job by ID.

**Parameters:**
- `id` (path): The scan job ID

**Responses:**
- `200 OK`: Scan job details
- `404 Not Found`: Scan job not found

**Example:**
```bash
curl https://localhost:7071/api/scan/1
```

#### GET /scan/{id}/progress

Gets the progress of a running scan job.

**Parameters:**
- `id` (path): The scan job ID

**Responses:**
- `200 OK`: Scan progress details
- `404 Not Found`: Scan job not found

**Example:**
```bash
curl https://localhost:7071/api/scan/1/progress
```

#### POST /scan/{id}/cancel

Cancels a running scan job.

**Parameters:**
- `id` (path): The scan job ID

**Responses:**
- `200 OK`: Scan job cancelled successfully
- `400 Bad Request`: Cannot cancel scan job (may already be completed)

**Example:**
```bash
curl -X POST https://localhost:7071/api/scan/1/cancel
```

#### DELETE /scan/{id}

Deletes a scan job.

**Parameters:**
- `id` (path): The scan job ID

**Responses:**
- `200 OK`: Scan job deleted successfully
- `404 Not Found`: Scan job not found

**Example:**
```bash
curl -X DELETE https://localhost:7071/api/scan/1
```

### Tracks Controller

#### GET /tracks

Gets all tracks with pagination and filtering support.

**Query Parameters:**
- `page` (optional): Page number (default: 1)
- `pageSize` (optional): Number of items per page (default: 50)
- `search` (optional): Search term for title, artist, or album
- `artist` (optional): Filter by artist name
- `album` (optional): Filter by album title
- `genre` (optional): Filter by genre name

**Responses:**
- `200 OK`: Paginated list of tracks

**Example:**
```bash
curl "https://localhost:7071/api/tracks?page=1&pageSize=20&search=rock"
```

#### GET /tracks/{id}

Gets a specific track by ID.

**Parameters:**
- `id` (path): The track ID

**Responses:**
- `200 OK`: Track details
- `404 Not Found`: Track not found

**Example:**
```bash
curl https://localhost:7071/api/tracks/1
```

#### GET /tracks/artists

Gets all artist names for metadata editing.

**Responses:**
- `200 OK`: List of artist names

**Example:**
```bash
curl https://localhost:7071/api/tracks/artists
```

#### GET /tracks/albums

Gets all album titles for metadata editing.

**Responses:**
- `200 OK`: List of album titles

**Example:**
```bash
curl https://localhost:7071/api/tracks/albums
```

#### GET /tracks/genres

Gets all genre names for metadata editing.

**Responses:**
- `200 OK`: List of genre names

**Example:**
```bash
curl https://localhost:7071/api/tracks/genres
```

#### POST /tracks/search

Advanced search for tracks with multiple criteria.

**Request Body:**
```json
{
  "title": "string",
  "artist": "string",
  "album": "string",
  "genre": "string",
  "year": 2024,
  "hasMissingMetadata": true,
  "maxResults": 100
}
```

**Responses:**
- `200 OK`: List of matching tracks

**Example:**
```bash
curl -X POST https://localhost:7071/api/tracks/search \
  -H "Content-Type: application/json" \
  -d '{"hasMissingMetadata": true, "maxResults": 50}'
```

### Metadata Controller

#### GET /metadata/track/{id}

Gets detailed metadata for a specific track.

**Parameters:**
- `id` (path): The track ID

**Responses:**
- `200 OK`: Track metadata details
- `404 Not Found`: Track not found

**Example:**
```bash
curl https://localhost:7071/api/metadata/track/1
```

#### PUT /metadata/track/{id}

Edits metadata for a single track.

**Parameters:**
- `id` (path): The track ID

**Request Body:**
```json
{
  "title": "Updated Song Title",
  "artist": "Updated Artist",
  "album": "Updated Album",
  "genre": "Rock",
  "year": 2024,
  "trackNumber": 1,
  "discNumber": 1,
  "comment": "Updated comment",
  "composer": "Composer Name",
  "albumArtist": "Album Artist",
  "additionalMetadata": {
    "customField": "customValue"
  }
}
```

**Responses:**
- `200 OK`: Metadata updated successfully
- `400 Bad Request`: Validation error
- `404 Not Found`: Track not found

**Example:**
```bash
curl -X PUT https://localhost:7071/api/metadata/track/1 \
  -H "Content-Type: application/json" \
  -d '{"title": "New Title", "artist": "New Artist"}'
```

#### PUT /metadata/batch

Edits metadata for multiple tracks (batch operation).

**Request Body:**
```json
{
  "trackIds": [1, 2, 3],
  "metadata": {
    "artist": "Various Artists",
    "album": "Compilation Album",
    "year": 2024
  },
  "overwriteExisting": false
}
```

**Responses:**
- `200 OK`: Batch edit results
- `400 Bad Request`: Validation error

**Example:**
```bash
curl -X PUT https://localhost:7071/api/metadata/batch \
  -H "Content-Type: application/json" \
  -d '{"trackIds": [1,2,3], "metadata": {"album": "Greatest Hits"}}'
```

#### GET /metadata/track/{id}/validate

Validates metadata for a specific track and highlights missing/invalid fields.

**Parameters:**
- `id` (path): The track ID

**Responses:**
- `200 OK`: Validation results
- `404 Not Found`: Track not found

**Example:**
```bash
curl https://localhost:7071/api/metadata/track/1/validate
```

#### POST /metadata/validate

Validates metadata for multiple tracks.

**Request Body:**
```json
[1, 2, 3, 4, 5]
```

**Responses:**
- `200 OK`: Validation results for all tracks
- `400 Bad Request`: Invalid track IDs

**Example:**
```bash
curl -X POST https://localhost:7071/api/metadata/validate \
  -H "Content-Type: application/json" \
  -d '[1, 2, 3]'
```

#### GET /metadata/issues

Gets all tracks with metadata issues (missing or invalid data).

**Responses:**
- `200 OK`: List of tracks with metadata issues

**Example:**
```bash
curl https://localhost:7071/api/metadata/issues
```

#### POST /metadata/track/{id}/revert

Reverts metadata changes for a track (restores from file).

**Parameters:**
- `id` (path): The track ID

**Responses:**
- `200 OK`: Metadata reverted successfully
- `400 Bad Request`: Cannot revert metadata
- `404 Not Found`: Track not found

**Example:**
```bash
curl -X POST https://localhost:7071/api/metadata/track/1/revert
```

### Provider Controller

#### GET /provider

Gets all available media providers.

**Responses:**
- `200 OK`: List of available providers

**Example:**
```bash
curl https://localhost:7071/api/provider
```

## Data Transfer Objects (DTOs)

### Track DTOs

#### TrackDto
```json
{
  "id": 1,
  "title": "Song Title",
  "filePath": "C:\\Music\\Artist\\Album\\song.mp3",
  "fileFormat": "MP3",
  "fileSize": 5242880,
  "trackNumber": 1,
  "discNumber": 1,
  "duration": "00:03:45",
  "year": 2024,
  "bitRate": 320,
  "sampleRate": 44100,
  "artistName": "Artist Name",
  "albumTitle": "Album Title",
  "genreName": "Rock",
  "createdAt": "2024-01-15T10:30:00Z",
  "updatedAt": "2024-01-15T10:30:00Z"
}
```

#### TrackMetadataDto
```json
{
  "id": 1,
  "title": "Song Title",
  "artist": "Artist Name",
  "album": "Album Title",
  "genre": "Rock",
  "year": 2024,
  "trackNumber": 1,
  "discNumber": 1,
  "comment": "Song comment",
  "composer": "Composer Name",
  "albumArtist": "Album Artist",
  "filePath": "C:\\Music\\Artist\\Album\\song.mp3",
  "additionalMetadata": {
    "customField": "customValue"
  }
}
```

#### EditTrackMetadataDto
```json
{
  "title": "Updated Song Title",
  "artist": "Updated Artist",
  "album": "Updated Album",
  "genre": "Rock",
  "year": 2024,
  "trackNumber": 1,
  "discNumber": 1,
  "comment": "Updated comment",
  "composer": "Composer Name",
  "albumArtist": "Album Artist",
  "additionalMetadata": {
    "customField": "customValue"
  }
}
```

#### BatchEditMetadataDto
```json
{
  "trackIds": [1, 2, 3],
  "metadata": {
    "artist": "Various Artists",
    "album": "Compilation Album",
    "year": 2024
  },
  "overwriteExisting": false
}
```

#### MetadataValidationResultDto
```json
{
  "trackId": 1,
  "filePath": "C:\\Music\\Artist\\Album\\song.mp3",
  "missingFields": ["genre", "year"],
  "invalidFields": ["trackNumber"],
  "warnings": ["Album art not found"]
}
```

#### MetadataEditResultDto
```json
{
  "trackId": 1,
  "filePath": "C:\\Music\\Artist\\Album\\song.mp3",
  "success": true,
  "errorMessage": null,
  "updatedFields": ["title", "artist", "year"]
}
```

#### TrackSearchDto
```json
{
  "title": "search term",
  "artist": "artist name",
  "album": "album title",
  "genre": "genre name",
  "year": 2024,
  "hasMissingMetadata": true,
  "maxResults": 100
}
```

### Scan DTOs

#### ScanJobDto

```json
{
  "id": 1,
  "directoryPath": "C:\\Music\\MyCollection",
  "status": "Running",
  "startTime": "2024-01-15T10:30:00Z",
  "endTime": null,
  "filesScanned": 150,
  "filesAdded": 145,
  "duplicatesFound": 3,
  "errorsCount": 2,
  "errorMessage": null,
  "duration": null
}
```

#### ScanJobCreateDto

```json
{
  "directoryPath": "C:\\Music\\MyCollection"
}
```

#### ScanProgressDto

```json
{
  "jobId": 1,
  "status": "Running",
  "filesScanned": 150,
  "filesAdded": 145,
  "duplicatesFound": 3,
  "errorsCount": 2,
  "currentFile": "artist - song.mp3",
  "progressPercentage": 75.5
}
```

## Status Values

### Scan Job Status

- `Pending`: Job is queued but not started
- `Running`: Job is currently processing
- `Completed`: Job finished successfully
- `Cancelled`: Job was cancelled by user
- `Failed`: Job failed due to an error

## Metadata Editing Features

### Single Track Editing
- Edit individual track metadata fields
- Save changes directly to audio file tags using TagLibSharp
- Validate input data (year range, track numbers, etc.)
- Update database records automatically

### Batch Editing
- Edit metadata for multiple tracks simultaneously
- Option to overwrite existing values or only fill missing fields
- Comprehensive error reporting for each track
- Atomic operations per track (failure of one doesn't affect others)

### Metadata Validation
- Detect missing required fields (title, artist, album, genre)
- Identify invalid field values (invalid years, negative track numbers)
- Provide warnings for potential issues
- Batch validation for multiple tracks

### Metadata Issues Detection
- Find all tracks with missing or invalid metadata
- Helpful for cleaning up music library
- Prioritize tracks that need attention

### Revert Functionality
- Restore track metadata from original audio file
- Useful when manual edits need to be undone
- Extracts fresh metadata using TagLibSharp

## Error Handling

The API uses standard HTTP status codes and provides detailed error messages in the response body:

### Common Error Responses

#### 400 Bad Request
```json
{
  "success": false,
  "data": null,
  "message": "Title is required",
  "errors": ["Title cannot be empty"]
}
```

#### 404 Not Found
```json
{
  "success": false,
  "data": null,
  "message": "Track not found",
  "errors": []
}
```

#### 500 Internal Server Error
```json
{
  "success": false,
  "data": null,
  "message": "An internal server error occurred",
  "errors": ["Detailed error message"]
}
```

## Rate Limiting

Currently, there are no rate limits imposed on API endpoints. This may change in future versions.

## Swagger/OpenAPI

Interactive API documentation is available at:
```
https://localhost:7071/swagger
```

This provides a user-friendly interface to explore and test the API endpoints.