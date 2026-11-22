"use client";

import React, { useEffect, useState } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { apiFetch } from "@/lib/api";
import { useAuth } from "@/context/AuthContext";
import { ChatPartner, CustomerDashboardDto, TaskDto } from "@/types";
import { toast } from "sonner";

export default function CustomerDashboardPage() {
  const router = useRouter();
  const { user, isAuthenticated, isLoading } = useAuth();
  const [dashboard, setDashboard] = useState<CustomerDashboardDto | null>(null);
  const [chatPartners, setChatPartners] = useState<ChatPartner[]>([]);
  const [loading, setLoading] = useState(true);
  const [payingTaskId, setPayingTaskId] = useState<number | null>(null);

  const loadData = async () => {
    if (!user) return;
    setLoading(true);
    try {
      const [dashData, partnersData] = await Promise.all([
        apiFetch<CustomerDashboardDto>(`/api/Dashboard/customer/${user.id}`),
        apiFetch<ChatPartner[]>(`/api/Message/GetChatPartners/${user.id}`).catch(
          () => []
        ),
      ]);
      setDashboard(dashData);
      setChatPartners(partnersData || []);
    } catch {
      toast.error("فشل تحميل بيانات لوحة التحكم.");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    if (!isLoading && !isAuthenticated) {
      router.push("/login");
      return;
    }
    if (isAuthenticated && user) {
      loadData();
    }
  }, [isLoading, isAuthenticated, user?.id]);

  const handlePayOnline = async (task: TaskDto) => {
    if (!user) return;
    setPayingTaskId(task.id);
    try {
      const res = await apiFetch<{ url: string }>(
        "/api/Payment/create-checkout-session",
        {
          method: "POST",
          body: JSON.stringify({
            taskId: task.id,
            price: task.price || 50,
            serviceName: task.serviceName || task.title,
            customerId: user.id,
            frontendUrl: window.location.origin,
          }),
        }
      );
      if (res.url) {
        window.location.href = res.url;
      } else {
        toast.error("لم يتم استلام رابط الدفع من الخادم.");
        setPayingTaskId(null);
      }
    } catch (err: any) {
      toast.error(err.message || "فشل بدء عملية الدفع.");
      setPayingTaskId(null);
    }
  };

  const handleCancelBooking = async (taskId: number) => {
    if (!confirm("هل أنت متأكد من إلغاء هذا الحجز؟")) return;
    try {
      await apiFetch(`/api/TechnicianSchedule/CancelBooking/${taskId}`, {
        method: "POST",
      });
      toast.success("تم إلغاء الحجز بنجاح.");
      loadData();
    } catch (err: any) {
      toast.error(err.message || "فشل إلغاء الحجز.");
    }
  };

  const getStatusBadge = (status: string) => {
    switch (status) {
      case "Pending":
        return { cls: "bg-warning text-dark", icon: "fa-clock", label: "قيد الانتظار (Pending)" };
      case "Accepted":
        return { cls: "bg-info text-white", icon: "fa-check-circle", label: "مقبول (Accepted)" };
      case "In Progress":
        return { cls: "bg-primary", icon: "fa-spinner", label: "جاري التنفيذ" };
      case "Paid":
        return { cls: "bg-success", icon: "fa-money-bill-wave", label: "تم الدفع (Paid)" };
      case "Completed":
        return { cls: "bg-success", icon: "fa-check-double", label: "مكتمل (Completed)" };
      default:
        return { cls: "bg-secondary", icon: "fa-times-circle", label: status };
    }
  };

  if (loading) {
    return (
      <div className="container py-5 mt-5 text-center">
        <div className="spinner-border text-success" style={{ width: "3rem", height: "3rem" }} />
        <p className="mt-3 text-secondary">جاري تحميل البيانات...</p>
      </div>
    );
  }

  return (
    <div className="container pt-5 pb-5 mt-5">
      <div className="row mb-4">
        <div className="col-12">
          <h2 className="fw-bold mb-0" style={{ color: "var(--primary-green)" }}>
            <i className="fas fa-tachometer-alt ms-2"></i>لوحة تحكم العميل
          </h2>
          <p className="text-secondary">مرحباً بك في لوحة التحكم الخاصة بك</p>
        </div>
      </div>

      {dashboard && (
        <>
          {/* Profile Section */}
          <div className="card shadow-sm mb-4">
            <div className="card-header d-flex justify-content-between align-items-center">
              <span>
                <i className="fas fa-user-circle ms-2"></i>معلومات الحساب
              </span>
              <Link href="/profile" className="btn btn-light btn-sm">
                <i className="fas fa-edit ms-1"></i>تعديل الحساب
              </Link>
            </div>
            <div className="card-body">
              <div className="row align-items-center">
                <div className="col-md-2 text-center mb-3 mb-md-0">
                  <img
                    src={
                      dashboard.profile.profilePictureUrl ||
                      "https://placehold.co/120x120?text=Profile"
                    }
                    alt="Profile"
                    className="img-thumbnail rounded-circle shadow-sm"
                    style={{ width: 120, height: 120, objectFit: "cover" }}
                    onError={(e) => {
                      (e.target as HTMLImageElement).src =
                        "https://placehold.co/120x120?text=Profile";
                    }}
                  />
                </div>
                <div className="col-md-10">
                  <h4 className="fw-bold mb-3">
                    {dashboard.profile.firstName} {dashboard.profile.lastName}
                  </h4>
                  <div className="row g-3">
                    <div className="col-md-6">
                      <p className="mb-2">
                        <i
                          className="fas fa-envelope ms-2"
                          style={{ color: "var(--primary-green)" }}
                        ></i>
                        <span className="text-secondary">
                          {dashboard.profile.email}
                        </span>
                      </p>
                      <p className="mb-2">
                        <i className="fas fa-phone ms-2 text-success"></i>
                        <span className="text-secondary">
                          {dashboard.profile.phone}
                        </span>
                      </p>
                    </div>
                    <div className="col-md-6">
                      <p className="mb-2">
                        <i className="fas fa-map-marker-alt text-danger ms-2"></i>
                        <span className="text-secondary">
                          {dashboard.profile.location}
                        </span>
                      </p>
                    </div>
                  </div>
                  <p className="text-muted mt-3 mb-0 fst-italic">
                    {dashboard.profile.bio || "لا توجد معلومات إضافية"}
                  </p>
                </div>
              </div>
            </div>
          </div>

          <div className="row">
            {/* Upcoming Appointments */}
            <div className="col-lg-6 mb-4">
              <div className="card shadow-sm h-100">
                <div className="card-header bg-info">
                  <i className="fas fa-calendar-check ms-2"></i>المواعيد القادمة
                </div>
                <div className="card-body">
                  {dashboard.upcomingAppointments &&
                  dashboard.upcomingAppointments.length > 0 ? (
                    <ul className="list-group list-group-flush">
                      {dashboard.upcomingAppointments.map((task) => {
                        const badge = getStatusBadge(task.status);
                        return (
                          <li
                            key={task.id}
                            className="list-group-item border rounded mb-2 shadow-sm p-3"
                          >
                            <div className="d-flex justify-content-between align-items-start mb-2">
                              <div className="flex-grow-1">
                                <h6 className="mb-1 fw-bold">{task.title}</h6>
                                <small className="text-muted d-block">
                                  <i className="fas fa-user ms-1"></i>
                                  {task.otherPartyName}
                                </small>
                                <small className="text-muted d-block">
                                  <i className="fas fa-tag ms-1"></i>
                                  {task.serviceName} ({task.price} ج.م)
                                </small>
                              </div>

                              <div className="d-flex flex-column gap-1">
                                {task.status === "Accepted" &&
                                  task.paymentMethodId === 2 && (
                                    <button
                                      type="button"
                                      disabled={payingTaskId === task.id}
                                      onClick={() => handlePayOnline(task)}
                                      className="btn btn-success btn-sm shadow-sm"
                                    >
                                      <i className="fas fa-credit-card ms-1"></i>
                                      {payingTaskId === task.id
                                        ? "جاري التحويل..."
                                        : "دفع أونلاين"}
                                    </button>
                                  )}

                                {task.status === "Pending" && (
                                  <button
                                    type="button"
                                    onClick={() => handleCancelBooking(task.id)}
                                    className="btn btn-outline-danger btn-sm"
                                  >
                                    إلغاء
                                  </button>
                                )}

                                {task.otherPartyId && (
                                  <Link
                                    href={`/chat/${task.otherPartyId}`}
                                    className="btn btn-outline-primary btn-sm"
                                  >
                                    <i className="fas fa-comments ms-1"></i> محادثة
                                  </Link>
                                )}
                              </div>
                            </div>
                            <span className={`badge ${badge.cls}`}>
                              <i className={`fas ${badge.icon} ms-1`}></i>
                              {badge.label}
                            </span>
                          </li>
                        );
                      })}
                    </ul>
                  ) : (
                    <div className="empty-state">
                      <i className="fas fa-calendar-times"></i>
                      <p className="mt-3 mb-0">لا توجد مواعيد قادمة</p>
                    </div>
                  )}
                </div>
              </div>
            </div>

            {/* Past Bookings */}
            <div className="col-lg-6 mb-4">
              <div className="card shadow-sm h-100">
                <div className="card-header bg-secondary">
                  <i className="fas fa-history ms-2"></i>الحجوزات السابقة
                </div>
                <div className="card-body">
                  {dashboard.pastBookings &&
                  dashboard.pastBookings.length > 0 ? (
                    <ul className="list-group list-group-flush">
                      {dashboard.pastBookings.map((task) => {
                        const badge = getStatusBadge(task.status);
                        return (
                          <li
                            key={task.id}
                            className="list-group-item border rounded mb-2 p-3"
                          >
                            <h6 className="mb-1 fw-bold">{task.title}</h6>
                            <small className="text-muted d-block">
                              <i className="fas fa-user ms-1"></i>
                              {task.otherPartyName}
                            </small>
                            <span className={`badge ${badge.cls} mt-2`}>
                              <i className={`fas ${badge.icon} ms-1`}></i>
                              {badge.label}
                            </span>
                          </li>
                        );
                      })}
                    </ul>
                  ) : (
                    <div className="empty-state">
                      <i className="fas fa-inbox"></i>
                      <p className="mt-3 mb-0">لا يوجد حجوزات سابقة</p>
                    </div>
                  )}
                </div>
              </div>
            </div>
          </div>

          {/* Recent Chats */}
          <div className="card shadow-sm mb-4">
            <div className="card-header">
              <i className="fas fa-comments ms-2"></i>المحادثات الأخيرة
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
                            "https://placehold.co/60x60?text=User"
                          }
                          alt={partner.userName}
                          className="rounded-circle ms-3"
                          style={{ width: 60, height: 60, objectFit: "cover" }}
                        />
                        <div>
                          <h6 className="mb-0 fw-bold">
                            {partner.shopName ||
                              `${partner.firstName} ${partner.lastName}`}
                          </h6>
                          <small className="text-muted">انقر للمحادثة</small>
                        </div>
                      </Link>
                    </div>
                  ))}
                </div>
              ) : (
                <div className="empty-state py-4">
                  <i className="fas fa-comment-slash"></i>
                  <p className="mt-2 mb-0">لا توجد محادثات سابقة</p>
                </div>
              )}
            </div>
          </div>
        </>
      )}
    </div>
  );
}
