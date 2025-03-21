# Music Interaction Service

This service is part of a larger Music Evaluation Platform that allows users to rate, review, and interact with music content (songs, albums, artists). The Music Interaction Service specifically handles user interactions such as liking content, writing reviews, and rating music using both simple and complex grading methods.

## Architecture Overview

The application follows a Clean Architecture pattern with Domain-Driven Design principles:

### Domain Layer
- Core business entities and logic
- Independent of external frameworks and services
- Contains entities such as `InteractionsAggregate`, `Rating`, `Review`, `Like`, etc.
- Includes the complex grading system with `Grade`, `GradingBlock`, and `GradingMethod` entities

### Application Layer
- Implements use cases as command/query handlers using MediatR
- Defines commands, queries, DTOs, and interfaces
- Orchestrates domain object interactions
- Examples: `PostInteractionUseCase`, `GetRatingsUseCase`, etc.

### Infrastructure Layer
- Implements the persistence using PostgreSQL and MongoDB
- PostgreSQL stores interactions (ratings, reviews, likes)
- MongoDB stores grading method templates
- Both implementations use their respective mappers to convert between domain and database entities

### Presentation Layer
- API Controllers that process HTTP requests
- Handles validation and converts requests to appropriate commands/queries

## Database Structure

### PostgreSQL (Interactions Storage)
- `Interactions`: Core entity that ties together ratings, reviews, and likes
- `Likes`: Stores user likes for content
- `Reviews`: Stores user reviews with detailed text
- `Ratings`: Stores user ratings, connecting to either simple or complex grading components
- Multiple tables for grading components and their relationships

### MongoDB (Grading Methods Storage)
- Stores reusable grading method templates that users can create and share
- Allows for complex, hierarchical grading structures

## API Endpoints

### Grading Method Endpoints

- **Create Grading Method**: `POST /api/GradingMethod/create`
- **Get Public Grading Methods**: `GET /api/GradingMethod/public`
- **Get Grading Method by ID**: `GET /api/GradingMethod/{id}`

### Interaction Endpoints

- **Post Interaction**: `POST /postInteraction`
- **Get All Interactions**: `GET /getInteractions`
- **Get All Likes**: `GET /getLikes`
- **Get All Reviews**: `GET /getReviews`
- **Get All Ratings**: `GET /getRatings`
- **Get Rating by ID**: `GET /getRating/{id}`

## Grading System

The service implements a flexible grading system that allows users to:

1. **Create custom grading methods** with:
    - Simple grades with min/max values and step amounts
    - Nested blocks of grades (categories and subcategories)
    - Mathematical operations between grades (add, subtract, multiply, divide)

2. **Use grading methods to rate content** with:
    - Simple 1-10 ratings
    - Complex hierarchical ratings that evaluate different aspects

3. **Normalize and calculate overall grades** automatically

## Setup and Deployment

The service is containerized using Docker and can be run with Docker Compose. The configuration includes:
- API container
- PostgreSQL database container
- MongoDB container

To start the service, run the following command in the root directory:

```bash
docker compose up --build -d
```

This will build the containers if needed and start them in detached mode.

## Usage Examples

### Creating a Simple Grading Method

```http
POST /api/GradingMethod/create
Content-Type: application/json

{
  "name": "Album Rating System",
  "userId": "user123",
  "isPublic": true,
  "components": [
    {
      "componentType": "grade",
      "name": "Lyrics",
      "minGrade": 1,
      "maxGrade": 10,
      "stepAmount": 0.5,
      "description": "Quality of the song lyrics, themes, and poetic elements"
    },
    {
      "componentType": "block",
      "name": "Production",
      "subComponents": [
        {
          "componentType": "grade",
          "name": "Mixing",
          "minGrade": 1,
          "maxGrade": 10,
          "stepAmount": 0.5,
          "description": "Quality of instrument balance, clarity, and spatial positioning"
        },
        {
          "componentType": "grade",
          "name": "Mastering",
          "minGrade": 1,
          "maxGrade": 10,
          "stepAmount": 0.5,
          "description": "Overall sound quality, loudness, dynamics, and final polish"
        }
      ],
      "actions": [0]
    }
  ],
  "actions": [0]
}
```

Response:
```json
{
  "success": true,
  "gradingMethodId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "errorMessage": null
}
```

### Posting an Interaction with a Complex Rating

```http
POST /postInteraction
Content-Type: application/json

{
  "userId": "user123",
  "itemId": "album456",
  "itemType": "Album",
  "isLiked": true,
  "reviewText": "This album has excellent lyrics, though the production could be better. The mixing is a bit muddy but the mastering is well done.",
  "useComplexGrading": true,
  "gradingMethodId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "gradeInputs": [
    {
      "componentName": "Lyrics",
      "value": 9.5
    },
    {
      "componentName": "Production.Mixing",
      "value": 6.5
    },
    {
      "componentName": "Production.Mastering",
      "value": 8.0
    }
  ]
}
```

Response:
```json
{
  "interactionCreated": true,
  "liked": true,
  "reviewCreated": true,
  "graded": true,
  "interactionId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "errorMessage": null
}
```

