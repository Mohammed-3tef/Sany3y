"use client";

import React, { useEffect, useState } from "react";
import { useParams } from "next/navigation";
import { apiFetch } from "@/lib/api";
import { TechnicianDetails } from "@/types";
import { FloatingChatWidget } from "@/components/FloatingChatWidget";

export default function ShopDetailsPage() {
  const params = useParams();
  const shopId = Number(params?.id);

  const [details, setDetails] = useState<TechnicianDetails | null>(null);
  const [loading, setLoading] = useState(true);
  const [activeTab, setActiveTab] = useState<"about" | "reviews">("about");

  useEffect(() => {
    if (!shopId) return;
    setLoading(true);
    apiFetch<TechnicianDetails>(`/api/Technician/Details/${shopId}`)
      .then((data) => setDetails(data))
      .catch(() => setDetails(null))
      .finally(() => setLoading(false));
  }, [shopId]);

  if (loading) {
    return (
      <div className="container py-5 mt-5 text-center">
        <div className="spinner-border text-success" />
        <p className="mt-3 text-muted">جاري تحميل بيانات المحل...</p>
      </div>
    );
  }

  if (!details) {
    return (
      <div className="container py-5 mt-5 text-center">
        <h3>المحل غير موجود</h3>
      </div>
    );
  }

  return (
    <div className="pt-5">
      <div className="profile-hero text-center">
        <div className="container">
          <h1 className="fw-bold mb-2">
            {details.shopName || `${details.firstName} ${details.lastName}`}
          </h1>
          <p className="opacity-75 mb-0">
            <i className="fas fa-store ms-2"></i>
            {details.categoryName || "محل عام"}
            <span className="mx-2">|</span>
            <i className="fas fa-map-marker-alt ms-2"></i>
            {details.address?.city || details.city || "القاهرة"}
          </p>
        </div>
      </div>

      <div className="container mb-5" style={{ paddingTop: "6rem" }}>
        <div className="row">
          <div className="col-12">
            <div className="profile-main-card">
              <div className="text-center mb-4" style={{ marginTop: -110 }}>
                <div className="profile-avatar-container">
                  <img
                    src={
                      details.profilePictureUrl ||
                      "https://placehold.co/200x200?text=Shop"
                    }
                    className="profile-avatar"
                    alt={details.shopName || details.firstName}
                    onError={(e) => {
                      (e.target as HTMLImageElement).src =
                        "https://placehold.co/200x200?text=Shop";
                    }}
                  />
                  <div className="verified-badge" title="تم التحقق">
                    <i className="fas fa-check"></i>
                  </div>
                </div>
              </div>

              <div className="row g-3 mb-4 justify-content-center">
                <div className="col-4 col-md-3">
                  <div className="stat-box">
                    <div className="stat-value">
                      {details.averageRating.toFixed(1)}{" "}
                      <i className="fas fa-star text-warning fs-6"></i>
                    </div>
                    <div className="stat-label">التقييم العام</div>
                  </div>
                </div>
                <div className="col-4 col-md-3">
                  <div className="stat-box">
                    <div className="stat-value fs-5">
                      {details.createdAt
                        ? new Date(details.createdAt).toLocaleDateString("ar-EG")
                        : "2025"}
                    </div>
                    <div className="stat-label">تأسس منذ</div>
                  </div>
                </div>
                <div className="col-4 col-md-3">
                  <div className="stat-box">
                    <div className="stat-value">{details.reviewCount}</div>
                    <div className="stat-label">مراجعة</div>
                  </div>
                </div>
              </div>

              <ul className="nav nav-pills mb-4 justify-content-center gap-2">
                <li className="nav-item">
                  <button
                    type="button"
                    className={`nav-link rounded-pill px-4 ${
                      activeTab === "about"
                        ? "active bg-success text-white"
                        : "text-secondary"
                    }`}
                    onClick={() => setActiveTab("about")}
                  >
                    عن المحل
                  </button>
                </li>
                <li className="nav-item">
                  <button
                    type="button"
                    className={`nav-link rounded-pill px-4 ${
                      activeTab === "reviews"
                        ? "active bg-success text-white"
                        : "text-secondary"
                    }`}
                    onClick={() => setActiveTab("reviews")}
                  >
                    التقييمات ({details.reviewCount})
                  </button>
                </li>
              </ul>

              {activeTab === "about" ? (
                <div>
                  <h5 className="fw-bold mb-3">
                    <i className="fas fa-info-circle text-success ms-2"></i>{" "}
                    نبذة عن المحل
                  </h5>
                  <p className="text-muted">
                    {details.bio ||
                      "لا توجد نبذة تعريفية متاحة حالياً لهذا المحل. ولكنه يقدم خدمات ممتازة ولديه خبرة جيدة في التعامل مع العملاء."}
                  </p>

                  <hr className="my-4" />

                  <h5 className="fw-bold mb-3">
                    <i className="fas fa-info-circle text-success ms-2"></i>{" "}
                    معلومات إضافية
                  </h5>
                  <div className="row g-3">
                    <div className="col-md-6">
                      <div className="d-flex align-items-center text-muted">
                        <i className="fas fa-map-marked-alt fa-fw ms-2 text-secondary"></i>
                        <span>
                          العنوان:{" "}
                          {details.address
                            ? `${details.address.governorate} - ${details.address.city} - ${details.address.street}`
                            : "القاهرة"}
                        </span>
                      </div>
                    </div>
                    <div className="col-md-6">
                      <div className="d-flex align-items-center text-muted">
                        <i className="fas fa-clock fa-fw ms-2 text-secondary"></i>
                        <span>مواعيد العمل: 9 ص - 10 م</span>
                      </div>
                    </div>
                  </div>
                </div>
              ) : (
                <div>
                  {details.ratings && details.ratings.length > 0 ? (
                    details.ratings.map((review) => (
                      <div
                        key={review.id}
                        className="border-bottom py-3 d-flex gap-3"
                      >
                        <div
                          className="rounded-circle bg-light d-flex align-items-center justify-content-center text-secondary fw-bold"
                          style={{ width: 48, height: 48 }}
                        >
                          <i className="fas fa-user"></i>
                        </div>
                        <div>
                          <h6 className="fw-bold mb-1">
                            {review.clientName || "عميل"}
                          </h6>
                          <div className="mb-2">
                            {[1, 2, 3, 4, 5].map((i) => (
                              <i
                                key={i}
                                className={`fas fa-star small ${
                                  i <= review.score
                                    ? "text-warning"
                                    : "text-muted opacity-25"
                                }`}
                              />
                            ))}
                            <span className="text-muted small me-2">
                              {new Date(review.createdAt).toLocaleDateString(
                                "ar-EG"
                              )}
                            </span>
                          </div>
                          <p className="text-muted mb-0">{review.comment}</p>
                        </div>
                      </div>
                    ))
                  ) : (
                    <div className="text-center py-5 text-muted">
                      <i className="far fa-comment-dots fa-3x mb-3"></i>
                      <p>لا توجد تقييمات بعد.</p>
                    </div>
                  )}
                </div>
              )}
            </div>
          </div>
        </div>
      </div>

      <FloatingChatWidget
        receiverId={details.id}
        receiverUserName={details.userName}
        receiverFirstName={details.shopName || details.firstName}
      />
    </div>
  );
}
