"use client";

import { createContext, useContext, useEffect, useState, ReactNode } from "react";
import { useRouter } from "next/navigation";
import { api } from "./api";
import { AuthUser } from "./types";

interface AuthResponse extends AuthUser {
  token: string;
}

interface AuthContextType {
  user: AuthUser | null;
  loading: boolean;
  login: (email: string, password: string) => Promise<void>;
  register: (fullName: string, email: string, password: string) => Promise<void>;
  logout: () => void;
}

const AuthContext = createContext<AuthContextType | null>(null);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<AuthUser | null>(null);
  const [loading, setLoading] = useState(true);
  const router = useRouter();

  useEffect(() => {
    const saved = localStorage.getItem("user");
    if (saved && localStorage.getItem("token")) setUser(JSON.parse(saved));
    setLoading(false);
  }, []);

  function saveSession(res: AuthResponse) {
    const { token, ...u } = res;
    localStorage.setItem("token", token);
    localStorage.setItem("user", JSON.stringify(u));
    setUser(u);
  }

  async function login(email: string, password: string) {
    const res = await api<AuthResponse>("/api/auth/login", {
      method: "POST",
      body: JSON.stringify({ email, password }),
    });
    saveSession(res);
    router.push(res.role === "Customer" ? "/tickets" : "/dashboard");
  }

  async function register(fullName: string, email: string, password: string) {
    const res = await api<AuthResponse>("/api/auth/register", {
      method: "POST",
      body: JSON.stringify({ fullName, email, password }),
    });
    saveSession(res);
    router.push("/tickets");
  }

  function logout() {
    localStorage.removeItem("token");
    localStorage.removeItem("user");
    setUser(null);
    router.push("/login");
  }

  return (
    <AuthContext.Provider value={{ user, loading, login, register, logout }}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error("useAuth must be used inside AuthProvider");
  return ctx;
}

// Call at the top of any page that needs a logged-in user
export function useRequireAuth() {
  const { user, loading } = useAuth();
  const router = useRouter();

  useEffect(() => {
    if (!loading && !user) router.replace("/login");
  }, [loading, user, router]);

  return { user, loading };
}
