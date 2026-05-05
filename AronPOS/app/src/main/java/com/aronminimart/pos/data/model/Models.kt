package com.aronminimart.pos.data.model

import com.google.gson.annotations.SerializedName

// ── Menu ──────────────────────────────────────────────────────────────────────

data class MenuCategory(
    val id: Int,
    val name: String,
    val description: String?,
    val isActive: Boolean,
    val items: List<MenuItem> = emptyList()
)

data class MenuItem(
    val id: Int,
    val name: String,
    val description: String?,
    val price: Double,
    val isAvailable: Boolean,
    val menuCategoryId: Int,
    val categoryName: String
)

// ── Order ─────────────────────────────────────────────────────────────────────

data class CreateOrderRequest(
    val cashGiven: Double,
    val cashierName: String?,
    val items: List<OrderItemRequest>
)

data class OrderItemRequest(
    val menuItemId: Int,
    val quantity: Int
)

data class OrderResponse(
    val id: Int,
    val orderNumber: String,
    val orderDate: String,
    val subTotal: Double,
    val cashGiven: Double,
    val change: Double,
    val status: String,
    val items: List<OrderItemResponse>
)

data class OrderItemResponse(
    val itemName: String,
    val unitPrice: Double,
    val quantity: Int,
    val lineTotal: Double
)

// ── Store ─────────────────────────────────────────────────────────────────────

data class StoreInfo(
    val name: String,
    val version: String
)

// ── Cart ─────────────────────────────────────────────────────────────────────

data class CartItem(
    val menuItem: MenuItem,
    var quantity: Int = 1
) {
    val lineTotal: Double get() = menuItem.price * quantity
}
