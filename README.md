# ImageDrop

> A lightweight image-sharing platform inspired by [Imgur](https://imgur.com/), built with .NET and AWS-compatible infrastructure.

ImageDrop is a backend service for uploading, storing, processing, and sharing images.

The system allows users to:

* create an account;
* authenticate using JWT access and refresh tokens;
* upload images;
* retrieve their own uploaded images;
* retrieve individual images using their public image ID;
* delete their own images.

Images are stored in **Amazon S3**, asynchronous image processing is handled through **Amazon SQS**, authentication and rate limiting use **Redis**, and application data is persisted in a relational database.

The system is split into several independent applications and supporting libraries, with background processing delegated to dedicated workers.

---

## ✨ Features

* 🔐 JWT-based authentication with access and refresh tokens
* 👤 User registration
* 🖼️ Image upload and retrieval
* 🗑️ Image deletion
* ⚡ Asynchronous image processing
* 📦 Amazon S3 object storage
* 📨 Amazon SQS FIFO message broker
* 🧹 Automatic cleanup of orphaned images
* 🚦 Redis-based IP rate limiting
* 🆔 UUIDv7-based image identifiers
* 🐳 Docker support for all executable applications
* 🗄️ Dedicated database migration application
* 🧪 Unit test projects for core components
* ☁️ AWS-compatible infrastructure with local development support

---

# 🏗️ Architecture

ImageDrop follows a modular architecture where the HTTP API is responsible for client interaction, while potentially long-running and scheduled operations are delegated to background workers.

At a high level, the system looks like this:

```text
                         ┌──────────────────┐
                         │      Client      │
                         └────────┬─────────┘
                                  │
                                  │ HTTP
                                  ▼
                         ┌──────────────────┐
                         │  ImageDrop.Api   │
                         └───────┬───┬──────┘
                                 │   │
                    ┌────────────┘   └─────────────┐
                    │                              │
                    ▼                              ▼
             ┌──────────────┐               ┌──────────────┐
             │  PostgreSQL  │               │    Redis     │
             │   Database   │               │ Auth / Rate  │
             └──────────────┘               │    Limit     │
                                            └──────────────┘
                    │
                    │ Image metadata
                    │
                    ▼
             ┌──────────────┐
             │   Amazon S3  │
             │    Images    │
             └──────┬───────┘
                    │
                    │
             ┌──────▼───────┐
             │  Amazon SQS  │
             │  FIFO Queue  │
             └──────┬───────┘
                    │
                    │ ImageCreated
                    ▼
          ┌─────────────────────────┐
          │ ImageProcessing.Worker  │
          └────────────┬────────────┘
                       │
                       ▼
                  ┌─────────┐
                  │   S3    │
                  │Processed│
                  │ Images  │
                  └─────────┘


          ┌─────────────────────────┐
          │   ImageCleanup.Worker   │
          └────────────┬────────────┘
                       │
                       ▼
              Find orphaned images
                       │
                       ▼
                  Delete from
                  database/S3
```

---

# 🧩 Solution Structure

The solution is divided into application projects, infrastructure integrations, shared libraries, and test projects.

```text
ImageDrop/
│
├── src/
│   ├── ImageDrop.Api
│   ├── ImageDrop.Application
│   │
│   ├── ImageDrop.AWS.S3
│   ├── ImageDrop.AWS.SecretsManager
│   ├── ImageDrop.AWS.SQS.Consumer
│   ├── ImageDrop.AWS.SQS.Publisher
│   │
│   ├── ImageDrop.ImageCleanup.Worker
│   ├── ImageDrop.ImageProcessing.Worker
│   │
│   ├── ImageDrop.Migrator
│   ├── ImageDrop.Persistence
│   │
│   ├── ImageDrop.Shared.Constants
│   ├── ImageDrop.Shared.Services
│   └── ImageDrop.Shared.SQS.Contracts
│
└── Tests/
    ├── ImageDrop.Api.Tests.Unit
    ├── ImageDrop.Application.Tests.Unit
    ├── ImageDrop.AWS.S3.Tests.Unit
    ├── ImageDrop.AWS.SQS.Consumer.Tests.Unit
    └── ImageDrop.Shared.Services.Tests.Unit
```

---

# 📦 Projects

## `ImageDrop.Api`

The main HTTP API responsible for communication with clients.

It exposes endpoints for:

* authentication;
* user creation;
* image upload;
* image deletion;
* retrieving the authenticated user's images;
* retrieving a specific image.

The API coordinates application services but delegates infrastructure responsibilities such as S3 access, SQS publishing, Redis, and persistence to their respective abstractions and implementations.

This is one of the executable applications and has its own Dockerfile.

---

## `ImageDrop.Application`

Contains the application's core business logic and use cases.

This layer is responsible for coordinating application operations without being tightly coupled to the transport layer.

Examples of application operations include:

* authentication;
* user creation;
* image creation;
* image deletion;
* image retrieval;
* user image listing.

The project is intentionally separated from the API so that business logic can be tested independently from HTTP infrastructure.

---

## `ImageDrop.AWS.S3`

Provides integration with Amazon S3.

S3 is used as the persistent object storage for uploaded images.

The application does not need to deal directly with S3 SDK details. S3-specific functionality is isolated inside this project.

Typical responsibilities include:

* uploading images;
* retrieving images;
* deleting images;
* generating or resolving object locations.

---

## `ImageDrop.AWS.SecretsManager`

Provides integration with AWS Secrets Manager.

Configuration secrets are retrieved from AWS Secrets Manager instead of being hardcoded into the application.

The expected secret naming convention is:

```text
{Environment}_image_drop_secrets
```

For example:

```text
Development_image_drop_secrets
Production_image_drop_secrets
```

---

## `ImageDrop.AWS.SQS.Consumer`

Contains the SQS consumer implementation.

It is responsible for receiving messages from the ImageDrop FIFO queue and passing them to the appropriate processing logic.

The consumer is primarily used by the image processing worker.

The consumer also reads the SQS `SentTimestamp` system attribute, allowing message metadata such as the original message timestamp to be available during processing.

---

## `ImageDrop.AWS.SQS.Publisher`

Contains the SQS publisher implementation.

When a new image is created, the API publishes a message to the ImageDrop SQS FIFO queue.

The message contains information required by the processing worker to locate and process the image.

The publisher is responsible for:

* serializing the message;
* publishing it to SQS;
* setting message attributes;
* configuring FIFO-specific message properties.

---

## `ImageDrop.ImageCleanup.Worker`

A background worker responsible for cleaning up images that are no longer associated with a user.

An image can exist temporarily without an owner, for example when an image upload has been started but the corresponding user relationship was not established successfully.

To prevent these objects from accumulating indefinitely, the cleanup worker periodically searches for old unowned images.

The retention period is configurable.

Default configuration:

```text
Unowned image retention: 7 days
Cleanup interval:        6 hours
Maximum S3 delete batch: 1000 objects
```

The cleanup worker is one of the executable applications and has its own Dockerfile.

### Cleanup Flow

```text
             ┌──────────────────────┐
             │ ImageCleanup.Worker   │
             └──────────┬───────────┘
                        │
                        ▼
              Find unowned images
                        │
                        ▼
              Check retention time
                        │
                 older than limit?
                    /        \
                  No          Yes
                  │            │
                  ▼            ▼
                Skip       Delete objects
                               │
                               ▼
                          Remove metadata
```

The worker runs periodically according to `ImageCleanupOptions.IntervalInHours`.

---

## `ImageDrop.ImageProcessing.Worker`

A background worker responsible for processing newly uploaded images.

Image processing is intentionally performed asynchronously so that the API does not need to keep the HTTP request open while the image is being processed.

The worker consumes image creation messages from Amazon SQS and processes the corresponding image.

Image compression settings are configurable.

Default settings include:

```text
JPEG quality:             75
PNG compression level:    9
```

The processing worker is one of the executable applications and has its own Dockerfile.

### Image Processing Flow

```text
Client
  │
  │ POST /api/images
  ▼
ImageDrop.Api
  │
  ├──────────────► Database
  │                  │
  │                  └── Image metadata
  │
  ├──────────────► S3
  │                  │
  │                  └── Uploaded image
  │
  └──────────────► SQS
                     │
                     │ ImageCreated
                     ▼
             ImageProcessing.Worker
                     │
                     ▼
              Process image
                     │
                     ▼
                    S3
```

---

## `ImageDrop.Migrator`

A dedicated application responsible for applying Entity Framework Core database migrations.

Separating migrations from the API and workers allows database schema management to be handled independently from application startup.

The migrator is an executable application and has its own Dockerfile.

---

## `ImageDrop.Persistence`

Contains database-related functionality.

This project is responsible for:

* Entity Framework Core configuration;
* database context;
* entity mappings;
* persistence-related abstractions;
* database access.

Database schema changes are applied through `ImageDrop.Migrator`.

---

## `ImageDrop.Shared.Constants`

Contains constants shared across multiple projects.

Keeping shared constants in a dedicated project prevents duplication and keeps common values consistent across the solution.

---

## `ImageDrop.Shared.Services`

Contains services that are shared between multiple application components.

This project is used for functionality that does not belong exclusively to the API, a worker, or a specific infrastructure integration.

---

## `ImageDrop.Shared.SQS.Contracts`

Contains message contracts shared between SQS publishers and consumers.

This prevents the publisher and consumer from maintaining separate definitions of the same message structures.

For example, the image processing flow relies on a shared contract representing the creation of an image.

```text
ImageDrop.Api
      │
      │ publishes
      ▼
SQS Publisher
      │
      │ ImageDrop.Shared.SQS.Contracts
      ▼
     SQS
      │
      ▼
SQS Consumer
      │
      ▼
ImageProcessing.Worker
```

---

# 🖼️ Image Lifecycle

An image passes through several stages during its lifetime.

```text
                 ┌───────────────┐
                 │ Upload Image  │
                 └───────┬───────┘
                         │
                         ▼
                 ┌───────────────┐
                 │ Store in S3   │
                 └───────┬───────┘
                         │
                         ▼
                 ┌───────────────┐
                 │ Store metadata│
                 │   in database │
                 └───────┬───────┘
                         │
                         ▼
                 ┌───────────────┐
                 │ Publish SQS   │
                 │    message    │
                 └───────┬───────┘
                         │
                         ▼
                 ┌───────────────┐
                 │ Processing    │
                 │    Worker     │
                 └───────┬───────┘
                         │
                         ▼
                 ┌───────────────┐
                 │ Process image │
                 └───────┬───────┘
                         │
                         ▼
                 ┌───────────────┐
                 │ Final image   │
                 │     in S3     │
                 └───────────────┘
```

Images are identified using **UUIDv7**.

---

# 🆔 Image IDs — UUIDv7

Image identifiers are generated using **UUID version 7**.

UUIDv7 provides globally unique identifiers while also embedding a time component into the identifier.

This makes UUIDv7 particularly useful for entities such as images because identifiers retain a time-ordered characteristic while remaining globally unique.

Instead of relying on sequential integer IDs, ImageDrop can expose opaque, globally unique image identifiers without requiring a centralized ID-generation mechanism.

---

# 🔐 Authentication

ImageDrop uses JWT-based authentication.

The authentication flow uses:

* access tokens;
* refresh tokens;
* configurable token expiration periods;
* issuer and audience validation;
* Redis-backed infrastructure where required by the authentication/session implementation.

The configured defaults are:

```text
Access token:          1 day
Refresh token:         2 days
Reset password token:  1 day
```

Clients use the access token to authenticate protected API requests.

Protected endpoints require the token to be supplied with the request.

Example:

```http
Authorization: Bearer <access_token>
```

---

# 🚦 Rate Limiting

Redis is used for API rate limiting.

The default IP-based rate limit is:

```text
Requests allowed: 20
Window:           1 minute
```

Configuration:

```json
"RateLimitingOptions": {
  "IpPermitLimit": 20,
  "IpWindowMinutes": 1
}
```

This protects the API from excessive requests from a single IP address.

---

# 🔌 API

The API exposes endpoints for authentication, user management, and image operations.

## Authentication

### Login

```http
POST /api/auth/login
```

Authenticates a user and returns authentication tokens.

Authentication required:

```text
No
```

---

### Refresh Token

```http
POST /api/auth/refresh
```

Creates a new access token using a valid refresh token.

Authentication required:

```text
No
```

---

# 👤 Users

### Create User

```http
POST /api/users/create
```

Creates a new ImageDrop user.

Authentication required:

```text
No
```

---

# 🖼️ Images

## Upload Image

```http
POST /api/images
```

Uploads a new image.

Authentication required:

```text
Yes
```

The image upload starts the image lifecycle:

```text
Client
  │
  ▼
POST /api/images
  │
  ├──► Database
  │
  ├──► S3
  │
  └──► SQS
          │
          ▼
    Processing Worker
```

Image processing is performed asynchronously.

---

## Delete Image

```http
DELETE /api/images/{imageId}
```

Deletes an image owned by the authenticated user.

Authentication required:

```text
Yes
```

The `imageId` is the UUIDv7 identifier assigned to the image.

---

## Get User Images

```http
GET /api/images
```

Returns the images belonging to the currently authenticated user.

Authentication required:

```text
Yes
```

The user identity is obtained from the authentication token rather than being supplied as a route parameter.

This prevents users from requesting another user's private image collection through this endpoint.

---

## Get Image

```http
GET /api/images/{imageId}
```

Retrieves an individual image by its public image identifier.

Authentication required:

```text
No
```

Any user can request an image when its `imageId` is known.

---

# 📋 API Overview

| Method   | Endpoint                | Authentication | Description                     |
| -------- | ----------------------- | -------------: | ------------------------------- |
| `POST`   | `/api/auth/login`       |              ❌ | Authenticate user               |
| `POST`   | `/api/auth/refresh`     |              ❌ | Refresh access token            |
| `POST`   | `/api/users/create`     |              ❌ | Create user                     |
| `POST`   | `/api/images`           |              ✅ | Upload image                    |
| `DELETE` | `/api/images/{imageId}` |              ✅ | Delete user's image             |
| `GET`    | `/api/images`           |              ✅ | Get authenticated user's images |
| `GET`    | `/api/images/{imageId}` |              ❌ | Get image by ID                 |

---

# ☁️ Amazon S3

Amazon S3 is used as the image object storage.

The application separates object storage from relational metadata.

Conceptually:

```text
PostgreSQL
   │
   └── Image metadata
        ├── ImageId
        ├── UserId
        └── Storage information

S3
   │
   └── Actual image data
```

This allows the database to remain focused on application metadata while S3 handles binary object storage.

The configured public base URL is:

```text
http://localhost:4566/imagedrop
```

for local development.

---

# 📨 Amazon SQS

Amazon SQS is used as the message broker between the API and the image processing worker.

The ImageDrop queue is configured as a **FIFO queue**:

```text
image-drop-queue.fifo
```

The asynchronous architecture provides several benefits:

* API requests do not have to wait for image processing;
* image processing can be retried independently;
* workers can be scaled independently from the API;
* temporary processing failures do not necessarily fail the original upload request;
* API and processing workloads are decoupled.

---

# 🔄 SQS Message Flow

```text
┌───────────────┐
│ ImageDrop.Api │
└───────┬───────┘
        │
        │ Publish
        ▼
┌─────────────────────┐
│ SQS Publisher       │
└──────────┬──────────┘
           │
           ▼
┌─────────────────────┐
│ image-drop-queue.fifo│
└──────────┬──────────┘
           │
           │ Consume
           ▼
┌─────────────────────┐
│ SQS Consumer        │
└──────────┬──────────┘
           │
           ▼
┌─────────────────────────────┐
│ ImageProcessing.Worker      │
└─────────────────────────────┘
```

The consumer is configured with:

```json
"Consumer": {
  "WaitTimeSeconds": 10,
  "VisibilityTimeout": 60,
  "MessageAttributeNames": [
    "All"
  ],
  "MessageSystemAttributeNames": [
    "SentTimestamp"
  ]
}
```

The `SentTimestamp` system attribute is used to make the original message timestamp available to the consumer.

---

# 🗄️ Database

ImageDrop uses a relational database for application data.

The database is accessed through Entity Framework Core and the persistence layer is isolated in:

```text
ImageDrop.Persistence
```

Database schema changes are applied using the dedicated:

```text
ImageDrop.Migrator
```

application.

This keeps migrations separate from the runtime API and workers.

---

# 🔑 Secrets & Configuration

Application secrets are stored in AWS Secrets Manager.

The expected secret name is:

```text
{Environment}_image_drop_secrets
```

For example:

```text
Development_image_drop_secrets
```

A secret follows this structure:

```json
{
  "S3Options": {
    "PublicBaseUrl": string,
    "BucketName": string
  },
  "SqsOptions": {
    "ImageDropQueueName": string,
    "Consumer": {
      "WaitTimeSeconds": int,
      "VisibilityTimeout": int,
      "MessageAttributeNames": [
        "All"
      ],
      "MessageSystemAttributeNames": [
        string
      ]
    }
  },
  "AWSCoreOptions": {
    "AccessKey": string,
    "SecretKey": string
  },
  "DbConnectionOptions": {
    "ImageDropConnectionString": string
  },
  "RedisOptions": {
    "Configuration": string,
    "InstanceName": string
  },
  "JwtOptions": {
    "JwtSecretKey": string,
    "Issuer": string,
    "Audience": string,
    "AccessTokenExpirationDays": int,
    "RefreshTokenExpirationDays": int,
    "ResetPasswordTokenExpirationDays": int
  },
  "ImageProcessingOptions": {
    "JpegQuality": int,
    "PngCompressionLevel": int
  },
  "ImageCleanupOptions": {
    "UnownedImageRetentionDays": int,
    "MaxDeleteObjectsBatchSize": int,
    "TriggerIdentity": string,
    "IntervalInHours": int
  },
  "RateLimitingOptions": {
    "IpPermitLimit": int,
    "IpWindowMinutes": int
  }
}
```

> **Security:** The values above are intended as a local-development example. Production environments must use secure credentials and secrets. JWT signing keys, AWS credentials, and database passwords should never be committed to source control.

---

# ⚙️ Configuration Reference

## S3

| Setting                   | Description                           |
| ------------------------- | ------------------------------------- |
| `S3Options:PublicBaseUrl` | Base URL used to access stored images |
| `S3Options:BucketName`    | S3 bucket containing images           |

---

## SQS

| Setting                         | Description                           |
| ------------------------------- | ------------------------------------- |
| `SqsOptions:ImageDropQueueName` | Image processing FIFO queue           |
| `WaitTimeSeconds`               | SQS long-polling wait time            |
| `VisibilityTimeout`             | Message visibility timeout            |
| `MessageAttributeNames`         | Message attributes requested from SQS |
| `MessageSystemAttributeNames`   | System attributes requested from SQS  |

---

## JWT

| Setting                            | Description                   |
| ---------------------------------- | ----------------------------- |
| `JwtSecretKey`                     | JWT signing secret            |
| `Issuer`                           | Token issuer                  |
| `Audience`                         | Token audience                |
| `AccessTokenExpirationDays`        | Access token lifetime         |
| `RefreshTokenExpirationDays`       | Refresh token lifetime        |
| `ResetPasswordTokenExpirationDays` | Password reset token lifetime |

---

## Image Processing

| Setting               | Default | Description              |
| --------------------- | ------: | ------------------------ |
| `JpegQuality`         |    `75` | JPEG compression quality |
| `PngCompressionLevel` |     `9` | PNG compression level    |

---

## Image Cleanup

| Setting                     |                     Default | Description                                    |
| --------------------------- | --------------------------: | ---------------------------------------------- |
| `UnownedImageRetentionDays` |                         `7` | How long an unowned image can remain           |
| `MaxDeleteObjectsBatchSize` |                      `1000` | Maximum number of objects deleted in one batch |
| `TriggerIdentity`           | `DeleteOldImagesJobTrigger` | Cleanup job trigger identity                   |
| `IntervalInHours`           |                         `6` | Cleanup execution interval                     |

---

## Rate Limiting

| Setting           | Default | Description             |
| ----------------- | ------: | ----------------------- |
| `IpPermitLimit`   |    `20` | Maximum requests per IP |
| `IpWindowMinutes` |     `1` | Rate limit window       |

---

# 🐳 Docker

The solution contains four executable applications that are intended to run as separate containers:

```text
┌──────────────────────────────┐
│        ImageDrop.Api         │
└──────────────────────────────┘

┌──────────────────────────────┐
│ ImageDrop.ImageProcessing    │
│          .Worker             │
└──────────────────────────────┘

┌──────────────────────────────┐
│   ImageDrop.ImageCleanup     │
│          .Worker             │
└──────────────────────────────┘

┌──────────────────────────────┐
│       ImageDrop.Migrator     │
└──────────────────────────────┘
```

Each executable project has its own Dockerfile.

The supporting projects are libraries and are packaged into the corresponding application containers.

---

# 🧪 Testing

The solution contains dedicated unit test projects:

```text
ImageDrop.Api.Tests.Unit
ImageDrop.Application.Tests.Unit
ImageDrop.AWS.S3.Tests.Unit
ImageDrop.AWS.SQS.Consumer.Tests.Unit
ImageDrop.Shared.Services.Tests.Unit
```

The tests are organized according to the component being tested.

```text
ImageDrop.Api
       │
       └── ImageDrop.Api.Tests.Unit

ImageDrop.Application
       │
       └── ImageDrop.Application.Tests.Unit

ImageDrop.AWS.S3
       │
       └── ImageDrop.AWS.S3.Tests.Unit

ImageDrop.AWS.SQS.Consumer
       │
       └── ImageDrop.AWS.SQS.Consumer.Tests.Unit

ImageDrop.Shared.Services
       │
       └── ImageDrop.Shared.Services.Tests.Unit
```

The separation allows application and infrastructure components to be tested independently.

---

# 🔁 Complete System Flow

The following sequence summarizes the complete upload and processing workflow.

```text
                         CLIENT
                           │
                           │ POST /api/images
                           ▼
                    ┌───────────────┐
                    │ ImageDrop.Api │
                    └───────┬───────┘
                            │
                 ┌──────────┼───────────┐
                 │          │           │
                 ▼          ▼           ▼
            PostgreSQL     S3        SQS Publisher
                 │          │           │
                 │          │           ▼
                 │          │      ┌─────────────┐
                 │          │      │     SQS     │
                 │          │      │    FIFO     │
                 │          │      └──────┬──────┘
                 │          │             │
                 │          │             ▼
                 │          │      SQS Consumer
                 │          │             │
                 │          │             ▼
                 │          │     Processing Worker
                 │          │             │
                 │          │             ▼
                 │          │       Process Image
                 │          │             │
                 │          └─────────────┘
                 │
                 ▼
             Image Metadata


             ┌────────────────────┐
             │  Cleanup Worker    │
             └─────────┬──────────┘
                       │
                       ▼
                Find old unowned
                     images
                       │
                       ▼
                Delete S3 objects
                       │
                       ▼
                Cleanup metadata
```

---

# 🧱 Architectural Principles

ImageDrop is designed around several architectural principles.

### Separation of responsibilities

HTTP handling, business logic, persistence, AWS integrations, and background processing are separated into different projects.

### Asynchronous processing

Image processing is not performed directly as part of the API request. SQS is used to decouple image creation from image processing.

### Independent workers

Cleanup and image processing are implemented as independent workers and can be deployed and scaled separately from the API.

### External object storage

Binary image data is stored in S3 rather than directly in the relational database.

### Shared contracts

SQS messages are represented by shared contracts so that publishers and consumers operate on the same message definitions.

### Dedicated migrations

Database migrations are executed by a dedicated application rather than being coupled to API startup.

### Configuration-driven behavior

Important operational settings such as image quality, cleanup retention, SQS visibility timeout, JWT expiration, and rate limits are configurable without changing application code.

---

# 📁 Complete Project Map

```text
ImageDrop
│
├── ImageDrop.Api
│   └── HTTP API
│
├── ImageDrop.Application
│   └── Application / business logic
│
├── ImageDrop.AWS.S3
│   └── Amazon S3 integration
│
├── ImageDrop.AWS.SecretsManager
│   └── AWS Secrets Manager integration
│
├── ImageDrop.AWS.SQS.Consumer
│   └── SQS message consumption
│
├── ImageDrop.AWS.SQS.Publisher
│   └── SQS message publishing
│
├── ImageDrop.ImageCleanup.Worker
│   └── Orphaned image cleanup
│
├── ImageDrop.ImageProcessing.Worker
│   └── Asynchronous image processing
│
├── ImageDrop.Migrator
│   └── Database migrations
│
├── ImageDrop.Persistence
│   └── Entity Framework Core / database access
│
├── ImageDrop.Shared.Constants
│   └── Shared constants
│
├── ImageDrop.Shared.Services
│   └── Shared application services
│
├── ImageDrop.Shared.SQS.Contracts
│   └── SQS message contracts
│
└── Tests
    │
    ├── ImageDrop.Api.Tests.Unit
    ├── ImageDrop.Application.Tests.Unit
    ├── ImageDrop.AWS.S3.Tests.Unit
    ├── ImageDrop.AWS.SQS.Consumer.Tests.Unit
    └── ImageDrop.Shared.Services.Tests.Unit
```

---

# 🚀 Summary

ImageDrop is a modular image-sharing backend inspired by services such as Imgur.

The system combines:

* **ASP.NET Core** for the HTTP API;
* **JWT** for authentication;
* **Redis** for rate limiting and supporting authentication-related functionality;
* **PostgreSQL / relational persistence** for application metadata;
* **Amazon S3** for image storage;
* **Amazon SQS FIFO** for asynchronous image processing;
* **Background workers** for image processing and cleanup;
* **UUIDv7** for image identifiers;
* **AWS Secrets Manager** for application secrets;
* **Docker** for application deployment;
* **Entity Framework Core migrations** through a dedicated migrator;
* **Unit tests** for the main application and infrastructure components.

The resulting architecture keeps client communication, business logic, persistence, external infrastructure, asynchronous processing, and maintenance tasks separated while allowing each executable component to be deployed independently.
