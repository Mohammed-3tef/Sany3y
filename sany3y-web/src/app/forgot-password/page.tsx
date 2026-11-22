"use client";

import React, { useState } from "react";
import Link from "next/link";
import { apiFetch } from "@/lib/api";
import { toast } from "sonner";

export default function ForgotPasswordPage() {
  const [email, setEmail] = useState("");
  const [sent, setSent] = useState(false);
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    try {
      const res = await apiFetch<any>("/api/Auth/forgot-password", {
        method: "POST",
        body: JSON.stringify({ email }),
      });
      toast.success(
        res.message || "تم إرسال رابط تغيير كلمة المرور إلى بريدك الإلكتروني."
      );
      setSent(true);
    } catch (err: any) {
      toast.error(err.message || "لم يتم العثور على المستخدم.");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="container py-5 mt-5" style={{ maxWidth: 480 }}>
      <div className="card shadow-sm border-0 rounded-4 p-4 p-md-5 bg-white">
        <h3 className="fw-bold text-center mb-3" style={{ color: "var(--primary-green)" }}>
          تأكيد البريد الإلكتروني
        </h3>
        <p className="text-muted text-center small mb-4">
          أدخل بريدك الإلكتروني لإرسال رابط تغيير كلمة المرور
        </p>

        {sent ? (
          <div className="text-center">
            <div className="alert alert-success">
              تم إرسال رابط تغيير كلمة المرور إلى <strong>{email}</strong>.
            </div>
            <Link
              href={`/reset-password?email=${encodeURIComponent(email)}`}
              className="btn btn-outline-success btn-sm mt-2"
            >
              تغيير كلمة المرور الآن
            </Link>
          </div>
        ) : (
          <form onSubmit={handleSubmit}>
            <div className="mb-3">
              <label className="form-label">البريد الإلكتروني</label>
              <input
                type="email"
                className="form-control"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                required
              />
            </div>
            <button
              type="submit"
              disabled={loading}
              className="btn btn-success w-100 py-2 fw-bold"
            >
              {loading ? "جاري الإرسال..." : "تأكيد"}
            </button>
          </form>
        )}
      </div>
    </div>
  );
}
