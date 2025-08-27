# API Documentation

## Overview

The dafukMedia API provides endpoints for managing music library scanning and media provider operations. All endpoints return responses in a standardized `ApiResponse<T>` format.

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

### ScanJobDto

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

### ScanJobCreateDto

```json
{
  "directoryPath": "C:\\Music\\MyCollection"
}
```

### ScanProgressDto

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

## Error Handling

The API uses standard HTTP status codes and provides detailed error messages in the response body:

### Common Error Responses

#### 400 Bad Request
```json
{
  "success": false,
  "data": null,
  "message": "Directory path is required",
  "errors": ["DirectoryPath cannot be empty"]
}
```

#### 404 Not Found
```json
{
  "success": false,
  "data": null,
  "message": "Scan job not found",
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