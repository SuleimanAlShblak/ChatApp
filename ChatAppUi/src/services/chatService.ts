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
}

export interface ChatMessageDto {
  Type: 'connect' | 'chat' | 'typing' | 'error';
  SenderId: string;
  ReceiverId: string;
  Data: string;
}

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
      console.log('ChatService: Received message:', message);
      this.onReceiveMessage?.(message);
    });

    this.connection.on('UserListUpdated', (users: User[]) => {
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

    // this.connection.on('UserConnected', (user: UserState) => {
    //   console.log('ChatService: User connected:', user);
    //   this.onUserConnected?.(user);
    // });

    await this.connection.start();
    console.log('ChatService: Connected to SignalR hub');

    // Connect user
    console.log('ChatService: Invoking Connect with', user);
    await this.connection.invoke('Connect', user);
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