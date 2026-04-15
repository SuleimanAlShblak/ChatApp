<script setup lang="ts">
import { computed, ref, onMounted, onUnmounted } from 'vue'
import TopBar from './componets/TopBar.vue'
import SideBar from './componets/SideBar.vue'
import Button from './componets/Button.vue'
import InputField from './componets/InputField.vue'
import MessageBubble from './componets/MessageBubble.vue'
import chatService, { type ChatMessageDto, type User } from './services/chatService'
import { useRouter } from 'vue-router'
import axios from 'axios'


interface ChatMessage {
  id: string
  sender: string
  message: string
  time: string
  isOwn?: boolean
}

const router = useRouter()
const sidebarCollapsed = ref(false)
const activeChatId = ref<string | null>(null)
const messageText = ref('')
const currentUser = ref({ id: '', name: '' })
const users = ref<User[]>([])
const messages = ref<Record<string, ChatMessage[]>>({})
const typingUsers = ref<Set<string>>(new Set())
const connectError = ref('')

// Initialize user (check localStorage first, then URL params)
const initUser = () => {
  // Check localStorage for saved session
  const savedUserId = localStorage.getItem('chatUserId')
  const savedUserName = localStorage.getItem('chatUserName')

  if (savedUserId && savedUserName) {
    currentUser.value = { id: savedUserId, name: savedUserName }
    return
  }

  // Fallback to URL params
  const urlParams = new URLSearchParams(window.location.search)
  const userId = urlParams.get('user') || 'user1'
  const userName = urlParams.get('name') || 'User 1'
  currentUser.value = { id: userId, name: userName }

  // Save to localStorage for future sessions
  localStorage.setItem('chatUserId', userId)
  localStorage.setItem('chatUserName', userName)
}

onMounted(async () => {
  console.log('Chat mounted, initializing user')
  initUser()
  console.log('Current user:', currentUser.value)

  // TODO: look how fetching is done
  // Fetch user list from backend REST API on mount 
  try {
    const response = await axios.get('http://localhost:5001/api/user/all')
    users.value = response.data
    if (!activeChatId.value && users.value.length > 0) {
      activeChatId.value = users.value[0]?.Id || ''
    }
    console.log('Fetched user list from backend:', users.value)
  } catch (err) {
    console.error('Failed to fetch user list from backend:', err)
  }

  // Set up event handlers
  chatService.onReceiveMessage = (message: ChatMessageDto) => {
    console.log('Received message:', message)
    if (message.Type === 'chat') {
      const chatMessage: ChatMessage = {
        id: `msg-${Date.now()}`,
        sender: message.SenderId,
        message: message.Data,
        time: new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }),
        isOwn: message.SenderId === currentUser.value.id
      }
      const chatId = message.SenderId === currentUser.value.id ? message.ReceiverId : message.SenderId
      if (!messages.value[chatId]) messages.value[chatId] = []
      messages.value[chatId].push(chatMessage)
    }
  }


  chatService.onTyping = (typingEvent: ChatMessageDto) => {
    console.log('Typing event:', typingEvent)
    if (typingEvent.Data === 'true') {
      typingUsers.value.add(typingEvent.SenderId)
    } else {
      typingUsers.value.delete(typingEvent.SenderId)
    }
  }

  chatService.onReceiveError = (error: string) => {
    console.error('Received error:', error)
    connectError.value = error
    alert(`Error: ${error}`)
  }

  chatService.onUserConnected = (user: User) => {
    console.log('User connected:', user)
    // Add the user to the list (including self for testing)
    users.value = users.value.filter(u => u.Id !== user.Id) // Remove if already exists
    users.value.push(user)
    if (!activeChatId.value) {
      activeChatId.value = user.Id || ''
    }
  }

  console.log('Connecting to chat...')
  try {
    await chatService.connect({ Id: currentUser.value.id, UserName: currentUser.value.name, ChatRoom: 'general' })
    console.log('Connected successfully')
  } catch (error) {
    console.error('Chat connection failed', error)
    connectError.value = 'Chat connection failed: see console for details.'
  }
})

onUnmounted(() => {
  chatService.disconnect()
})

const sidebarItems = computed(() => {
    console.log('Computing sidebar items with users:', users.value, 'activeChatId:', activeChatId.value)
    return users.value.map(user => ({
      id: user.Id,
      type: 'card',
      title: user.UserName,
      description: user.Status === 'online' ? 'Online' : 'Offline',
      icon: '👤',
      clickable: true,
      selected: user.Id === activeChatId.value
    }))
}
  
)

const activeChat = computed(() => users.value.find(u => u.Id === activeChatId.value))
const activeMessages = computed(() => activeChatId.value ? messages.value[activeChatId.value] || [] : [])
const isTyping = computed(() => activeChatId.value && typingUsers.value.has(activeChatId.value))

const handleSidebarItemClick = (item: any) => {
  if (item.type === 'card' && item.id) {
    activeChatId.value = item.id
  } else if (item.type === 'button' && item.data?.action === 'new-chat') {
    // Handle new chat
    console.log('New chat clicked')
    // For now, just alert
    alert('New chat functionality not implemented yet')
  }
}

const handleMenuClick = () => {
  sidebarCollapsed.value = !sidebarCollapsed.value
}

