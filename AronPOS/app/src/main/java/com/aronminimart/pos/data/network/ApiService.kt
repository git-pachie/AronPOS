package com.aronminimart.pos.data.network

import com.aronminimart.pos.data.model.*
import retrofit2.Response
import retrofit2.http.*

interface ApiService {

    @GET("api/menu/categories/with-items")
    suspend fun getCategoriesWithItems(): Response<List<MenuCategory>>

    @GET("api/menu/items")
    suspend fun getItems(@Query("categoryId") categoryId: Int? = null): Response<List<MenuItem>>

    @POST("api/orders")
    suspend fun createOrder(@Body request: CreateOrderRequest): Response<OrderResponse>

    @GET("api/store/info")
    suspend fun getStoreInfo(): Response<StoreInfo>

    @GET("api/store/health")
    suspend fun health(): Response<Any>
}
