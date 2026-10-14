"use client";

import Link from "next/link";
import { BookOpen, LifeBuoy, LogOut } from "lucide-react";
import { useAuth } from "@/lib/auth";
import { Avatar } from "./ui";

export default function Navbar() {
    const { user, logout } = useAuth();

    return (
        <header className="sticky top-0 z-10 border-b border-slate-200 bg-white/80 backdrop-blur">
            <div className="mx-auto flex max-w-4xl items-center justify-between px-6 py-3">
                <Link href="/tickets" className="flex items-center gap-2 font-semibold">
                    <span className="flex h-8 w-8 items-center justify-center rounded-lg bg-indigo-600 text-white">
                        <LifeBuoy className="h-4 w-4" />
                    </span>
                    Support Desk
                </Link>

                {user && (
                    <div className="flex items-center gap-3">
                        <div className="hidden text-right sm:block">
                            <p className="text-sm font-medium leading-tight">{user.fullName}</p>
                            <p className="text-xs text-slate-500">{user.role}</p>
                        </div>

                        {user.role === "Admin" && (
                            <Link
                                href="/admin/kb"
                                className="inline-flex items-center gap-1.5 rounded-lg px-3 py-1.5 text-sm text-slate-600 hover:bg-slate-100"
                            >
                                <BookOpen className="h-4 w-4" /> Knowledge base
                            </Link>
                        )}

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