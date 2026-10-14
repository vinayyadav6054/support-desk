"use client";

import { useState } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { ArrowLeft } from "lucide-react";
import { api } from "@/lib/api";
import { useRequireAuth } from "@/lib/auth";
import { PRIORITIES, TicketDetail } from "@/lib/types";
import { Button, inputClass } from "@/components/ui";

export default function NewTicketPage() {
  const { user, loading } = useRequireAuth();
  const router = useRouter();
  const [subject, setSubject] = useState("");
  const [description, setDescription] = useState("");
  const [priority, setPriority] = useState<string>("Medium");
  const [error, setError] = useState("");
  const [busy, setBusy] = useState(false);

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    setError("");
    setBusy(true);
    try {
      const ticket = await api<TicketDetail>("/api/tickets", {
        method: "POST",
        body: JSON.stringify({ subject, description, priority }),
      });
      router.push(`/tickets/${ticket.id}`);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Could not create ticket");
      setBusy(false);
    }
  }

  if (loading || !user) return null;

  return (
    <div className="mx-auto max-w-xl">
      <Link href="/tickets" className="mb-4 inline-flex items-center gap-1 text-sm text-slate-500 hover:text-slate-900">
        <ArrowLeft className="h-4 w-4" /> Back to tickets
      </Link>

      <div className="rounded-2xl border border-slate-200 bg-white p-6 shadow-sm">
        <h1 className="text-xl font-semibold">Create a ticket</h1>
        <p className="mb-5 text-sm text-slate-500">Tell us what is going wrong and we will help.</p>

        <form onSubmit={handleSubmit} className="space-y-4">
          <div>
            <label className="mb-1 block text-sm font-medium">Subject</label>
            <input
              required maxLength={200} placeholder="Short summary of the problem" value={subject}
              onChange={(e) => setSubject(e.target.value)} className={inputClass}
            />
          </div>

          <div>
            <label className="mb-1 block text-sm font-medium">Description</label>
            <textarea
              rows={5} placeholder="What happened? What did you expect?" value={description}
              onChange={(e) => setDescription(e.target.value)} className={inputClass}
            />
          </div>

          <div>
            <label className="mb-1 block text-sm font-medium">Priority</label>
            <div className="grid grid-cols-4 gap-2">
              {PRIORITIES.map((p) => (
                <button
                  key={p} type="button" onClick={() => setPriority(p)}
                  className={`rounded-lg border px-3 py-2 text-sm transition ${
                    priority === p
                      ? "border-indigo-600 bg-indigo-50 font-medium text-indigo-700"
                      : "border-slate-300 text-slate-600 hover:bg-slate-50"
                  }`}
                >
                  {p}
                </button>
              ))}
            </div>
          </div>

          {error && <p className="rounded-lg bg-red-50 px-3 py-2 text-sm text-red-700">{error}</p>}

          <Button type="submit" loading={busy}>
            {busy ? "Submitting..." : "Submit ticket"}
          </Button>
        </form>
      </div>
    </div>
  );
}