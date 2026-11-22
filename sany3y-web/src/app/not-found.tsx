import React from "react";
import Link from "next/link";

export default function NotFound() {
  return (
    <div className="container py-5 mt-5 text-center" style={{ minHeight: "70vh" }}>
      <div className="py-5">
        <h1 className="display-1 fw-bold text-success">404</h1>
        <h2 className="fw-bold mb-3">الصفحة غير موجودة</h2>
        <p className="lead text-muted mb-4">
          عذراً، الصفحة التي تبحث عنها غير موجودة أو تم نقلها.
        </p>
        <Link href="/" className="btn btn-primary btn-lg rounded-pill px-5">
          <i className="fas fa-home ms-2"></i> العودة للرئيسية
        </Link>
      </div>
    </div>
  );
}
