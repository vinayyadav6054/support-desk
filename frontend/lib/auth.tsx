"use client";

import { createContext, useContext, useEffect, useMemo, useSyncExternalStore, ReactNode } from "react";
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

const AUTH_EVENT = "auth-change";

function subscribe(callback: () => void) {
  window.addEventListener("storage", callback);   // other tabs
  window.addEventListener(AUTH_EVENT, callback);  // this tab
  return () => {
    window.removeEventListener("storage", callback);
    window.removeEventListener(AUTH_EVENT, callback);
  };
}

// Returns a string (or null) so React can compare snapshots cheaply
function getSnapshot(): string | null {
  return localStorage.getItem("token") ? localStorage.getItem("user") : null;
}

const getServerSnapshot = () => null;

// "false while hydrating, true afterwards"
const subscribeNoop = () => () => {};

export function AuthProvider({ children }: { children: ReactNode }) {
  const router = useRouter();

  const raw = useSyncExternalStore(subscribe, getSnapshot, getServerSnapshot);
  const hydrated = useSyncExternalStore(subscribeNoop, () => true, () => false);

  const user = useMemo<AuthUser | null>(() => {
    if (!raw) return null;
    try {
      return JSON.parse(raw) as AuthUser;
    } catch {
      return null;
    }
  }, [raw]);

  function saveSession(res: AuthResponse) {
    const { token, ...u } = res;
    localStorage.setItem("token", token);
    localStorage.setItem("user", JSON.stringify(u));
    window.dispatchEvent(new Event(AUTH_EVENT));
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
    window.dispatchEvent(new Event(AUTH_EVENT));
    router.push("/login");
  }

  return (
    <AuthContext.Provider value={{ user, loading: !hydrated, login, register, logout }}>
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