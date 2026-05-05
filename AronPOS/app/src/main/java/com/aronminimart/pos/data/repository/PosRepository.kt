package com.aronminimart.pos.data.repository

import android.content.Context
import android.util.Log
import com.aronminimart.pos.data.model.*
import com.aronminimart.pos.data.network.RetrofitClient

sealed class Result<out T> {
    data class Success<T>(val data: T) : Result<T>()
    data class Error(val message: String) : Result<Nothing>()
}

class PosRepository(private val context: Context) {

    private val TAG = "POS_REPO"
    private val api get() = RetrofitClient.getService(context)

    suspend fun getCategoriesWithItems(): Result<List<MenuCategory>> {
        Log.d(TAG, "getCategoriesWithItems() called")
        return try {
            val response = api.getCategoriesWithItems()
            Log.d(TAG, "getCategoriesWithItems() → HTTP ${response.code()}")
            if (response.isSuccessful) {
                val data = response.body() ?: emptyList()
                Log.i(TAG, "Categories loaded: ${data.size} categories")
                data.forEach { cat ->
                    Log.d(TAG, "  Category: '${cat.name}' — ${cat.items.size} items")
                }
                Result.Success(data)
            } else {
                val err = "HTTP ${response.code()}: ${response.errorBody()?.string()}"
                Log.e(TAG, "getCategoriesWithItems() failed: $err")
                Result.Error("Server error: ${response.code()}")
            }
        } catch (e: java.net.ConnectException) {
            Log.e(TAG, "ConnectException — API server is unreachable")
            Log.e(TAG, "  URL tried: ${RetrofitClient.getCurrentUrl(context)}")
            Log.e(TAG, "  Tip: Make sure the API is running and port 5000 is open")
            Log.e(TAG, "  Emulator users: URL must be http://10.0.2.2:5000/")
            Log.e(TAG, "  Real device: URL must be http://<YOUR_PC_IP>:5000/")
            Result.Error("Cannot connect to server.\nCheck API is running on port 5000.\nEmulator URL: http://10.0.2.2:5000/")
        } catch (e: java.net.SocketTimeoutException) {
            Log.e(TAG, "SocketTimeoutException — connection timed out")
            Log.e(TAG, "  URL: ${RetrofitClient.getCurrentUrl(context)}")
            Result.Error("Connection timed out. Is the API server running?")
        } catch (e: java.net.UnknownHostException) {
            Log.e(TAG, "UnknownHostException — cannot resolve host")
            Log.e(TAG, "  URL: ${RetrofitClient.getCurrentUrl(context)}")
            Result.Error("Unknown host. Check the API URL in Settings.")
        } catch (e: Exception) {
            Log.e(TAG, "Unexpected error in getCategoriesWithItems()")
            Log.e(TAG, "  Type   : ${e.javaClass.name}")
            Log.e(TAG, "  Message: ${e.message}")
            Log.e(TAG, "  Stack  :", e)
            Result.Error("Network error: ${e.message}")
        }
    }

    suspend fun getItemsByCategory(categoryId: Int): Result<List<MenuItem>> {
        Log.d(TAG, "getItemsByCategory(categoryId=$categoryId) called")
        return try {
            val response = api.getItems(categoryId)
            Log.d(TAG, "getItemsByCategory() → HTTP ${response.code()}")
            if (response.isSuccessful) {
                val data = response.body() ?: emptyList()
                Log.i(TAG, "Items loaded: ${data.size} items for category $categoryId")
                Result.Success(data)
            } else {
                val err = "HTTP ${response.code()}: ${response.errorBody()?.string()}"
                Log.e(TAG, "getItemsByCategory() failed: $err")
                Result.Error("Server error: ${response.code()}")
            }
        } catch (e: Exception) {
            Log.e(TAG, "Error in getItemsByCategory(): ${e.message}", e)
            Result.Error("Network error: ${e.message}")
        }
    }

    suspend fun submitOrder(request: CreateOrderRequest): Result<OrderResponse> {
        Log.d(TAG, "submitOrder() called — ${request.items.size} items, cash=₱${request.cashGiven}")
        return try {
            val response = api.createOrder(request)
            Log.d(TAG, "submitOrder() → HTTP ${response.code()}")
            if (response.isSuccessful) {
                val order = response.body()!!
                Log.i(TAG, "Order created: ${order.orderNumber}, change=₱${order.change}")
                Result.Success(order)
            } else {
                val errorBody = response.errorBody()?.string() ?: "Unknown error"
                Log.e(TAG, "submitOrder() failed: HTTP ${response.code()} — $errorBody")
                Result.Error(errorBody)
            }
        } catch (e: java.net.ConnectException) {
            Log.e(TAG, "submitOrder() — ConnectException: ${e.message}")
            Result.Error("Cannot connect to server. Is the API running?")
        } catch (e: Exception) {
            Log.e(TAG, "Error in submitOrder(): ${e.message}", e)
            Result.Error("Network error: ${e.message}")
        }
    }

    suspend fun getStoreInfo(): Result<StoreInfo> {
        Log.d(TAG, "getStoreInfo() called — URL: ${RetrofitClient.getCurrentUrl(context)}")
        return try {
            val response = api.getStoreInfo()
            Log.d(TAG, "getStoreInfo() → HTTP ${response.code()}")
            if (response.isSuccessful) {
                val info = response.body()!!
                Log.i(TAG, "Store info: name='${info.name}', version='${info.version}'")
                Result.Success(info)
            } else {
                Log.e(TAG, "getStoreInfo() failed: HTTP ${response.code()}")
                Result.Error("Could not load store info")
            }
        } catch (e: Exception) {
            Log.e(TAG, "getStoreInfo() error: ${e.javaClass.simpleName} — ${e.message}")
            Result.Error("Network error: ${e.message}")
        }
    }
}
