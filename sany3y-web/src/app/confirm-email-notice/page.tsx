"use client";

import React, { Suspense } from "react";
import Link from "next/link";
import { useSearchParams } from "next/navigation";

function ConfirmNoticeContent() {
  const searchParams = useSearchParams();
  const email = searchParams.get("email");

  return (
    <div className="container py-5 mt-5 text-center" style={{ maxWidth: 640 }}>
      <div className="alert alert-info shadow-sm p-5 rounded-4 bg-white border">
        <i className="fas fa-envelope-open-text fa-4x text-success mb-3"></i>
        <h3 className="fw-bold mb-3" style={{ color: "var(--primary-green)" }}>
          تنبيه تأكيد البريد الإلكتروني
        </h3>
        <p className="lead mb-3 text-secondary">
          تم إرسال رابط التفعيل إلى بريدك الإلكتروني{" "}
          {email && <strong>({email})</strong>}.
        </p>
        <p className="text-muted mb-4">
          من فضلك راجع بريدك الإلكتروني واضغط على رابط التأكيد المرسل إليك لتفعيل
          حسابك.
        </p>
        <div className="d-flex justify-content-center gap-3">
          <Link href="/login" className="btn btn-primary px-4 rounded-pill">
            الذهاب لتسجيل الدخول
          </Link>
          <Link
            href="/resend-confirmation"
            className="btn btn-outline-secondary px-4 rounded-pill"
          >
            إعادة إرسال الرابط
          </Link>
        </div>
      </div>
    </div>
  );
}

export default function ConfirmEmailNoticePage() {
  return (
    <Suspense fallback={<div className="container py-5 mt-5 text-center">جاري التحميل...</div>}>
      <ConfirmNoticeContent />
    </Suspense>
  );
}
