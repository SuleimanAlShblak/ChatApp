import axios from 'axios';
import { HubConnectionBuilder, HubConnection, LogLevel } from '@microsoft/signalr';

const API_BASE_URL = 'http://localhost:5001';

export interface UserConnection {
  userName: string;
  chatRoom: string;
}

export interface ChatMessageDto {
  type: 'connect' | 'chat' | 'typing' | 'error';
  senderId: string;
  receiverId: string;
  data: string;
}

export interface UserState {
  connectionId: string;
  userId: string;
  userName: string;
  status: string;
}

class ChatService {
  private connection: HubConnection | null = null;

  // Callbacks for UI updates
  onReceiveMessage?: (message: ChatMessageDto) => void;
  onUserListUpdated?: (users: UserState[]) => void;
  onTyping?: (typingEvent: ChatMessageDto) => void;
  onReceiveError?: (error: string) => void;

  async connect(userConnection: UserConnection): Promise<void> {
    console.log('ChatService: Starting connection to', `${API_BASE_URL}/Chat`)
    this.connection = new HubConnectionBuilder()
      .withUrl(`${API_BASE_URL}/Chat`)
      .configureLogging(LogLevel.Information)
      .build();

    // Event handlers
    this.connection.on('ReceiveMessage', (message: ChatMessageDto) => {
      console.log('ChatService: Received message:', message);
      this.onReceiveMessage?.(message);
    });

    this.connection.on('UserListUpdated', (users: UserState[]) => {
      console.log('ChatService: User list updated:', users);
      this.onUserListUpdated?.(users);
    });

    this.connection.on('Typing', (typingEvent: ChatMessageDto) => {
      console.log('ChatService: Typing event:', typingEvent);
      this.onTyping?.(typingEvent);
    });

    this.connection.on('ReceiveError', (error: string) => {
      console.error('ChatService: Received error:', error);
      this.onReceiveError?.(error);
    });

    await this.connection.start();
    console.log('ChatService: Connected to SignalR hub');

    // Connect user
    console.log('ChatService: Invoking Connect with', userConnection);
    await this.connection.invoke('Connect', userConnection);
  }

  async sendMessage(message: ChatMessageDto): Promise<void> {
    console.log('ChatService: sendMessage called with', message);
    if (this.connection) {
      console.log('ChatService: Invoking SendMessage');
      await this.connection.invoke('SendMessage', message);
    } else {
      console.error('ChatService: No connection');
    }
  }

  async sendTyping(typingEvent: ChatMessageDto): Promise<void> {
    if (this.connection) {
      await this.connection.invoke('Typing', typingEvent);
    }
  }

  disconnect(): void {
    if (this.connection) {
      this.connection.stop();
    }
  }
}

export default new ChatService();