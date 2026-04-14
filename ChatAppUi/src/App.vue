<script setup lang="ts">
import { computed, ref } from 'vue'
import TopBar from './componets/TopBar.vue'
import SideBar from './componets/SideBar.vue'
import Button from './componets/Button.vue'
import InputField from './componets/InputField.vue'
import MessageBubble from './componets/MessageBubble.vue'

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
const activeChatId = ref('chat-1')
const messageText = ref('')

const sidebarItems = ref<SidebarItem[]>([
  {
    id: 'chat-1',
    type: 'card',
    title: 'Hekmat',
    description: 'Online now',
    icon: '👤',
    clickable: true,
    selected: true,
    data: { chatId: 'chat-1' },
  },
  {
    id: 'chat-2',
    type: 'card',
    title: 'Robin',
    description: 'Last seen 5m ago',
    icon: '👤',
    clickable: true,
    selected: false,
    data: { chatId: 'chat-2' },
  },
  {
    id: 'chat-3',
    type: 'card',
    title: 'Ava',
    description: 'Typing…',
    icon: '👤',
    clickable: true,
    selected: false,
    data: { chatId: 'chat-3' },
  },
  {
    id: 'divider-1',
    type: 'divider',
  },
  {
    id: 'new-chat',
    type: 'button',
    text: 'New Chat',
    variant: 'primary',
    size: 'md',
    data: { action: 'new-chat' },
  },
])

const chatMessages = ref<Record<string, ChatMessage[]>>({
  'chat-1': [
    {
      id: 'm1',
      sender: 'Hekmat',
      message: 'Hey! Are you ready for today’s update?',
      time: '10:12',
    },
    {
      id: 'm2',
      sender: 'You',
      message: 'Yes, I’m here. Let’s review the current tasks.',
      time: '10:13',
      isOwn: true,
    },
    {
      id: 'm3',
      sender: 'Hekmat',
      message: 'Great. I sent the latest wireframes yesterday.',
      time: '10:14',
    },
  ],
  'chat-2': [
    {
      id: 'm4',
      sender: 'Robin',
      message: 'The server was updated, please check the logs.',
      time: '09:45',
    },
    {
      id: 'm5',
      sender: 'You',
      message: 'I will verify the deployment now.',
      time: '09:48',
      isOwn: true,
    },
  ],
  'chat-3': [
    { id: 'm6', sender: 'Ava', message: 'Can you share the design assets?', time: '08:30' },
  ],
})

const activeChat = computed(() => sidebarItems.value.find((item) => item.id === activeChatId.value))
const activeMessages = computed(() => chatMessages.value[activeChatId.value] || [])

const handleSidebarItemClick = (item: any) => {
  if (item.type !== 'card' || !item.id) return
  activeChatId.value = item.id
  sidebarItems.value = sidebarItems.value.map((sidebarItem) => {
    if (sidebarItem.type !== 'card') return sidebarItem
    return {
      ...sidebarItem,
      selected: sidebarItem.id === item.id,
    }
  })
}

const handleMenuClick = () => {
  sidebarCollapsed.value = !sidebarCollapsed.value
}

const sendMessage = () => {
  const trimmed = messageText.value.trim()
  if (!trimmed) return
  const newMessage: ChatMessage = {
    id: `msg-${Date.now()}`,
    sender: 'You',
    message: trimmed,
    time: new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }),
    isOwn: true,
  }
  const currentMessages = chatMessages.value[activeChatId.value] ?? []
  currentMessages.push(newMessage)
  chatMessages.value[activeChatId.value] = currentMessages
  messageText.value = ''
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
        </section>

        <section class="chat-input-row">
          <InputField v-model="messageText" placeholder="Type a message..." @enter="sendMessage" />
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
</style>
