package com.aronminimart.pos.data.session

import android.content.Context
import android.content.SharedPreferences

class SessionManager(context: Context) {

    private val prefs: SharedPreferences =
        context.getSharedPreferences("pos_session", Context.MODE_PRIVATE)

    companion object {
        private const val KEY_IS_LOGGED_IN       = "is_logged_in"
        private const val KEY_USER_ID            = "user_id"
        private const val KEY_USERNAME           = "username"
        private const val KEY_FULL_NAME          = "full_name"
        private const val KEY_EMAIL              = "email"
        private const val KEY_ROLES              = "roles"
        private const val KEY_PROFILE_IMAGE_PATH = "profile_image_path"
        private const val KEY_POSITION           = "position"
        private const val KEY_DEPARTMENT         = "department"
        private const val KEY_PHONE              = "phone"
    }

    fun saveSession(
        userId: String,
        username: String,
        fullName: String,
        email: String,
        roles: List<String>,
        profileImagePath: String? = null,
        position: String? = null,
        department: String? = null,
        phoneNumber: String? = null
    ) {
        prefs.edit()
            .putBoolean(KEY_IS_LOGGED_IN,       true)
            .putString(KEY_USER_ID,             userId)
            .putString(KEY_USERNAME,            username)
            .putString(KEY_FULL_NAME,           fullName)
            .putString(KEY_EMAIL,               email)
            .putString(KEY_ROLES,               roles.joinToString(","))
            .putString(KEY_PROFILE_IMAGE_PATH,  profileImagePath)
            .putString(KEY_POSITION,            position)
            .putString(KEY_DEPARTMENT,          department)
            .putString(KEY_PHONE,               phoneNumber)
            .apply()
    }

    fun clearSession() {
        prefs.edit().clear().apply()
    }

    fun isLoggedIn(): Boolean    = prefs.getBoolean(KEY_IS_LOGGED_IN, false)
    fun getUserId(): String      = prefs.getString(KEY_USER_ID,   "") ?: ""
    fun getUsername(): String    = prefs.getString(KEY_USERNAME,  "") ?: ""
    fun getFullName(): String    = prefs.getString(KEY_FULL_NAME, "") ?: ""
    fun getEmail(): String       = prefs.getString(KEY_EMAIL,     "") ?: ""
    fun getPosition(): String    = prefs.getString(KEY_POSITION,  "") ?: ""
    fun getDepartment(): String  = prefs.getString(KEY_DEPARTMENT,"") ?: ""
    fun getPhone(): String       = prefs.getString(KEY_PHONE,     "") ?: ""
    fun getProfileImagePath(): String? = prefs.getString(KEY_PROFILE_IMAGE_PATH, null)

    fun getRoles(): List<String> {
        val raw = prefs.getString(KEY_ROLES, "") ?: ""
        return if (raw.isEmpty()) emptyList() else raw.split(",")
    }
}
