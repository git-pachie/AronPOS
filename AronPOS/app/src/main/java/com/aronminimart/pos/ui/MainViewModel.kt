package com.aronminimart.pos.ui

import android.app.Application
import android.util.Log
import androidx.lifecycle.AndroidViewModel
import androidx.lifecycle.LiveData
import androidx.lifecycle.MutableLiveData
import androidx.lifecycle.viewModelScope
import com.aronminimart.pos.data.model.*
import com.aronminimart.pos.data.repository.PosRepository
import com.aronminimart.pos.data.repository.Result
import kotlinx.coroutines.launch

class MainViewModel(application: Application) : AndroidViewModel(application) {

    private val TAG = "POS_VIEWMODEL"
    private val repository = PosRepository(application)

    // ── Store ─────────────────────────────────────────────────────────────────
    private val _storeName = MutableLiveData("ARON MINI MART")
    val storeName: LiveData<String> = _storeName

    // ── Categories ────────────────────────────────────────────────────────────
    private val _categories = MutableLiveData<List<MenuCategory>>()
    val categories: LiveData<List<MenuCategory>> = _categories

    private val _selectedCategoryIndex = MutableLiveData(0)
    val selectedCategoryIndex: LiveData<Int> = _selectedCategoryIndex

    // ── Items ─────────────────────────────────────────────────────────────────
    private val _items = MutableLiveData<List<MenuItem>>()
    val items: LiveData<List<MenuItem>> = _items

    // ── Cart ──────────────────────────────────────────────────────────────────
    private val _cart = MutableLiveData<MutableList<CartItem>>(mutableListOf())
    val cart: LiveData<MutableList<CartItem>> = _cart

    private val _subTotal = MutableLiveData(0.0)
    val subTotal: LiveData<Double> = _subTotal

    private val _cashGiven = MutableLiveData(0.0)
    val cashGiven: LiveData<Double> = _cashGiven

    private val _change = MutableLiveData(0.0)
    val change: LiveData<Double> = _change

    // ── Loading / Error ───────────────────────────────────────────────────────
    private val _isLoading = MutableLiveData(false)
    val isLoading: LiveData<Boolean> = _isLoading

    private val _error = MutableLiveData<String?>()
    val error: LiveData<String?> = _error

    private val _orderResult = MutableLiveData<OrderResponse?>()
    val orderResult: LiveData<OrderResponse?> = _orderResult

    // ── Init ──────────────────────────────────────────────────────────────────
    init {
        loadData()
    }

    fun loadData() {
        viewModelScope.launch {
            _isLoading.value = true
            _error.value = null
            Log.i(TAG, "loadData() started")

            // Load store info
            when (val result = repository.getStoreInfo()) {
                is Result.Success -> {
                    _storeName.value = result.data.name
                    Log.i(TAG, "Store name: ${result.data.name}")
                }
                is Result.Error -> Log.w(TAG, "Store info failed: ${result.message}")
            }

            // Load categories with items
            Log.i(TAG, "Loading categories with items...")
            when (val result = repository.getCategoriesWithItems()) {
                is Result.Success -> {
                    Log.i(TAG, "Loaded ${result.data.size} categories")
                    _categories.value = result.data
                    if (result.data.isNotEmpty()) {
                        selectCategory(0)
                    } else {
                        Log.w(TAG, "No categories returned — add categories in the Admin panel")
                    }
                }
                is Result.Error -> {
                    Log.e(TAG, "Failed to load categories: ${result.message}")
                    _error.value = result.message
                }
            }

            _isLoading.value = false
            Log.i(TAG, "loadData() finished")
        }
    }

    fun selectCategory(index: Int) {
        _selectedCategoryIndex.value = index
        val cats = _categories.value ?: return
        if (index < cats.size) {
            _items.value = cats[index].items
        }
    }

    // ── Cart operations ───────────────────────────────────────────────────────

    fun addToCart(item: MenuItem) {
        val currentCart = _cart.value ?: mutableListOf()
        val existing = currentCart.find { it.menuItem.id == item.id }
        if (existing != null) {
            existing.quantity++
        } else {
            currentCart.add(CartItem(item))
        }
        _cart.value = currentCart
        recalculate()
    }

    fun removeFromCart(item: MenuItem) {
        val currentCart = _cart.value ?: return
        val existing = currentCart.find { it.menuItem.id == item.id }
        if (existing != null) {
            if (existing.quantity > 1) {
                existing.quantity--
            } else {
                currentCart.remove(existing)
            }
        }
        _cart.value = currentCart
        recalculate()
    }

    fun clearCart() {
        _cart.value = mutableListOf()
        _cashGiven.value = 0.0
        recalculate()
    }

    fun getCartQuantity(itemId: Int): Int {
        return _cart.value?.find { it.menuItem.id == itemId }?.quantity ?: 0
    }

    fun setCashGiven(amount: Double) {
        _cashGiven.value = amount
        recalculate()
    }

    private fun recalculate() {
        val total = _cart.value?.sumOf { it.lineTotal } ?: 0.0
        _subTotal.value = total
        val cash = _cashGiven.value ?: 0.0
        _change.value = if (cash >= total) cash - total else 0.0
    }

    // ── Submit order ──────────────────────────────────────────────────────────

    fun submitOrder() {
        val cartItems = _cart.value
        if (cartItems.isNullOrEmpty()) {
            _error.value = "Cart is empty"
            return
        }
        val cash = _cashGiven.value ?: 0.0
        val total = _subTotal.value ?: 0.0
        if (cash < total) {
            _error.value = "Cash given is less than the total"
            return
        }

        viewModelScope.launch {
            _isLoading.value = true
            val request = CreateOrderRequest(
                cashGiven = cash,
                cashierName = "Cashier",
                items = cartItems.map { OrderItemRequest(it.menuItem.id, it.quantity) }
            )
            when (val result = repository.submitOrder(request)) {
                is Result.Success -> {
                    _orderResult.value = result.data
                    clearCart()
                }
                is Result.Error -> _error.value = result.message
            }
            _isLoading.value = false
        }
    }

    fun clearOrderResult() { _orderResult.value = null }
    fun clearError() { _error.value = null }
}
