export function parseUtc(value: string): Date {
  return new Date(/Z|[+-]\d\d:?\d\d$/.test(value) ? value : value + "Z");
}

export function formatDate(value: string): string {
  return parseUtc(value).toLocaleString(undefined, {
    day: "numeric",
    month: "short",
    hour: "numeric",
    minute: "2-digit",
  });
}

export function timeAgo(value: string): string {
  const seconds = Math.floor((Date.now() - parseUtc(value).getTime()) / 1000);
  if (seconds < 60) return "just now";
  const minutes = Math.floor(seconds / 60);
  if (minutes < 60) return `${minutes}m ago`;
  const hours = Math.floor(minutes / 60);
  if (hours < 24) return `${hours}h ago`;
  const days = Math.floor(hours / 24);
  if (days < 7) return `${days}d ago`;
  return formatDate(value);
}