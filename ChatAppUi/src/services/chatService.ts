import axios from 'axios';
import { HubConnectionBuilder, HubConnection, LogLevel } from '@microsoft/signalr';

const API_BASE_URL = 'http://localhost:5001';

export interface User {
  Id?: string;
  UserName?: string;
  DisplayName?: string;
  Image?: string;
  Status?: string;
  SenderId?: string;
  isTyping?: boolean;
  ReceiverId?: string;
  ChatRoom?: string;
  id?: string;
  userName?: string;
  displayName?: string;
  image?: string;
  status?: string;
  senderId?: string;
  receiverId?: string;
  chatRoom?: string;
}

export interface ChatMessageDto {
  Type: 'connect' | 'chat' | 'typing' | 'error';
  SenderId: string;
  ReceiverId: string;
  Data: string;
}

export interface StoredChatMessage extends ChatMessageDto {
  Timestamp?: string;
  timestamp?: string;
}

export const normalizeUser = (user: User): User => ({
  Id: user.Id ?? user.id ?? '',
  UserName: user.UserName ?? user.userName ?? user.DisplayName ?? user.displayName ?? '',
  DisplayName: user.DisplayName ?? user.displayName ?? user.UserName ?? user.userName ?? '',
  Image: user.Image ?? user.image ?? '',
  Status: user.Status ?? user.status ?? 'offline',
  SenderId: user.SenderId ?? user.senderId ?? '',
  ReceiverId: user.ReceiverId ?? user.receiverId ?? '',
  ChatRoom: user.ChatRoom ?? user.chatRoom ?? 'general',
  isTyping: user.isTyping ?? false,
});

export const normalizeMessage = (message: any): ChatMessageDto => ({
  Type: message.Type ?? message.type ?? 'chat',
  SenderId: message.SenderId ?? message.senderId ?? '',
  ReceiverId: message.ReceiverId ?? message.receiverId ?? '',
  Data: message.Data ?? message.data ?? '',
});

// export interface UserState {
//   connectionId: string;
//   userId: string;
//   userName: string;
//   status: string;
// }

class ChatService {
  private connection: HubConnection | null = null;

  // Callbacks for UI updates
  onReceiveMessage?: (message: ChatMessageDto) => void;
  onUserListUpdated?: (users: User[]) => void;
  onTyping?: (typingEvent: ChatMessageDto) => void;
  onReceiveError?: (error: string) => void;
  onUserConnected?: (user: User) => void;

  async connect(user: User): Promise<void> {
    console.log('ChatService: Starting connection to', `${API_BASE_URL}/Chat`)
    this.connection = new HubConnectionBuilder()
      .withUrl(`${API_BASE_URL}/Chat`)
      .configureLogging(LogLevel.Information)
      .build();

    // Event handlers
    this.connection.on('ReceiveSpecificMessage', (message: ChatMessageDto) => {
      const normalizedMessage = normalizeMessage(message);
      console.log('ChatService: Received message:', normalizedMessage);
      this.onReceiveMessage?.(normalizedMessage);
    });

    this.connection.on('UserListUpdated', (users: User[]) => {
      const normalizedUsers = users.map(normalizeUser);
      console.log('ChatService: User list updated:', normalizedUsers);
      this.onUserListUpdated?.(normalizedUsers);
    });

    this.connection.on('Typing', (typingEvent: ChatMessageDto) => {
      console.log('ChatService: Typing event:', typingEvent);
      this.onTyping?.(typingEvent);
    });

    this.connection.on('ReceiveError', (error: string) => {
      console.error('ChatService: Received error:', error);
      this.onReceiveError?.(error);
    });

    this.connection.on('UserConnected', (user: User) => {
      const normalizedUser = normalizeUser(user);
      console.log('ChatService: User connected:', normalizedUser);
      this.onUserConnected?.(normalizedUser);
    });

    await this.connection.start();
    console.log('ChatService: Connected to SignalR hub');

    // Connect user
    console.log('ChatService: Invoking Connect with', user);
    await this.connection.invoke('Connect', user);
  }

  async sendMessage(message: ChatMessageDto): Promise<void> {
    const normalizedMessage = normalizeMessage(message);
    console.log('ChatService: sendMessage called with', normalizedMessage);

    await axios.post(
      `${API_BASE_URL}/api/chat/message/${encodeURIComponent(normalizedMessage.SenderId)}/${encodeURIComponent(normalizedMessage.ReceiverId)}/${encodeURIComponent(normalizedMessage.Data)}`
    );

    if (this.connection) {
      console.log('ChatService: Invoking SendMessage');
      await this.connection.invoke('SendMessage', normalizedMessage);
    }
  }

  async sendTyping(typingEvent: ChatMessageDto): Promise<void> {
    if (this.connection) {
      await this.connection.invoke('Typing', typingEvent);
    }
  }

  async getChatHistory(userId: string, chatPartnerId: string): Promise<StoredChatMessage[]> {
    const response = await axios.get(
      `${API_BASE_URL}/api/chat/history/${encodeURIComponent(userId)}/${encodeURIComponent(chatPartnerId)}`
    );

    return response.data as StoredChatMessage[];
  }

  disconnect(): void {
    if (this.connection) {
      this.connection.stop();
    }
  }
}

export default new ChatService();