package com.aronminimart.pos.ui

import android.app.AlertDialog
import android.app.Dialog
import android.content.Intent
import android.os.Bundle
import android.view.View
import android.view.Window
import android.widget.Toast
import androidx.activity.viewModels
import androidx.appcompat.app.AppCompatActivity
import androidx.recyclerview.widget.GridLayoutManager
import com.aronminimart.pos.R
import com.aronminimart.pos.data.network.RetrofitClient
import com.aronminimart.pos.data.session.SessionManager
import com.aronminimart.pos.databinding.ActivityMainBinding
import com.aronminimart.pos.databinding.DialogUserProfileBinding
import com.aronminimart.pos.ui.adapter.CartAdapter
import com.aronminimart.pos.ui.adapter.MenuItemAdapter
import com.bumptech.glide.Glide
import com.bumptech.glide.load.engine.DiskCacheStrategy
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

        if (!session.isLoggedIn()) {
            goToLogin()
            return
        }

        binding = ActivityMainBinding.inflate(layoutInflater)
        setContentView(binding.root)

        setupAdapters()
        observeViewModel()
        setupListeners()
        setupHeaderAvatar()
    }

    // ── Header avatar setup ───────────────────────────────────────────────────

    private fun setupHeaderAvatar() {
        val fullName  = session.getFullName().ifEmpty { session.getUsername() }
        val imagePath = session.getProfileImagePath()

        android.util.Log.d("POS_PROFILE", "setupHeaderAvatar: imagePath=$imagePath")

        if (!imagePath.isNullOrEmpty()) {
            // Use getCurrentUrl(context) to ensure we have the saved URL, not the default
            val baseUrl = RetrofitClient.getCurrentUrl(this).trimEnd('/')
            val fullUrl = if (imagePath.startsWith("http")) imagePath
                          else "$baseUrl$imagePath"

            android.util.Log.i("POS_PROFILE", "Loading avatar from: $fullUrl")

            Glide.with(this)
                .load(fullUrl)
                .diskCacheStrategy(DiskCacheStrategy.NONE)   // disable cache for debugging
                .skipMemoryCache(true)
                .circleCrop()
                .placeholder(android.R.drawable.ic_menu_gallery)
                .error(android.R.drawable.ic_menu_report_image)
                .listener(object : com.bumptech.glide.request.RequestListener<android.graphics.drawable.Drawable> {
                    override fun onLoadFailed(e: com.bumptech.glide.load.engine.GlideException?,
                        model: Any?, target: com.bumptech.glide.request.target.Target<android.graphics.drawable.Drawable>?,
                        isFirstResource: Boolean): Boolean {
                        android.util.Log.e("POS_PROFILE", "Avatar load FAILED: $fullUrl — ${e?.message}")
                        e?.logRootCauses("POS_PROFILE")
                        return false
                    }
                    override fun onResourceReady(resource: android.graphics.drawable.Drawable?,
                        model: Any?, target: com.bumptech.glide.request.target.Target<android.graphics.drawable.Drawable>?,
                        dataSource: com.bumptech.glide.load.DataSource?, isFirstResource: Boolean): Boolean {
                        android.util.Log.i("POS_PROFILE", "Avatar load SUCCESS: $fullUrl")
                        return false
                    }
                })
                .into(binding.ivAvatarPhoto)

            binding.ivAvatarPhoto.visibility    = View.VISIBLE
            binding.tvAvatarInitials.visibility = View.GONE
        } else {
            android.util.Log.d("POS_PROFILE", "No profile image path — showing initials")
            val initials = fullName
                .split(" ").filter { it.isNotEmpty() }.take(2)
                .joinToString("") { it[0].uppercaseChar().toString() }
            binding.tvAvatarInitials.text       = initials.ifEmpty { "?" }
            binding.tvAvatarInitials.visibility = View.VISIBLE
            binding.ivAvatarPhoto.visibility    = View.GONE
        }
    }

    // ── User profile popup ────────────────────────────────────────────────────

    private fun showUserProfileDialog() {
        val dialog = Dialog(this)
        dialog.requestWindowFeature(Window.FEATURE_NO_TITLE)
        val dialogBinding = DialogUserProfileBinding.inflate(layoutInflater)
        dialog.setContentView(dialogBinding.root)

        // Set dialog width to 90% of screen
        dialog.window?.setLayout(
            (resources.displayMetrics.widthPixels * 0.90).toInt(),
            android.view.ViewGroup.LayoutParams.WRAP_CONTENT
        )
        dialog.window?.setBackgroundDrawableResource(android.R.color.transparent)

        // Populate data from session
        val fullName   = session.getFullName().ifEmpty { session.getUsername() }
        val username   = session.getUsername()
        val email      = session.getEmail()
        val position   = session.getPosition().ifEmpty { "—" }
        val department = session.getDepartment().ifEmpty { "—" }
        val phone      = session.getPhone().ifEmpty { "—" }
        val roles      = session.getRoles()
        val imagePath  = session.getProfileImagePath()

        dialogBinding.tvDialogFullName.text  = fullName
        dialogBinding.tvDialogUsername.text  = username.ifEmpty { "—" }
        dialogBinding.tvDialogEmail.text     = email.ifEmpty { "—" }
        dialogBinding.tvDialogPosition.text  = position
        dialogBinding.tvDialogDepartment.text = department
        dialogBinding.tvDialogPhone.text     = phone
        dialogBinding.tvDialogRoles.text     = if (roles.isNotEmpty())
            roles.joinToString(" • ") else "No roles"

        // Load profile photo or show initials
        if (!imagePath.isNullOrEmpty()) {
            val baseUrl = RetrofitClient.getCurrentUrl(this).trimEnd('/')
            val fullUrl = if (imagePath.startsWith("http")) imagePath else "$baseUrl$imagePath"

            android.util.Log.i("POS_PROFILE", "Loading dialog photo from: $fullUrl")

            Glide.with(this)
                .load(fullUrl)
                .diskCacheStrategy(DiskCacheStrategy.NONE)
                .skipMemoryCache(true)
                .circleCrop()
                .placeholder(android.R.drawable.ic_menu_gallery)
                .error(android.R.drawable.ic_menu_report_image)
                .into(dialogBinding.ivDialogPhoto)

            dialogBinding.ivDialogPhoto.visibility    = View.VISIBLE
            dialogBinding.tvDialogInitials.visibility = View.GONE
        } else {
            val initials = fullName
                .split(" ")
                .filter { it.isNotEmpty() }
                .take(2)
                .joinToString("") { it[0].uppercaseChar().toString() }
            dialogBinding.tvDialogInitials.text = initials.ifEmpty { "?" }
            dialogBinding.tvDialogInitials.visibility = View.VISIBLE
            dialogBinding.ivDialogPhoto.visibility    = View.GONE
        }

        // Buttons
        dialogBinding.btnDialogClose.setOnClickListener { dialog.dismiss() }
        dialogBinding.btnDialogLogout.setOnClickListener {
            dialog.dismiss()
            confirmLogout()
        }

        dialog.show()
    }

    // ── Adapters ──────────────────────────────────────────────────────────────

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

    // ── ViewModel observers ───────────────────────────────────────────────────

    private fun observeViewModel() {
        viewModel.storeName.observe(this) { name ->
            binding.tvStoreName.text = name
            val cashier = session.getFullName().ifEmpty { session.getUsername() }
            binding.tvCashierName.text = "Cashier: $cashier"
        }

        viewModel.isLoading.observe(this) { loading ->
            binding.progressBar.visibility   = if (loading) View.VISIBLE else View.GONE
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
                    putExtra("order_number", order.orderNumber)
                    putExtra("sub_total",    order.subTotal)
                    putExtra("cash_given",   order.cashGiven)
                    putExtra("change",       order.change)
                    putExtra("order_date",   order.orderDate)
                    putExtra("items_count",  order.items.size)
                    putExtra("item_names",   order.items.map { it.itemName }.toTypedArray())
                    putExtra("item_qtys",    order.items.map { it.quantity }.toIntArray())
                    putExtra("item_prices",  order.items.map { it.unitPrice }.toDoubleArray())
                    putExtra("item_totals",  order.items.map { it.lineTotal }.toDoubleArray())
                    putExtra("cashier_name", session.getFullName().ifEmpty { session.getUsername() })
                }
                startActivity(intent)
                viewModel.clearOrderResult()
            }
        }
    }

    // ── Listeners ─────────────────────────────────────────────────────────────

    private fun setupListeners() {
        binding.btnSubmitOrder.setOnClickListener {
            val cash = binding.etCashGiven.text.toString().toDoubleOrNull() ?: 0.0
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

        binding.btnRefresh.setOnClickListener { viewModel.loadData() }
        binding.btnSettings.setOnClickListener {
            startActivity(Intent(this, SettingsActivity::class.java))
        }

        // Profile avatar button → show popup
        binding.btnUserProfile.setOnClickListener { showUserProfileDialog() }
    }

    // ── Logout ────────────────────────────────────────────────────────────────

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
