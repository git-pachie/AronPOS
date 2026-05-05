package com.aronminimart.pos.ui.adapter

import android.view.LayoutInflater
import android.view.ViewGroup
import androidx.recyclerview.widget.DiffUtil
import androidx.recyclerview.widget.ListAdapter
import androidx.recyclerview.widget.RecyclerView
import com.aronminimart.pos.data.model.MenuItem
import com.aronminimart.pos.databinding.ItemMenuProductBinding

class MenuItemAdapter(
    private val onAdd: (MenuItem) -> Unit,
    private val onRemove: (MenuItem) -> Unit,
    private val getQty: (Int) -> Int
) : ListAdapter<MenuItem, MenuItemAdapter.ViewHolder>(DIFF) {

    companion object {
        val DIFF = object : DiffUtil.ItemCallback<MenuItem>() {
            override fun areItemsTheSame(a: MenuItem, b: MenuItem) = a.id == b.id
            override fun areContentsTheSame(a: MenuItem, b: MenuItem) = a == b
        }
    }

    inner class ViewHolder(val binding: ItemMenuProductBinding) :
        RecyclerView.ViewHolder(binding.root) {

        fun bind(item: MenuItem) {
            binding.tvItemName.text = item.name
            binding.tvItemPrice.text = "₱${String.format("%.2f", item.price)}"

            val qty = getQty(item.id)
            updateQtyDisplay(qty)

            binding.btnAdd.setOnClickListener {
                onAdd(item)
                updateQtyDisplay(getQty(item.id))
            }
            binding.btnRemove.setOnClickListener {
                onRemove(item)
                updateQtyDisplay(getQty(item.id))
            }
        }

        private fun updateQtyDisplay(qty: Int) {
            binding.tvQty.text = if (qty > 0) qty.toString() else ""
            binding.btnRemove.isEnabled = qty > 0
            binding.btnRemove.alpha = if (qty > 0) 1f else 0.3f
            // Highlight card if in cart
            binding.root.strokeWidth = if (qty > 0) 3 else 0
        }
    }

    override fun onCreateViewHolder(parent: ViewGroup, viewType: Int): ViewHolder {
        val binding = ItemMenuProductBinding.inflate(
            LayoutInflater.from(parent.context), parent, false
        )
        return ViewHolder(binding)
    }

    override fun onBindViewHolder(holder: ViewHolder, position: Int) {
        holder.bind(getItem(position))
    }

    fun refreshQuantities() {
        notifyDataSetChanged()
    }
}
