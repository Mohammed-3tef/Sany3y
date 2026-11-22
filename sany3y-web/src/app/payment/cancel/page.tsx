"use client";

import React, { Suspense, useEffect } from "react";
import Link from "next/link";
import { useSearchParams } from "next/navigation";
import { apiFetch } from "@/lib/api";

function PaymentCancelContent() {
  const searchParams = useSearchParams();
  const taskId = Number(searchParams.get("taskId") || 0);

  useEffect(() => {
    if (taskId > 0) {
      apiFetch(`/api/TechnicianSchedule/CancelBooking/${taskId}`, {
        method: "POST",
      }).catch(() => {});
    }
  }, [taskId]);

  return (
    <div className="container py-5 mt-5 text-center" style={{ maxWidth: 640 }}>
      <div className="card shadow-lg p-5 border-0 rounded-4 bg-white">
        <div className="mb-4">
          <i className="fas fa-times-circle text-danger fa-5x"></i>
        </div>
        <h2 className="fw-bold text-danger mb-3">تم إلغاء الدفع</h2>
        <p className="lead text-muted mb-4">
          لم يتم خصم أي مبلغ. يمكنك المحاولة مرة أخرى أو اختيار الدفع كاش.
        </p>

        <Link
          href="/dashboard/customer"
          className="btn btn-outline-secondary btn-lg rounded-pill px-5"
        >
          <i className="fas fa-arrow-right ms-2"></i> العودة للوحة التحكم
        </Link>
      </div>
    </div>
  );
}

export default function PaymentCancelPage() {
  return (
    <Suspense fallback={<div className="container py-5 mt-5 text-center">جاري التحميل...</div>}>
      <PaymentCancelContent />
    </Suspense>
  );
}
