<script setup lang="ts">
import { computed, ref, onMounted, onUnmounted } from 'vue'
import TopBar from './componets/TopBar.vue'
import SideBar from './componets/SideBar.vue'
import Button from './componets/Button.vue'
import InputField from './componets/InputField.vue'
import MessageBubble from './componets/MessageBubble.vue'
import chatService, { type ChatMessageDto, type UserState } from './services/chatService'

interface SidebarItem {
  id?: string
  type: 'card' | 'button' | 'divider' | 'spacer'
  title?: string
  description?: string
  icon?: string
  clickable?: boolean
  selected?: boolean
  text?: string
  variant?: 'primary' | 'secondary' | 'outline' | 'ghost' | 'danger'
  size?: 'sm' | 'md' | 'lg'
  data?: any
}

interface ChatMessage {
  id: string
  sender: string
  message: string
  time: string
  isOwn?: boolean
}

const sidebarCollapsed = ref(false)
const activeChatId = ref<string | null>(null)
const messageText = ref('')
const currentUser = ref({ id: '', name: '' })
const users = ref<UserState[]>([])
const messages = ref<Record<string, ChatMessage[]>>({})
const typingUsers = ref<Set<string>>(new Set())
const connectError = ref('')

// Initialize user (in a real app, this would be from login)
const initUser = () => {
  const urlParams = new URLSearchParams(window.location.search)
  const userId = urlParams.get('user') || 'user1'
  const userName = urlParams.get('name') || 'User 1'
  currentUser.value = { id: userId, name: userName }
}

onMounted(async () => {
  console.log('App mounted, initializing user')
  initUser()
  console.log('Current user:', currentUser.value)

  // Set up event handlers
  chatService.onReceiveMessage = (message: ChatMessageDto) => {
    console.log('Received message:', message)
    if (message.type === 'chat') {
      const chatMessage: ChatMessage = {
        id: `msg-${Date.now()}`,
        sender: message.senderId,
        message: message.data,
        time: new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }),
        isOwn: message.senderId === currentUser.value.id
      }
      const chatId = message.senderId === currentUser.value.id ? message.receiverId : message.senderId
      if (!messages.value[chatId]) messages.value[chatId] = []
      messages.value[chatId].push(chatMessage)
    }
  }

  chatService.onUserListUpdated = (userList: UserState[]) => {
    console.log('User list updated:', userList)
    users.value = userList.filter(u => u.userId !== currentUser.value.id)
    if (!activeChatId.value && users.value.length > 0) {
      activeChatId.value = users.value[0].userId
    }
  }

  chatService.onTyping = (typingEvent: ChatMessageDto) => {
    console.log('Typing event:', typingEvent)
    if (typingEvent.data === 'true') {
      typingUsers.value.add(typingEvent.senderId)
    } else {
      typingUsers.value.delete(typingEvent.senderId)
    }
  }

  chatService.onReceiveError = (error: string) => {
    console.error('Received error:', error)
    connectError.value = error
    alert(`Error: ${error}`)
  }

  console.log('Connecting to chat...')
  try {
    await chatService.connect({ userName: currentUser.value.id, chatRoom: 'general' })
    console.log('Connected successfully')
  } catch (error) {
    console.error('Chat connection failed', error)
    connectError.value = 'Chat connection failed: see console for details.'
  }
})

onUnmounted(() => {
  chatService.disconnect()
})

const sidebarItems = computed<SidebarItem[]>(() => [
  ...users.value.map(user => ({
    id: user.userId,
    type: 'card' as const,
    title: user.userName,
    description: user.status === 'online' ? 'Online' : 'Offline',
    icon: '👤',
    clickable: true,
    selected: activeChatId.value === user.userId,
    data: { userId: user.userId }
  })),
  { type: 'divider' as const },
  {
    type: 'button' as const,
    text: 'New Chat',
    variant: 'primary' as const,
    size: 'md' as const,
    data: { action: 'new-chat' }
  }
])

const activeChat = computed(() => users.value.find(u => u.userId === activeChatId.value))
const activeMessages = computed(() => messages.value[activeChatId.value] || [])
const isTyping = computed(() => activeChatId.value && typingUsers.value.has(activeChatId.value))

const handleSidebarItemClick = (item: any) => {
  if (item.type === 'card' && item.id) {
    activeChatId.value = item.id
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
    type: 'chat',
    senderId: currentUser.value.id,
    receiverId: activeChatId.value,
    data: trimmed
  }

  console.log('sending message:', message)
  await chatService.sendMessage(message)

  // Optimistically add to local messages
  const chatMessage: ChatMessage = {
    id: `msg-${Date.now()}`,
    sender: currentUser.value.name,
    message: trimmed,
    time: new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }),
    isOwn: true
  }
  if (!messages.value[activeChatId.value]) messages.value[activeChatId.value] = []
  messages.value[activeChatId.value].push(chatMessage)

  messageText.value = ''
}

// Typing indicator
let typingTimeout: number | null = null
const handleInput = () => {
  if (!activeChatId.value) return

  const typingEvent: ChatMessageDto = {
    type: 'typing',
    senderId: currentUser.value.id,
    receiverId: activeChatId.value,
    data: 'true'
  }
  chatService.sendTyping(typingEvent)

  if (typingTimeout) clearTimeout(typingTimeout)
  typingTimeout = window.setTimeout(() => {
    const stopTypingEvent: ChatMessageDto = {
      type: 'typing',
      senderId: currentUser.value.id,
      receiverId: activeChatId.value,
      data: 'false'
    }
    chatService.sendTyping(stopTypingEvent)
  }, 1000)
}
</script>

<template>
  <div class="app">
    <TopBar title="Dirs21 Chat" showMenuButton :sticky="true" @menuClick="handleMenuClick">
      <template #actions>
        <Button text="Logout" variant="outline" size="sm" />
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
              <h2>{{ activeChat?.title || 'Select a chat' }}</h2>
              <p>{{ activeChat?.description || 'Choose a conversation from the left' }}</p>
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
            {{ activeChat?.userName }} is typing...
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
