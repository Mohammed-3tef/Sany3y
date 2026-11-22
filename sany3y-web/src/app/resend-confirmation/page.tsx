"use client";

import React, { useState } from "react";
import { useRouter } from "next/navigation";
import { apiFetch } from "@/lib/api";
import { toast } from "sonner";

export default function ResendConfirmationPage() {
  const router = useRouter();
  const [email, setEmail] = useState("");
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    try {
      const res = await apiFetch<any>("/api/Auth/resend-confirmation", {
        method: "POST",
        body: JSON.stringify({ email }),
      });
      toast.success(res.message || "تم إرسال رابط تأكيد جديد.");
      if (res.alreadyConfirmed) {
        router.push("/login");
      } else {
        router.push(`/confirm-email-notice?email=${encodeURIComponent(email)}`);
      }
    } catch (err: any) {
      toast.error(err.message || "فشل إرسال البريد.");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="container py-5 mt-5" style={{ maxWidth: 480 }}>
      <div className="card shadow-sm border-0 rounded-4 p-4 p-md-5 bg-white">
        <h3 className="fw-bold text-center mb-3" style={{ color: "var(--primary-green)" }}>
          إعادة إرسال بريد التفعيل
        </h3>
        <p className="text-muted text-center small mb-4">
          أدخل بريدك الإلكتروني لإرسال رابط تفعيل جديد
        </p>

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
            className="btn btn-primary w-100 py-2"
          >
            {loading ? "جاري الإرسال..." : "إرسال رابط التفعيل"}
          </button>
        </form>
      </div>
    </div>
  );
}
