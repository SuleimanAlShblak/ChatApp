<script setup lang="ts">
import { computed, ref, onMounted, onUnmounted, watch } from 'vue'
import TopBar from './componets/TopBar.vue'
import SideBar from './componets/SideBar.vue'
import Button from './componets/Button.vue'
import InputField from './componets/InputField.vue'
import MessageBubble from './componets/MessageBubble.vue'
import chatService, { normalizeUser, type ChatMessageDto, type StoredChatMessage, type User } from './services/chatService'
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

const isValidUserId = (value: string | null | undefined): value is string => {
  return Boolean(value && value !== 'undefined' && value !== 'null')
}

const syncCurrentUserId = (availableUsers: User[]) => {
  if (!currentUser.value.name.trim()) {
    return
  }

  const normalizedName = currentUser.value.name.trim().toLowerCase()
  const matchingUser = availableUsers.find(user => {
    const candidateName = (user.UserName || user.DisplayName || '').trim().toLowerCase()
    return candidateName === normalizedName
  })

  if (matchingUser?.Id && matchingUser.Id !== currentUser.value.id) {
    currentUser.value = { ...currentUser.value, id: matchingUser.Id }
    localStorage.setItem('chatUserId', matchingUser.Id)
    localStorage.setItem('chatUserName', currentUser.value.name)
  }
}

const mapMessageUI = (message: StoredChatMessage): ChatMessage => {
  const senderName = message.SenderId === currentUser.value.id
    ? currentUser.value.name
    : users.value.find(user => user.Id === message.SenderId)?.UserName || message.SenderId

  const timestamp = message.Timestamp || message.timestamp
  const displayTime = timestamp
    ? new Date(timestamp).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })
    : new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })

  return {
    id: `${message.SenderId}-${message.ReceiverId}-${timestamp || Date.now()}-${message.Data}`,
    sender: senderName,
    message: message.Data,
    time: displayTime,
    isOwn: message.SenderId === currentUser.value.id
  }
}

const loadConversation = async (chatId: string) => {
  if (!currentUser.value.id || !chatId) {
    return
  }

  try {
    const history = await chatService.getChatHistory(currentUser.value.id, chatId)
    messages.value[chatId] = history.map(mapMessageUI)
  } catch (error) {
    console.error('Failed to load conversation history:', error)
  }
}

// Initialize user (check localStorage first, then URL params)
const initUser = () => {
  const savedUserId = localStorage.getItem('chatUserId')
  const savedUserName = localStorage.getItem('chatUserName')

  if (savedUserName) {
    currentUser.value = {
      id: isValidUserId(savedUserId) ? savedUserId : '',
      name: savedUserName
    }

    if (!isValidUserId(savedUserId)) {
      localStorage.removeItem('chatUserId')
    }
    return
  }

  const urlParams = new URLSearchParams(window.location.search)
  const userId = urlParams.get('user')
  const userName = urlParams.get('name') || 'User 1'

  currentUser.value = {
    id: isValidUserId(userId) ? userId : '',
    name: userName
  }

  if (isValidUserId(userId)) {
    localStorage.setItem('chatUserId', userId)
  }
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
    users.value = (response.data as User[]).map(normalizeUser)
    syncCurrentUserId(users.value)
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
      const chatId = message.SenderId === currentUser.value.id ? message.ReceiverId : message.SenderId
      const chatMessage = mapMessageUI(message)

      if (!messages.value[chatId]) {
        messages.value[chatId] = []
      }

      const alreadyExists = messages.value[chatId].some(existing =>
        existing.message === chatMessage.message &&
        existing.sender === chatMessage.sender &&
        existing.isOwn === chatMessage.isOwn
      )

      if (!alreadyExists) {
        messages.value[chatId].push(chatMessage)
      }
    }
  }

  chatService.onUserListUpdated = (updatedUsers: User[]) => {
    const normalizedUsers = updatedUsers.map(normalizeUser)
    syncCurrentUserId(normalizedUsers)
    users.value = normalizedUsers.filter(user => user.Id && user.Id !== currentUser.value.id)
    if (!activeChatId.value || !users.value.some(user => user.Id === activeChatId.value)) {
      activeChatId.value = users.value[0]?.Id || null
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
    const normalizedUser = normalizeUser(user)
    console.log('User connected:', normalizedUser)

    const normalizedCurrentName = currentUser.value.name.trim().toLowerCase()
    const normalizedIncomingName = (normalizedUser.UserName || normalizedUser.DisplayName || '').trim().toLowerCase()

    if (normalizedIncomingName && normalizedIncomingName === normalizedCurrentName) {
      syncCurrentUserId([normalizedUser])
      return
    }

    if (!normalizedUser.Id || normalizedUser.Id === currentUser.value.id) {
      return
    }

    users.value = users.value.filter(u => u.Id !== normalizedUser.Id)
    users.value.push(normalizedUser)
    if (!activeChatId.value) {
      activeChatId.value = normalizedUser.Id || ''
    }
  }

  console.log('Connecting to chat...')
  try {
    await chatService.connect({ Id: currentUser.value.id, UserName: currentUser.value.name, ChatRoom: 'general' })
    console.log('Connected successfully')

    if (activeChatId.value) {
      await loadConversation(activeChatId.value)
    }
  } catch (error) {
    console.error('Chat connection failed', error)
    connectError.value = 'Chat connection failed: see console for details.'
  }
})

watch(activeChatId, (chatId) => {
  if (chatId) {
    loadConversation(chatId)
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
    userName: user.UserName || 'Unknown user',
    status: user.Status || 'offline',
    clickable: true,
    selected: user.Id === activeChatId.value
  }))
})

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

  if (!isValidUserId(currentUser.value.id)) {
    connectError.value = 'Your user session is still connecting. Please wait a moment and try again.'
    return
  }

  console.log(currentUser.value)
  const message: ChatMessageDto = {
    Type: 'chat',
    SenderId: currentUser.value.id,
    ReceiverId: activeChatId.value,
    Data: trimmed
  }

  try {
    console.log('sending message:', message)
    await chatService.sendMessage(message)
    messageText.value = ''
    connectError.value = ''
  } catch (error) {
    console.error('Send message failed:', error)
    connectError.value = 'Failed to send message.'
  }
}

// Typing indicator
let typingTimeout: number | null = null
const handleInput = () => {
  if (!activeChatId.value || !isValidUserId(currentUser.value.id)) return

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
        :items="sidebarItems"
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