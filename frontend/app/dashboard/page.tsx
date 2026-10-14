"use client";

import { useCallback, useEffect, useState } from "react";
import Link from "next/link";
import { AlertTriangle, Bot, CheckCircle2, Clock, Inbox, UserCheck } from "lucide-react";
import { api } from "@/lib/api";
import { useRequireAuth } from "@/lib/auth";
import { useChatHub } from "@/lib/useChatHub";
import { Dashboard } from "@/lib/types";

const priorityBar: Record<string, string> = {
  Low: "bg-slate-400",
  Medium: "bg-sky-500",
  High: "bg-orange-500",
  Urgent: "bg-red-500",
};

function formatMinutes(m: number | null) {
  if (m === null) return "No data yet";
  if (m < 60) return `${Math.round(m)} min`;
  if (m < 60 * 24) return `${(m / 60).toFixed(1)} h`;
  return `${(m / 60 / 24).toFixed(1)} days`;
}

function StatCard({
  label, value, icon: Icon, tone, hint,
}: {
  label: string;
  value: number | string;
  icon: React.ComponentType<{ className?: string }>;
  tone: string;
  hint?: string;
}) {
  return (
    <div className="rounded-xl border border-slate-200 bg-white p-4 shadow-sm">
      <div className="flex items-center justify-between">
        <p className="text-sm text-slate-500">{label}</p>
        <span className={`flex h-8 w-8 items-center justify-center rounded-lg ${tone}`}>
          <Icon className="h-4 w-4" />
        </span>
      </div>
      <p className="mt-2 text-3xl font-semibold">{value}</p>
      {hint && <p className="mt-1 text-xs text-slate-400">{hint}</p>}
    </div>
  );
}

export default function DashboardPage() {
  const { user, loading } = useRequireAuth();
  const [data, setData] = useState<Dashboard | null>(null);
  const [error, setError] = useState("");

  const isStaff = user?.role === "Agent" || user?.role === "Admin";

  const load = useCallback(
    () =>
      api<Dashboard>("/api/dashboard")
        .then(setData)
        .catch((e) => setError(e.message)),
    []
  );

  useEffect(() => {
    if (isStaff) load();
  }, [isStaff, load]);

  // Any ticket change refreshes the numbers
  useChatHub(!!isStaff, { TicketChanged: () => load() });

  if (loading || !user) return null;
  if (!isStaff) {
    return (
      <div className="rounded-xl border border-slate-200 bg-white py-14 text-center">
        <p className="font-medium">The dashboard is for agents and admins.</p>
        <Link href="/tickets" className="mt-2 inline-block text-sm text-indigo-600 hover:underline">
          Go to my tickets
        </Link>
      </div>
    );
  }

  if (error && !data) return <p className="rounded-lg bg-red-50 px-3 py-2 text-sm text-red-700">{error}</p>;
  if (!data) return <div className="h-60 animate-pulse rounded-xl bg-slate-200/70" />;

  const maxDay = Math.max(1, ...data.last7Days.map((d) => d.count));
  const maxPriority = Math.max(1, ...data.byPriority.map((p) => p.count));

  return (
    <div className="space-y-8">
      <div className="flex items-end justify-between">
        <div>
          <h1 className="text-2xl font-semibold">Dashboard</h1>
          <p className="text-sm text-slate-500">Live overview of the support queue.</p>
        </div>
        <Link
          href="/tickets"
          className="rounded-lg border border-slate-300 bg-white px-3 py-2 text-sm font-medium hover:bg-slate-50"
        >
          View all tickets
        </Link>
      </div>

      <div className="grid grid-cols-2 gap-4 md:grid-cols-4">
        <StatCard label="Needs an agent" value={data.needsAgent} icon={AlertTriangle}
          tone="bg-red-50 text-red-600" hint="Unassigned, not with the bot" />
        <StatCard label="Escalated from bot" value={data.escalatedOpen} icon={UserCheck}
          tone="bg-rose-50 text-rose-600" hint="Still open" />
        <StatCard label="My open tickets" value={data.myOpen} icon={Inbox}
          tone="bg-indigo-50 text-indigo-600" hint="Assigned to you" />
        <StatCard label="With the bot now" value={data.botHandling} icon={Bot}
          tone="bg-violet-50 text-violet-600" hint="Being answered automatically" />
      </div>

      <div className="grid grid-cols-2 gap-4 md:grid-cols-4">
        <StatCard label="Open" value={data.open} icon={Inbox} tone="bg-blue-50 text-blue-600" />
        <StatCard label="In progress" value={data.inProgress} icon={Clock} tone="bg-amber-50 text-amber-600" />
        <StatCard label="Resolved" value={data.resolved} icon={CheckCircle2} tone="bg-emerald-50 text-emerald-600"
          hint={`${data.resolvedByBot} resolved by the bot`} />
        <StatCard label="Avg first reply" value={formatMinutes(data.avgFirstResponseMinutes)} icon={Clock}
          tone="bg-slate-100 text-slate-600" hint="Agent response, last 30 days" />
      </div>

      <div className="grid gap-6 md:grid-cols-2">
        <section className="rounded-xl border border-slate-200 bg-white p-5 shadow-sm">
          <h2 className="mb-4 font-semibold">Tickets created, last 7 days</h2>
          <div className="flex h-40 items-end gap-3">
            {data.last7Days.map((d) => (
              <div key={d.date} className="flex flex-1 flex-col items-center justify-end gap-1">
                <span className="text-xs text-slate-500">{d.count}</span>
                <div
                  className="w-full rounded-t bg-indigo-500"
                  style={{ height: d.count ? Math.max(6, Math.round((d.count / maxDay) * 100)) : 2 }}
                />
                <span className="text-xs text-slate-500">
                  {new Date(d.date + "T00:00:00").toLocaleDateString(undefined, { weekday: "short" })}
                </span>
              </div>
            ))}
          </div>
        </section>

        <section className="rounded-xl border border-slate-200 bg-white p-5 shadow-sm">
          <h2 className="mb-4 font-semibold">By priority</h2>
          <ul className="space-y-3">
            {data.byPriority.map((p) => (
              <li key={p.priority}>
                <div className="mb-1 flex justify-between text-sm">
                  <span>{p.priority}</span>
                  <span className="text-slate-500">{p.count}</span>
                </div>
                <div className="h-2 rounded-full bg-slate-100">
                  <div
                    className={`h-2 rounded-full ${priorityBar[p.priority] ?? "bg-slate-400"}`}
                    style={{ width: `${(p.count / maxPriority) * 100}%` }}
                  />
                </div>
              </li>
            ))}
          </ul>
        </section>
      </div>
    </div>
  );
}
