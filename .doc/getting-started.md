[Back to README](../README.md)

## Getting Started

This guide explains how to run the application locally using Docker Compose.

---

### Prerequisites

- [Docker Desktop](https://www.docker.com/products/docker-desktop/) installed and running
- Ports `8080`, `5432`, `27017` and `6379` available on your machine

---

### Running the Application

From the `template/backend/` directory, execute:

```bash
docker compose up -d
```

Docker will:

1. Build the WebApi image from the Dockerfile
2. Pull `postgres:13`, `mongo:8.0` and `redis:7.4.1-alpine`
3. Start the database and wait for it to pass the health check
4. Start the API only after the database is ready
5. Apply all pending database migrations automatically on first boot

> On the first run the image build may take a few minutes while .NET packages are restored.

---

### Accessing the Application

| Resource         | URL                           |
| ---------------- | ----------------------------- |
| **Swagger UI**   | http://localhost:8080/swagger |
| **API base URL** | http://localhost:8080/api     |
| **Health check** | http://localhost:8080/health  |

---

### Container Status

To verify all four containers are running:

```bash
docker compose ps
```

Expected output:

```
NAME                                  STATUS                   PORTS
ambev_developer_evaluation_webapi     Up                       0.0.0.0:8080->8080/tcp
ambev_developer_evaluation_database   Up (healthy)             0.0.0.0:5432->5432/tcp
ambev_developer_evaluation_nosql      Up                       0.0.0.0:27017->27017/tcp
ambev_developer_evaluation_cache      Up                       0.0.0.0:6379->6379/tcp
```

---

### Viewing Logs

```bash
# All services
docker compose logs -f

# API only
docker compose logs -f ambev.developerevaluation.webapi

# Database only
docker compose logs -f ambev.developerevaluation.database
```

---

### Stopping the Application

```bash
# Stop containers (keeps data)
docker compose stop

# Stop and remove containers + network
docker compose down

# Stop, remove containers + network + volumes (wipes database)
docker compose down -v
```

---

### Rebuilding After Code Changes

```bash
docker compose build ambev.developerevaluation.webapi
docker compose up -d --no-deps --force-recreate ambev.developerevaluation.webapi
```

---

### Database Connection

If you need to connect directly to PostgreSQL:

| Field    | Value                  |
| -------- | ---------------------- |
| Host     | `localhost`            |
| Port     | `5432`                 |
| Database | `developer_evaluation` |
| Username | `developer`            |
| Password | `ev@luAt10n`           |

---

### Quick API Test

After the application is running, you can create a sale directly from the Swagger UI at **http://localhost:8080/swagger**, or via curl:

```bash
curl -X POST http://localhost:8080/api/sales \
  -H "Content-Type: application/json" \
  -d '{
    "customerId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "customerName": "ACME Corp",
    "branchId": "4fa85f64-5717-4562-b3fc-2c963f66afa7",
    "branchName": "Main Branch",
    "saleDate": "2026-06-03T10:00:00Z",
    "items": [
      {
        "productId": "5fa85f64-5717-4562-b3fc-2c963f66afa8",
        "productName": "Widget Pro",
        "quantity": 10,
        "unitPrice": 99.90
      }
    ]
  }'
```

Expected response — item with quantity 10 receives 20% discount, total = `10 × 99.90 × 0.80 = 799.20`:

```json
{
  "success": true,
  "message": "Sale created successfully",
  "data": {
    "saleNumber": 1,
    "totalAmount": 799.2,
    "isCancelled": false,
    "items": [
      {
        "quantity": 10,
        "unitPrice": 99.9,
        "discount": 0.2,
        "totalAmount": 799.2
      }
    ]
  }
}
```

<br/>
<div style="display: flex; justify-content: space-between;">
  <a href="./technical-implementation.md">Previous: Technical Implementation</a>
</div>
