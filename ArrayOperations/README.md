# SumBySchema - Power Automate Custom Connector

This connector lets you easily sum values from JSON data structures in Power Automate cloud flows — no `Apply to each`, no messy expressions.

## ✨ Features

- Accepts flat arrays, nested arrays, or object-of-arrays
- Schema-driven — only returns what you specify
- Returns `section.prop.sum` and a total `prop.sum` for each field

## 🛠 Usage

### Flat Array of Objects

**Input:**
```json
{
  "array": [
    { "count": 5, "size": 2000 },
    { "count": 10, "size": 3000 }
  ],
  "schema": {
    "": ["count", "size"]
  }
}
```

**Output:**
```json
{
  "count.sum": 15,
  "size.sum": 5000
}
```

### Object-of-Arrays

**Input:**
```json
{
  "object": {
    "email": [ { "count": 5 }, { "count": 7 } ],
    "calendar": [ { "count": 2 }, { "count": 3 } ]
  },
  "schema": {
    "email": ["count"],
    "calendar": ["count"]
  }
}
```

**Output:**
```json
{
  "email.count.sum": 12,
  "calendar.count.sum": 5,
  "count.sum": 17
}
```

## 📦 Files

- `array-operations.csx` — custom logic
- `swagger.yaml` — OpenAPI definition for the connector
