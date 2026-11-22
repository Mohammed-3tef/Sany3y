"use client";

import React, { useEffect, useState } from "react";
import { useParams, useRouter } from "next/navigation";
import { apiFetch } from "@/lib/api";
import { useAuth } from "@/context/AuthContext";
import { TechnicianDetails, TechnicianScheduleSlot } from "@/types";
import { FloatingChatWidget } from "@/components/FloatingChatWidget";
import { toast } from "sonner";

export default function TechnicianDetailsPage() {
  const params = useParams();
  const router = useRouter();
  const technicianId = Number(params?.id);
  const { user, isAuthenticated } = useAuth();

  const [details, setDetails] = useState<TechnicianDetails | null>(null);
  const [loading, setLoading] = useState(true);
  const [activeTab, setActiveTab] = useState<"about" | "reviews">("about");
  const [paymentMethod, setPaymentMethod] = useState<number>(1);

  // Schedule Booking Modal
  const [showScheduleModal, setShowScheduleModal] = useState(false);
  const [slots, setSlots] = useState<TechnicianScheduleSlot[]>([]);
  const [loadingSlots, setLoadingSlots] = useState(false);
  const [bookingSlotId, setBookingSlotId] = useState<number | null>(null);
  const [bookingSuccess, setBookingSuccess] = useState(false);

  // Add Rating Form
  const [reviewScore, setReviewScore] = useState(5);
  const [reviewComment, setReviewComment] = useState("");
  const [submittingReview, setSubmittingReview] = useState(false);

  const loadDetails = async () => {
    if (!technicianId) return;
    setLoading(true);
    try {
      const data = await apiFetch<TechnicianDetails>(
        `/api/Technician/Details/${technicianId}`
      );
      setDetails(data);
    } catch {
      setDetails(null);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadDetails();
  }, [technicianId]);

  const openScheduleModal = async () => {
    if (!isAuthenticated || !user) {
      toast.info("يرجى تسجيل الدخول أولاً لحجز موعد.");
      router.push("/login");
      return;
    }

    setShowScheduleModal(true);
    setBookingSuccess(false);
    setLoadingSlots(true);
    try {
      const data = await apiFetch<TechnicianScheduleSlot[]>(
        `/api/TechnicianSchedule/GetSchedule/${technicianId}`
      );
      setSlots(data || []);
    } catch {
      setSlots([]);
    } finally {
      setLoadingSlots(false);
    }
  };

  const handleBookSlot = async (scheduleId: number) => {
    if (!user) return;
    setBookingSlotId(scheduleId);
    try {
      await apiFetch("/api/TechnicianSchedule/BookSlot", {
        method: "POST",
        body: JSON.stringify({
          scheduleId,
          customerId: user.id,
          paymentMethodId: paymentMethod,
        }),
      });

      setBookingSuccess(true);
      setSlots((prev) => prev.filter((s) => s.id !== scheduleId));
      toast.success("تم إرسال طلب الحجز بنجاح! في انتظار موافقة الفني.");
    } catch (err: any) {
      toast.error(err.message || "فشل الحجز، يرجى المحاولة مرة أخرى.");
    } finally {
      setBookingSlotId(null);
    }
  };

  const handleReviewSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!user || !details) return;
    setSubmittingReview(true);
    try {
      await apiFetch("/api/Rating/Create", {
        method: "POST",
        body: JSON.stringify({
          score: reviewScore,
          comment: reviewComment,
          clientId: user.id,
          taskerId: details.id,
          createdAt: new Date().toISOString(),
        }),
      });
      toast.success("تم إضافة تقييمك بنجاح!");
      setReviewComment("");
      loadDetails();
    } catch {
      toast.error("فشل إرسال التقييم.");
    } finally {
      setSubmittingReview(false);
    }
  };

  if (loading) {
    return (
      <div className="container py-5 mt-5 text-center">
        <div className="spinner-border text-success" />
        <p className="mt-3 text-muted">جاري تحميل بيانات الصنايعي...</p>
      </div>
    );
  }

  if (!details) {
    return (
      <div className="container py-5 mt-5 text-center">
        <h3>الفني غير موجود</h3>
      </div>
    );
  }

  return (
    <div className="pt-5">
      {/* Hero Header */}
      <div className="profile-hero text-center">
        <div className="container">
          <h1 className="fw-bold mb-2">
            {details.firstName} {details.lastName}
          </h1>
          <p className="opacity-75 mb-0">
            <i className="fas fa-briefcase ms-2"></i>
            {details.categoryName || "صنايعي عام"}
            <span className="mx-2">|</span>
            <i className="fas fa-map-marker-alt ms-2"></i>
            {details.address?.city || details.city || "القاهرة"}
          </p>
        </div>
      </div>

      <div className="container mb-5" style={{ paddingTop: "6rem" }}>
        <div className="row">
          {/* Main Content */}
          <div className="col-lg-8">
            <div className="profile-main-card">
              <div className="text-center mb-4" style={{ marginTop: -110 }}>
                <div className="profile-avatar-container">
                  <img
                    src={
                      details.profilePictureUrl ||
                      "https://placehold.co/200x200?text=User"
                    }
                    className="profile-avatar"
                    alt={details.firstName}
                    onError={(e) => {
                      (e.target as HTMLImageElement).src =
                        "https://placehold.co/200x200?text=User";
                    }}
                  />
                  <div className="verified-badge" title="تم التحقق">
                    <i className="fas fa-check"></i>
                  </div>
                </div>
              </div>

              {/* Quick Stats */}
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
                    <div className="stat-value">
                      {details.experienceYears || 5}+
                    </div>
                    <div className="stat-label">سنوات خبرة</div>
                  </div>
                </div>
                <div className="col-4 col-md-3">
                  <div className="stat-box">
                    <div className="stat-value">{details.reviewCount}</div>
                    <div className="stat-label">مراجعة</div>
                  </div>
                </div>
              </div>

              {/* Tabs */}
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
                    عن الصنايعي
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
                    <i className="fas fa-user-circle text-success ms-2"></i> نبذة
                    تعريفية
                  </h5>
                  <p className="text-muted">
                    {details.bio ||
                      "لا توجد نبذة تعريفية متاحة حالياً لهذا الصنايعي. ولكنه متخصص في مجاله ولديه خبرة جيدة في التعامل مع مختلف المشاكل والصيانات."}
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
                          يخدم في مناطق:{" "}
                          {details.address?.city || details.city || "القاهرة"}
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
                  <div className="d-flex align-items-center justify-content-between mb-4">
                    <h5 className="fw-bold mb-0">آراء العملاء</h5>
                    <div className="badge bg-warning text-dark px-3 py-2 rounded-pill">
                      <i className="fas fa-star ms-1"></i>{" "}
                      {details.averageRating.toFixed(1)} من 5
                    </div>
                  </div>

                  {isAuthenticated && user && user.id !== details.id && (
                    <form
                      onSubmit={handleReviewSubmit}
                      className="bg-light p-3 rounded-3 mb-4 border"
                    >
                      <h6 className="fw-bold mb-2">أضف تقييمك</h6>
                      <div className="d-flex align-items-center gap-2 mb-2">
                        <span className="small text-muted">التقييم:</span>
                        {[1, 2, 3, 4, 5].map((star) => (
                          <i
                            key={star}
                            onClick={() => setReviewScore(star)}
                            className={`fas fa-star ${
                              star <= reviewScore
                                ? "text-warning"
                                : "text-secondary opacity-25"
                            }`}
                            style={{ cursor: "pointer", fontSize: "1.2rem" }}
                          />
                        ))}
                      </div>
                      <textarea
                        className="form-control mb-2"
                        rows={2}
                        placeholder="اكتب رأيك في الخدمة..."
                        value={reviewComment}
                        onChange={(e) => setReviewComment(e.target.value)}
                        required
                      />
                      <button
                        type="submit"
                        disabled={submittingReview}
                        className="btn btn-success btn-sm"
                      >
                        {submittingReview ? "جاري الإرسال..." : "إرسال التقييم"}
                      </button>
                    </form>
                  )}

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
                            {review.clientName || "عميل مستخدم"}
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

          {/* Booking Sidebar */}
          <div className="col-lg-4 mt-4 mt-lg-0">
            <div className="booking-card">
              <h5 className="fw-bold mb-4 text-center">معلومات الحجز</h5>

              <div className="text-center mb-4">
                <span className="text-muted">سعر المعاينة</span>
                <div className="price-tag">{details.price || 50} ج.م</div>
              </div>

              <div className="d-flex justify-content-between mb-3 border-bottom pb-3">
                <span className="text-muted">
                  <i className="fas fa-money-bill-wave ms-2"></i> الدفع
                </span>
                <div className="d-flex gap-3">
                  <div className="form-check">
                    <input
                      className="form-check-input"
                      type="radio"
                      name="paymentMethod"
                      id="paymentCash"
                      checked={paymentMethod === 1}
                      onChange={() => setPaymentMethod(1)}
                    />
                    <label className="form-check-label" htmlFor="paymentCash">
                      كاش
                    </label>
                  </div>
                  <div className="form-check">
                    <input
                      className="form-check-input"
                      type="radio"
                      name="paymentMethod"
                      id="paymentOnline"
                      checked={paymentMethod === 2}
                      onChange={() => setPaymentMethod(2)}
                    />
                    <label className="form-check-label" htmlFor="paymentOnline">
                      أونلاين
                    </label>
                  </div>
                </div>
              </div>

              <div className="d-flex justify-content-between mb-4">
                <span className="text-muted">
                  <i className="fas fa-shield-alt ms-2"></i> الضمان
                </span>
                <span className="fw-bold text-success">ضمان 30 يوم</span>
              </div>

              <button
                type="button"
                className="btn-book-lg"
                onClick={openScheduleModal}
              >
                <i className="far fa-calendar-check ms-2"></i> احجز موعد الآن
              </button>

              <div className="text-center mt-3 small text-muted">
                <i className="fas fa-lock ms-1"></i> حجز مجاني وآمن 100%
              </div>
            </div>
          </div>
        </div>
      </div>

      {/* Schedule Booking Modal */}
      {showScheduleModal && (
        <div
          className="modal fade show d-block"
          style={{ backgroundColor: "rgba(0,0,0,0.5)" }}
        >
          <div className="modal-dialog modal-lg modal-dialog-centered">
            <div className="modal-content shadow-lg border-0">
              <div className="modal-header bg-primary text-white">
                <h5 className="modal-title fw-bold">اختر الموعد المناسب</h5>
                <button
                  type="button"
                  className="btn-close btn-close-white"
                  onClick={() => setShowScheduleModal(false)}
                ></button>
              </div>
              <div className="modal-body p-4">
                {bookingSuccess && (
                  <div className="alert alert-success text-center">
                    <i className="fas fa-check-circle fa-2x mb-2 d-block"></i>
                    <strong>تم إرسال طلب الحجز بنجاح!</strong>
                    <p className="mb-0">
                      سيتواصل معك الفني قريباً لتأكيد التفاصيل، ويمكنك متابعة
                      الطلب من لوحتك.
                    </p>
                  </div>
                )}

                {loadingSlots ? (
                  <div className="text-center my-5">
                    <div className="spinner-border text-success" />
                    <p className="mt-2 text-muted">
                      جاري تحميل المواعيد المتاحة...
                    </p>
                  </div>
                ) : (
                  <div className="table-responsive">
                    <table className="table table-hover align-middle text-center mt-2">
                      <thead className="table-light">
                        <tr>
                          <th className="py-3">اليوم</th>
                          <th className="py-3">من</th>
                          <th className="py-3">إلى</th>
                          <th className="py-3">إجراء</th>
                        </tr>
                      </thead>
                      <tbody>
                        {slots.length === 0 ? (
                          <tr>
                            <td colSpan={4} className="text-muted py-4">
                              <i className="far fa-calendar-times fa-2x mb-2 d-block"></i>
                              لا توجد مواعيد متاحة حالياً
                            </td>
                          </tr>
                        ) : (
                          slots.map((slot) => (
                            <tr key={slot.id}>
                              <td className="fw-bold text-dark">
                                {String(slot.date).split("T")[0]}
                              </td>
                              <td>{slot.startTime}</td>
                              <td>{slot.endTime}</td>
                              <td>
                                <button
                                  type="button"
                                  disabled={bookingSlotId === slot.id}
                                  onClick={() => handleBookSlot(slot.id)}
                                  className="btn btn-outline-success btn-sm px-4 rounded-pill fw-bold"
                                >
                                  {bookingSlotId === slot.id
                                    ? "جاري الحجز..."
                                    : "حجز"}
                                </button>
                              </td>
                            </tr>
                          ))
                        )}
                      </tbody>
                    </table>
                  </div>
                )}
              </div>
            </div>
          </div>
        </div>
      )}

      {/* Real-time Floating Chat */}
      <FloatingChatWidget
        receiverId={details.id}
        receiverUserName={details.userName}
        receiverFirstName={details.firstName}
      />
    </div>
  );
}
