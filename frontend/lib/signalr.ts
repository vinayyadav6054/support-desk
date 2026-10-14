import { HubConnection, HubConnectionBuilder, LogLevel } from "@microsoft/signalr";

export function buildConnection(): HubConnection {
  return new HubConnectionBuilder()
    .withUrl(`${process.env.NEXT_PUBLIC_API_URL}/hubs/chat`, {
      accessTokenFactory: () => localStorage.getItem("token") ?? "",
    })
    .withAutomaticReconnect()
    .configureLogging(LogLevel.Warning)
    .build();
}