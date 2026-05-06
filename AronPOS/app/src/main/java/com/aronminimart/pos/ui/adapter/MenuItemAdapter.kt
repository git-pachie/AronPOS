package com.aronminimart.pos.ui.adapter

import android.view.LayoutInflater
import android.view.View
import android.view.ViewGroup
import androidx.recyclerview.widget.DiffUtil
import androidx.recyclerview.widget.ListAdapter
import androidx.recyclerview.widget.RecyclerView
import com.aronminimart.pos.data.model.MenuItem
import com.aronminimart.pos.data.network.RetrofitClient
import com.aronminimart.pos.databinding.ItemMenuProductBinding
import com.bumptech.glide.Glide
import com.bumptech.glide.load.engine.DiskCacheStrategy

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
            binding.tvItemName.text  = item.name
            binding.tvItemPrice.text = "₱${String.format("%.2f", item.price)}"

            // Load product image with Glide
            val imagePath = item.imagePath
            if (!imagePath.isNullOrEmpty()) {
                // Build full URL from base URL + relative path
                val baseUrl = RetrofitClient.getBaseUrl()
                    .trimEnd('/')
                val fullUrl = if (imagePath.startsWith("http")) imagePath
                              else "$baseUrl$imagePath"

                Glide.with(binding.root.context)
                    .load(fullUrl)
                    .diskCacheStrategy(DiskCacheStrategy.ALL)
                    .centerCrop()
                    .placeholder(android.R.drawable.ic_menu_gallery)
                    .error(android.R.drawable.ic_menu_report_image)
                    .into(binding.ivItemImage)

                binding.ivItemImage.visibility = View.VISIBLE
                binding.tvItemIcon.visibility  = View.GONE
            } else {
                binding.ivItemImage.visibility = View.GONE
                binding.tvItemIcon.visibility  = View.VISIBLE
            }

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
            binding.tvQty.text         = if (qty > 0) qty.toString() else ""
            binding.btnRemove.isEnabled = qty > 0
            binding.btnRemove.alpha    = if (qty > 0) 1f else 0.3f
            binding.root.strokeWidth   = if (qty > 0) 3 else 0
        }
    }

    override fun onCreateViewHolder(parent: ViewGroup, viewType: Int): ViewHolder {
        val binding = ItemMenuProductBinding.inflate(
            LayoutInflater.from(parent.context), parent, false
        )
        return ViewHolder(binding)
    }

    override fun onBindViewHolder(holder: ViewHolder, position: Int) =
        holder.bind(getItem(position))

    fun refreshQuantities() = notifyDataSetChanged()
}
