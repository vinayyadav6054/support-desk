export type Role = "Customer" | "Agent" | "Admin";

export interface AuthUser {
  userId: number;
  fullName: string;
  email: string;
  role: Role;
}

export interface TicketSummary {
  id: number;
  subject: string;
  status: string;
  priority: string;
  customerName: string;
  assignedAgentName: string | null;
  createdAt: string;
  updatedAt: string | null;
  botActive: boolean;
  escalatedFromBot: boolean;
}

export interface Message {
  id: number;
  content: string;
  senderType: string;
  senderName: string | null;
  sentAt: string;
}

export interface TicketDetail extends TicketSummary {
  description: string | null;
  customerId: number;
  assignedAgentId: number | null;
  messages: Message[];
}

export const STATUSES = ["Open", "InProgress", "Resolved", "Closed"] as const;
export const PRIORITIES = ["Low", "Medium", "High", "Urgent"] as const;

export interface Article {
  id: number;
  title: string;
  keywords: string;
  answer: string;
  isActive: boolean;
}

export interface TestBotResult {
  matched: boolean;
  reply: string;
  articleTitle: string | null;
  score: number;
}

export interface ManagedUser {
  id: number;
  fullName: string;
  email: string;
  role: Role;
  createdAt: string;
}