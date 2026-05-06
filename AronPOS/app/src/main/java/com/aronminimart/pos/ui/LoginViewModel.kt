package com.aronminimart.pos.ui

import android.app.Application
import android.util.Log
import androidx.lifecycle.AndroidViewModel
import androidx.lifecycle.LiveData
import androidx.lifecycle.MutableLiveData
import androidx.lifecycle.viewModelScope
import com.aronminimart.pos.data.model.LoginRequest
import com.aronminimart.pos.data.model.LoginResponse
import com.aronminimart.pos.data.network.RetrofitClient
import com.aronminimart.pos.data.session.SessionManager
import kotlinx.coroutines.launch

class LoginViewModel(application: Application) : AndroidViewModel(application) {

    private val TAG = "POS_LOGIN"
    private val session = SessionManager(application)

    private val _loginState = MutableLiveData<LoginState>()
    val loginState: LiveData<LoginState> = _loginState

    private val _isLoading = MutableLiveData(false)
    val isLoading: LiveData<Boolean> = _isLoading

    sealed class LoginState {
        data class Success(val response: LoginResponse) : LoginState()
        data class Error(val message: String) : LoginState()
    }

    fun login(username: String, password: String) {
        if (username.isBlank() || password.isBlank()) {
            _loginState.value = LoginState.Error("Username and password are required.")
            return
        }

        viewModelScope.launch {
            _isLoading.value = true
            Log.i(TAG, "Attempting login for: $username")

            try {
                val api = RetrofitClient.getService(getApplication())
                val response = api.login(LoginRequest(username.trim(), password))

                if (response.isSuccessful) {
                    val body = response.body()!!
                    Log.i(TAG, "Login success: ${body.username}, roles: ${body.roles}")

                    // Save session
                    session.saveSession(
                        userId           = body.userId,
                        username         = body.username,
                        fullName         = body.fullName,
                        email            = body.email,
                        roles            = body.roles,
                        profileImagePath = body.profileImagePath,
                        position         = body.position,
                        department       = body.department,
                        phoneNumber      = body.phoneNumber
                    )
                    _loginState.value = LoginState.Success(body)
                } else {
                    val errorMsg = when (response.code()) {
                        401  -> "Invalid username or password."
                        400  -> "Username and password are required."
                        else -> "Login failed (${response.code()})"
                    }
                    Log.w(TAG, "Login failed: HTTP ${response.code()}")
                    _loginState.value = LoginState.Error(errorMsg)
                }
            } catch (e: java.net.ConnectException) {
                Log.e(TAG, "Login - ConnectException: ${e.message}")
                _loginState.value = LoginState.Error(
                    "Cannot connect to server.\nCheck API URL in Settings."
                )
            } catch (e: Exception) {
                Log.e(TAG, "Login error: ${e.message}", e)
                _loginState.value = LoginState.Error("Network error: ${e.message}")
            }

            _isLoading.value = false
        }
    }

    fun isLoggedIn(): Boolean = session.isLoggedIn()
    fun getUsername(): String = session.getUsername()
    fun getFullName(): String = session.getFullName()
}
