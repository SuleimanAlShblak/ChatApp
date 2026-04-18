<script setup lang="ts">
import { computed, ref, onMounted, onUnmounted, watch } from 'vue'
import TopBar from '../components/TopBar.vue'
import SideBar from '../components/SideBar.vue'
import Button from '../components/Button.vue'
import InputField from '../components/InputField.vue'
import MessageBubble from '../components/MessageBubble.vue'
import TypingIndicator from '../components/TypingIndicator.vue'
import chatService, {
  normalizeMessage,
  normalizeUser,
  type Message,
  type StoredChatMessage,
  type User,
} from '../services/chatService'
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
let conversationRefreshTimer: number | null = null

const isValidUserId = (value: string | null | undefined): value is string => {
  return Boolean(value && value !== 'undefined' && value !== 'null')
}

const syncCurrentUserId = (availableUsers: User[]) => {
  if (!currentUser.value.name.trim()) {
    return
  }

  const normalizedName = currentUser.value.name.trim().toLowerCase()
  const matchingUser = availableUsers.find((user) => {
    const candidateName = (user.UserName || user.DisplayName || '').trim().toLowerCase()
    return candidateName === normalizedName
  })

  if (matchingUser?.Id && matchingUser.Id !== currentUser.value.id) {
    currentUser.value = { ...currentUser.value, id: matchingUser.Id }
    localStorage.setItem('chatUserId', matchingUser.Id)
    localStorage.setItem('chatUserName', currentUser.value.name)
  }
}

// Append message to conversation
const appendMessageToConversation = (chatId: string, chatMessage: ChatMessage) => {
  if (!messages.value[chatId]) {
    messages.value[chatId] = []
  }

  const alreadyExists = messages.value[chatId].some((existing) => existing.id === chatMessage.id)

  if (!alreadyExists) {
    messages.value[chatId].push(chatMessage)
  }
}

const mapMessageUI = (message: StoredChatMessage): ChatMessage => {
  const normalizedMessage = normalizeMessage(message ?? {})
  const timestamp = message?.Timestamp

  const senderName =
    normalizedMessage.SenderId === currentUser.value.id
      ? currentUser.value.name
      : users.value.find((user) => user.Id === normalizedMessage.SenderId)?.UserName ||
        normalizedMessage.SenderId ||
        'Unknown'

  const displayTime = timestamp
    ? new Date(timestamp).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })
    : new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })

  return {
    id: `${normalizedMessage.SenderId || 'unknown'}-${normalizedMessage.ReceiverId || 'unknown'}-${timestamp || Date.now()}-${normalizedMessage.Data || ''}`,
    sender: senderName,
    message: normalizedMessage.Data || '',
    time: displayTime,
    isOwn: normalizedMessage.SenderId === currentUser.value.id,
  }
}

const loadConversation = async (chatId: string) => {
  if (!currentUser.value.id || !chatId) {
    return
  }

  try {
    const history = await chatService.getChatHistory(currentUser.value.id, chatId)
    const historyMessages = history.map(mapMessageUI)

    if (!messages.value[chatId]) {
      messages.value[chatId] = []
    }

    for (const historyMessage of historyMessages) {
      appendMessageToConversation(chatId, historyMessage)
    }
  } catch (error) {
    console.error('Failed to load conversation history:', error)
  }
}

const startConversationRefresh = () => {
  if (conversationRefreshTimer) {
    window.clearInterval(conversationRefreshTimer)
  }

  conversationRefreshTimer = window.setInterval(() => {
    if (activeChatId.value) {
      loadConversation(activeChatId.value)
    }
  }, 1500)
}

const stopConversationRefresh = () => {
  if (conversationRefreshTimer) {
    window.clearInterval(conversationRefreshTimer)
    conversationRefreshTimer = null
  }
}

