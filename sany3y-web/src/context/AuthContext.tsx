"use client";

import React, { createContext, useContext, useEffect, useState } from "react";
import { AuthUser } from "@/types";
import { apiFetch } from "@/lib/api";
import { useRouter } from "next/navigation";

interface AuthContextType {
  user: AuthUser | null;
  token: string | null;
  isLoading: boolean;
  isAuthenticated: boolean;
  isAdmin: boolean;
  isTechnician: boolean;
  isClient: boolean;
  isShop: boolean;
  setSession: (token: string, user: AuthUser) => void;
  updateUser: (user: AuthUser) => void;
  logout: () => Promise<void>;
}

const AuthContext = createContext<AuthContextType>({
  user: null,
  token: null,
  isLoading: true,
  isAuthenticated: false,
  isAdmin: false,
  isTechnician: false,
  isClient: false,
  isShop: false,
  setSession: () => {},
  updateUser: () => {},
  logout: async () => {},
});

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [user, setUser] = useState<AuthUser | null>(null);
  const [token, setToken] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const router = useRouter();

  useEffect(() => {
    const savedToken = localStorage.getItem("sany3y_token");
    const savedUser = localStorage.getItem("sany3y_user");

    if (savedToken && savedUser) {
      try {
        setToken(savedToken);
        setUser(JSON.parse(savedUser));
      } catch {
        localStorage.removeItem("sany3y_token");
        localStorage.removeItem("sany3y_user");
      }
    }
    setIsLoading(false);
  }, []);

  const setSession = (newToken: string, newUser: AuthUser) => {
    setToken(newToken);
    setUser(newUser);
    localStorage.setItem("sany3y_token", newToken);
    localStorage.setItem("sany3y_user", JSON.stringify(newUser));
    document.cookie = `sany3y_token=${newToken}; path=/; max-age=86400; SameSite=Lax`;
  };

  const updateUser = (updatedUser: AuthUser) => {
    setUser(updatedUser);
    localStorage.setItem("sany3y_user", JSON.stringify(updatedUser));
  };

  const logout = async () => {
    try {
      if (token) {
        await apiFetch("/api/Auth/logout", { method: "POST" });
      }
    } catch {
      // Ignore network errors on logout
    }
    setToken(null);
    setUser(null);
    localStorage.removeItem("sany3y_token");
    localStorage.removeItem("sany3y_user");
    document.cookie = "sany3y_token=; path=/; max-age=0";
    router.push("/");
  };

  const roles = user?.roles || (user?.role ? [user.role] : []);
  const isAdmin = roles.includes("Admin");
  const isTechnician = roles.includes("Technician");
  const isClient = roles.includes("Client") || roles.includes("User");
  const isShop = Boolean(user?.isShop);

  return (
    <AuthContext.Provider
      value={{
        user,
        token,
        isLoading,
        isAuthenticated: Boolean(token && user),
        isAdmin,
        isTechnician,
        isClient,
        isShop,
        setSession,
        updateUser,
        logout,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
}

export const useAuth = () => useContext(AuthContext);
