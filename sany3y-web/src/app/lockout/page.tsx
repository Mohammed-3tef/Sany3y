import React from "react";
import Link from "next/link";

export default function LockoutPage() {
  return (
    <div className="container py-5 mt-5 text-center" style={{ maxWidth: 600 }}>
      <div className="card shadow-sm border-0 rounded-4 p-5 bg-white">
        <i className="fas fa-user-lock text-danger fa-4x mb-3"></i>
        <h2 className="fw-bold text-danger mb-3">تم إيقاف الحساب مؤقتاً</h2>
        <p className="text-muted mb-4">
          تم قفل هذا الحساب مؤقتاً بسبب محاولات تسجيل دخول فاشلة متكررة. يرجى
          المحاولة مرة أخرى بعد 15 دقيقة.
        </p>
        <Link href="/login" className="btn btn-outline-primary rounded-pill px-5">
          العودة لتسجيل الدخول
        </Link>
      </div>
    </div>
  );
}