// Initialize user (check localStorage first, then URL params)
const initUser = () => {
  const savedUserId = localStorage.getItem('chatUserId')
  const savedUserName = localStorage.getItem('chatUserName')

  if (savedUserName) {
    currentUser.value = {
      id: isValidUserId(savedUserId) ? savedUserId : '',
      name: savedUserName,
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
    name: userName,
  }

  if (isValidUserId(userId)) {
    localStorage.setItem('chatUserId', userId)
  }
  localStorage.setItem('chatUserName', userName)
}

onMounted(async () => {
  console.log('Chat mounted, initializing user')
  initUser()
  startConversationRefresh()
  console.log('Current user:', currentUser.value)

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
  chatService.onReceiveMessage = (message: Message) => {
    console.log('Received message:', message)

    if (message.Type !== 'chat') {
      return
    }

    const chatId = message.SenderId === currentUser.value.id ? message.ReceiverId : message.SenderId
    const chatMessage = mapMessageUI(message)
    appendMessageToConversation(chatId, chatMessage)
  }

  chatService.onUserListUpdated = (updatedUsers: User[]) => {
    const normalizedUsers = updatedUsers.map(normalizeUser)
    syncCurrentUserId(normalizedUsers)
    users.value = normalizedUsers.filter((user) => user.Id && user.Id !== currentUser.value.id)
    if (!activeChatId.value || !users.value.some((user) => user.Id === activeChatId.value)) {
      activeChatId.value = users.value[0]?.Id || null
    }
  }

  // Handle typing indicators
  chatService.onTyping = (typingEvent: Message) => {
    const normalizedTypingEvent = normalizeMessage(typingEvent)
    console.log('Typing event:', normalizedTypingEvent)
    const updatedTypingUsers = new Set(typingUsers.value)

    if (normalizedTypingEvent.Data === 'true' && normalizedTypingEvent.SenderId) {
      updatedTypingUsers.add(normalizedTypingEvent.SenderId)
    } else if (normalizedTypingEvent.SenderId) {
      updatedTypingUsers.delete(normalizedTypingEvent.SenderId)
    }

    typingUsers.value = updatedTypingUsers
  }

  chatService.onReceiveError = (error: string) => {
    console.error('Received error:', error)
    connectError.value = error
  }

  chatService.onUserConnected = (user: User) => {
    const normalizedUser = normalizeUser(user)
    console.log('User connected:', normalizedUser)

    const normalizedCurrentName = currentUser.value.name.trim().toLowerCase()
    const normalizedIncomingName = (normalizedUser.UserName || normalizedUser.DisplayName || '')
      .trim()
      .toLowerCase()

    if (normalizedIncomingName && normalizedIncomingName === normalizedCurrentName) {
      syncCurrentUserId([normalizedUser])
      return
    }

    if (!normalizedUser.Id || normalizedUser.Id === currentUser.value.id) {
      return
    }

    users.value = users.value.filter((u) => u.Id !== normalizedUser.Id)
    users.value.push(normalizedUser)
    if (!activeChatId.value) {
      activeChatId.value = normalizedUser.Id || ''
    }
  }

  console.log('Connecting to chat...')
  try {
    await chatService.connect({
      Id: currentUser.value.id,
      UserName: currentUser.value.name,
      ChatRoom: 'general',
    })
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
  stopConversationRefresh()
  chatService.disconnect()
})

const sidebarItems = computed(() => {
  console.log(
    'Computing sidebar items with users:',
    users.value,
    'activeChatId:',
    activeChatId.value,
  )
  return users.value.map((user) => ({
    id: user.Id,
    type: 'card',
    userName: user.UserName || 'Unknown user',
    status: user.Status || 'offline',
    clickable: true,
    selected: user.Id === activeChatId.value,
  }))
})

const activeChat = computed(() => users.value.find((u) => u.Id === activeChatId.value))
const activeMessages = computed(() =>
  activeChatId.value ? messages.value[activeChatId.value] || [] : [],
)
const isTyping = computed(() =>
  Boolean(activeChatId.value && typingUsers.value.has(activeChatId.value)),
)

const handleSidebarItemClick = (item: any) => {
  if (item.type === 'card' && item.id) {
    activeChatId.value = item.id
  } else if (item.type === 'button' && item.data?.action === 'new-chat') {
    // Handle new chat
    console.log('New chat clicked')
    // // For now, just alert
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
    connectError.value =
      'Your user session is still connecting. Please wait a moment and try again.'
    return
  }

  console.log(currentUser.value)
  const message: Message = {
    Type: 'chat',
    SenderId: currentUser.value.id,
    ReceiverId: activeChatId.value,
    Data: trimmed,
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

  const typingEvent: Message = {
    Type: 'typing',
    SenderId: currentUser.value.id,
    ReceiverId: activeChatId.value as string,
    Data: 'true',
  }
  chatService.sendTyping(typingEvent)

  if (typingTimeout) clearTimeout(typingTimeout)
  typingTimeout = window.setTimeout(() => {
    const stopTypingEvent: Message = {
      Type: 'typing',
      SenderId: currentUser.value.id,
      ReceiverId: activeChatId.value as string,
      Data: 'false',
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
        <Button
          text="Log out"
          class="logout-button"
          variant="outline"
          size="sm"
          @click="handleLogout"
        />
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
              <p>
                {{
                  activeChat
                    ? activeChat.Status === 'online'
                      ? 'Online'
                      : 'Offline'
                    : 'Choose a conversation from the left'
                }}
              </p>
            </div>
          </div>
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
          <TypingIndicator v-if="isTyping && activeChat" :name="activeChat.UserName || 'Someone'" />
        </section>

        <section class="chat-input-row">
          <InputField
            v-model="messageText"
            placeholder="Type a message..."
            @enter="sendMessage"
            @input="handleInput"
          />
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
  height: 100dvh;
  background: linear-gradient(135deg, #f4f8ff 0%, #e8f1ff 100%);
  overflow: hidden;
}

.app-content {
  display: flex;
  flex: 1;
  min-height: 0;
  overflow: hidden;
}

.chat-panel {
  flex: 1;
  display: flex;
  flex-direction: column;
  min-width: 0;
  min-height: 0;
  height: 100%;
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
  flex-shrink: 0;
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

.logout-button {
  color: #e34949 !important;
}

.logout-button:hover {
  color: #eb4b4b !important;
}

.chat-body {
  flex: 1;
  min-height: 0;
  overflow-y: auto;
  padding: 1.5rem;
  overscroll-behavior: contain;
}

.message-list {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.chat-input-row {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  padding: 1rem 1.5rem 1.5rem;
  background: white;
  border-top: 1px solid #e5e7eb;
  position: sticky;
  bottom: 0;
  z-index: 2;
  flex-shrink: 0;
}

.chat-input-row .input-field-container {
  flex: 1;
  min-width: 0;
}

.chat-input-row .input-field {
  width: 100%;
}

@media (max-width: 900px) {
  .app-content {
    flex-direction: column;
  }

  .sidebar {
    position: relative !important;
    top: auto !important;
    left: auto !important;
    transform: none !important;
    width: 100% !important;
    height: auto !important;
    max-height: 220px;
    border-right: none;
    border-bottom: 1px solid #e5e7eb;
  }

  .chat-panel {
    min-height: 0;
  }

  .chat-header,
  .chat-body {
    padding: 1rem;
  }

  .chat-input-row {
    padding: 0.75rem 1rem 1rem;
  }
}

@media (max-width: 640px) {
  .chat-contact-info {
    gap: 0.75rem;
  }

  .chat-avatar {
    width: 2.5rem;
    height: 2.5rem;
    border-radius: 0.75rem;
  }

  .chat-header h2 {
    font-size: 1rem;
  }

  .chat-header p {
    font-size: 0.85rem;
  }

  .chat-input-row {
    flex-wrap: wrap;
    gap: 0.5rem;
  }

  .chat-input-row .input-field-container,
  .chat-input-row .button {
    width: 100%;
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
