package com.aronminimart.pos.ui

import android.os.Bundle
import android.view.MenuItem
import android.widget.Toast
import androidx.appcompat.app.AppCompatActivity
import com.aronminimart.pos.data.network.RetrofitClient
import com.aronminimart.pos.databinding.ActivitySettingsBinding

class SettingsActivity : AppCompatActivity() {

    private lateinit var binding: ActivitySettingsBinding

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        binding = ActivitySettingsBinding.inflate(layoutInflater)
        setContentView(binding.root)

        setSupportActionBar(binding.toolbar)
        supportActionBar?.setDisplayHomeAsUpEnabled(true)
        supportActionBar?.title = "Settings"

        binding.etApiUrl.setText(RetrofitClient.getCurrentUrl(this))

        binding.btnSaveUrl.setOnClickListener {
            val url = binding.etApiUrl.text.toString().trim()
            if (url.isEmpty()) {
                Toast.makeText(this, "URL cannot be empty", Toast.LENGTH_SHORT).show()
                return@setOnClickListener
            }
            val finalUrl = if (url.endsWith("/")) url else "$url/"
            RetrofitClient.updateBaseUrl(this, finalUrl)
            Toast.makeText(this, "API URL saved. Restart the app to apply.", Toast.LENGTH_LONG).show()
        }

        binding.btnResetUrl.setOnClickListener {
            val default = "http://10.0.2.2:5000/"
            binding.etApiUrl.setText(default)
            RetrofitClient.updateBaseUrl(this, default)
            Toast.makeText(this, "Reset to default URL", Toast.LENGTH_SHORT).show()
        }
    }

    override fun onOptionsItemSelected(item: MenuItem): Boolean {
        if (item.itemId == android.R.id.home) { finish(); return true }
        return super.onOptionsItemSelected(item)
    }
}
