"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { BookOpen, LayoutDashboard, LifeBuoy, LogOut, Ticket, Users } from "lucide-react";
import { useAuth } from "@/lib/auth";
import { Role } from "@/lib/types";
import { Avatar } from "./ui";
import { useHydrated } from "@/lib/useHydrated";

const links: {
  href: string;
  label: string;
  icon: React.ComponentType<{ className?: string }>;
  roles: Role[];
}[] = [
  { href: "/dashboard", label: "Dashboard", icon: LayoutDashboard, roles: ["Agent", "Admin"] },
  { href: "/tickets", label: "Tickets", icon: Ticket, roles: ["Customer", "Agent", "Admin"] },
  { href: "/admin/users", label: "Users", icon: Users, roles: ["Admin"] },
  { href: "/admin/kb", label: "Knowledge base", icon: BookOpen, roles: ["Admin"] },
];

export default function Navbar() {
  const { user: authUser, logout } = useAuth();
  const hydrated = useHydrated();
  const user = hydrated ? authUser : null;
  const pathname = usePathname();

  return (
    <header className="sticky top-0 z-10 border-b border-slate-200 bg-white/80 backdrop-blur">
      <div className="mx-auto flex max-w-4xl items-center justify-between gap-4 px-6 py-3">
        <div className="flex items-center gap-6">
          <Link href="/tickets" className="flex items-center gap-2 font-semibold">
            <span className="flex h-8 w-8 items-center justify-center rounded-lg bg-indigo-600 text-white">
              <LifeBuoy className="h-4 w-4" />
            </span>
            <span className="hidden sm:inline">Support Desk</span>
          </Link>

          {user && (
            <nav className="flex items-center gap-1">
              {links
                .filter((l) => l.roles.includes(user.role))
                .map(({ href, label, icon: Icon }) => {
                  const active = pathname.startsWith(href);
                  return (
                    <Link
                      key={href}
                      href={href}
                      title={label}
                      className={`inline-flex items-center gap-1.5 rounded-lg px-3 py-1.5 text-sm transition ${
                        active
                          ? "bg-indigo-50 font-medium text-indigo-700"
                          : "text-slate-600 hover:bg-slate-100"
                      }`}
                    >
                      <Icon className="h-4 w-4" />
                      <span className="hidden md:inline">{label}</span>
                    </Link>
                  );
                })}
            </nav>
          )}
        </div>

        {user && (
          <div className="flex items-center gap-3">
            <div className="hidden text-right lg:block">
              <p className="text-sm font-medium leading-tight">{user.fullName}</p>
              <p className="text-xs text-slate-500">{user.role}</p>
            </div>
            <Avatar name={user.fullName} />
            <button
              onClick={logout}
              title="Log out"
              className="rounded-lg p-2 text-slate-500 transition hover:bg-slate-100 hover:text-slate-900"
            >
              <LogOut className="h-4 w-4" />
            </button>
          </div>
        )}
      </div>
    </header>
  );
}