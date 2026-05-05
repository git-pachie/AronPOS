package com.aronminimart.pos.ui

import android.os.Bundle
import android.view.MenuItem
import androidx.appcompat.app.AppCompatActivity
import com.aronminimart.pos.databinding.ActivityReceiptBinding

class ReceiptActivity : AppCompatActivity() {

    private lateinit var binding: ActivityReceiptBinding

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        binding = ActivityReceiptBinding.inflate(layoutInflater)
        setContentView(binding.root)

        setSupportActionBar(binding.toolbar)
        supportActionBar?.setDisplayHomeAsUpEnabled(true)
        supportActionBar?.title = "Receipt"

        val orderNumber = intent.getStringExtra("order_number") ?: ""
        val subTotal    = intent.getDoubleExtra("sub_total", 0.0)
        val cashGiven   = intent.getDoubleExtra("cash_given", 0.0)
        val change      = intent.getDoubleExtra("change", 0.0)
        val orderDate   = intent.getStringExtra("order_date") ?: ""
        val itemNames   = intent.getStringArrayExtra("item_names") ?: emptyArray()
        val itemQtys    = intent.getIntArrayExtra("item_qtys") ?: intArrayOf()
        val itemPrices  = intent.getDoubleArrayExtra("item_prices") ?: doubleArrayOf()
        val itemTotals  = intent.getDoubleArrayExtra("item_totals") ?: doubleArrayOf()

        binding.tvReceiptOrderNumber.text = orderNumber
        binding.tvReceiptDate.text = orderDate.take(19).replace("T", " ")
        binding.tvReceiptSubTotal.text = "₱${String.format("%.2f", subTotal)}"
        binding.tvReceiptCashGiven.text = "₱${String.format("%.2f", cashGiven)}"
        binding.tvReceiptChange.text = "₱${String.format("%.2f", change)}"

        // Build items text
        val sb = StringBuilder()
        itemNames.forEachIndexed { i, name ->
            val qty   = if (i < itemQtys.size) itemQtys[i] else 0
            val price = if (i < itemPrices.size) itemPrices[i] else 0.0
            val total = if (i < itemTotals.size) itemTotals[i] else 0.0
            sb.appendLine("$name")
            sb.appendLine("  ${qty}x ₱${String.format("%.2f", price)} = ₱${String.format("%.2f", total)}")
        }
        binding.tvReceiptItems.text = sb.toString().trimEnd()

        binding.btnNewOrder.setOnClickListener { finish() }
    }

    override fun onOptionsItemSelected(item: MenuItem): Boolean {
        if (item.itemId == android.R.id.home) { finish(); return true }
        return super.onOptionsItemSelected(item)
    }
}
