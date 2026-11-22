"use client";

import React, { useEffect, useRef, useState } from "react";
import { useParams, useRouter } from "next/navigation";
import { apiFetch } from "@/lib/api";
import { useAuth } from "@/context/AuthContext";
import { useSignalR } from "@/context/SignalRContext";
import { ChatMessage } from "@/types";

export default function FullPageChat() {
  const params = useParams();
  const router = useRouter();
  const receiverId = Number(params?.id);
  const { user, isAuthenticated, isLoading } = useAuth();
  const {
    onlineUsersById,
    onlineUsersByName,
    sendPrivateMessage,
    sendTyping,
    onReceiveMessage,
    onMessageSent,
    onTypingIndicator,
  } = useSignalR();

  const [receiver, setReceiver] = useState<any | null>(null);
  const [receiverImage, setReceiverImage] = useState(
    "https://placehold.co/100x100?text=User"
  );
  const [messages, setMessages] = useState<
    { isSent: boolean; content: string; time: string }[]
  >([]);
  const [input, setInput] = useState("");
  const [isTyping, setIsTyping] = useState(false);
  const messagesEndRef = useRef<HTMLDivElement | null>(null);

  useEffect(() => {
    if (!isLoading && !isAuthenticated) {
      router.push("/login");
      return;
    }
    if (!isAuthenticated || !user || !receiverId) return;

    // Fetch receiver info & conversation
    Promise.all([
      apiFetch<any>(`/api/User/GetByID/${receiverId}`),
      apiFetch<ChatMessage[]>(
        `/api/Message/GetConversation/${user.id}/${receiverId}`
      ).catch(() => []),
    ]).then(async ([recUser, conv]) => {
      setReceiver(recUser);
      if (recUser?.profilePictureId) {
        try {
          const pic = await apiFetch<{ path: string }>(
            `/api/ProfilePicture/GetByID/${recUser.profilePictureId}`
          );
          if (pic?.path) setReceiverImage(pic.path);
        } catch {
          // keep fallback
        }
      }
      setMessages(
        (conv || []).map((m) => ({
          isSent: m.senderId === user.id,
          content: m.content,
          time: new Date(m.sentAt).toLocaleTimeString("ar-EG", {
            hour: "2-digit",
            minute: "2-digit",
          }),
        }))
      );
    });
  }, [isLoading, isAuthenticated, user?.id, receiverId]);

  useEffect(() => {
    if (!receiver) return;

    const unsubReceive = onReceiveMessage(({ senderUserName, message, time }) => {
      if (senderUserName === receiver.userName) {
        setMessages((prev) => [
          ...prev,
          { isSent: false, content: message, time },
        ]);
      }
    });

    const unsubSent = onMessageSent(({ receiverUserName, message, time }) => {
      if (receiverUserName === receiver.userName) {
        setMessages((prev) => [
          ...prev,
          { isSent: true, content: message, time },
        ]);
      }
    });

    const unsubTyping = onTypingIndicator((senderUserName) => {
      if (!senderUserName || senderUserName === receiver.userName) {
        setIsTyping(true);
        setTimeout(() => setIsTyping(false), 1500);
      }
    });

    return () => {
      unsubReceive();
      unsubSent();
      unsubTyping();
    };
  }, [receiver?.userName, onReceiveMessage, onMessageSent, onTypingIndicator]);

  useEffect(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: "smooth" });
  }, [messages, isTyping]);

  const handleSend = async () => {
    const trimmed = input.trim();
    if (!trimmed || !user || !receiver) return;
    setInput("");

    try {
      await sendPrivateMessage(receiver.userName, trimmed);
    } catch {
      await apiFetch("/api/Message/Create", {
        method: "POST",
        body: JSON.stringify({
          senderId: user.id,
          receiverId: receiver.id,
          content: trimmed,
          sentAt: new Date().toISOString(),
        }),
      });
      setMessages((prev) => [
        ...prev,
        {
          isSent: true,
          content: trimmed,
          time: new Date().toLocaleTimeString("ar-EG", {
            hour: "2-digit",
            minute: "2-digit",
          }),
        },
      ]);
    }
  };

  const receiverOnline = receiver
    ? onlineUsersById[receiver.id] ??
      onlineUsersByName[receiver.userName] ??
      receiver.isOnline
    : false;

  return (
    <div className="container py-5 mt-5">
      <div
        className="bg-white mx-auto rounded-4 shadow d-flex flex-column overflow-hidden"
        style={{ maxWidth: 800, height: "78vh" }}
      >
        {/* Header */}
        <div className="p-3 border-bottom d-flex align-items-center gap-3 bg-white">
          <img
            src={receiverImage}
            alt={receiver?.userName || "User"}
            style={{ width: 48, height: 48, borderRadius: "50%", objectFit: "cover" }}
            onError={(e) => {
              (e.target as HTMLImageElement).src =
                "https://placehold.co/100x100?text=User";
            }}
          />
          <div>
            <div className="fw-bold">
              {receiver?.shopName ||
                (receiver
                  ? `${receiver.firstName} ${receiver.lastName} (@${receiver.userName})`
                  : "جارِ التحميل...")}
            </div>
            <div
              className={`small ${
                receiverOnline ? "text-success fw-semibold" : "text-muted"
              }`}
            >
              {receiverOnline ? "● متصل الآن" : "غير متصل"}
            </div>
          </div>
        </div>

        {/* Messages */}
        <div
          className="flex-grow-1 p-3 overflow-auto d-flex flex-column gap-2"
          style={{ backgroundColor: "#f1f3f5" }}
        >
          {messages.map((m, idx) => (
            <div
              key={idx}
              className="p-3 rounded-4"
              style={{
                maxWidth: "65%",
                alignSelf: m.isSent ? "flex-start" : "flex-end",
                backgroundColor: m.isSent ? "#e9ecef" : "var(--primary-green)",
                color: m.isSent ? "#222" : "#fff",
              }}
            >
              <div>{m.content}</div>
              <div
                className="text-start mt-1"
                style={{ fontSize: "0.72rem", opacity: 0.75 }}
              >
                {m.time}
              </div>
            </div>
          ))}
          <div ref={messagesEndRef} />
        </div>

        {/* Typing Indicator */}
        {isTyping && (
          <div className="px-3 py-1 text-muted small bg-light">
            يكتب الآن...
          </div>
        )}

        {/* Input */}
        <div className="p-3 border-top d-flex gap-2 bg-white">
          <input
            type="text"
            className="form-control"
            placeholder="اكتب رسالة..."
            value={input}
            onChange={(e) => {
              setInput(e.target.value);
              if (receiver?.userName) {
                sendTyping(receiver.userName).catch(() => {});
              }
            }}
            onKeyDown={(e) => {
              if (e.key === "Enter") handleSend();
            }}
          />
          <button
            type="button"
            onClick={handleSend}
            className="btn btn-primary px-4"
          >
            <i className="fas fa-paper-plane"></i>
          </button>
        </div>
      </div>
    </div>
  );
}
