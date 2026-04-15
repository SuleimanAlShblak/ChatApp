<template>
  <div class="login-container">
    <div class="login-card">
      <h1>ChatApp Login</h1>
      <form @submit.prevent="handleLogin" class="login-form">
        <div class="form-group">
          <label for="username">Username</label>
          <input
            id="username"
            v-model="username"
            type="text"
            required
            placeholder="Enter your username"
            class="form-input"
          />
        </div>
        <button type="submit" :disabled="loading" class="login-btn">
          {{ loading ? 'Logging in...' : 'Login' }}
        </button>
      </form>
      <p v-if="message" class="message">{{ message }}</p>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import axios from 'axios'

const router = useRouter()
const username = ref('')
const loading = ref(false)
const message = ref('')

// Check if user is already logged in
const checkLoggedIn = () => {
  const savedUser = localStorage.getItem('chatUser')
  if (savedUser) {
    const user = JSON.parse(savedUser)
    router.push({
      path: '/chat',
      query: { user: user.id, name: user.name }
    })
  }
}

onMounted(() => {
  checkLoggedIn()
})

const handleLogin = async () => {
  if (!username.value.trim()) return

  loading.value = true
  message.value = ''

  try {
    const response = await axios.post(`http://localhost:5001/api/user/login/${encodeURIComponent(username.value)}`)
    const data = response.data
    message.value = data.message

    // Save session to localStorage
    localStorage.setItem('chatUserId', data.userId)
    localStorage.setItem('chatUserName', username.value)

    // On success, navigate to chat with user params
    router.push({
      path: '/chat',
      query: { user: data.userId, name: username.value }
    })
  } catch (error) {
    console.error('Login error:', error)
    message.value = 'Login failed. Please try again.'
  } finally {
    loading.value = false
  }
}
</script>

<style scoped>
.login-container {
  min-height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
}

.login-card {
  background: white;
  padding: 2rem;
  border-radius: 10px;
  box-shadow: 0 10px 25px rgba(0, 0, 0, 0.1);
  width: 100%;
  max-width: 400px;
}

h1 {
  margin-bottom: 1.5rem;
  color: #333;
  font-size: 2rem;
}

.login-form {
  display: flex;
  flex-direction: column;
}

.form-group {
  margin-bottom: 1rem;
  text-align: left;
}

label {
  display: block;
  margin-bottom: 0.5rem;
  color: #555;
  font-weight: 500;
}

.form-input {
  width: 100%;
  padding: 0.75rem;
  border: 1px solid #ddd;
  border-radius: 5px;
  font-size: 1rem;
  transition: border-color 0.3s;
}

.form-input:focus {
  outline: none;
  border-color: #667eea;
}

.login-btn {
  background: #667eea;
  color: white;
  padding: 0.75rem;
  border: none;
  border-radius: 5px;
  font-size: 1rem;
  cursor: pointer;
  transition: background 0.3s;
  margin-top: 1rem;
}

.login-btn:hover:not(:disabled) {
  background: #5a6fd8;
}

.login-btn:disabled {
  background: #ccc;
  cursor: not-allowed;
}

.message {
  margin-top: 1rem;
  padding: 0.5rem;
  border-radius: 5px;
  font-weight: 500;
}

.message {
  background: #d4edda;
  color: #155724;
  border: 1px solid #c3e6cb;
}
</style>