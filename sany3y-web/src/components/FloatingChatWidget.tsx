"use client";

import React, { useEffect, useRef, useState } from "react";
import { useAuth } from "@/context/AuthContext";
import { useSignalR } from "@/context/SignalRContext";
import { apiFetch } from "@/lib/api";
import { ChatMessage } from "@/types";

interface FloatingChatWidgetProps {
  receiverId: number;
  receiverUserName: string;
  receiverFirstName: string;
}

export function FloatingChatWidget({
  receiverId,
  receiverUserName,
  receiverFirstName,
}: FloatingChatWidgetProps) {
  const { user, isAuthenticated } = useAuth();
  const { sendPrivateMessage, onReceiveMessage, onMessageSent } = useSignalR();
  const [isOpen, setIsOpen] = useState(false);
  const [messages, setMessages] = useState<{ text: string; type: "sent" | "received" }[]>([]);
  const [loading, setLoading] = useState(false);
  const [input, setInput] = useState("");
  const messagesEndRef = useRef<HTMLDivElement | null>(null);

  const shouldRender =
    isAuthenticated && user && user.id !== receiverId && Boolean(receiverUserName);

  useEffect(() => {
    if (!shouldRender) return;

    const unsubReceive = onReceiveMessage(({ senderUserName, message }) => {
      if (senderUserName === receiverUserName) {
        setMessages((prev) => [...prev, { text: message, type: "received" }]);
      }
    });

    const unsubSent = onMessageSent(({ receiverUserName: toUser, message }) => {
      if (toUser === receiverUserName) {
        setMessages((prev) => [...prev, { text: message, type: "sent" }]);
      }
    });

    return () => {
      unsubReceive();
      unsubSent();
    };
  }, [shouldRender, receiverUserName, onReceiveMessage, onMessageSent]);

  useEffect(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: "smooth" });
  }, [messages, isOpen]);

  const loadHistory = async () => {
    if (!user) return;
    setLoading(true);
    try {
      const history = await apiFetch<ChatMessage[]>(
        `/api/Message/GetConversation/${user.id}/${receiverId}`
      );
      setMessages(
        (history || []).map((m) => ({
          text: m.content,
          type: m.senderId === user.id ? "sent" : "received",
        }))
      );
    } catch {
      // Ignore error
    } finally {
      setLoading(false);
    }
  };

  const handleToggle = () => {
    const nextState = !isOpen;
    setIsOpen(nextState);
    if (nextState) {
      loadHistory();
    }
  };

  const handleSend = async () => {
    const trimmed = input.trim();
    if (!trimmed || !user) return;
    setInput("");

    try {
      await sendPrivateMessage(receiverUserName, trimmed);
    } catch {
      // Fallback via REST if WebSocket is not connected
      await apiFetch("/api/Message/Create", {
        method: "POST",
        body: JSON.stringify({
          senderId: user.id,
          receiverId,
          content: trimmed,
          sentAt: new Date().toISOString(),
        }),
      });
      setMessages((prev) => [...prev, { text: trimmed, type: "sent" }]);
    }
  };

  if (!shouldRender) return null;

  return (
    <>
      <button
        type="button"
        className="floating-chat-btn"
        onClick={handleToggle}
        title={`محادثة مع ${receiverFirstName}`}
      >
        <i className="fas fa-comments"></i>
      </button>

      {isOpen && (
        <div className="floating-chat-window">
          <div className="floating-chat-header">
            <span>محادثة مع {receiverFirstName}</span>
            <i
              className="fas fa-times"
              style={{ cursor: "pointer" }}
              onClick={() => setIsOpen(false)}
            ></i>
          </div>

          <div className="floating-chat-messages">
            {loading ? (
              <div className="text-center text-muted small my-3">
                جاري تحميل المحادثة...
              </div>
            ) : messages.length === 0 ? (
              <div className="text-center text-muted small mt-3">
                ابدأ المحادثة الآن!
              </div>
            ) : (
              messages.map((m, idx) => (
                <div key={idx} className={`chat-bubble ${m.type}`}>
                  {m.text}
                </div>
              ))
            )}
            <div ref={messagesEndRef} />
          </div>

          <div className="p-3 border-top bg-white d-flex gap-2">
            <input
              type="text"
              className="form-control rounded-pill"
              placeholder="اكتب رسالتك..."
              value={input}
              onChange={(e) => setInput(e.target.value)}
              onKeyDown={(e) => {
                if (e.key === "Enter") handleSend();
              }}
            />
            <button
              type="button"
              onClick={handleSend}
              className="btn btn-primary rounded-circle d-flex align-items-center justify-content-center"
              style={{ width: 40, height: 40, padding: 0 }}
            >
              <i className="fas fa-paper-plane"></i>
            </button>
          </div>
        </div>
      )}
    </>
  );
}
