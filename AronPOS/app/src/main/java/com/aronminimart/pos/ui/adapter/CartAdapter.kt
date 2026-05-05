package com.aronminimart.pos.ui.adapter

import android.view.LayoutInflater
import android.view.ViewGroup
import androidx.recyclerview.widget.RecyclerView
import com.aronminimart.pos.data.model.CartItem
import com.aronminimart.pos.databinding.ItemCartRowBinding

class CartAdapter(
    private val onRemove: (CartItem) -> Unit
) : RecyclerView.Adapter<CartAdapter.ViewHolder>() {

    private val items = mutableListOf<CartItem>()

    fun submitList(newItems: List<CartItem>) {
        items.clear()
        items.addAll(newItems)
        notifyDataSetChanged()
    }

    inner class ViewHolder(val binding: ItemCartRowBinding) :
        RecyclerView.ViewHolder(binding.root) {

        fun bind(item: CartItem) {
            binding.tvCartItemName.text = "${item.menuItem.name} x${item.quantity}"
            binding.tvCartItemTotal.text = "₱${String.format("%.2f", item.lineTotal)}"
            binding.btnCartRemove.setOnClickListener { onRemove(item) }
        }
    }

    override fun onCreateViewHolder(parent: ViewGroup, viewType: Int): ViewHolder {
        val binding = ItemCartRowBinding.inflate(
            LayoutInflater.from(parent.context), parent, false
        )
        return ViewHolder(binding)
    }

    override fun onBindViewHolder(holder: ViewHolder, position: Int) =
        holder.bind(items[position])

    override fun getItemCount() = items.size
}
