"use client";

import { useEffect, useRef, useState } from "react";
import { HubConnection } from "@microsoft/signalr";
import { buildConnection } from "./signalr";

// eslint-disable-next-line @typescript-eslint/no-explicit-any
type Handlers = Record<string, (...args: any[]) => void>;

export function useChatHub(
  enabled: boolean,
  handlers: Handlers,
  onReady?: (conn: HubConnection) => void
) {
  // Refs keep the latest handlers without reconnecting on every render
  const handlersRef = useRef(handlers);
  const onReadyRef = useRef(onReady);
  handlersRef.current = handlers;
  onReadyRef.current = onReady;

  const [connection, setConnection] = useState<HubConnection | null>(null);
  const [connected, setConnected] = useState(false);

  useEffect(() => {
    if (!enabled) return;

    const conn = buildConnection();
    let cancelled = false;

    for (const name of Object.keys(handlersRef.current)) {
      conn.on(name, (...args) => handlersRef.current[name]?.(...args));
    }

    conn.onreconnecting(() => setConnected(false));
    conn.onreconnected(() => {
      setConnected(true);
      onReadyRef.current?.(conn); // groups are lost on reconnect, so rejoin
    });
    conn.onclose(() => setConnected(false));

    conn
      .start()
      .then(() => {
        if (cancelled) {
          conn.stop();
          return;
        }
        setConnection(conn);
        setConnected(true);
        onReadyRef.current?.(conn);
      })
      .catch((err) => {
        if (!cancelled) console.error("SignalR connection failed:", err);
      });

    return () => {
      cancelled = true;
      setConnection(null);
      setConnected(false);
      conn.stop();
    };
  }, [enabled]);

  return { connection, connected };
}