### Creating a Grading Method with Nested Blocks

```http
POST /api/GradingMethod/create
Content-Type: application/json

{
  "name": "Comprehensive Album Review System",
  "userId": "music_critic_101",
  "isPublic": true,
  "components": [
    {
      "componentType": "block",
      "name": "Technical Execution",
      "subComponents": [
        {
          "componentType": "grade",
          "name": "Instrumental Proficiency",
          "minGrade": 1,
          "maxGrade": 10,
          "stepAmount": 0.5,
          "description": "Technical skill displayed by the musicians"
        },
        {
          "componentType": "block",
          "name": "Audio Engineering",
          "subComponents": [
            {
              "componentType": "grade",
              "name": "Recording Quality",
              "minGrade": 1,
              "maxGrade": 10,
              "stepAmount": 0.5,
              "description": "Clarity and quality of the audio recording"
            },
            {
              "componentType": "grade",
              "name": "Mixing Balance",
              "minGrade": 1,
              "maxGrade": 10,
              "stepAmount": 0.5,
              "description": "Balance between instruments and vocals"
            },
            {
              "componentType": "grade",
              "name": "Mastering Quality",
              "minGrade": 1,
              "maxGrade": 10,
              "stepAmount": 0.5,
              "description": "Overall polish and sound cohesion"
            }
          ],
          "actions": [0, 0]
        }
      ],
      "actions": [0]
    },
    {
      "componentType": "block",
      "name": "Artistic Merit",
      "subComponents": [
        {
          "componentType": "grade",
          "name": "Composition",
          "minGrade": 1,
          "maxGrade": 10,
          "stepAmount": 0.5,
          "description": "Quality of musical composition and arrangement"
        },
        {
          "componentType": "grade",
          "name": "Lyrics",
          "minGrade": 1,
          "maxGrade": 10,
          "stepAmount": 0.5,
          "description": "Quality of lyrical content and themes"
        },
        {
          "componentType": "grade",
          "name": "Originality",
          "minGrade": 1,
          "maxGrade": 10,
          "stepAmount": 0.5,
          "description": "Uniqueness and innovation in sound and approach"
        }
      ],
      "actions": [0, 0]
    }
  ],
  "actions": [0]
}
```

### Posting an Interaction with a Complex Nested Rating

```http
POST /postInteraction
Content-Type: application/json

{
  "userId": "music_critic_101",
  "itemId": "album789",
  "itemType": "Album",
  "isLiked": true,
  "reviewText": "This album showcases exceptional instrumental skill, though the audio engineering is somewhat uneven. The recording quality is pristine, but the mixing could use some work as certain elements get buried. The mastering is solid overall. From an artistic perspective, the compositions are innovative and the lyrics are thought-provoking, resulting in a highly original work despite some technical shortcomings.",
  "useComplexGrading": true,
  "gradingMethodId": "3fa85f64-5717-4562-b3fc-2c963f66afa7",
  "gradeInputs": [
    {
      "componentName": "Technical Execution.Instrumental Proficiency",
      "value": 9.0
    },
    {
      "componentName": "Technical Execution.Audio Engineering.Recording Quality",
      "value": 9.5
    },
    {
      "componentName": "Technical Execution.Audio Engineering.Mixing Balance",
      "value": 6.5
    },
    {
      "componentName": "Technical Execution.Audio Engineering.Mastering Quality",
      "value": 8.0
    },
    {
      "componentName": "Artistic Merit.Composition",
      "value": 9.0
    },
    {
      "componentName": "Artistic Merit.Lyrics",
      "value": 8.5
    },
    {
      "componentName": "Artistic Merit.Originality",
      "value": 9.5
    }
  ]
}
```

## Understanding the Domain Model

### Core Entities

1. **InteractionsAggregate**
    - Root entity that manages user interactions with musical content
    - Contains likes, ratings, and reviews related to a specific item

2. **Rating**
    - Contains evaluation information using either a simple or complex grading component
    - Connected to a specific interaction and item

3. **Review**
    - Contains the textual review left by a user
    - Connected to a specific interaction and item

4. **Like**
    - Simple entity that represents a user liking a specific item

### Grading Components

1. **IGradable Interface**
    - Base interface for all gradable components
    - Provides methods for getting grades, min/max values, and normalized grades

2. **Grade**
    - Simple gradable component with a min, max, and step amount
    - Used for basic rating scenarios

3. **GradingBlock**
    - Complex gradable component that contains other gradable components
    - Can apply mathematical operations between components

4. **GradingMethod**
    - Top-level grading component that can be shared between users
    - Defines a complete rating approach for specific content types

### Actions

The `Action` enum defines mathematical operations that can be applied between grading components:
- Add (0)
- Subtract (1)
- Multiply (2)
- Divide (3)

These operations determine how grades are calculated within blocks and methods.

## Additional Notes

- The service uses the MediatR library for implementing CQRS (Command Query Responsibility Segregation)
- Data persistence is managed through MongoDB for grading methods and PostgreSQL for interactions
- Docker is configured for easy deployment and development
- The application is designed to work with other microservices in the broader Music Evaluation Platform