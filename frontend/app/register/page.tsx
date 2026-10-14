"use client";

import { useState } from "react";
import Link from "next/link";
import { Lock, Mail, User } from "lucide-react";
import { useAuth } from "@/lib/auth";
import AuthShell from "@/components/AuthShell";
import { Button, inputClass } from "@/components/ui";

export default function RegisterPage() {
  const { register } = useAuth();
  const [fullName, setFullName] = useState("");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");
  const [busy, setBusy] = useState(false);

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    setError("");
    setBusy(true);
    try {
      await register(fullName, email, password);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Registration failed");
      setBusy(false);
    }
  }

  return (
    <AuthShell
      title="Create your account"
      subtitle="Raise tickets and chat with our support team"
      footer={
        <>
          Already have an account?{" "}
          <Link href="/login" className="font-medium text-indigo-600 hover:underline">
            Log in
          </Link>
        </>
      }
    >
      <form onSubmit={handleSubmit} className="space-y-4">
        <div className="relative">
          <User className="pointer-events-none absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-slate-400" />
          <input
            required placeholder="Full name" value={fullName}
            onChange={(e) => setFullName(e.target.value)}
            className={`${inputClass} pl-10`}
          />
        </div>
        <div className="relative">
          <Mail className="pointer-events-none absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-slate-400" />
          <input
            type="email" required placeholder="Email address" value={email}
            onChange={(e) => setEmail(e.target.value)}
            className={`${inputClass} pl-10`}
          />
        </div>
        <div className="relative">
          <Lock className="pointer-events-none absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-slate-400" />
          <input
            type="password" required minLength={8} placeholder="Password (min 8 characters)"
            value={password} onChange={(e) => setPassword(e.target.value)}
            className={`${inputClass} pl-10`}
          />
        </div>

        {error && (
          <p className="rounded-lg bg-red-50 px-3 py-2 text-sm text-red-700">{error}</p>
        )}

        <Button type="submit" loading={busy} className="w-full">
          {busy ? "Creating account..." : "Create account"}
        </Button>
      </form>
    </AuthShell>
  );
}