# Aron Mini Mart POS — Setup Guide

## Architecture

```
RestaurantAdmin (MVC)  ←→  AIPOS Database (SQL Server)  ←→  PosApi (REST API)  ←→  AronPOS (Android)
```

Both the Admin panel and the POS API share the **same AIPOS database**.
- Admin panel manages menu items, categories, users
- POS API exposes those items to the Android app
- Android app lets cashiers take orders and submit them

---

## Step 1: Run the POS API

1. Open `PosApi/PosApi.sln` in Visual Studio
2. Verify `appsettings.json` connection string points to your AIPOS database
3. Press **F5** — the API starts on `http://localhost:5000`

Test it in a browser:
- `http://localhost:5000/api/store/health` → `{"status":"ok"}`
- `http://localhost:5000/api/menu/categories/with-items` → JSON list of categories

---

## Step 2: Add menu data via Admin Panel

Run the `RestaurantAdmin` app, log in as admin, and:
1. Go to **Menu Categories** → create categories (Drinks, Snacks, etc.)
2. Go to **Menu Items** → add items with prices

The Android app will pull these items live from the API.

---

## Step 3: Open Android app in Android Studio

1. Open Android Studio
2. **File → Open** → select the `AronPOS/` folder
3. Wait for Gradle sync to complete
4. Run on emulator or device

### API URL configuration

| Environment | URL to use |
|---|---|
| Android Emulator | `http://10.0.2.2:5000/` (default) |
| Real Android device | `http://YOUR_PC_IP:5000/` |

To find your PC IP: run `ipconfig` in Command Prompt, look for IPv4 address (e.g. `192.168.1.5`).

To change the URL in the app: tap the **Settings** icon (top-right) → enter your URL → Save.

---

## API Endpoints

| Method | Endpoint | Description |
|---|---|---|
| GET | `/api/store/health` | Health check |
| GET | `/api/store/info` | Store name |
| GET | `/api/menu/categories` | All active categories |
| GET | `/api/menu/categories/with-items` | Categories + their items |
| GET | `/api/menu/items?categoryId=1` | Items by category |
| POST | `/api/orders` | Submit an order |
| GET | `/api/orders` | Order history |
| GET | `/api/orders/{id}` | Single order |

### POST /api/orders — Request body
```json
{
  "cashGiven": 200.00,
  "cashierName": "Cashier",
  "items": [
    { "menuItemId": 1, "quantity": 2 },
    { "menuItemId": 3, "quantity": 1 }
  ]
}
```

---

## Android App Screens

| Screen | Description |
|---|---|
| **Main (POS)** | Category tabs, product grid, cart, bill summary, submit |
| **Receipt** | Order confirmation with change calculation |
| **Settings** | Configure API server URL |

## POS Flow

1. Cashier selects a category tab
2. Taps **+** on products to add to cart
3. Enters cash amount in "Cash Given"
4. Change is calculated automatically
5. Taps **SUBMIT ORDER**
6. Receipt screen shows order number and change
7. Taps **NEW ORDER** to reset
