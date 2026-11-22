"use client";

import React from "react";
import { AuthProvider } from "@/context/AuthContext";
import { SignalRProvider } from "@/context/SignalRContext";
import { Toaster } from "sonner";

export function Providers({ children }: { children: React.ReactNode }) {
  return (
    <AuthProvider>
      <SignalRProvider>
        {children}
        <Toaster position="top-left" dir="rtl" richColors closeButton />
      </SignalRProvider>
    </AuthProvider>
  );
}
