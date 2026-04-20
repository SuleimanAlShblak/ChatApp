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

<style>
</style>
