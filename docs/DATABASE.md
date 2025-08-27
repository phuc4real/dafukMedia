# Database Schema Documentation

## Overview

The dafukMedia database schema is designed to efficiently store and organize music metadata with proper relationships between entities. The schema supports comprehensive music library management with duplicate detection and extensive metadata storage.

## Entity Relationship Diagram

```
???????????????????     ???????????????????     ???????????????????
?     Artist      ?     ?     Album       ?     ?     Genre       ?
???????????????????     ???????????????????     ???????????????????
? Id (PK)         ?     ? Id (PK)         ?     ? Id (PK)         ?
? Name            ?     ? Title           ?     ? Name            ?
? SortName        ?     ? SortTitle       ?     ? Description     ?
? Biography       ?     ? Year            ?     ? CreatedAt       ?
? CreatedAt       ?     ? ArtistId (FK)   ?     ? UpdatedAt       ?
? UpdatedAt       ?     ? GenreId (FK)    ?     ???????????????????
???????????????????     ? CreatedAt       ?              ?
         ?               ? UpdatedAt       ?              ?
         ?               ???????????????????              ?
         ?                        ?                       ?
         ?                        ?                       ?
         ??????????????????????????????????????????????????
                                  ?
                                  ?
                   ???????????????????????????????????
                   ?             Track               ?
                   ???????????????????????????????????
                   ? Id (PK)                        ?
                   ? Title                          ?
                   ? SortTitle                      ?
                   ? FilePath                       ?
                   ? FileFormat                     ?
                   ? FileSize                       ?
                   ? FileHash                       ?
                   ? TrackNumber                    ?
                   ? DiscNumber                     ?
                   ? Duration                       ?
                   ? Year                           ?
                   ? BitRate                        ?
                   ? SampleRate                     ?
                   ? ArtistId (FK)                  ?
                   ? AlbumId (FK)                   ?
                   ? GenreId (FK)                   ?
                   ? CreatedAt                      ?
                   ? UpdatedAt                      ?
                   ???????????????????????????????????
                                  ?
                                  ? 1:N
                                  ?
                   ???????????????????????????????????
                   ?           Metadata              ?
                   ???????????????????????????????????
                   ? Id (PK)                        ?
                   ? TrackId (FK)                   ?
                   ? Key                            ?
                   ? Value                          ?
                   ? CreatedAt                      ?
                   ???????????????????????????????????

         ???????????????????????????????????
         ?           ScanJob               ?
         ???????????????????????????????????
         ? Id (PK)                        ?
         ? DirectoryPath                  ?
         ? Status                         ?
         ? StartTime                      ?
         ? EndTime                        ?
         ? FilesScanned                   ?
         ? FilesAdded                     ?
         ? DuplicatesFound                ?
         ? ErrorsCount                    ?
         ? ErrorMessage                   ?
         ? CreatedAt                      ?
         ? UpdatedAt                      ?
         ???????????????????????????????????

         ???????????????????????????????????
         ?           Playlist              ?
         ???????????????????????????????????
         ? Id (PK)                        ?
         ? Name                           ?
         ? Description                    ?
         ? CreatedAt                      ?
         ? UpdatedAt                      ?
         ???????????????????????????????????
```

## Tables

### Artist

Stores information about music artists and performers.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | int | PRIMARY KEY, IDENTITY | Unique identifier |
| Name | nvarchar(255) | NOT NULL | Artist name |
| SortName | nvarchar(255) | NULL | Name for sorting purposes |
| Biography | ntext | NULL | Artist biography |
| CreatedAt | datetime2 | NOT NULL, DEFAULT GETUTCDATE() | Record creation timestamp |
| UpdatedAt | datetime2 | NOT NULL, DEFAULT GETUTCDATE() | Last update timestamp |

**Indexes:**
- `IX_Artist_Name` - Index on Name for fast lookups
- `IX_Artist_SortName` - Index on SortName for sorting

### Album

Stores album information.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | int | PRIMARY KEY, IDENTITY | Unique identifier |
| Title | nvarchar(255) | NOT NULL | Album title |
| SortTitle | nvarchar(255) | NULL | Title for sorting purposes |
| Year | int | NULL | Release year |
| ArtistId | int | FOREIGN KEY REFERENCES Artist(Id) | Primary artist |
| GenreId | int | FOREIGN KEY REFERENCES Genre(Id) | Primary genre |
| CreatedAt | datetime2 | NOT NULL | Record creation timestamp |
| UpdatedAt | datetime2 | NOT NULL | Last update timestamp |

