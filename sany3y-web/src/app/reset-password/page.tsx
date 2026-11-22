"use client";

import React, { Suspense, useState } from "react";
import { useRouter, useSearchParams } from "next/navigation";
import { apiFetch } from "@/lib/api";
import { toast } from "sonner";

function ResetPasswordContent() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const initialEmail = searchParams.get("email") || "";
  const token = searchParams.get("token") || "";

  const [email, setEmail] = useState(initialEmail);
  const [newPassword, setNewPassword] = useState("");
  const [confirmNewPassword, setConfirmNewPassword] = useState("");
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (newPassword !== confirmNewPassword) {
      toast.error("كلمة المرور وتأكيد كلمة المرور غير متطابقين.");
      return;
    }

    setLoading(true);
    try {
      await apiFetch("/api/Auth/reset-password", {
        method: "POST",
        body: JSON.stringify({
          email,
          token: token || undefined,
          newPassword,
          confirmNewPassword,
        }),
      });
      toast.success("تم تغيير كلمة المرور بنجاح!");
      router.push("/login");
    } catch (err: any) {
      toast.error(err.message || "فشل تغيير كلمة المرور.");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="container py-5 mt-5" style={{ maxWidth: 480 }}>
      <div className="card shadow-sm border-0 rounded-4 p-4 p-md-5 bg-white">
        <h3 className="fw-bold text-center mb-4" style={{ color: "var(--primary-green)" }}>
          تغيير كلمة المرور
        </h3>

        <form onSubmit={handleSubmit}>
          <div className="mb-3">
            <label className="form-label">البريد الإلكتروني</label>
            <input
              type="email"
              className="form-control bg-light"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              readOnly={Boolean(initialEmail)}
              required
            />
          </div>

          <div className="mb-3">
            <label className="form-label">كلمة المرور الجديدة</label>
            <input
              type="password"
              className="form-control"
              value={newPassword}
              onChange={(e) => setNewPassword(e.target.value)}
              required
            />
          </div>

          <div className="mb-4">
            <label className="form-label">تأكيد كلمة المرور</label>
            <input
              type="password"
              className="form-control"
              value={confirmNewPassword}
              onChange={(e) => setConfirmNewPassword(e.target.value)}
              required
            />
          </div>

          <button
            type="submit"
            disabled={loading}
            className="btn btn-success w-100 py-2 fw-bold"
          >
            {loading ? "جاري الحفظ..." : "حفظ"}
          </button>
        </form>
      </div>
    </div>
  );
}

export default function ResetPasswordPage() {
  return (
    <Suspense fallback={<div className="container py-5 mt-5 text-center">جاري التحميل...</div>}>
      <ResetPasswordContent />
    </Suspense>
  );
}
