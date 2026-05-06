package com.aronminimart.pos.data.session

import android.content.Context
import android.content.SharedPreferences

class SessionManager(context: Context) {

    private val prefs: SharedPreferences =
        context.getSharedPreferences("pos_session", Context.MODE_PRIVATE)

    companion object {
        private const val KEY_IS_LOGGED_IN = "is_logged_in"
        private const val KEY_USER_ID      = "user_id"
        private const val KEY_USERNAME     = "username"
        private const val KEY_FULL_NAME    = "full_name"
        private const val KEY_EMAIL        = "email"
        private const val KEY_ROLES        = "roles"
    }

    fun saveSession(
        userId: String,
        username: String,
        fullName: String,
        email: String,
        roles: List<String>
    ) {
        prefs.edit()
            .putBoolean(KEY_IS_LOGGED_IN, true)
            .putString(KEY_USER_ID,   userId)
            .putString(KEY_USERNAME,  username)
            .putString(KEY_FULL_NAME, fullName)
            .putString(KEY_EMAIL,     email)
            .putString(KEY_ROLES,     roles.joinToString(","))
            .apply()
    }

    fun clearSession() {
        prefs.edit()
            .putBoolean(KEY_IS_LOGGED_IN, false)
            .remove(KEY_USER_ID)
            .remove(KEY_USERNAME)
            .remove(KEY_FULL_NAME)
            .remove(KEY_EMAIL)
            .remove(KEY_ROLES)
            .apply()
    }

    fun isLoggedIn(): Boolean = prefs.getBoolean(KEY_IS_LOGGED_IN, false)

    fun getUserId(): String   = prefs.getString(KEY_USER_ID,   "") ?: ""
    fun getUsername(): String = prefs.getString(KEY_USERNAME,  "") ?: ""
    fun getFullName(): String = prefs.getString(KEY_FULL_NAME, "") ?: ""
    fun getEmail(): String    = prefs.getString(KEY_EMAIL,     "") ?: ""
    fun getRoles(): List<String> {
        val raw = prefs.getString(KEY_ROLES, "") ?: ""
        return if (raw.isEmpty()) emptyList() else raw.split(",")
    }
}