**Indexes:**
- `IX_Album_Title` - Index on Title
- `IX_Album_ArtistId` - Index on ArtistId
- `IX_Album_Year` - Index on Year

### Genre

Stores music genre information.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | int | PRIMARY KEY, IDENTITY | Unique identifier |
| Name | nvarchar(100) | NOT NULL, UNIQUE | Genre name |
| Description | ntext | NULL | Genre description |
| CreatedAt | datetime2 | NOT NULL | Record creation timestamp |
| UpdatedAt | datetime2 | NOT NULL | Last update timestamp |

**Indexes:**
- `UQ_Genre_Name` - Unique index on Name

### Track

Core entity storing individual music track information.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | int | PRIMARY KEY, IDENTITY | Unique identifier |
| Title | nvarchar(255) | NOT NULL | Track title |
| SortTitle | nvarchar(255) | NULL | Title for sorting |
| FilePath | nvarchar(500) | NOT NULL, UNIQUE | Full file path |
| FileFormat | nvarchar(50) | NOT NULL | File format (MP3, FLAC, etc.) |
| FileSize | bigint | NOT NULL | File size in bytes |
| FileHash | nvarchar(32) | NULL | MD5 hash for duplicate detection |
| TrackNumber | int | NULL | Track number on album |
| DiscNumber | int | NULL | Disc number for multi-disc albums |
| Duration | time | NULL | Track duration |
| Year | int | NULL | Release year |
| BitRate | int | NULL | Audio bitrate in kbps |
| SampleRate | int | NULL | Sample rate in Hz |
| ArtistId | int | FOREIGN KEY REFERENCES Artist(Id) | Track artist |
| AlbumId | int | FOREIGN KEY REFERENCES Album(Id) | Album reference |
| GenreId | int | FOREIGN KEY REFERENCES Genre(Id) | Track genre |
| CreatedAt | datetime2 | NOT NULL | Record creation timestamp |
| UpdatedAt | datetime2 | NOT NULL | Last update timestamp |

**Indexes:**
- `UQ_Track_FilePath` - Unique index on FilePath
- `IX_Track_FileHash` - Index on FileHash for duplicate detection
- `IX_Track_ArtistId` - Index on ArtistId
- `IX_Track_AlbumId` - Index on AlbumId
- `IX_Track_Title` - Index on Title for searching

### Metadata

Stores additional key-value metadata for tracks.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | int | PRIMARY KEY, IDENTITY | Unique identifier |
| TrackId | int | FOREIGN KEY REFERENCES Track(Id) ON DELETE CASCADE | Associated track |
| Key | nvarchar(100) | NOT NULL | Metadata key |
| Value | ntext | NULL | Metadata value |
| CreatedAt | datetime2 | NOT NULL | Record creation timestamp |

**Indexes:**
- `IX_Metadata_TrackId` - Index on TrackId
- `IX_Metadata_Key` - Index on Key for fast lookups

### ScanJob

Tracks scanning operations for audit and progress monitoring.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | int | PRIMARY KEY, IDENTITY | Unique identifier |
| DirectoryPath | nvarchar(500) | NOT NULL | Scanned directory path |
| Status | nvarchar(50) | NOT NULL | Job status (Pending, Running, Completed, Failed, Cancelled) |
| StartTime | datetime2 | NOT NULL | Job start time |
| EndTime | datetime2 | NULL | Job completion time |
| FilesScanned | int | NOT NULL, DEFAULT 0 | Number of files processed |
| FilesAdded | int | NOT NULL, DEFAULT 0 | Number of new files added |
| DuplicatesFound | int | NOT NULL, DEFAULT 0 | Number of duplicates detected |
| ErrorsCount | int | NOT NULL, DEFAULT 0 | Number of errors encountered |
| ErrorMessage | ntext | NULL | Detailed error information |
| CreatedAt | datetime2 | NOT NULL | Record creation timestamp |
| UpdatedAt | datetime2 | NOT NULL | Last update timestamp |

