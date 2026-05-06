package com.aronminimart.pos.data.network

import android.content.Context
import android.util.Log
import okhttp3.OkHttpClient
import okhttp3.logging.HttpLoggingInterceptor
import retrofit2.Retrofit
import retrofit2.converter.gson.GsonConverterFactory
import java.util.concurrent.TimeUnit

object RetrofitClient {

    private const val TAG = "POS_NETWORK"

    // Emulator → use 10.0.2.2 (maps to your PC localhost)
    // Real device → use your PC's LAN IP e.g. http://192.168.1.5:5000/
    private const val DEFAULT_BASE_URL = "http://192.168.254.102:5000/"

    private var baseUrl: String = DEFAULT_BASE_URL
    private var instance: ApiService? = null

    fun init(context: Context) {
        val prefs = context.getSharedPreferences("pos_prefs", Context.MODE_PRIVATE)
        baseUrl = prefs.getString("api_url", DEFAULT_BASE_URL) ?: DEFAULT_BASE_URL
        Log.i(TAG, "RetrofitClient initialized with base URL: $baseUrl")
        instance = null
    }

    fun updateBaseUrl(context: Context, url: String) {
        val prefs = context.getSharedPreferences("pos_prefs", Context.MODE_PRIVATE)
        prefs.edit().putString("api_url", url).apply()
        baseUrl = url
        instance = null
        Log.i(TAG, "Base URL updated to: $url")
    }

    fun getService(context: Context): ApiService {
        if (instance == null) {
            init(context)

            Log.d(TAG, "Building OkHttpClient and Retrofit for: $baseUrl")

            val logging = HttpLoggingInterceptor { message ->
                Log.d("POS_HTTP", message)
            }.apply {
                level = HttpLoggingInterceptor.Level.BODY
            }

            val client = OkHttpClient.Builder()
                .addInterceptor { chain ->
                    val request = chain.request()
                    Log.i(TAG, "→ ${request.method} ${request.url}")
                    try {
                        val response = chain.proceed(request)
                        Log.i(TAG, "← ${response.code} ${request.url}")
                        response
                    } catch (e: Exception) {
                        Log.e(TAG, "✗ Request FAILED: ${request.url}")
                        Log.e(TAG, "  Error type : ${e.javaClass.simpleName}")
                        Log.e(TAG, "  Message    : ${e.message}")
                        Log.e(TAG, "  Cause      : ${e.cause?.message}")
                        throw e
                    }
                }
                .addInterceptor(logging)
                .connectTimeout(30, TimeUnit.SECONDS)
                .readTimeout(30, TimeUnit.SECONDS)
                .writeTimeout(30, TimeUnit.SECONDS)
                .retryOnConnectionFailure(true)
                .build()

            instance = Retrofit.Builder()
                .baseUrl(baseUrl)
                .client(client)
                .addConverterFactory(GsonConverterFactory.create())
                .build()
                .create(ApiService::class.java)

            Log.i(TAG, "Retrofit instance created successfully")
        }
        return instance!!
    }

    fun getCurrentUrl(context: Context): String {
        val prefs = context.getSharedPreferences("pos_prefs", Context.MODE_PRIVATE)
        return prefs.getString("api_url", DEFAULT_BASE_URL) ?: DEFAULT_BASE_URL
    }

    // Returns the currently active base URL (no Context needed after init)
    fun getBaseUrl(): String = baseUrl
}
