# Music Catalog Service

A .NET-based service for managing music metadata from Spotify, with caching and local storage capabilities.

## Table of Contents
- [Service Overview](#service-overview)
    - [Architecture](#architecture)
- [Technology Overview](#technology-overview)
    - [Data Storage Strategy](#data-storage-strategy)
    - [Caching Strategy](#caching-strategy)
    - [Trade-offs and Considerations](#trade-offs-and-considerations)
- [API Documentation](#api-documentation)
    - [Albums](#albums)
    - [Tracks](#tracks)
    - [Search](#search)
    - [Error Handling](#error-handling)
- [Development Guidelines](#development-guidelines)
    - [Prerequisites](#prerequisites)
    - [Setup](#setup)
    - [Running Locally](#running-locally)
    - [Development Best Practices](#development-best-practices)
- [Monitoring Considerations](#monitoring-considerations)
- [License](#license)

## Service Overview

The Music Catalog Service is designed to provide a reliable and efficient API for music-related metadata. It fetches data from the Spotify API and implements strategic caching and storage mechanisms to optimize performance and reduce external API dependencies.

### Architecture

The service follows a clean, three-layered architecture:

1. **API Layer** (`MusicCatalogService.API`):
    - Handles HTTP requests and responses
    - Provides RESTful endpoints for client applications
    - Manages input validation and error handling

2. **Core Layer** (`MusicCatalogService.Core`):
    - Contains business logic and domain models
    - Defines interfaces and DTOs
    - Houses service implementations

3. **Infrastructure Layer** (`MusicCatalogService.Infrastructure`):
    - Implements data access and external integrations
    - Manages caching and database repositories
    - Handles communication with Spotify API

This separation of concerns ensures the codebase remains maintainable and testable, while allowing each layer to evolve independently.

## Technology Overview

### Data Storage Strategy

The service employs a hybrid storage approach, utilizing both Redis and MongoDB:

#### Redis (Distributed Cache)
- Used for high-speed, in-memory caching of frequently accessed data
- Reduces latency for common queries
- Mitigates rate limiting from the Spotify API
- Configured with appropriate TTL (Time-To-Live) values to ensure data freshness

#### MongoDB (Persistent Storage)
- Provides durable, document-based storage
- Stores music metadata with flexible schema capabilities
- Maintains catalog items with extended expiration dates
- Supports querying by both Spotify IDs and internal catalog IDs

### Caching Strategy

The service implements a "Cache-Aside" (or "Lazy Loading") pattern:

1. When a request for music data arrives, the system first checks Redis
2. If not in Redis, it checks MongoDB for a valid cached entry
3. If not in MongoDB (or expired), it fetches from Spotify API
4. The new data is then stored in both MongoDB and Redis for future requests

This multi-level caching strategy is particularly effective for:
- Read-heavy operations (common in catalog services)
- Reducing the number of requests to external APIs
- Improving overall system performance and reliability

### Trade-offs and Considerations

Using cache as a primary lookup mechanism provides performance benefits but introduces some challenges:

- **Redis offers excellent performance but limited durability** compared to MongoDB
- The system becomes more vulnerable to cache failures or evictions
- Cache hit rates monitoring becomes critical to system performance

This hybrid approach balances these concerns by:
1. Keeping frequently accessed items hot in cache
2. Maintaining persistent copies in MongoDB
3. Implementing graceful degradation if cache fails
4. Using intelligent expiration policies to ensure data freshness

## API Documentation

The service exposes a RESTful API with the following main endpoints:

### Albums

- **GET** `/api/v1/catalog/albums/{catalogId}` - Get album by internal catalog ID
    - Returns: 200 OK (success), 404 Not Found, 500 Internal Server Error

- **GET** `/api/v1/catalog/albums/spotify/{spotifyId}` - Get album by Spotify ID
    - Returns: 200 OK (success), 404 Not Found, 500 Internal Server Error

- **POST** `/api/v1/catalog/albums` - Save album permanently
    - Accepts: `{ "spotifyId": "string" }`
    - Returns: 201 Created (success), 400 Bad Request, 404 Not Found, 500 Internal Server Error

### Tracks

- **GET** `/api/v1/catalog/tracks/{catalogId}` - Get track by internal catalog ID
    - Returns: 200 OK (success), 404 Not Found, 500 Internal Server Error

- **GET** `/api/v1/catalog/tracks/spotify/{spotifyId}` - Get track by Spotify ID
    - Returns: 200 OK (success), 404 Not Found, 500 Internal Server Error

- **POST** `/api/v1/catalog/tracks` - Save track permanently
    - Accepts: `{ "spotifyId": "string" }`
    - Returns: 201 Created (success), 400 Bad Request, 404 Not Found, 500 Internal Server Error

### Search

- **GET** `/api/v1/catalog/search` - Search for music content
    - Query parameters:
        - `q` (search query, required)
        - `type` (comma-separated list of: album, artist, track, required)
        - `limit` (max results per type, default 20)
        - `offset` (pagination offset, default 0)
        - `market` (optional market code)
    - Returns: 200 OK (success), 400 Bad Request, 429 Too Many Requests, 500 Internal Server Error

Full API documentation is available via Swagger at the `/swagger` endpoint when running the service in development mode.

## Error Handling

The API uses standardized error responses with the following structure:

```json
{
  "message": "Human-readable error message",
  "errorCode": "MACHINE_READABLE_ERROR_CODE",
  "traceId": "Request-specific trace identifier",
  "details": {
    // Optional additional error details
  }
}
```

Common error codes include:
- `RESOURCE_NOT_FOUND` - The requested resource doesn't exist
- `SPOTIFY_API_ERROR` - Error communicating with Spotify
- `VALIDATION_ERROR` - Invalid request parameters
- `RATE_LIMIT_EXCEEDED` - Too many requests in a short time
- `INTERNAL_SERVER_ERROR` - Unexpected server error

## Development Guidelines

### Prerequisites

- .NET 8 SDK
- Docker and Docker Compose
- Spotify Developer account for API credentials

### Setup

1. Clone the repository
2. Create a `.env` file in the project root with the following content:

```
# MongoDB Configuration
MONGO_USER=catalog_user
MONGO_PASSWORD=Strong_Password123
MONGO_DATABASE=MusicCatalog
MONGO_PORT=27017

# Redis configuration
REDIS_PORT=6379
REDIS_CONNECTION=music-catalog-redis:6379,abortConnect=false

# Spotify API credentials
SPOTIFY_CLIENT_ID=<spotify_api_client_id>
SPOTIFY_CLIENT_SECRET=<spotify_api_client_secret>
```

3. Replace `<spotify_api_client_id>` and `<spotify_api_client_secret>` with your Spotify API credentials

### Running Locally

Start the solution using Docker Compose:

```bash
docker-compose up -d
```

This will:
- Start MongoDB container
- Start Redis container
- Build and start the Music Catalog Service

The service will be available at `http://localhost:5010`
