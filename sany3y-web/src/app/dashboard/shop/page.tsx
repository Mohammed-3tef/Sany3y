"use client";

import React, { useEffect, useState } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { apiFetch } from "@/lib/api";
import { useAuth } from "@/context/AuthContext";
import { ChatPartner, WorkerDashboardDto } from "@/types";
import { toast } from "sonner";

export default function ShopDashboardPage() {
  const router = useRouter();
  const { user, isAuthenticated, isLoading } = useAuth();
  const [dashboard, setDashboard] = useState<WorkerDashboardDto | null>(null);
  const [chatPartners, setChatPartners] = useState<ChatPartner[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    if (!isLoading && !isAuthenticated) {
      router.push("/login");
      return;
    }
    if (!isAuthenticated || !user) return;

    Promise.all([
      apiFetch<WorkerDashboardDto>(`/api/Dashboard/worker/${user.id}`),
      apiFetch<ChatPartner[]>(`/api/Message/GetChatPartners/${user.id}`).catch(
        () => []
      ),
    ])
      .then(([dash, partners]) => {
        setDashboard(dash);
        setChatPartners(partners || []);
      })
      .catch(() => toast.error("فشل تحميل بيانات المحل."))
      .finally(() => setLoading(false));
  }, [isLoading, isAuthenticated, user?.id]);

  if (loading) {
    return (
      <div className="container py-5 mt-5 text-center">
        <div className="spinner-border text-success" style={{ width: "3rem", height: "3rem" }} />
        <p className="mt-3 text-secondary">جاري تحميل البيانات...</p>
      </div>
    );
  }

  const displayName =
    dashboard?.profile?.shopName ||
    `${dashboard?.profile?.firstName || ""} ${dashboard?.profile?.lastName || ""}`;

  return (
    <div className="container pt-5 pb-5 mt-5">
      <div className="row mb-4">
        <div className="col-12">
          <h2 className="fw-bold mb-0" style={{ color: "var(--primary-green)" }}>
            <i className="fas fa-store ms-2"></i>لوحة تحكم المحل
          </h2>
          <p className="text-secondary">إدارة بيانات المحل ورسائل العملاء</p>
        </div>
      </div>

      {dashboard && (
        <>
          <div className="card shadow-sm mb-4 border-0 rounded-3">
            <div className="card-header d-flex justify-content-between align-items-center">
              <span className="fw-bold">
                <i className="fas fa-store-alt ms-2"></i>بيانات المحل
              </span>
              <Link
                href="/profile"
                className="btn btn-light btn-sm rounded-pill"
              >
                <i className="fas fa-edit ms-1"></i>تعديل
              </Link>
            </div>
            <div className="card-body">
              <div className="row align-items-center">
                <div className="col-md-3 text-center mb-3 mb-md-0">
                  <img
                    src={
                      dashboard.profile.profilePictureUrl ||
                      "https://placehold.co/150x150?text=Shop"
                    }
                    alt="Shop Logo"
                    className="img-thumbnail rounded-circle shadow-sm"
                    style={{ width: 150, height: 150, objectFit: "cover" }}
                    onError={(e) => {
                      (e.target as HTMLImageElement).src =
                        "https://placehold.co/150x150?text=Shop";
                    }}
                  />
                </div>
                <div className="col-md-9">
                  <h4 className="fw-bold mb-3 text-dark">{displayName}</h4>
                  <div className="row g-3">
                    <div className="col-md-6">
                      <p className="mb-2">
                        <i className="fas fa-user ms-2 text-success"></i>
                        <span className="text-secondary">
                          {dashboard.profile.firstName}{" "}
                          {dashboard.profile.lastName}
                        </span>
                      </p>
                      <p className="mb-0">
                        <i className="fas fa-envelope ms-2 text-success"></i>
                        <span className="text-secondary">
                          {dashboard.profile.email}
                        </span>
                      </p>
                    </div>
                    <div className="col-md-6">
                      <p className="mb-2">
                        <i className="fas fa-phone ms-2 text-success"></i>
                        <span className="text-secondary">
                          {dashboard.profile.phone}
                        </span>
                      </p>
                    </div>
                    <div className="col-md-12">
                      <p className="mb-2">
                        <i className="fas fa-map-marker-alt text-danger ms-2"></i>
                        <span className="text-secondary">
                          {dashboard.profile.location}
                        </span>
                      </p>
                    </div>
                  </div>
                  <p className="text-muted mt-3 mb-0 fst-italic small">
                    {dashboard.profile.bio || `أهلاً بك في ${displayName}`}
                  </p>
                </div>
              </div>
            </div>
          </div>

          <div className="card shadow-sm mb-4 border-0 rounded-3">
            <div className="card-header">
              <h5 className="mb-0 fw-bold">
                <i className="fas fa-comments ms-2"></i>رسائل العملاء
              </h5>
            </div>
            <div className="card-body">
              {chatPartners.length > 0 ? (
                <div className="row g-3">
                  {chatPartners.map((partner) => (
                    <div key={partner.id} className="col-md-4">
                      <Link
                        href={`/chat/${partner.id}`}
                        className="d-flex align-items-center p-2 border rounded-3 text-decoration-none text-dark bg-light"
                      >
                        <img
                          src={
                            partner.profilePicture?.path ||
                            "https://placehold.co/60x60?text=Profile"
                          }
                          alt={partner.userName}
                          className="rounded-circle ms-3"
                          style={{ width: 65, height: 65, objectFit: "cover" }}
                        />
                        <div>
                          <h6 className="mb-0 fw-bold">{partner.userName}</h6>
                          <small className="text-muted">انقر للمحادثة</small>
                        </div>
                      </Link>
                    </div>
                  ))}
                </div>
              ) : (
                <div className="empty-state text-center py-4">
                  <i className="fas fa-comment-slash fa-2x text-muted opacity-50 mb-3"></i>
                  <p className="mb-0 text-muted">لا توجد محادثات سابقة</p>
                </div>
              )}
            </div>
          </div>
        </>
      )}
    </div>
  );
}