const sendMessage = async () => {
  console.log('sendMessage called')
  const trimmed = messageText.value.trim()
  console.log('trimmed:', trimmed, 'activeChatId:', activeChatId.value)
  if (!trimmed || !activeChatId.value) {
    console.log('returning early')
    return
  }

  const message: ChatMessageDto = {
    Type: 'chat',
    SenderId: currentUser.value.id,
    ReceiverId: activeChatId.value as string,
    Data: trimmed
  }

  console.log('sending message:', message)
  await chatService.sendMessage(message)

  messageText.value = ''
}

// Typing indicator
let typingTimeout: number | null = null
const handleInput = () => {
  if (!activeChatId.value) return

  const typingEvent: ChatMessageDto = {
    Type: 'typing',
    SenderId: currentUser.value.id,
    ReceiverId: activeChatId.value as string,
    Data: 'true'
  }
  chatService.sendTyping(typingEvent)

  if (typingTimeout) clearTimeout(typingTimeout)
  typingTimeout = window.setTimeout(() => {
    const stopTypingEvent: ChatMessageDto = {
      Type: 'typing',
      SenderId: currentUser.value.id,
      ReceiverId: activeChatId.value as string,
      Data: 'false'
    }
    chatService.sendTyping(stopTypingEvent)
  }, 1000)
}

const handleLogout = async () => {
  try {
    await axios.post(`http://localhost:5001/api/user/logout/${currentUser.value.id}`)
    // Clear session from localStorage
    localStorage.removeItem('chatUserId')
    localStorage.removeItem('chatUserName')
    router.push('/')
  } catch (error) {
    console.error('Logout error:', error)
  }
}

</script>

<template>
  <div class="app">
    <TopBar title="Dirs21 Chat" showMenuButton :sticky="true" @menuClick="handleMenuClick">
      <template #actions>
        <Button text="Logout" variant="outline" size="sm" @click="handleLogout" />
      </template>
    </TopBar>

    <div class="app-content">
      <div v-if="connectError" class="connection-error">
        {{ connectError }}
      </div>
      <SideBar
        :items="users"
        title="Dirs21 Chat"
        :collapsible="true"
        :collapsed="sidebarCollapsed"
        width="280px"
        @itemClick="handleSidebarItemClick"
      />


      <main class="chat-panel">
        <section class="chat-header">
          <div class="chat-contact-info">
            <div class="chat-avatar">👤</div>
            <div>
              <h2>{{ activeChat?.UserName || 'Select a chat' }}</h2>
              <p>{{ activeChat ? (activeChat.Status === 'online' ? 'Online' : 'Offline') : 'Choose a conversation from the left' }}</p>
            </div>
          </div>
          <Button text="Info" variant="ghost" size="sm" />
        </section>

        <section class="chat-body">
          <div class="message-list">
            <MessageBubble
              v-for="message in activeMessages"
              :key="message.id"
              :sender="message.sender"
              :message="message.message"
              :time="message.time"
              :isOwn="message.isOwn"
            />
          </div>
          <div v-if="isTyping" class="typing-indicator">
            {{ activeChat?.UserName }} is typing...
          </div>
        </section>

        <section class="chat-input-row">
          <InputField v-model="messageText" placeholder="Type a message..." @enter="sendMessage" @input="handleInput" />
          <Button text="Send" variant="primary" size="md" @click="sendMessage" />
        </section>
      </main>
    </div>
  </div>
</template>

<style>
.app {
  display: flex;
  flex-direction: column;
  min-height: 100vh;
  background: #f3f4f6;
}

.app-content {
  display: flex;
  flex: 1;
  overflow: hidden;
}

.chat-panel {
  flex: 1;
  display: flex;
  flex-direction: column;
  background: #f8fafc;
  overflow: hidden;
}

.chat-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 1rem;
  padding: 1.5rem;
  border-bottom: 1px solid #e5e7eb;
  background: white;
}

.chat-contact-info {
  display: flex;
  align-items: center;
  gap: 1rem;
}

.chat-avatar {
  width: 3rem;
  height: 3rem;
  display: grid;
  place-items: center;
  background: #e2e8f0;
  border-radius: 1rem;
  font-size: 1.25rem;
}

.chat-header h2 {
  font-size: 1.25rem;
  font-weight: 700;
  margin: 0;
  color: #111827;
}

.chat-header p {
  margin: 0.25rem 0 0;
  color: #6b7280;
  font-size: 0.95rem;
}

.chat-body {
  flex: 1;
  overflow-y: auto;
  padding: 1.5rem;
}

.message-list {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.chat-input-row {
  display: flex;
  gap: 0.75rem;
  align-items: center;
  padding: 1rem 1.5rem 1.5rem;
  background: white;
  border-top: 1px solid #e5e7eb;
}

.chat-input-row .input-field {
  flex: 1;
}

@media (max-width: 900px) {
  .app-content {
    flex-direction: column;
  }

  .chat-panel {
    min-height: 0;
  }
}

.connection-error {
  padding: 1rem;
  margin: 0 1.5rem 1rem;
  background: #fee2e2;
  color: #991b1b;
  border: 1px solid #fecaca;
  border-radius: 0.75rem;
}
</style>