**Indexes:**
- `IX_ScanJob_Status` - Index on Status
- `IX_ScanJob_StartTime` - Index on StartTime

### Playlist

Stores user-created playlists (future feature).

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | int | PRIMARY KEY, IDENTITY | Unique identifier |
| Name | nvarchar(255) | NOT NULL | Playlist name |
| Description | ntext | NULL | Playlist description |
| CreatedAt | datetime2 | NOT NULL | Record creation timestamp |
| UpdatedAt | datetime2 | NOT NULL | Last update timestamp |

## Relationships

### One-to-Many Relationships

1. **Artist ? Album**: One artist can have multiple albums
2. **Artist ? Track**: One artist can have multiple tracks
3. **Album ? Track**: One album can contain multiple tracks
4. **Genre ? Album**: One genre can categorize multiple albums
5. **Genre ? Track**: One genre can categorize multiple tracks
6. **Track ? Metadata**: One track can have multiple metadata entries

### Foreign Key Constraints

All foreign keys are configured with:
- `ON UPDATE NO ACTION`
- `ON DELETE NO ACTION` (except Metadata ? Track which uses CASCADE)

This prevents accidental deletion of referenced entities while allowing explicit cleanup operations.

## Data Types and Constraints

### String Fields

- **Short strings** (? 255 chars): `nvarchar(255)`
- **Medium strings** (? 500 chars): `nvarchar(500)`
- **Long text**: `ntext`

### Numeric Fields

- **Identifiers**: `int` with IDENTITY
- **Large numbers** (file sizes): `bigint`
- **Time durations**: `time` type

### Timestamps

- All entities include `CreatedAt` and `UpdatedAt` fields
- Uses `datetime2` for precision
- Defaults to `GETUTCDATE()` for creation

## Performance Considerations

### Indexing Strategy

1. **Primary Keys**: Clustered indexes on Id columns
2. **Foreign Keys**: Non-clustered indexes on all FK columns
3. **Search Fields**: Indexes on Name, Title fields
4. **Unique Constraints**: Unique indexes on FilePath, FileHash

### Query Optimization

1. **File Path Lookups**: Unique index on Track.FilePath for O(1) duplicate detection
2. **Hash Lookups**: Index on FileHash for fast duplicate detection
3. **Artist/Album Browsing**: Indexes on relationship fields
4. **Search Operations**: Full-text search consideration for future implementation

### Storage Optimization

1. **File Paths**: Consider relative paths to reduce storage
2. **Metadata**: Separate table to avoid sparse columns in Track
3. **Binary Data**: File hash stored as string for simplicity

## Migration Scripts

### Initial Migration

```sql
-- Create Artists table
CREATE TABLE Artists (
    Id int IDENTITY(1,1) PRIMARY KEY,
    Name nvarchar(255) NOT NULL,
    SortName nvarchar(255) NULL,
    Biography ntext NULL,
    CreatedAt datetime2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt datetime2 NOT NULL DEFAULT GETUTCDATE()
);

-- Create Genres table
CREATE TABLE Genres (
    Id int IDENTITY(1,1) PRIMARY KEY,
    Name nvarchar(100) NOT NULL UNIQUE,
    Description ntext NULL,
    CreatedAt datetime2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt datetime2 NOT NULL DEFAULT GETUTCDATE()
);

-- Additional tables follow similar pattern...
```

## Backup and Maintenance

### Backup Strategy

1. **Full Backup**: Weekly full database backup
2. **Incremental**: Daily transaction log backups
3. **Point-in-time Recovery**: Maintain 30-day recovery window

### Maintenance Tasks

1. **Index Maintenance**: Weekly index reorganization
2. **Statistics Update**: Automatic statistics updates enabled
3. **Cleanup**: Archive old scan jobs after 90 days

## Security Considerations

### Data Protection

1. **File Paths**: Consider encryption for sensitive directory information
2. **Access Control**: Implement row-level security if multi-tenant
3. **Audit Trail**: Maintain change tracking for critical entities

### Performance Monitoring

1. **Query Performance**: Monitor slow queries on Track table
2. **Index Usage**: Regular analysis of index effectiveness
3. **Storage Growth**: Monitor file metadata storage growth