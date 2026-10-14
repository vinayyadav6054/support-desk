const styles: Record<string, string> = {
  Open: "bg-blue-50 text-blue-700 ring-blue-200",
  InProgress: "bg-amber-50 text-amber-700 ring-amber-200",
  Resolved: "bg-emerald-50 text-emerald-700 ring-emerald-200",
  Closed: "bg-slate-100 text-slate-600 ring-slate-200",
  Low: "bg-slate-50 text-slate-600 ring-slate-200",
  Medium: "bg-sky-50 text-sky-700 ring-sky-200",
  High: "bg-orange-50 text-orange-700 ring-orange-200",
  Urgent: "bg-red-50 text-red-700 ring-red-200",
  Bot: "bg-violet-50 text-violet-700 ring-violet-200",
  Escalated: "bg-rose-50 text-rose-700 ring-rose-200",
};

const labels: Record<string, string> = { InProgress: "In progress" };

export default function StatusBadge({ value }: { value: string }) {
  return (
    <span
      className={`inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium ring-1 ring-inset ${
        styles[value] ?? "bg-slate-100 text-slate-600 ring-slate-200"
      }`}
    >
      {labels[value] ?? value}
    </span>
  );
}