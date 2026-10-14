"use client";

import { useCallback, useEffect, useState } from "react";
import Link from "next/link";
import { Inbox, Plus, UserCheck } from "lucide-react";
import { api } from "@/lib/api";
import { useRequireAuth } from "@/lib/auth";
import { timeAgo } from "@/lib/format";
import { TicketSummary } from "@/lib/types";
import StatusBadge from "@/components/StatusBadge";
import { useChatHub } from "@/lib/useChatHub";

const FILTERS = ["All", "Open", "InProgress", "Resolved", "Closed"] as const;

const priorityBar: Record<string, string> = {
  Low: "bg-slate-300",
  Medium: "bg-sky-400",
  High: "bg-orange-400",
  Urgent: "bg-red-500",
};

export default function TicketsPage() {
  const { user, loading } = useRequireAuth();
  const [tickets, setTickets] = useState<TicketSummary[]>([]);
  const [filter, setFilter] = useState<(typeof FILTERS)[number]>("All");
  const [error, setError] = useState("");
  const [fetching, setFetching] = useState(true);

  const loadTickets = useCallback(() => {
  return api<TicketSummary[]>("/api/tickets")
    .then(setTickets)
    .catch((e) => setError(e.message))
    .finally(() => setFetching(false));
}, []);

useEffect(() => {
  if (user) loadTickets();
}, [user, loadTickets]);

useChatHub(!!user, {
  TicketChanged: () => loadTickets(),
});

  if (loading || !user) return null;

  const isCustomer = user.role === "Customer";
  const visible = filter === "All" ? tickets : tickets.filter((t) => t.status === filter);

  return (
    <div>
      <div className="mb-6 flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-semibold">{isCustomer ? "My tickets" : "All tickets"}</h1>
          <p className="text-sm text-slate-500">
            {tickets.length} {tickets.length === 1 ? "ticket" : "tickets"} in total
          </p>
        </div>
        {isCustomer && (
          <Link
            href="/tickets/new"
            className="inline-flex items-center gap-2 rounded-lg bg-indigo-600 px-4 py-2.5 text-sm font-medium text-white shadow-sm hover:bg-indigo-700"
          >
            <Plus className="h-4 w-4" /> New ticket
          </Link>
        )}
      </div>

      <div className="mb-4 flex flex-wrap gap-2">
        {FILTERS.map((f) => (
          <button
            key={f}
            onClick={() => setFilter(f)}
            className={`rounded-full px-3 py-1 text-sm transition ${
              filter === f
                ? "bg-indigo-600 text-white"
                : "bg-white text-slate-600 ring-1 ring-slate-200 hover:bg-slate-100"
            }`}
          >
            {f === "InProgress" ? "In progress" : f}
          </button>
        ))}
      </div>

      {error && <p className="mb-3 rounded-lg bg-red-50 px-3 py-2 text-sm text-red-700">{error}</p>}

      {fetching && (
        <div className="space-y-3">
          {[1, 2, 3].map((i) => (
            <div key={i} className="h-20 animate-pulse rounded-xl bg-slate-200/70" />
          ))}
        </div>
      )}

      {!fetching && visible.length === 0 && (
        <div className="rounded-xl border border-dashed border-slate-300 bg-white py-14 text-center">
          <Inbox className="mx-auto h-10 w-10 text-slate-300" />
          <p className="mt-3 font-medium">No tickets here</p>
          <p className="text-sm text-slate-500">
            {isCustomer && filter === "All"
              ? "Create your first ticket and our team will get back to you."
              : "Nothing matches this filter."}
          </p>
        </div>
      )}

      <ul className="space-y-3">
        {visible.map((t) => (
          <li key={t.id}>
            <Link
              href={`/tickets/${t.id}`}
              className="relative flex overflow-hidden rounded-xl border border-slate-200 bg-white shadow-sm transition hover:border-indigo-300 hover:shadow"
            >
              <span className={`w-1.5 ${priorityBar[t.priority] ?? "bg-slate-300"}`} />
              <div className="flex-1 p-4">
                <div className="flex items-start justify-between gap-3">
                  <p className="font-medium">
                    <span className="text-slate-400">#{t.id}</span> {t.subject}
                  </p>
                  <div className="flex shrink-0 gap-2">
                        {t.botActive && <StatusBadge value="Bot" />}
                        {t.escalatedFromBot && !t.botActive && t.status !== "Resolved" && t.status !== "Closed" && (
                            <StatusBadge value="Escalated" />
                        )}
                        <StatusBadge value={t.priority} />
                        <StatusBadge value={t.status} />
                    </div>
                </div>
                <p className="mt-1.5 flex flex-wrap items-center gap-x-3 text-sm text-slate-500">
                  <span>{t.customerName}</span>
                  <span>{timeAgo(t.updatedAt ?? t.createdAt)}</span>
                  {t.assignedAgentName && (
                    <span className="inline-flex items-center gap-1">
                      <UserCheck className="h-3.5 w-3.5" /> {t.assignedAgentName}
                    </span>
                  )}
                </p>
              </div>
            </Link>
          </li>
        ))}
      </ul>
    </div>
  );
}