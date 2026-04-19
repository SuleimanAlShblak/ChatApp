<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import axios from 'axios'
import Button from '../components/Button.vue'
import InputField from '../components/InputField.vue'

const router = useRouter()
const username = ref('')
const loading = ref(false)
const message = ref('')
const messageType = ref<'success' | 'error'>('success')

// Check if user is already logged in
const checkLoggedIn = () => {
  const savedUser = localStorage.getItem('chatUser')
  if (savedUser) {
    const user = JSON.parse(savedUser)
    router.push({
      path: '/chat',
      query: { user: user.id, name: user.name },
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
  messageType.value = 'success'

  try {
    const response = await axios.post(
      `http://localhost:5001/api/user/login/${encodeURIComponent(username.value)}`,
    )
    const data = response.data
    message.value = data.message
    messageType.value = 'success'

    // Save session to localStorage
    localStorage.setItem('chatUserId', data.userId)
    localStorage.setItem('chatUserName', username.value)

    // On success, navigate to chat with user params
    router.push({
      path: '/chat',
      query: { user: data.userId, name: username.value },
    })
  } catch (error) {
    console.error('Login error:', error)
    messageType.value = 'error'
    message.value = 'Login failed. Please try again.'
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <div class="login-container">
    <div class="login-card">
      <h1>Chat App Login</h1>
      <form @submit.prevent="handleLogin" class="login-form">
        <div class="form-group">
          <InputField
            input-id="username"
            v-model="username"
            type="text"
            label="Username"
            :required="true"
            :disabled="loading"
            placeholder="Enter your username"
            class="form-input"
          />
        </div>
        <Button :text="loading ? 'Logging in...' : 'Login'" :disabled="loading" class="login-btn" />
      </form>
      <p v-if="message" :class="['message', `message--${messageType}`]">{{ message }}</p>
    </div>
  </div>
</template>

<style scoped>
.login-container {
  min-height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  background: linear-gradient(135deg, #f4f8ff 0%, #e8f1ff 100%);
}

.login-card {
  background: #ffffff;
  padding: 2rem;
  border-radius: 10px;
  box-shadow: 0 10px 30px rgba(59, 130, 246, 0.12);
  border: 1px solid #dbeafe;
  width: 100%;
  max-width: 400px;
}

h1 {
  margin-bottom: 1.5rem;
  color: #000000;
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
  border-color: #60a5fa;
}

.login-btn:hover:not(:disabled) {
  background: #ffffff;
  color: #000000;
  border: 1px solid #667eea;
}

.login-btn:disabled {
  background: #ccc;
  color: white;
  border: none;
  cursor: not-allowed;
}

.login-btn:disabled:hover {
  background: #ccc;
  color: white;
  border: none;
}

.message {
  margin-top: 1rem;
  padding: 0.5rem;
  border-radius: 5px;
  font-weight: 500;
}
.message--error {
  background: #f8d7da;
  color: #842029;
  border: 1px solid #f5c2c7;
}
</style>
