import axios from 'axios'
import { HubConnectionBuilder, HubConnection, LogLevel } from '@microsoft/signalr'

const API_BASE_URL = 'http://localhost:5001'

export interface User {
  Id?: string
  UserName?: string
  DisplayName?: string
  Image?: string
  Status?: string
  SenderId?: string
  isTyping?: boolean
  ReceiverId?: string
  ChatRoom?: string
}

export interface Message {
  Type: 'connect' | 'chat' | 'typing' | 'error'
  SenderId: string
  ReceiverId: string
  MessageContent: string
  Timestamp?: string
}

export interface StoredChatMessage extends Message {
  Timestamp?: string
}
// Normalization functions
export const normalizeUser = (user: User): User => ({
  Id: user.Id ?? '',
  UserName: user.UserName ?? user.DisplayName ?? '',
  DisplayName: user.DisplayName ?? user.UserName ?? '',
  Image: user.Image ?? '',
  Status: normalizeStatus(user.Status),
  SenderId: user.SenderId ?? '',
  ReceiverId: user.ReceiverId ?? '',
  ChatRoom: user.ChatRoom ?? 'general',
})

export const normalizeMessage = (message: Message): Message => ({
  Type: message.Type ?? 'chat',
  SenderId: message.SenderId ?? '',
  ReceiverId: message.ReceiverId ?? '',
  MessageContent: message.MessageContent ?? '',
  Timestamp: message.Timestamp ?? undefined,
})

const normalizeStatus = (status: unknown): string => {
  if (status === 0 || status === 'Online' || status === 'online') return 'online'
  if (status === 1 || status === 'Offline' || status === 'offline') return 'offline'
  return 'offline'
}

const hasValidValue = (value: string | null | undefined): value is string => {
  return Boolean(value && value !== 'undefined' && value !== 'null')
}

class ChatService {
  private connection: HubConnection | null = null

  // Callbacks for UI updates
  onReceiveMessage?: (message: Message) => void
  onUserListUpdated?: (users: User[]) => void
  onTyping?: (typingEvent: Message) => void
  onReceiveError?: (error: string) => void
  onUserConnected?: (user: User) => void

  async connect(user: User): Promise<void> {
    console.log('ChatService: Starting connection to', `${API_BASE_URL}/Chat`)
    this.connection = new HubConnectionBuilder()
      .withUrl(`${API_BASE_URL}/Chat`)
      .configureLogging(LogLevel.Information)
      .build()

    // Event handlers
    this.connection.on('ReceiveSpecificMessage', (message: Message) => {
      const normalizedMessage = normalizeMessage(message)
      console.log('ChatService: Received message:', normalizedMessage)
      this.onReceiveMessage?.(normalizedMessage)
    })

    this.connection.on('UserListUpdated', (users: User[]) => {
      const normalizedUsers = users.map(normalizeUser)
      console.log('ChatService: User list updated:', normalizedUsers)
      this.onUserListUpdated?.(normalizedUsers)
    })

    this.connection.on('Typing', (typingEvent: Message) => {
      const normalizedTypingEvent = normalizeMessage(typingEvent)
      console.log('ChatService: Typing event:', normalizedTypingEvent)
      this.onTyping?.(normalizedTypingEvent)
    })

    this.connection.on('ReceiveError', (error: string) => {
      console.error('ChatService: Received error:', error)
      this.onReceiveError?.(error)
    })

    this.connection.on('UserConnected', (user: User) => {
      const normalizedUser = normalizeUser(user)
      console.log('ChatService: User connected:', normalizedUser)
      this.onUserConnected?.(normalizedUser)
    })

    await this.connection.start()
    console.log('ChatService: Connected to SignalR hub')

    // Connect user
    console.log('ChatService: Invoking Connect with', user)
    await this.connection.invoke('Connect', user)
  }

  // Send messages
  async sendMessage(message: Message): Promise<StoredChatMessage> {
    const normalizedMessage = normalizeMessage(message)
    console.log('ChatService: sendMessage called with', normalizedMessage)

    if (
      !hasValidValue(normalizedMessage.SenderId) ||
      !hasValidValue(normalizedMessage.ReceiverId)
    ) {
      throw new Error('Cannot send message without a valid sender and receiver.')
    }

    const response = await axios.post(`${API_BASE_URL}/api/chat/message`, {
      Type: normalizedMessage.Type,
      SenderId: normalizedMessage.SenderId,
      ReceiverId: normalizedMessage.ReceiverId,
      MessageContent: normalizedMessage.MessageContent,
    })

    const savedMessage: StoredChatMessage = {
      ...normalizedMessage,
      ...(response.data ?? {}),
      Timestamp: response.data?.Timestamp ?? response.data?.timestamp ?? new Date().toISOString(),
    }

    if (this.connection) {
      console.log('ChatService: Invoking SendMessage')
      await this.connection.invoke('SendMessage', savedMessage)
    }

    return savedMessage
  }

  async sendTyping(typingEvent: Message): Promise<void> {
    const normalizedTypingEvent = normalizeMessage(typingEvent)

    if (
      this.connection &&
      hasValidValue(normalizedTypingEvent.SenderId) &&
      hasValidValue(normalizedTypingEvent.ReceiverId)
    ) {
      try {
        await this.connection.invoke('Typing', normalizedTypingEvent)
      } catch (error) {
        console.warn('ChatService: Typing invoke failed', error)
      }
    }
  }

  async getChatHistory(userId: string, chatPartnerId: string): Promise<StoredChatMessage[]> {
    const response = await axios.get(
      `${API_BASE_URL}/api/chat/history/${encodeURIComponent(userId)}/${encodeURIComponent(chatPartnerId)}`,
    )

    return response.data as StoredChatMessage[]
  }

  disconnect(): void {
    if (this.connection) {
      this.connection.stop()
    }
  }
}

export default new ChatService()
