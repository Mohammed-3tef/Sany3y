"use client";

import React, { Suspense, useEffect, useState } from "react";
import Link from "next/link";
import { useSearchParams } from "next/navigation";
import { apiFetch } from "@/lib/api";

function PaymentSuccessContent() {
  const searchParams = useSearchParams();
  const sessionId = searchParams.get("session_id") || "";

  const [status, setStatus] = useState<"loading" | "Paid" | "Pending">("loading");
  const [taskId, setTaskId] = useState<string>("");

  useEffect(() => {
    if (!sessionId) {
      setStatus("Pending");
      return;
    }

    apiFetch<{ status: string; taskId?: string }>(
      `/api/Payment/verify-payment/${encodeURIComponent(sessionId)}`
    )
      .then((res) => {
        if (res.status === "Paid" || res.status === "paid") {
          setStatus("Paid");
          setTaskId(res.taskId || "");
        } else {
          setStatus("Pending");
        }
      })
      .catch(() => setStatus("Pending"));
  }, [sessionId]);

  return (
    <div className="container py-5 mt-5 text-center" style={{ maxWidth: 640 }}>
      <div className="card shadow-lg p-5 border-0 rounded-4 bg-white">
        {status === "Paid" ? (
          <>
            <div className="mb-4">
              <i
                className="fas fa-check-circle text-success"
                style={{ fontSize: "5rem" }}
              ></i>
            </div>
            <h2 className="text-success fw-bold mb-3">تم الدفع بنجاح!</h2>
            <p className="lead text-muted">
              شكراً لك، تم تأكيد عملية الدفع الخاصة بك.
            </p>
            {taskId && (
              <p className="text-muted">
                رقم العملية: <strong>#{taskId}</strong>
              </p>
            )}
            <div className="mt-4">
              <Link
                href="/dashboard/customer"
                className="btn btn-primary btn-lg px-5 rounded-pill"
              >
                <i className="fas fa-home ms-2"></i> العودة للوحة التحكم
              </Link>
            </div>
          </>
        ) : (
          <>
            <div className="mb-4">
              <i
                className="fas fa-exclamation-circle text-warning"
                style={{ fontSize: "5rem" }}
              ></i>
            </div>
            <h2 className="text-warning fw-bold mb-3">
              جاري التحقق من الدفع...
            </h2>
            <p className="lead text-muted">
              يرجى الانتظار قليلاً حتى يتم تحديث حالة الطلب.
            </p>
            <div className="mt-4">
              <Link
                href="/dashboard/customer"
                className="btn btn-outline-primary btn-lg px-5 rounded-pill"
              >
                <i className="fas fa-sync ms-2"></i> العودة للوحة التحكم
              </Link>
            </div>
          </>
        )}
      </div>
    </div>
  );
}

export default function PaymentSuccessPage() {
  return (
    <Suspense fallback={<div className="container py-5 mt-5 text-center">جاري التحقق...</div>}>
      <PaymentSuccessContent />
    </Suspense>
  );
}
