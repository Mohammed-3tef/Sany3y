"use client";

import React, {
  createContext,
  useContext,
  useEffect,
  useRef,
  useState,
} from "react";
import * as signalR from "@microsoft/signalr";
import { API_BASE_URL } from "@/lib/api";
import { useAuth } from "@/context/AuthContext";

interface IncomingMessageEvent {
  senderUserName: string;
  message: string;
  time: string;
}

interface SentMessageEvent {
  receiverUserName: string;
  message: string;
  time: string;
}

interface SignalRContextType {
  onlineUsersById: Record<number, boolean>;
  onlineUsersByName: Record<string, boolean>;
  sendPrivateMessage: (receiverUserName: string, message: string) => Promise<void>;
  sendTyping: (receiverUserName: string) => Promise<void>;
  onReceiveMessage: (cb: (e: IncomingMessageEvent) => void) => () => void;
  onMessageSent: (cb: (e: SentMessageEvent) => void) => () => void;
  onTypingIndicator: (cb: (senderUserName?: string) => void) => () => void;
}

const SignalRContext = createContext<SignalRContextType>({
  onlineUsersById: {},
  onlineUsersByName: {},
  sendPrivateMessage: async () => {},
  sendTyping: async () => {},
  onReceiveMessage: () => () => {},
  onMessageSent: () => () => {},
  onTypingIndicator: () => () => {},
});

export function SignalRProvider({ children }: { children: React.ReactNode }) {
  const { user, token, isAuthenticated } = useAuth();
  const [onlineUsersById, setOnlineUsersById] = useState<Record<number, boolean>>({});
  const [onlineUsersByName, setOnlineUsersByName] = useState<Record<string, boolean>>({});

  const chatHubRef = useRef<signalR.HubConnection | null>(null);
  const statusHubRef = useRef<signalR.HubConnection | null>(null);

  const receiveListenersRef = useRef<Set<(e: IncomingMessageEvent) => void>>(new Set());
  const sentListenersRef = useRef<Set<(e: SentMessageEvent) => void>>(new Set());
  const typingListenersRef = useRef<Set<(senderUserName?: string) => void>>(new Set());

  useEffect(() => {
    if (!isAuthenticated || !user || !token) {
      chatHubRef.current?.stop();
      statusHubRef.current?.stop();
      chatHubRef.current = null;
      statusHubRef.current = null;
      return;
    }

    const encodedUser = encodeURIComponent(user.userName);

    const statusConnection = new signalR.HubConnectionBuilder()
      .withUrl(`${API_BASE_URL}/userStatusHub?userName=${encodedUser}`, {
        accessTokenFactory: () => token,
      })
      .withAutomaticReconnect()
      .build();

    statusConnection.on("ReceiveUserStatus", (userId: number, isOnline: boolean) => {
      setOnlineUsersById((prev) => ({ ...prev, [userId]: isOnline }));
    });

    statusConnection.on("UserStatusChanged", (userName: string, isOnline: boolean) => {
      setOnlineUsersByName((prev) => ({ ...prev, [userName]: isOnline }));
    });

    statusConnection.start().catch(() => {
      // Ignore connection warning when backend is offline
    });
    statusHubRef.current = statusConnection;

    const chatConnection = new signalR.HubConnectionBuilder()
      .withUrl(`${API_BASE_URL}/chatHub?userName=${encodedUser}`, {
        accessTokenFactory: () => token,
      })
      .withAutomaticReconnect()
      .build();

    chatConnection.on(
      "ReceiveMessage",
      (senderUserName: string, message: string, time: string) => {
        receiveListenersRef.current.forEach((cb) =>
          cb({ senderUserName, message, time })
        );
      }
    );

    chatConnection.on(
      "MessageSent",
      (receiverUserName: string, message: string, time: string) => {
        sentListenersRef.current.forEach((cb) =>
          cb({ receiverUserName, message, time })
        );
      }
    );

    chatConnection.on("TypingIndicator", (senderUserName?: string) => {
      typingListenersRef.current.forEach((cb) => cb(senderUserName));
    });

    chatConnection.start().catch(() => {
      // Ignore connection warning when backend is offline
    });
    chatHubRef.current = chatConnection;

    return () => {
      statusConnection.stop();
      chatConnection.stop();
    };
  }, [isAuthenticated, user?.userName, token]);

  const sendPrivateMessage = async (receiverUserName: string, message: string) => {
    if (
      chatHubRef.current &&
      chatHubRef.current.state === signalR.HubConnectionState.Connected
    ) {
      await chatHubRef.current.invoke("SendPrivateMessage", receiverUserName, message);
    }
  };

  const sendTyping = async (receiverUserName: string) => {
    if (
      chatHubRef.current &&
      chatHubRef.current.state === signalR.HubConnectionState.Connected
    ) {
      await chatHubRef.current.invoke("Typing", receiverUserName);
    }
  };

  const onReceiveMessage = (cb: (e: IncomingMessageEvent) => void) => {
    receiveListenersRef.current.add(cb);
    return () => {
      receiveListenersRef.current.delete(cb);
    };
  };

  const onMessageSent = (cb: (e: SentMessageEvent) => void) => {
    sentListenersRef.current.add(cb);
    return () => {
      sentListenersRef.current.delete(cb);
    };
  };

  const onTypingIndicator = (cb: (senderUserName?: string) => void) => {
    typingListenersRef.current.add(cb);
    return () => {
      typingListenersRef.current.delete(cb);
    };
  };

  return (
    <SignalRContext.Provider
      value={{
        onlineUsersById,
        onlineUsersByName,
        sendPrivateMessage,
        sendTyping,
        onReceiveMessage,
        onMessageSent,
        onTypingIndicator,
      }}
    >
      {children}
    </SignalRContext.Provider>
  );
}

export const useSignalR = () => useContext(SignalRContext);
