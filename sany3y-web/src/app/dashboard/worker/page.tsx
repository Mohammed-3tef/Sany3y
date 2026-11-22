"use client";

import React, { useEffect, useState } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { apiFetch } from "@/lib/api";
import { useAuth } from "@/context/AuthContext";
import {
  ChatPartner,
  TechnicianScheduleSlot,
  WorkerDashboardDto,
} from "@/types";
import { toast } from "sonner";

export default function WorkerDashboardPage() {
  const router = useRouter();
  const { user, isAuthenticated, isLoading, isShop } = useAuth();

  const [dashboard, setDashboard] = useState<WorkerDashboardDto | null>(null);
  const [schedule, setSchedule] = useState<TechnicianScheduleSlot[]>([]);
  const [chatPartners, setChatPartners] = useState<ChatPartner[]>([]);
  const [loading, setLoading] = useState(true);

  // Add Slot Modal
  const [showAddSlotModal, setShowAddSlotModal] = useState(false);
  const [slotDate, setSlotDate] = useState("");
  const [slotStart, setSlotStart] = useState("");
  const [slotEnd, setSlotEnd] = useState("");
  const [addingSlot, setAddingSlot] = useState(false);

  const loadSchedule = async (userId: number) => {
    try {
      const slots = await apiFetch<TechnicianScheduleSlot[]>(
        `/api/TechnicianSchedule/GetMySchedule/${userId}`
      );
      setSchedule(slots || []);
    } catch {
      setSchedule([]);
    }
  };

  const loadDashboard = async () => {
    if (!user) return;
    setLoading(true);
    try {
      const [dashData, partnersData] = await Promise.all([
        apiFetch<WorkerDashboardDto>(`/api/Dashboard/worker/${user.id}`),
        apiFetch<ChatPartner[]>(`/api/Message/GetChatPartners/${user.id}`).catch(
          () => []
        ),
      ]);

      if (dashData?.profile?.isShop) {
        router.replace("/dashboard/shop");
        return;
      }

      setDashboard(dashData);
      setChatPartners(partnersData || []);
      await loadSchedule(user.id);
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
    if (isAuthenticated && isShop) {
      router.replace("/dashboard/shop");
      return;
    }
    if (isAuthenticated && user) {
      loadDashboard();
    }
  }, [isLoading, isAuthenticated, isShop, user?.id]);

  const handleAddSlot = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!user) return;

    if (slotStart >= slotEnd) {
      toast.error("وقت البدء يجب أن يكون قبل وقت الانتهاء");
      return;
    }

    setAddingSlot(true);
    try {
      await apiFetch("/api/TechnicianSchedule/AddSlot", {
        method: "POST",
        body: JSON.stringify({
          technicianId: user.id,
          date: slotDate,
          startTime: slotStart.length === 5 ? `${slotStart}:00` : slotStart,
          endTime: slotEnd.length === 5 ? `${slotEnd}:00` : slotEnd,
        }),
      });
      toast.success("تم إضافة الموعد بنجاح");
      setShowAddSlotModal(false);
      setSlotDate("");
      setSlotStart("");
      setSlotEnd("");
      await loadSchedule(user.id);
    } catch (err: any) {
      toast.error(err.message || "فشل إضافة الموعد");
    } finally {
      setAddingSlot(false);
    }
  };

  const handleDeleteSlot = async (slotId: number) => {
    if (!user || !confirm("هل أنت متأكد من حذف هذا الموعد؟")) return;
    try {
      await apiFetch(`/api/TechnicianSchedule/DeleteSlot/${slotId}`, {
        method: "DELETE",
      });
      toast.success("تم حذف الموعد بنجاح");
      await loadSchedule(user.id);
    } catch (err: any) {
      toast.error(err.message || "فشل حذف الموعد");
    }
  };

  const handleAcceptRequest = async (taskId: number) => {
    try {
      await apiFetch(`/api/TechnicianSchedule/AcceptRequest/${taskId}`, {
        method: "POST",
      });
      toast.success("تم قبول الطلب بنجاح");
      loadDashboard();
    } catch (err: any) {
      toast.error(err.message || "فشل قبول الطلب");
    }
  };

  const handleRejectRequest = async (taskId: number) => {
    if (!confirm("هل أنت متأكد من رفض هذا الطلب؟")) return;
    try {
      await apiFetch(`/api/TechnicianSchedule/RejectRequest/${taskId}`, {
        method: "POST",
      });
      toast.success("تم رفض الطلب");
      loadDashboard();
    } catch (err: any) {
      toast.error(err.message || "فشل رفض الطلب");
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
            <i className="fas fa-tools ms-2"></i>لوحة تحكم العامل
          </h2>
          <p className="text-secondary">إدارة طلباتك ومواعيدك</p>
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
                <div className="col-md-7">
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
                <div className="col-md-3 mt-3 mt-md-0">
                  <div className="earnings-card">
                    <p className="mb-1 opacity-75">إجمالي الأرباح</p>
                    <h4 className="mb-0">
                      {(dashboard.totalEarnings || 0).toFixed(2)} جنيه
                    </h4>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <div className="row">
            {/* Received Requests */}
            <div className="col-lg-6 mb-4">
              <div className="card shadow-sm h-100">
                <div className="card-header bg-warning text-dark">
                  <i className="fas fa-inbox ms-2"></i>الطلبات الواردة
                </div>
                <div className="card-body">
                  {dashboard.receivedRequests &&
                  dashboard.receivedRequests.length > 0 ? (
                    <ul className="list-group list-group-flush">
                      {dashboard.receivedRequests.map((task) => (
                        <li
                          key={task.id}
                          className="list-group-item border rounded mb-2 shadow-sm p-3"
                        >
                          <div className="d-flex justify-content-between align-items-start">
                            <div className="flex-grow-1">
                              <h6 className="mb-1 fw-bold">{task.title}</h6>
                              <small className="text-muted d-block">
                                <i className="fas fa-user ms-1"></i>
                                {task.otherPartyName}
                              </small>
                              <span className="badge bg-warning text-dark mt-2">
                                <i className="fas fa-clock ms-1"></i>
                                {task.status}
                              </span>
                            </div>
                            <div className="d-flex flex-column gap-1">
                              <button
                                type="button"
                                onClick={() => handleAcceptRequest(task.id)}
                                className="btn btn-success btn-sm"
                              >
                                <i className="fas fa-check ms-1"></i>قبول
                              </button>
                              <button
                                type="button"
                                onClick={() => handleRejectRequest(task.id)}
                                className="btn btn-danger btn-sm"
                              >
                                <i className="fas fa-times ms-1"></i>رفض
                              </button>
                            </div>
                          </div>
                        </li>
                      ))}
                    </ul>
                  ) : (
                    <div className="empty-state">
                      <i className="fas fa-inbox"></i>
                      <p className="mt-3 mb-0">لا توجد طلبات جديدة</p>
                    </div>
                  )}
                </div>
              </div>
            </div>

            {/* Accepted Jobs */}
            <div className="col-lg-6 mb-4">
              <div className="card shadow-sm h-100">
                <div className="card-header bg-success">
                  <i className="fas fa-briefcase ms-2"></i>الطلبات المقبولة
                </div>
                <div className="card-body">
                  {dashboard.acceptedJobs &&
                  dashboard.acceptedJobs.length > 0 ? (
                    <ul className="list-group list-group-flush">
                      {dashboard.acceptedJobs.map((task) => (
                        <li
                          key={task.id}
                          className="list-group-item border rounded mb-2 p-3 d-flex justify-content-between align-items-center"
                        >
                          <div>
                            <h6 className="mb-1 fw-bold">{task.title}</h6>
                            <small className="text-muted d-block">
                              <i className="fas fa-user ms-1"></i>
                              {task.otherPartyName}
                            </small>
                            <span
                              className={`badge ${
                                task.status === "In Progress"
                                  ? "bg-primary"
                                  : "bg-success"
                              } mt-2`}
                            >
                              <i className="fas fa-check-circle ms-1"></i>
                              {task.status}
                            </span>
                          </div>
                          {task.otherPartyId && (
                            <Link
                              href={`/chat/${task.otherPartyId}`}
                              className="btn btn-outline-success btn-sm"
                            >
                              <i className="fas fa-comments ms-1"></i> محادثة
                            </Link>
                          )}
                        </li>
                      ))}
                    </ul>
                  ) : (
                    <div className="empty-state">
                      <i className="fas fa-clipboard-check"></i>
                      <p className="mt-3 mb-0">لا توجد طلبات نشطة</p>
                    </div>
                  )}
                </div>
              </div>
            </div>
          </div>

          {/* Schedule Management */}
          <div className="card shadow-sm mb-4">
            <div className="card-header bg-primary d-flex justify-content-between align-items-center">
              <span>
                <i className="fas fa-calendar-alt ms-2"></i>إدارة المواعيد
              </span>
              <button
                type="button"
                className="btn btn-light btn-sm"
                onClick={() => setShowAddSlotModal(true)}
              >
                <i className="fas fa-plus ms-1"></i> إضافة موعد
              </button>
            </div>
            <div className="card-body">
              {schedule.length > 0 ? (
                <div className="table-responsive">
                  <table className="table table-hover align-middle text-center">
                    <thead className="table-light">
                      <tr>
                        <th>التاريخ</th>
                        <th>من</th>
                        <th>إلى</th>
                        <th>الحالة</th>
                        <th>إجراءات</th>
                      </tr>
                    </thead>
                    <tbody>
                      {schedule.map((slot) => (
                        <tr key={slot.id}>
                          <td>{String(slot.date).split("T")[0]}</td>
                          <td>
                            <strong>{slot.startTime}</strong>
                          </td>
                          <td>
                            <strong>{slot.endTime}</strong>
                          </td>
                          <td>
                            {slot.isBooked ? (
                              <span className="badge bg-danger">
                                <i className="fas fa-lock ms-1"></i>محجوز
                              </span>
                            ) : (
                              <span className="badge bg-success">
                                <i className="fas fa-check-circle ms-1"></i>متاح
                              </span>
                            )}
                          </td>
                          <td>
                            <button
                              type="button"
                              disabled={slot.isBooked}
                              onClick={() => handleDeleteSlot(slot.id)}
                              className={`btn btn-sm ${
                                slot.isBooked ? "btn-secondary" : "btn-danger"
                              }`}
                            >
                              <i className="fas fa-trash"></i>
                            </button>
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              ) : (
                <div className="empty-state">
                  <i className="fas fa-calendar-times"></i>
                  <p className="mt-3 mb-0">لا توجد مواعيد مضافة</p>
                </div>
              )}
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
                <div className="empty-state">
                  <i className="fas fa-comment-slash"></i>
                  <p className="mt-3 mb-0">لا توجد محادثات سابقة</p>
                </div>
              )}
            </div>
          </div>
        </>
      )}

      {/* Add Slot Modal */}
      {showAddSlotModal && (
        <div
          className="modal fade show d-block"
          style={{ backgroundColor: "rgba(0,0,0,0.5)" }}
        >
          <div className="modal-dialog modal-dialog-centered">
            <div className="modal-content">
              <div className="modal-header">
                <h5 className="modal-title">
                  <i className="fas fa-plus-circle ms-2"></i>إضافة موعد جديد
                </h5>
                <button
                  type="button"
                  className="btn-close btn-close-white"
                  onClick={() => setShowAddSlotModal(false)}
                ></button>
              </div>
              <form onSubmit={handleAddSlot}>
                <div className="modal-body">
                  <div className="mb-3">
                    <label className="form-label">التاريخ</label>
                    <input
                      type="date"
                      className="form-control"
                      min={new Date().toISOString().split("T")[0]}
                      value={slotDate}
                      onChange={(e) => setSlotDate(e.target.value)}
                      required
                    />
                  </div>
                  <div className="row">
                    <div className="col-6 mb-3">
                      <label className="form-label">من الساعة</label>
                      <input
                        type="time"
                        className="form-control"
                        value={slotStart}
                        onChange={(e) => setSlotStart(e.target.value)}
                        required
                      />
                    </div>
                    <div className="col-6 mb-3">
                      <label className="form-label">إلى الساعة</label>
                      <input
                        type="time"
                        className="form-control"
                        value={slotEnd}
                        onChange={(e) => setSlotEnd(e.target.value)}
                        required
                      />
                    </div>
                  </div>
                  <button
                    type="submit"
                    disabled={addingSlot}
                    className="btn btn-primary w-100"
                  >
                    <i className="fas fa-save ms-1"></i>
                    {addingSlot ? "جاري الحفظ..." : "حفظ الموعد"}
                  </button>
                </div>
              </form>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
