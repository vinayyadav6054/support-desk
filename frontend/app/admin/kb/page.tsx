"use client";

import { useCallback, useEffect, useState } from "react";
import { FlaskConical, Pencil, Trash2 } from "lucide-react";
import { api } from "@/lib/api";
import { useRequireAuth } from "@/lib/auth";
import { Article, TestBotResult } from "@/lib/types";
import { Button, inputClass } from "@/components/ui";

const emptyForm = { title: "", keywords: "", answer: "", isActive: true };

export default function KnowledgeBasePage() {
  const { user, loading } = useRequireAuth();
  const [articles, setArticles] = useState<Article[]>([]);
  const [form, setForm] = useState(emptyForm);
  const [editingId, setEditingId] = useState<number | null>(null);
  const [error, setError] = useState("");
  const [saving, setSaving] = useState(false);
  const [testMessage, setTestMessage] = useState("");
  const [testResult, setTestResult] = useState<TestBotResult | null>(null);
  const [testing, setTesting] = useState(false);

  const isAdmin = user?.role === "Admin";

  const load = useCallback(
    () =>
      api<Article[]>("/api/kb")
        .then(setArticles)
        .catch((e) => setError(e.message)),
    []
  );

  useEffect(() => {
    if (isAdmin) load();
  }, [isAdmin, load]);

  async function save(e: React.FormEvent) {
    e.preventDefault();
    setSaving(true);
    setError("");
    try {
      if (editingId) {
        await api<Article>(`/api/kb/${editingId}`, { method: "PUT", body: JSON.stringify(form) });
      } else {
        await api<Article>("/api/kb", { method: "POST", body: JSON.stringify(form) });
      }
      setForm(emptyForm);
      setEditingId(null);
      await load();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Could not save article");
    } finally {
      setSaving(false);
    }
  }

  function startEdit(a: Article) {
    setEditingId(a.id);
    setForm({ title: a.title, keywords: a.keywords, answer: a.answer, isActive: a.isActive });
    window.scrollTo({ top: 0, behavior: "smooth" });
  }

  async function remove(a: Article) {
    if (!confirm(`Delete "${a.title}"?`)) return;
    try {
      await api<{ deleted: boolean }>(`/api/kb/${a.id}`, { method: "DELETE" });
      await load();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Could not delete article");
    }
  }

  async function runTest(e: React.FormEvent) {
    e.preventDefault();
    if (!testMessage.trim()) return;
    setTesting(true);
    try {
      setTestResult(
        await api<TestBotResult>("/api/kb/test", {
          method: "POST",
          body: JSON.stringify({ message: testMessage }),
        })
      );
    } catch (err) {
      setError(err instanceof Error ? err.message : "Test failed");
    } finally {
      setTesting(false);
    }
  }

  if (loading || !user) return null;
  if (!isAdmin) {
    return (
      <div className="rounded-xl border border-slate-200 bg-white py-14 text-center">
        <p className="font-medium">Only admins can manage the knowledge base.</p>
      </div>
    );
  }

  return (
    <div className="space-y-8">
      <div>
        <h1 className="text-2xl font-semibold">Knowledge base</h1>
        <p className="text-sm text-slate-500">
          The bot answers customers using these articles. Add keywords customers are likely to use.
        </p>
      </div>

      {error && <p className="rounded-lg bg-red-50 px-3 py-2 text-sm text-red-700">{error}</p>}

      <form onSubmit={save} className="space-y-3 rounded-2xl border border-slate-200 bg-white p-5 shadow-sm">
        <h2 className="font-semibold">{editingId ? "Edit article" : "Add article"}</h2>
        <input
          required maxLength={200} placeholder="Title" value={form.title}
          onChange={(e) => setForm({ ...form, title: e.target.value })} className={inputClass}
        />
        <input
          required maxLength={500} placeholder="Keywords, comma separated (e.g. refund, money back, charged twice)"
          value={form.keywords}
          onChange={(e) => setForm({ ...form, keywords: e.target.value })} className={inputClass}
        />
        <textarea
          required rows={4} maxLength={4000} placeholder="Answer the bot will send" value={form.answer}
          onChange={(e) => setForm({ ...form, answer: e.target.value })} className={inputClass}
        />
        <label className="flex items-center gap-2 text-sm">
          <input
            type="checkbox" checked={form.isActive}
            onChange={(e) => setForm({ ...form, isActive: e.target.checked })}
          />
          Active (the bot can use this article)
        </label>
        <div className="flex gap-2">
          <Button type="submit" loading={saving}>
            {editingId ? "Save changes" : "Add article"}
          </Button>
          {editingId && (
            <button
              type="button"
              onClick={() => { setEditingId(null); setForm(emptyForm); }}
              className="rounded-lg border border-slate-300 px-4 py-2.5 text-sm hover:bg-slate-50"
            >
              Cancel
            </button>
          )}
        </div>
      </form>

      <form onSubmit={runTest} className="space-y-3 rounded-2xl border border-violet-200 bg-violet-50 p-5">
        <h2 className="flex items-center gap-2 font-semibold text-violet-900">
          <FlaskConical className="h-4 w-4" /> Test the bot
        </h2>
        <div className="flex gap-2">
          <input
            placeholder="Type a customer message..." value={testMessage}
            onChange={(e) => setTestMessage(e.target.value)} className={inputClass}
          />
          <Button type="submit" loading={testing}>Test</Button>
        </div>
        {testResult && (
          <div className="rounded-lg bg-white p-3 text-sm">
            {testResult.matched ? (
              <>
                <p className="font-medium text-emerald-700">
                  Would answer with "{testResult.articleTitle}" (score {testResult.score})
                </p>
                <p className="mt-1 whitespace-pre-wrap text-slate-600">{testResult.reply}</p>
              </>
            ) : (
              <p className="font-medium text-amber-700">
                Would escalate to a human.
                {testResult.articleTitle &&
                  ` Closest article: "${testResult.articleTitle}" (score ${testResult.score}, needs 0.4).`}
              </p>
            )}
          </div>
        )}
      </form>

      <ul className="space-y-3">
        {articles.map((a) => (
          <li key={a.id} className="rounded-xl border border-slate-200 bg-white p-4 shadow-sm">
            <div className="flex items-start justify-between gap-3">
              <div>
                <p className="font-medium">
                  {a.title}
                  {!a.isActive && (
                    <span className="ml-2 rounded-full bg-slate-100 px-2 py-0.5 text-xs text-slate-500">
                      Inactive
                    </span>
                  )}
                </p>
                <p className="mt-0.5 text-xs text-slate-500">{a.keywords}</p>
                <p className="mt-2 line-clamp-3 whitespace-pre-wrap text-sm text-slate-600">{a.answer}</p>
              </div>
              <div className="flex shrink-0 gap-1">
                <button onClick={() => startEdit(a)} title="Edit" className="rounded-lg p-2 text-slate-500 hover:bg-slate-100">
                  <Pencil className="h-4 w-4" />
                </button>
                <button onClick={() => remove(a)} title="Delete" className="rounded-lg p-2 text-red-500 hover:bg-red-50">
                  <Trash2 className="h-4 w-4" />
                </button>
              </div>
            </div>
          </li>
        ))}
      </ul>
    </div>
  );
}