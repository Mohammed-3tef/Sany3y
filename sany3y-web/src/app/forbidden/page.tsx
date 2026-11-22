import React from "react";
import Link from "next/link";

export default function ForbiddenPage() {
  return (
    <div className="container py-5 mt-5 text-center" style={{ minHeight: "70vh" }}>
      <div className="py-5">
        <i className="fas fa-shield-alt text-danger fa-5x mb-4"></i>
        <h1 className="fw-bold text-danger mb-3">غير مصرح بالدخول (403)</h1>
        <p className="lead text-muted mb-4">
          ليس لديك الصلاحيات الكافية للوصول إلى هذه الصفحة.
        </p>
        <Link href="/" className="btn btn-primary btn-lg rounded-pill px-5">
          <i className="fas fa-home ms-2"></i> العودة للرئيسية
        </Link>
      </div>
    </div>
  );
}
