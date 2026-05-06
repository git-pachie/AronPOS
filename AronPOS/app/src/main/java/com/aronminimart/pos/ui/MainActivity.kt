package com.aronminimart.pos.ui

import android.app.AlertDialog
import android.content.Intent
import android.os.Bundle
import android.view.Menu
import android.view.MenuItem
import android.view.View
import android.widget.Toast
import androidx.activity.viewModels
import androidx.appcompat.app.AppCompatActivity
import androidx.recyclerview.widget.GridLayoutManager
import com.aronminimart.pos.R
import com.aronminimart.pos.data.model.MenuItem as PosMenuItem
import com.aronminimart.pos.data.session.SessionManager
import com.aronminimart.pos.databinding.ActivityMainBinding
import com.aronminimart.pos.ui.adapter.CartAdapter
import com.aronminimart.pos.ui.adapter.MenuItemAdapter
import com.google.android.material.chip.Chip

class MainActivity : AppCompatActivity() {

    private lateinit var binding: ActivityMainBinding
    private val viewModel: MainViewModel by viewModels()
    private lateinit var session: SessionManager

    private lateinit var menuAdapter: MenuItemAdapter
    private lateinit var cartAdapter: CartAdapter

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)

        session = SessionManager(this)

        // Guard: if not logged in, redirect to login
        if (!session.isLoggedIn()) {
            goToLogin()
            return
        }

        binding = ActivityMainBinding.inflate(layoutInflater)
        setContentView(binding.root)

        setSupportActionBar(binding.toolbar)
        setupAdapters()
        observeViewModel()
        setupListeners()
    }

    private fun setupAdapters() {
        menuAdapter = MenuItemAdapter(
            onAdd    = { item -> viewModel.addToCart(item); menuAdapter.refreshQuantities() },
            onRemove = { item -> viewModel.removeFromCart(item); menuAdapter.refreshQuantities() },
            getQty   = { id -> viewModel.getCartQuantity(id) }
        )
        binding.rvMenuItems.apply {
            layoutManager = GridLayoutManager(this@MainActivity, 3)
            adapter = menuAdapter
        }

        cartAdapter = CartAdapter(
            onRemove = { cartItem ->
                repeat(cartItem.quantity) { viewModel.removeFromCart(cartItem.menuItem) }
                menuAdapter.refreshQuantities()
            }
        )
        binding.rvCart.apply {
            layoutManager = androidx.recyclerview.widget.LinearLayoutManager(this@MainActivity)
            adapter = cartAdapter
        }
    }

    private fun observeViewModel() {
        viewModel.storeName.observe(this) { name ->
            binding.tvStoreName.text = name
            supportActionBar?.title = name
            // Show cashier name in subtitle
            val cashier = session.getFullName().ifEmpty { session.getUsername() }
            supportActionBar?.subtitle = "Cashier: $cashier"
        }

        viewModel.isLoading.observe(this) { loading ->
            binding.progressBar.visibility  = if (loading) View.VISIBLE else View.GONE
            binding.layoutContent.visibility = if (loading) View.GONE else View.VISIBLE
        }

        viewModel.error.observe(this) { msg ->
            if (!msg.isNullOrEmpty()) {
                Toast.makeText(this, msg, Toast.LENGTH_LONG).show()
                viewModel.clearError()
            }
        }

        viewModel.categories.observe(this) { categories ->
            binding.chipGroupCategories.removeAllViews()
            categories.forEachIndexed { index, category ->
                val chip = Chip(this).apply {
                    text = category.name
                    isCheckable = true
                    isChecked = index == 0
                    setOnClickListener { viewModel.selectCategory(index) }
                }
                binding.chipGroupCategories.addView(chip)
            }
        }

        viewModel.selectedCategoryIndex.observe(this) { index ->
            val chipGroup = binding.chipGroupCategories
            if (index < chipGroup.childCount) {
                (chipGroup.getChildAt(index) as? Chip)?.isChecked = true
            }
        }

        viewModel.items.observe(this) { items ->
            menuAdapter.submitList(items)
            binding.tvEmptyItems.visibility = if (items.isEmpty()) View.VISIBLE else View.GONE
        }

        viewModel.cart.observe(this) { cart ->
            cartAdapter.submitList(cart.toList())
            val count = cart.sumOf { it.quantity }
            binding.tvCartCount.text = if (count > 0) "$count item(s)" else "Cart is empty"
            binding.btnClearCart.visibility = if (cart.isNotEmpty()) View.VISIBLE else View.GONE
        }

        viewModel.subTotal.observe(this) { total ->
            binding.tvSubTotal.text = "₱${String.format("%.2f", total)}"
        }

        viewModel.cashGiven.observe(this) { cash ->
            if (cash > 0) binding.etCashGiven.setText(String.format("%.2f", cash))
        }

        viewModel.change.observe(this) { change ->
            binding.tvChange.text = "₱${String.format("%.2f", change)}"
            binding.tvChange.setTextColor(
                if (change >= 0) getColor(R.color.green_500)
                else getColor(R.color.red_500)
            )
        }

        viewModel.orderResult.observe(this) { order ->
            if (order != null) {
                val intent = Intent(this, ReceiptActivity::class.java).apply {
                    putExtra("order_number",  order.orderNumber)
                    putExtra("sub_total",     order.subTotal)
                    putExtra("cash_given",    order.cashGiven)
                    putExtra("change",        order.change)
                    putExtra("order_date",    order.orderDate)
                    putExtra("items_count",   order.items.size)
                    putExtra("item_names",    order.items.map { it.itemName }.toTypedArray())
                    putExtra("item_qtys",     order.items.map { it.quantity }.toIntArray())
                    putExtra("item_prices",   order.items.map { it.unitPrice }.toDoubleArray())
                    putExtra("item_totals",   order.items.map { it.lineTotal }.toDoubleArray())
                    putExtra("cashier_name",  session.getFullName().ifEmpty { session.getUsername() })
                }
                startActivity(intent)
                viewModel.clearOrderResult()
            }
        }
    }

    private fun setupListeners() {
        binding.btnSubmitOrder.setOnClickListener {
            val cashText = binding.etCashGiven.text.toString()
            val cash = cashText.toDoubleOrNull() ?: 0.0
            viewModel.setCashGiven(cash)

            val total = viewModel.subTotal.value ?: 0.0
            if (viewModel.cart.value.isNullOrEmpty()) {
                Toast.makeText(this, "Cart is empty", Toast.LENGTH_SHORT).show()
                return@setOnClickListener
            }
            if (cash < total) {
                Toast.makeText(this, "Cash given is less than total", Toast.LENGTH_SHORT).show()
                return@setOnClickListener
            }
            viewModel.submitOrder()
        }

        binding.etCashGiven.setOnFocusChangeListener { _, hasFocus ->
            if (!hasFocus) {
                val cash = binding.etCashGiven.text.toString().toDoubleOrNull() ?: 0.0
                viewModel.setCashGiven(cash)
            }
        }

        binding.btnClearCart.setOnClickListener {
            AlertDialog.Builder(this)
                .setTitle("Clear Cart")
                .setMessage("Remove all items from cart?")
                .setPositiveButton("Clear") { _, _ ->
                    viewModel.clearCart()
                    binding.etCashGiven.setText("")
                    menuAdapter.refreshQuantities()
                }
                .setNegativeButton("Cancel", null)
                .show()
        }

        binding.swipeRefresh.setOnRefreshListener {
            viewModel.loadData()
            binding.swipeRefresh.isRefreshing = false
        }
    }

    override fun onCreateOptionsMenu(menu: Menu): Boolean {
        menuInflater.inflate(R.menu.main_menu, menu)
        return true
    }

    override fun onOptionsItemSelected(item: MenuItem): Boolean {
        return when (item.itemId) {
            R.id.action_settings -> {
                startActivity(Intent(this, SettingsActivity::class.java))
                true
            }
            R.id.action_refresh -> {
                viewModel.loadData()
                true
            }
            R.id.action_logout -> {
                confirmLogout()
                true
            }
            else -> super.onOptionsItemSelected(item)
        }
    }

    private fun confirmLogout() {
        AlertDialog.Builder(this)
            .setTitle("Logout")
            .setMessage("Are you sure you want to logout?")
            .setPositiveButton("Logout") { _, _ ->
                session.clearSession()
                viewModel.clearCart()
                goToLogin()
            }
            .setNegativeButton("Cancel", null)
            .show()
    }

    private fun goToLogin() {
        val intent = Intent(this, LoginActivity::class.java)
        intent.flags = Intent.FLAG_ACTIVITY_NEW_TASK or Intent.FLAG_ACTIVITY_CLEAR_TASK
        startActivity(intent)
        finish()
    }
}
