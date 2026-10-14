"use client";

import { Suspense, useCallback, useEffect, useRef, useState } from "react";
import { useParams } from "next/navigation";
import Link from "next/link";
import { ArrowLeft, Bot, CheckCircle2, Send, UserCheck, UserRound } from "lucide-react";
import { api } from "@/lib/api";
import { useRequireAuth } from "@/lib/auth";
import { useChatHub } from "@/lib/useChatHub";
import { formatDate } from "@/lib/format";
import { Message, STATUSES, TicketDetail } from "@/lib/types";
import StatusBadge from "@/components/StatusBadge";
import { Avatar, Button, inputClass } from "@/components/ui";


function TicketDetailContent() {
    const { id } = useParams<{ id: string }>();
    const ticketId = Number(id);
    const { user, loading } = useRequireAuth();
    const [ticket, setTicket] = useState<TicketDetail | null>(null);
    const [error, setError] = useState("");
    const [reply, setReply] = useState("");
    const [sending, setSending] = useState(false);
    const [botBusy, setBotBusy] = useState(false);
    const [typingName, setTypingName] = useState<string | null>(null);
    const bottomRef = useRef<HTMLDivElement>(null);
    const typingClear = useRef<ReturnType<typeof setTimeout> | undefined>(undefined);
    const typingStop = useRef<ReturnType<typeof setTimeout> | undefined>(undefined);
    const lastTypingSent = useRef(0);

    const isStaff = user?.role === "Agent" || user?.role === "Admin";

    const loadTicket = useCallback(() => {
        return api<TicketDetail>(`/api/tickets/${id}`)
            .then(setTicket)
            .catch((e) => setError(e.status === 404 ? "Ticket not found." : e.message));
    }, [id]);

    useEffect(() => {
        if (user) loadTicket();
    }, [user, loadTicket]);

    // Adds a message once, even if it arrives from both the POST response and SignalR
    function addMessage(msg: Message) {
        setTicket((t) =>
            t && !t.messages.some((m) => m.id === msg.id)
                ? { ...t, messages: [...t.messages, msg] }
                : t
        );
    }

    const { connection, connected } = useChatHub(
        !!user,
        {
            ReceiveMessage: (incomingTicketId: number, msg: Message) => {
                if (incomingTicketId !== ticketId) return;
                addMessage(msg);
                setTypingName(null);
            },
            TicketUpdated: (updated: TicketDetail) => {
                if (updated.id === ticketId) setTicket(updated);
            },
            Typing: (p: { ticketId: number; name: string; isTyping: boolean }) => {
                if (p.ticketId !== ticketId) return;
                clearTimeout(typingClear.current);
                if (p.isTyping) {
                    setTypingName(p.name);
                    typingClear.current = setTimeout(() => setTypingName(null), 4000);
                } else {
                    setTypingName(null);
                }
            },
        },
        async (conn) => {
            await conn.invoke("JoinTicket", ticketId);
            loadTicket(); // catch anything missed while disconnected
        }
    );

    useEffect(() => {
        bottomRef.current?.scrollIntoView({ behavior: "smooth" });
    }, [ticket?.messages.length, typingName]);

    function handleReplyChange(value: string) {
        setReply(value);
        if (!connection) return;

        const now = Date.now();
        if (now - lastTypingSent.current > 2000) {
            lastTypingSent.current = now;
            connection.invoke("Typing", ticketId, true).catch(() => { });
        }
        clearTimeout(typingStop.current);
        typingStop.current = setTimeout(() => {
            lastTypingSent.current = 0;
            connection.invoke("Typing", ticketId, false).catch(() => { });
        }, 2500);
    }

    async function sendReply(e: React.FormEvent) {
        e.preventDefault();
        if (!reply.trim() || !ticket) return;
        setSending(true);
        try {
            const msg = await api<Message>(`/api/tickets/${id}/messages`, {
                method: "POST",
                body: JSON.stringify({ content: reply }),
            });
            addMessage(msg);
            setReply("");
            setError("");
            clearTimeout(typingStop.current);
            lastTypingSent.current = 0;
            connection?.invoke("Typing", ticketId, false).catch(() => { });
        } catch (err) {
            setError(err instanceof Error ? err.message : "Could not send message");
        } finally {
            setSending(false);
        }
    }

    async function assignToMe() {
        try {
            setTicket(await api<TicketDetail>(`/api/tickets/${id}/assign-to-me`, { method: "POST" }));
        } catch (err) {
            setError(err instanceof Error ? err.message : "Could not assign ticket");
        }
    }

    async function changeStatus(status: string) {
        try {
            setTicket(
                await api<TicketDetail>(`/api/tickets/${id}`, {
                    method: "PATCH",
                    body: JSON.stringify({ status }),
                })
            );
        } catch (err) {
            setError(err instanceof Error ? err.message : "Could not update status");
        }
    }

    async function botAction(action: "resolve" | "escalate") {
        setBotBusy(true);
        try {
            setTicket(await api<TicketDetail>(`/api/tickets/${id}/bot/${action}`, { method: "POST" }));
            setError("");
        } catch (err) {
            setError(err instanceof Error ? err.message : "Could not update ticket");
        } finally {
            setBotBusy(false);
        }
    }

    if (loading || !user) return null;
    if (error && !ticket) {
        return (
            <div className="rounded-xl border border-slate-200 bg-white py-14 text-center">
                <p className="font-medium">{error}</p>
                <Link href="/tickets" className="mt-2 inline-block text-sm text-indigo-600 hover:underline">
                    Back to tickets
                </Link>
            </div>
        );
    }
    if (!ticket) return <div className="h-40 animate-pulse rounded-xl bg-slate-200/70" />;

    return (
        <div>
            <div className="mb-4 flex items-center justify-between">
                <Link href="/tickets" className="inline-flex items-center gap-1 text-sm text-slate-500 hover:text-slate-900">
                    <ArrowLeft className="h-4 w-4" /> Back to tickets
                </Link>
                <span className="inline-flex items-center gap-1.5 text-xs text-slate-500">
                    <span className={`h-2 w-2 rounded-full ${connected ? "bg-emerald-500" : "bg-amber-400 animate-pulse"}`} />
                    {connected ? "Live" : "Connecting..."}
                </span>
            </div>

            <div className="rounded-2xl border border-slate-200 bg-white p-5 shadow-sm">
                <div className="flex flex-wrap items-start justify-between gap-3">
                    <div>
                        <h1 className="text-xl font-semibold">
                            <span className="text-slate-400">#{ticket.id}</span> {ticket.subject}
                        </h1>
                        <p className="mt-1 flex flex-wrap items-center gap-x-3 text-sm text-slate-500">
                            <span>Raised by {ticket.customerName}</span>
                            <span>{formatDate(ticket.createdAt)}</span>
                            {ticket.assignedAgentName && (
                                <span className="inline-flex items-center gap-1">
                                    <UserCheck className="h-3.5 w-3.5" /> {ticket.assignedAgentName}
                                </span>
                            )}
                        </p>
                    </div>
                    <div className="flex gap-2">
                        {ticket.botActive && <StatusBadge value="Bot" />}
                        {ticket.escalatedFromBot && !ticket.botActive && <StatusBadge value="Escalated" />}
                        <StatusBadge value={ticket.priority} />
                        <StatusBadge value={ticket.status} />
                    </div>
                </div>

                {isStaff && (
                    <div className="mt-4 flex flex-wrap items-center gap-3 border-t border-slate-100 pt-4">
                        {ticket.assignedAgentId !== user.userId && (
                            <button
                                onClick={assignToMe}
                                className="rounded-lg border border-slate-300 px-3 py-1.5 text-sm font-medium hover:bg-slate-50"
                            >
                                Assign to me
                            </button>
                        )}
                        <label className="flex items-center gap-2 text-sm text-slate-600">
                            Status
                            <select
                                value={ticket.status}
                                onChange={(e) => changeStatus(e.target.value)}
                                className="rounded-lg border border-slate-300 bg-white px-2 py-1.5 text-sm"
                            >
                                {STATUSES.map((s) => (
                                    <option key={s} value={s}>{s === "InProgress" ? "In progress" : s}</option>
                                ))}
                            </select>
                        </label>
                    </div>
                )}
            </div>

            <div className="mt-6 space-y-4">
                {ticket.messages.map((m) => {
                    const isBot = m.senderType === "Bot";
                    const mine = isStaff ? m.senderType === "Agent" : m.senderType === "Customer";

                    return (
                        <div key={m.id} className={`flex items-end gap-2 ${mine ? "flex-row-reverse" : ""}`}>
                            {isBot ? (
                                <span className="flex h-8 w-8 shrink-0 items-center justify-center rounded-full bg-violet-100 text-violet-700">
                                    <Bot className="h-4 w-4" />
                                </span>
                            ) : (
                                <Avatar name={m.senderName ?? "?"} />
                            )}
                            <div className={`max-w-[75%] ${mine ? "items-end text-right" : ""}`}>
                                <p className="mb-1 text-xs text-slate-500">
                                    {m.senderName ?? "Support Bot"} · {m.senderType} · {formatDate(m.sentAt)}
                                </p>
                                <div
                                    className={`inline-block whitespace-pre-wrap rounded-2xl px-4 py-2.5 text-left text-sm shadow-sm ${mine
                                            ? "rounded-br-sm bg-indigo-600 text-white"
                                            : isBot
                                                ? "rounded-bl-sm border border-violet-200 bg-violet-50 text-violet-900"
                                                : "rounded-bl-sm border border-slate-200 bg-white"
                                        }`}
                                >
                                    {m.content}
                                </div>
                            </div>
                        </div>
                    );
                })}

                {!isStaff &&
                    ticket.botActive &&
                    ticket.status !== "Closed" &&
                    ticket.messages[ticket.messages.length - 1]?.senderType === "Bot" && (
                        <div className="flex flex-wrap items-center justify-center gap-2 rounded-xl border border-violet-200 bg-violet-50 p-3 text-sm">
                            <span className="text-violet-900">Did this answer your question?</span>
                            <button
                                onClick={() => botAction("resolve")}
                                disabled={botBusy}
                                className="inline-flex items-center gap-1.5 rounded-lg bg-emerald-600 px-3 py-1.5 font-medium text-white hover:bg-emerald-700 disabled:opacity-60"
                            >
                                <CheckCircle2 className="h-4 w-4" /> Yes, it's solved
                            </button>
                            <button
                                onClick={() => botAction("escalate")}
                                disabled={botBusy}
                                className="inline-flex items-center gap-1.5 rounded-lg border border-slate-300 bg-white px-3 py-1.5 font-medium hover:bg-slate-50 disabled:opacity-60"
                            >
                                <UserRound className="h-4 w-4" /> Talk to a human
                            </button>
                        </div>
                    )}

                {typingName && (
                    <p className="animate-pulse text-sm text-slate-500">{typingName} is typing...</p>
                )}
                <div ref={bottomRef} />
            </div>

            {error && <p className="mt-3 rounded-lg bg-red-50 px-3 py-2 text-sm text-red-700">{error}</p>}

            {ticket.status !== "Closed" ? (
                <form onSubmit={sendReply} className="sticky bottom-4 mt-6 flex gap-2 rounded-2xl border border-slate-200 bg-white p-2 shadow-md">
                    <input
                        value={reply}
                        onChange={(e) => handleReplyChange(e.target.value)}
                        placeholder="Type your reply..."
                        className={`${inputClass} border-0 focus:ring-0`}
                    />
                    <Button type="submit" loading={sending} disabled={!reply.trim()}>
                        <Send className="h-4 w-4" /> Send
                    </Button>
                </form>
            ) : (
                <p className="mt-6 rounded-lg bg-slate-100 px-4 py-3 text-center text-sm text-slate-600">
                    This ticket is closed.
                </p>
            )}
        </div>
    );
}

export default function TicketDetailPage() {
    return (
        <Suspense fallback={<div className="h-40 animate-pulse rounded-xl bg-slate-200/70" />}>
            <TicketDetailContent />
        </Suspense>
    );
}