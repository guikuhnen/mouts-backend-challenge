[Back to README](../README.md)

## Sales API

The Sales API provides a complete set of operations to manage sales records. A sale represents a commercial transaction that groups one or more products (items), associates them to a customer and a branch, and automatically applies quantity-based discounts following the business rules.

---

### Business Rules

#### Discount Tiers

Discounts are calculated automatically per item based on the quantity purchased:

| Quantity       | Discount                              |
| -------------- | ------------------------------------- |
| 1 – 3 items    | No discount                           |
| 4 – 9 items    | 10% discount                          |
| 10 – 20 items  | 20% discount                          |
| Above 20 items | **Not allowed** — request is rejected |

> Discounts are applied to each item individually. Different items in the same sale can have different discount tiers.

#### Sale Cancellation

- A sale can be **fully cancelled** via `PATCH /api/sales/{id}/cancel`.
- Individual items can be **cancelled independently** via `PATCH /api/sales/{id}/items/{itemId}/cancel`.
- A cancelled sale **cannot be updated or have its items cancelled**.
- Cancelled items are **excluded from the sale's total amount** calculation.

---

### External Identities

Following the DDD External Identities pattern, the Sale stores **denormalized references** to external entities. This means Customer and Branch data (name, ID) are recorded at the time of the sale and remain immutable even if the original records change.

---

### Endpoints

---

#### `POST /api/sales`

Creates a new sale record.

**Request Body:**

```json
{
  "customerId": "uuid",
  "customerName": "string",
  "branchId": "uuid",
  "branchName": "string",
  "saleDate": "datetime (ISO 8601)",
  "items": [
    {
      "productId": "uuid",
      "productName": "string",
      "quantity": "integer (1–20)",
      "unitPrice": "decimal (> 0)"
    }
  ]
}
```

**Response `201 Created`:**

```json
{
  "success": true,
  "message": "Sale created successfully",
  "data": {
    "id": "uuid",
    "saleNumber": "integer",
    "saleDate": "datetime",
    "customerId": "uuid",
    "customerName": "string",
    "branchId": "uuid",
    "branchName": "string",
    "totalAmount": "decimal",
    "isCancelled": false,
    "items": [
      {
        "id": "uuid",
        "productId": "uuid",
        "productName": "string",
        "quantity": "integer",
        "unitPrice": "decimal",
        "discount": "decimal (0.00 / 0.10 / 0.20)",
        "totalAmount": "decimal"
      }
    ]
  }
}
```

**Errors:**

- `400 Bad Request` — Missing required fields, quantity out of range (< 1 or > 20), unit price ≤ 0, no items provided.

---

#### `GET /api/sales`

Returns a paginated list of all sales, ordered by sale date descending.

**Query Parameters:**

| Parameter  | Type    | Default | Description    |
| ---------- | ------- | ------- | -------------- |
| `page`     | integer | 1       | Page number    |
| `pageSize` | integer | 10      | Items per page |

**Example:**

```
GET /api/sales?page=1&pageSize=20
```

**Response `200 OK`:**

```json
{
  "success": true,
  "message": "Sales retrieved successfully",
  "data": {
    "sales": [
      {
        "id": "uuid",
        "saleNumber": "integer",
        "saleDate": "datetime",
        "customerId": "uuid",
        "customerName": "string",
        "branchId": "uuid",
        "branchName": "string",
        "totalAmount": "decimal",
        "isCancelled": "boolean"
      }
    ],
    "totalCount": "integer",
    "page": "integer",
    "pageSize": "integer",
    "totalPages": "integer"
  }
}
```

---

#### `GET /api/sales/{id}`

Retrieves a single sale by its unique identifier, including all items.

**Path Parameters:**

| Parameter | Type | Description                  |
| --------- | ---- | ---------------------------- |
| `id`      | uuid | The sale's unique identifier |

**Response `200 OK`:**

