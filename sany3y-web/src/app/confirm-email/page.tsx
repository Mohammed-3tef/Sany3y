"use client";

import React, { Suspense, useEffect, useState } from "react";
import Link from "next/link";
import { useSearchParams } from "next/navigation";
import { apiFetch } from "@/lib/api";

function ConfirmEmailContent() {
  const searchParams = useSearchParams();
  const userId = searchParams.get("userId") || "";
  const token = searchParams.get("token") || "";

  const [state, setState] = useState<"loading" | "Success" | "Failed">("loading");
  const [message, setMessage] = useState("جاري تأكيد البريد الإلكتروني...");

  useEffect(() => {
    if (!userId || !token) {
      setState("Failed");
      setMessage("رابط تأكيد البريد الإلكتروني غير مكتمل.");
      return;
    }

    apiFetch<{ state: string; message: string }>(
      `/api/Auth/confirm-email?userId=${encodeURIComponent(
        userId
      )}&token=${encodeURIComponent(token)}`
    )
      .then((res) => {
        setState("Success");
        setMessage(
          res.message || "تم تأكيد البريد الإلكتروني بنجاح! يمكنك الآن تسجيل الدخول."
        );
      })
      .catch((err) => {
        setState("Failed");
        setMessage(
          err.message ||
            "فشل تأكيد البريد الإلكتروني. قد يكون الرابط غير صالح أو منتهي الصلاحية."
        );
      });
  }, [userId, token]);

  return (
    <div className="container py-5 mt-5 text-center" style={{ maxWidth: 640 }}>
      {state === "loading" && (
        <div className="p-5 bg-white rounded-4 shadow-sm">
          <div className="spinner-border text-success mb-3" />
          <p className="mb-0">{message}</p>
        </div>
      )}

      {state === "Success" && (
        <div className="alert alert-success shadow-sm p-5 rounded-4">
          <h3 className="fw-bold mb-3">✅ تم تأكيد البريد بنجاح</h3>
          <p className="mb-4">{message}</p>
          <Link href="/login" className="btn btn-success px-5 rounded-pill">
            تسجيل الدخول الآن
          </Link>
        </div>
      )}

      {state === "Failed" && (
        <div className="alert alert-danger shadow-sm p-5 rounded-4">
          <h3 className="fw-bold mb-3">❌ فشل تأكيد البريد الإلكتروني</h3>
          <p className="mb-4">{message}</p>
          <Link
            href="/resend-confirmation"
            className="btn btn-outline-danger px-4 rounded-pill"
          >
            إعادة إرسال بريد التفعيل
          </Link>
        </div>
      )}
    </div>
  );
}

export default function ConfirmEmailPage() {
  return (
    <Suspense fallback={<div className="container py-5 mt-5 text-center">جاري التحميل...</div>}>
      <ConfirmEmailContent />
    </Suspense>
  );
}
