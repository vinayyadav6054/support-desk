"use client";

import { useCallback, useEffect, useMemo, useState } from "react";
import { Search } from "lucide-react";
import { api } from "@/lib/api";
import { useRequireAuth } from "@/lib/auth";
import { formatDate } from "@/lib/format";
import { ManagedUser, Role } from "@/lib/types";
import { Avatar, inputClass } from "@/components/ui";

const ROLES: Role[] = ["Customer", "Agent", "Admin"];

const roleStyles: Record<Role, string> = {
  Customer: "bg-slate-100 text-slate-700 ring-slate-200",
  Agent: "bg-sky-50 text-sky-700 ring-sky-200",
  Admin: "bg-violet-50 text-violet-700 ring-violet-200",
};

export default function UsersPage() {
  const { user, loading } = useRequireAuth();
  const [users, setUsers] = useState<ManagedUser[]>([]);
  const [search, setSearch] = useState("");
  const [savingId, setSavingId] = useState<number | null>(null);
  const [error, setError] = useState("");
  const [notice, setNotice] = useState("");
  const [fetching, setFetching] = useState(true);

  const isAdmin = user?.role === "Admin";

  const load = useCallback(
    () =>
      api<ManagedUser[]>("/api/users")
        .then(setUsers)
        .catch((e) => setError(e.message))
        .finally(() => setFetching(false)),
    []
  );

  useEffect(() => {
    if (isAdmin) load();
  }, [isAdmin, load]);

  const filtered = useMemo(() => {
    const q = search.trim().toLowerCase();
    if (!q) return users;
    return users.filter(
      (u) => u.fullName.toLowerCase().includes(q) || u.email.toLowerCase().includes(q)
    );
  }, [users, search]);

  async function changeRole(target: ManagedUser, role: Role) {
    if (role === target.role) return;
    setSavingId(target.id);
    setError("");
    setNotice("");
    try {
      const updated = await api<ManagedUser>(`/api/users/${target.id}/role`, {
        method: "PUT",
        body: JSON.stringify({ role }),
      });
      setUsers((list) => list.map((u) => (u.id === updated.id ? updated : u)));
      setNotice(`${updated.fullName} is now ${updated.role}. The change applies the next time they log in.`);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Could not change role");
    } finally {
      setSavingId(null);
    }
  }

  if (loading || !user) return null;
  if (!isAdmin) {
    return (
      <div className="rounded-xl border border-slate-200 bg-white py-14 text-center">
        <p className="font-medium">Only admins can manage users.</p>
      </div>
    );
  }

  return (
    <div>
      <div className="mb-6">
        <h1 className="text-2xl font-semibold">Users</h1>
        <p className="text-sm text-slate-500">
          New sign-ups start as Customers. Promote people to Agent to let them handle tickets.
        </p>
      </div>

      <div className="relative mb-4">
        <Search className="pointer-events-none absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-slate-400" />
        <input
          placeholder="Search by name or email" value={search}
          onChange={(e) => setSearch(e.target.value)} className={`${inputClass} pl-10`}
        />
      </div>

      {error && <p className="mb-3 rounded-lg bg-red-50 px-3 py-2 text-sm text-red-700">{error}</p>}
      {notice && <p className="mb-3 rounded-lg bg-emerald-50 px-3 py-2 text-sm text-emerald-700">{notice}</p>}

      {fetching && <div className="h-40 animate-pulse rounded-xl bg-slate-200/70" />}

      <ul className="divide-y divide-slate-100 overflow-hidden rounded-xl border border-slate-200 bg-white shadow-sm">
        {filtered.map((u) => {
          const isMe = u.id === user.userId;
          return (
            <li key={u.id} className="flex flex-wrap items-center justify-between gap-3 p-4">
              <div className="flex items-center gap-3">
                <Avatar name={u.fullName} />
                <div>
                  <p className="text-sm font-medium">
                    {u.fullName}
                    {isMe && <span className="ml-2 text-xs text-slate-400">(you)</span>}
                  </p>
                  <p className="text-xs text-slate-500">
                    {u.email} · joined {formatDate(u.createdAt)}
                  </p>
                </div>
              </div>

              <div className="flex items-center gap-3">
                <span className={`rounded-full px-2.5 py-0.5 text-xs font-medium ring-1 ring-inset ${roleStyles[u.role]}`}>
                  {u.role}
                </span>
                <select
                  value={u.role}
                  disabled={isMe || savingId === u.id}
                  onChange={(e) => changeRole(u, e.target.value as Role)}
                  title={isMe ? "You can't change your own role" : "Change role"}
                  className="rounded-lg border border-slate-300 bg-white px-2 py-1.5 text-sm disabled:cursor-not-allowed disabled:opacity-50"
                >
                  {ROLES.map((r) => <option key={r}>{r}</option>)}
                </select>
              </div>
            </li>
          );
        })}
        {!fetching && filtered.length === 0 && (
          <li className="p-8 text-center text-sm text-slate-500">No users match your search.</li>
        )}
      </ul>
    </div>
  );
}