```json
{
  "success": true,
  "message": "Sale retrieved successfully",
  "data": {
    "id": "uuid",
    "saleNumber": "integer",
    "saleDate": "datetime",
    "customerId": "uuid",
    "customerName": "string",
    "branchId": "uuid",
    "branchName": "string",
    "totalAmount": "decimal",
    "isCancelled": "boolean",
    "items": [
      {
        "id": "uuid",
        "productId": "uuid",
        "productName": "string",
        "quantity": "integer",
        "unitPrice": "decimal",
        "discount": "decimal",
        "totalAmount": "decimal",
        "isCancelled": "boolean"
      }
    ]
  }
}
```

**Errors:**

- `404 Not Found` — Sale with the given ID does not exist.

---

#### `PUT /api/sales/{id}`

Fully replaces the data of an existing sale, including its items. Cancelled sales cannot be updated.

**Path Parameters:**

| Parameter | Type | Description                  |
| --------- | ---- | ---------------------------- |
| `id`      | uuid | The sale's unique identifier |

**Request Body:** Same structure as `POST /api/sales`.

**Response `200 OK`:**

```json
{
  "success": true,
  "message": "Sale updated successfully",
  "data": { ... }
}
```

**Errors:**

- `400 Bad Request` — Validation failure.
- `404 Not Found` — Sale not found.
- `409 Conflict` — Sale is already cancelled.

---

#### `PATCH /api/sales/{id}/cancel`

Cancels an entire sale. All items are considered cancelled and the total amount becomes zero.

**Path Parameters:**

| Parameter | Type | Description                  |
| --------- | ---- | ---------------------------- |
| `id`      | uuid | The sale's unique identifier |

**Response `200 OK`:**

```json
{
  "success": true,
  "message": "Sale cancelled successfully",
  "data": {
    "id": "uuid",
    "saleNumber": "integer",
    "isCancelled": true
  }
}
```

**Errors:**

- `404 Not Found` — Sale not found.
- `409 Conflict` — Sale is already cancelled.

---

#### `PATCH /api/sales/{id}/items/{itemId}/cancel`

Cancels a single item within a sale without cancelling the entire sale. The sale's total amount is recalculated excluding the cancelled item.

**Path Parameters:**

| Parameter | Type | Description                  |
| --------- | ---- | ---------------------------- |
| `id`      | uuid | The sale's unique identifier |
| `itemId`  | uuid | The item's unique identifier |

**Response `200 OK`:**

```json
{
  "success": true,
  "message": "Sale item cancelled successfully",
  "data": {
    "saleId": "uuid",
    "itemId": "uuid",
    "isCancelled": true
  }
}
```

**Errors:**

- `404 Not Found` — Sale or item not found.
- `409 Conflict` — The parent sale is already cancelled.

---

#### `DELETE /api/sales/{id}`

Permanently removes a sale record from the system.

**Path Parameters:**

| Parameter | Type | Description                  |
| --------- | ---- | ---------------------------- |
| `id`      | uuid | The sale's unique identifier |

**Response `200 OK`:**

```json
{
  "success": true,
  "message": "Sale deleted successfully"
}
```

**Errors:**

- `404 Not Found` — Sale not found.

---

### Discount Calculation Example

**Sale with 3 items:**

| Product        | Quantity | Unit Price | Discount | Item Total      |
| -------------- | -------- | ---------- | -------- | --------------- |
| Widget A       | 3        | R$ 100,00  | 0%       | R$ 300,00       |
| Widget B       | 5        | R$ 80,00   | 10%      | R$ 360,00       |
| Widget C       | 12       | R$ 50,00   | 20%      | R$ 480,00       |
| **Sale Total** |          |            |          | **R$ 1.140,00** |

---

### Domain Events

The following events are published (logged) during sale lifecycle transitions:

| Event           | Trigger                                  |
| --------------- | ---------------------------------------- |
| `SaleCreated`   | A new sale is successfully created       |
| `SaleModified`  | A sale is successfully updated           |
| `SaleCancelled` | A sale is fully cancelled                |
| `ItemCancelled` | A single item within a sale is cancelled |

Events are emitted as structured log entries and can be forwarded to a message broker in future integrations.

<br/>
<div style="display: flex; justify-content: space-between;">
  <a href="./sales-api.md">Previous: Sales API</a>
  <a href="./technical-implementation.md">Next: Technical Implementation</a>
</div>
