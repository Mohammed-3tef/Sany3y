"use client";

import React, { useEffect, useState } from "react";
import Link from "next/link";
import { apiFetch, downloadApiFile } from "@/lib/api";
import { useSignalR } from "@/context/SignalRContext";
import { Category } from "@/types";
import { useGovernoratesAndCities } from "@/components/GovernorateCitySelect";
import { toast } from "sonner";

interface AdminUsersTableProps {
  onlyShops: boolean;
}

export function AdminUsersTable({ onlyShops }: AdminUsersTableProps) {
  const { onlineUsersById, onlineUsersByName } = useSignalR();
  const { governorates, cities, loadCitiesForGovernorate } =
    useGovernoratesAndCities();

  const [users, setUsers] = useState<any[]>([]);
  const [categories, setCategories] = useState<Category[]>([]);
  const [rolesList, setRolesList] = useState<string[]>([
    "Client",
    "Technician",
    "Admin",
  ]);
  const [loading, setLoading] = useState(true);
  const [search, setSearch] = useState("");

  // Modals
  const [viewUser, setViewUser] = useState<any | null>(null);
  const [editUser, setEditUser] = useState<any | null>(null);
  const [deleteUser, setDeleteUser] = useState<any | null>(null);
  const [showAddModal, setShowAddModal] = useState(false);

  // Add Form State
  const [addForm, setAddForm] = useState({
    nationalId: "",
    userName: "",
    firstName: "",
    lastName: "",
    birthDate: "",
    gender: "M",
    email: "",
    phoneNumber: "",
    password: "",
    confirmPassword: "",
    role: "Client",
    categoryId: "",
    experienceYears: "",
    price: "",
    governorate: "",
    city: "",
    street: "",
    bio: "",
    shopName: "",
  });

  // Edit Form State
  const [editForm, setEditForm] = useState({
    id: 0,
    firstName: "",
    lastName: "",
    email: "",
    phoneNumber: "",
    birthDate: "",
    governorate: "",
    city: "",
    street: "",
    role: "Client",
    categoryId: "",
    experienceYears: "",
    price: "",
    bio: "",
    shopName: "",
  });

  const loadUsers = async () => {
    setLoading(true);
    try {
      const data = await apiFetch<any[]>(
        `/api/Admin/users?onlyShops=${onlyShops}`
      );
      setUsers(data || []);
    } catch {
      toast.error("فشل تحميل قائمة المستخدمين");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadUsers();
    apiFetch<Category[]>("/api/Category/GetAll")
      .then((c) => setCategories(c || []))
      .catch(() => {});
    apiFetch<any[]>("/api/Role/GetAll")
      .then((r) => {
        if (r && r.length > 0) {
          setRolesList(r.map((x) => x.name));
        }
      })
      .catch(() => {});
  }, [onlyShops]);

  const openEditModal = (u: any) => {
    setEditUser(u);
    const gov = u.address?.governorate || "";
    setEditForm({
      id: u.id,
      firstName: u.firstName || "",
      lastName: u.lastName || "",
      email: u.email || "",
      phoneNumber: u.phoneNumber || "",
      birthDate: u.birthDate ? String(u.birthDate).split("T")[0] : "",
      governorate: gov,
      city: u.address?.city || "",
      street: u.address?.street || "",
      role: u.role || "Client",
      categoryId: u.categoryId ? String(u.categoryId) : "",
      experienceYears: u.experienceYears ? String(u.experienceYears) : "",
      price: u.price ? String(u.price) : "",
      bio: u.bio || "",
      shopName: u.shopName || "",
    });
    if (gov) loadCitiesForGovernorate(gov);
  };

  const handleCreateUser = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      const formData = new FormData();
      formData.append("NationalId", addForm.nationalId);
      formData.append("FirstName", addForm.firstName);
      formData.append("LastName", addForm.lastName);
      formData.append("UserName", addForm.userName);
      formData.append("Email", addForm.email);
      formData.append("PhoneNumber", addForm.phoneNumber);
      formData.append("BirthDate", addForm.birthDate);
      formData.append("IsMale", String(addForm.gender === "M"));
      formData.append("IsClient", String(addForm.role !== "Technician"));
      formData.append("Governorate", addForm.governorate);
      formData.append("City", addForm.city);
      formData.append("Street", addForm.street);
      formData.append("Password", addForm.password);
      formData.append("ConfirmPassword", addForm.confirmPassword);
      formData.append("role", addForm.role);
      if (onlyShops) {
        formData.append("IsShop", "true");
        formData.append("ShopName", addForm.shopName);
      }
      if (addForm.role === "Technician") {
        if (addForm.categoryId) formData.append("CategoryId", addForm.categoryId);
        if (addForm.experienceYears)
          formData.append("ExperienceYears", addForm.experienceYears);
        if (addForm.price) formData.append("Price", addForm.price);
      }

      await apiFetch("/api/Admin/users", {
        method: "POST",
        body: formData,
      });
      toast.success("تم إنشاء المستخدم بنجاح");
      setShowAddModal(false);
      loadUsers();
    } catch (err: any) {
      toast.error(err.message || "فشل إنشاء المستخدم");
    }
  };

  const handleUpdateUser = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      await apiFetch(`/api/Admin/users/${editForm.id}`, {
        method: "PUT",
        body: JSON.stringify({
          firstName: editForm.firstName,
          lastName: editForm.lastName,
          email: editForm.email,
          phoneNumber: editForm.phoneNumber,
          birthDate: editForm.birthDate,
          governorate: editForm.governorate,
          city: editForm.city,
          street: editForm.street,
          role: editForm.role,
          categoryId:
            editForm.role === "Technician" && editForm.categoryId
              ? Number(editForm.categoryId)
              : null,
          experienceYears:
            editForm.role === "Technician" && editForm.experienceYears
              ? Number(editForm.experienceYears)
              : null,
          price:
            editForm.role === "Technician" && editForm.price
              ? Number(editForm.price)
              : null,
          bio: editForm.bio,
          shopName: editForm.shopName,
        }),
      });
      toast.success("تم تحديث بيانات المستخدم بنجاح");
      setEditUser(null);
      loadUsers();
    } catch (err: any) {
      toast.error(err.message || "فشل تحديث المستخدم");
    }
  };

  const handleDeleteUser = async () => {
    if (!deleteUser) return;
    try {
      await apiFetch(`/api/User/Delete/${deleteUser.id}`, {
        method: "DELETE",
      });
      toast.success("تم حذف المستخدم بنجاح");
      setDeleteUser(null);
      loadUsers();
    } catch (err: any) {
      toast.error(err.message || "فشل حذف المستخدم");
    }
  };

  const filteredUsers = users.filter((u) => {
    if (!search.trim()) return true;
    const q = search.toLowerCase();
    return (
      u.userName?.toLowerCase().includes(q) ||
      u.email?.toLowerCase().includes(q) ||
      u.shopName?.toLowerCase().includes(q) ||
      `${u.firstName} ${u.lastName}`.toLowerCase().includes(q)
    );
  });

  const exportTarget = onlyShops ? "stores" : "users";

  return (
    <div>
      <div className="d-flex flex-column flex-lg-row justify-content-between align-items-center mb-4">
        <h2 className="fw-bold mb-3 mb-lg-0">
          {onlyShops ? "إدارة المحلات" : "إدارة المستخدمين"}
        </h2>

        <div className="d-flex flex-wrap gap-2">
          <button
            type="button"
            className="btn btn-outline-primary"
            onClick={() => setShowAddModal(true)}
          >
            <i className="fa-solid fa-user-plus ms-1"></i>
            {onlyShops ? "إضافة محل جديد" : "إضافة مستخدم جديد"}
          </button>
          <button
            type="button"
            onClick={() =>
              downloadApiFile(
                `/api/Admin/export/${exportTarget}/pdf`,
                `Sany3y_${exportTarget}.pdf`
              ).catch(() => toast.error("فشل تصدير PDF"))
            }
            className="btn btn-outline-danger"
          >
            <i className="fa-solid fa-file-pdf ms-1"></i> تصدير PDF
          </button>
          <button
            type="button"
            onClick={() =>
              downloadApiFile(
                `/api/Admin/export/${exportTarget}/csv`,
                `Sany3y_${exportTarget}.csv`
              ).catch(() => toast.error("فشل تصدير CSV"))
            }
            className="btn btn-outline-success"
          >
            <i className="fa-solid fa-file-csv ms-1"></i> تصدير CSV
          </button>
        </div>
      </div>

      <div className="bg-white rounded-4 shadow-sm border p-4">
        <div className="row mb-3">
          <div className="col-md-4">
            <input
              type="text"
              className="form-control"
              placeholder="بحث بالاسم أو البريد أو اسم المستخدم..."
              value={search}
              onChange={(e) => setSearch(e.target.value)}
            />
          </div>
        </div>

        {loading ? (
          <div className="text-center py-5">
            <div className="spinner-border text-success" />
          </div>
        ) : (
          <div className="table-responsive">
            <table className="table table-striped table-bordered align-middle text-center">
              <thead className="table-light">
                <tr>
                  {onlyShops && <th>اسم المحل</th>}
                  <th>اسم المستخدم</th>
                  <th>البريد الإلكتروني</th>
                  <th>الدور</th>
                  <th>الحالة</th>
                  <th>تاريخ الإنشاء</th>
                  <th>الإجراءات</th>
                </tr>
              </thead>
              <tbody>
                {filteredUsers.length === 0 ? (
                  <tr>
                    <td colSpan={onlyShops ? 7 : 6} className="py-4 text-muted">
                      لا توجد سجلات مطابقة
                    </td>
                  </tr>
                ) : (
                  filteredUsers.map((item) => {
                    const isOnline =
                      onlineUsersById[item.id] ??
                      onlineUsersByName[item.userName] ??
                      item.isOnline;

                    return (
                      <tr key={item.id}>
                        {onlyShops && (
                          <td className="fw-bold">{item.shopName || "-"}</td>
                        )}
                        <td>{item.userName}</td>
                        <td>{item.email}</td>
                        <td>{(item.roles || [item.role]).join(", ")}</td>
                        <td>
                          <span
                            className={`px-3 py-1 rounded-pill small border ${
                              isOnline
                                ? "text-success border-success"
                                : "text-danger border-danger"
                            }`}
                          >
                            {isOnline ? "متصل الآن" : "غير متصل"}
                          </span>
                        </td>
                        <td>
                          {item.createdAt
                            ? new Date(item.createdAt).toLocaleDateString(
                                "ar-EG"
                              )
                            : "-"}
                        </td>
                        <td>
                          <div className="d-flex gap-1 justify-content-center">
                            <Link
                              href={`/chat/${item.id}`}
                              className="btn btn-outline-success btn-sm"
                              title="محادثة"
                            >
                              <i className="fa-solid fa-comments"></i>
                            </Link>
                            <button
                              type="button"
                              className="btn btn-outline-primary btn-sm"
                              onClick={() => setViewUser(item)}
                              title="عرض"
                            >
                              <i className="fa-solid fa-eye"></i>
                            </button>
                            <button
                              type="button"
                              className="btn btn-outline-warning btn-sm"
                              onClick={() => openEditModal(item)}
                              title="تعديل"
                            >
                              <i className="fa-solid fa-pen"></i>
                            </button>
                            <button
                              type="button"
                              className="btn btn-outline-danger btn-sm"
                              onClick={() => setDeleteUser(item)}
                              title="حذف"
                            >
                              <i className="fa-solid fa-trash"></i>
                            </button>
                          </div>
                        </td>
                      </tr>
                    );
                  })
                )}
              </tbody>
            </table>
          </div>
        )}
      </div>

      {/* View User Modal */}
      {viewUser && (
        <div
          className="modal fade show d-block"
          style={{ backgroundColor: "rgba(0,0,0,0.5)" }}
        >
          <div className="modal-dialog modal-dialog-centered modal-lg">
            <div className="modal-content">
              <div className="modal-header">
                <h5 className="modal-title">تفاصيل {viewUser.userName}</h5>
                <button
                  type="button"
                  className="btn-close btn-close-white"
                  onClick={() => setViewUser(null)}
                ></button>
              </div>
              <div className="modal-body">
                <div className="row g-3">
                  <div className="col-md-12 text-center mb-2">
                    <img
                      src={viewUser.profilePictureUrl}
                      alt={viewUser.userName}
                      className="img-thumbnail rounded-circle"
                      style={{ width: 120, height: 120, objectFit: "cover" }}
                    />
                  </div>
                  <div className="col-md-6">
                    <label className="form-label">الرقم القومي</label>
                    <input
                      type="text"
                      className="form-control"
                      value={viewUser.nationalId || ""}
                      readOnly
                    />
                  </div>
                  <div className="col-md-6">
                    <label className="form-label">اسم المستخدم</label>
                    <input
                      type="text"
                      className="form-control"
                      value={viewUser.userName || ""}
                      readOnly
                    />
                  </div>
                  <div className="col-md-6">
                    <label className="form-label">الاسم الأول</label>
                    <input
                      type="text"
                      className="form-control"
                      value={viewUser.firstName || ""}
                      readOnly
                    />
                  </div>
                  <div className="col-md-6">
                    <label className="form-label">الاسم الأخير</label>
                    <input
                      type="text"
                      className="form-control"
                      value={viewUser.lastName || ""}
                      readOnly
                    />
                  </div>
                  <div className="col-md-6">
                    <label className="form-label">البريد الإلكتروني</label>
                    <input
                      type="text"
                      className="form-control"
                      value={viewUser.email || ""}
                      readOnly
                    />
                  </div>
                  <div className="col-md-6">
                    <label className="form-label">رقم الهاتف</label>
                    <input
                      type="text"
                      className="form-control"
                      value={viewUser.phoneNumber || ""}
                      readOnly
                    />
                  </div>
                  <div className="col-md-4">
                    <label className="form-label">المحافظة</label>
                    <input
                      type="text"
                      className="form-control"
                      value={viewUser.address?.governorate || ""}
                      readOnly
                    />
                  </div>
                  <div className="col-md-4">
                    <label className="form-label">المدينة</label>
                    <input
                      type="text"
                      className="form-control"
                      value={viewUser.address?.city || ""}
                      readOnly
                    />
                  </div>
                  <div className="col-md-4">
                    <label className="form-label">الشارع</label>
                    <input
                      type="text"
                      className="form-control"
                      value={viewUser.address?.street || ""}
                      readOnly
                    />
                  </div>
                  {viewUser.role === "Technician" && (
                    <>
                      <div className="col-md-4">
                        <label className="form-label">التخصص</label>
                        <input
                          type="text"
                          className="form-control"
                          value={viewUser.categoryName || ""}
                          readOnly
                        />
                      </div>
                      <div className="col-md-4">
                        <label className="form-label">سنوات الخبرة</label>
                        <input
                          type="text"
                          className="form-control"
                          value={viewUser.experienceYears ?? ""}
                          readOnly
                        />
                      </div>
                      <div className="col-md-4">
                        <label className="form-label">سعر المعاينة</label>
                        <input
                          type="text"
                          className="form-control"
                          value={viewUser.price ?? ""}
                          readOnly
                        />
                      </div>
                    </>
                  )}
                </div>
              </div>
              <div className="modal-footer">
                <button
                  type="button"
                  className="btn btn-secondary"
                  onClick={() => setViewUser(null)}
                >
                  إغلاق
                </button>
              </div>
            </div>
          </div>
        </div>
      )}

      {/* Add User Modal */}
      {showAddModal && (
        <div
          className="modal fade show d-block"
          style={{ backgroundColor: "rgba(0,0,0,0.5)" }}
        >
          <div className="modal-dialog modal-dialog-centered modal-lg">
            <div className="modal-content">
              <div className="modal-header">
                <h5 className="modal-title">إضافة مستخدم جديد</h5>
                <button
                  type="button"
                  className="btn-close btn-close-white"
                  onClick={() => setShowAddModal(false)}
                ></button>
              </div>
              <form onSubmit={handleCreateUser}>
                <div className="modal-body">
                  <div className="row g-3">
                    <div className="col-md-6">
                      <label className="form-label">الرقم القومي</label>
                      <input
                        type="text"
                        maxLength={14}
                        className="form-control"
                        value={addForm.nationalId}
                        onChange={(e) =>
                          setAddForm({ ...addForm, nationalId: e.target.value })
                        }
                        required
                      />
                    </div>
                    <div className="col-md-6">
                      <label className="form-label">اسم المستخدم</label>
                      <input
                        type="text"
                        className="form-control"
                        value={addForm.userName}
                        onChange={(e) =>
                          setAddForm({ ...addForm, userName: e.target.value })
                        }
                        required
                      />
                    </div>
                    <div className="col-md-6">
                      <label className="form-label">الاسم الأول</label>
                      <input
                        type="text"
                        className="form-control"
                        value={addForm.firstName}
                        onChange={(e) =>
                          setAddForm({ ...addForm, firstName: e.target.value })
                        }
                        required
                      />
                    </div>
                    <div className="col-md-6">
                      <label className="form-label">الاسم الأخير</label>
                      <input
                        type="text"
                        className="form-control"
                        value={addForm.lastName}
                        onChange={(e) =>
                          setAddForm({ ...addForm, lastName: e.target.value })
                        }
                        required
                      />
                    </div>
                    <div className="col-md-6">
                      <label className="form-label">تاريخ الميلاد</label>
                      <input
                        type="date"
                        className="form-control"
                        value={addForm.birthDate}
                        onChange={(e) =>
                          setAddForm({ ...addForm, birthDate: e.target.value })
                        }
                        required
                      />
                    </div>
                    <div className="col-md-6">
                      <label className="form-label">الدور</label>
                      <select
                        className="form-select"
                        value={addForm.role}
                        onChange={(e) =>
                          setAddForm({ ...addForm, role: e.target.value })
                        }
                      >
                        {rolesList.map((r) => (
                          <option key={r} value={r}>
                            {r}
                          </option>
                        ))}
                      </select>
                    </div>
                    <div className="col-md-6">
                      <label className="form-label">البريد الإلكتروني</label>
                      <input
                        type="email"
                        className="form-control"
                        value={addForm.email}
                        onChange={(e) =>
                          setAddForm({ ...addForm, email: e.target.value })
                        }
                        required
                      />
                    </div>
                    <div className="col-md-6">
                      <label className="form-label">رقم الهاتف</label>
                      <input
                        type="text"
                        className="form-control"
                        value={addForm.phoneNumber}
                        onChange={(e) =>
                          setAddForm({ ...addForm, phoneNumber: e.target.value })
                        }
                        required
                      />
                    </div>
                    <div className="col-md-6">
                      <label className="form-label">كلمة المرور</label>
                      <input
                        type="password"
                        className="form-control"
                        value={addForm.password}
                        onChange={(e) =>
                          setAddForm({ ...addForm, password: e.target.value })
                        }
                        required
                      />
                    </div>
                    <div className="col-md-6">
                      <label className="form-label">تأكيد كلمة المرور</label>
                      <input
                        type="password"
                        className="form-control"
                        value={addForm.confirmPassword}
                        onChange={(e) =>
                          setAddForm({
                            ...addForm,
                            confirmPassword: e.target.value,
                          })
                        }
                        required
                      />
                    </div>
                    {addForm.role === "Technician" && (
                      <>
                        <div className="col-md-4">
                          <label className="form-label">التخصص</label>
                          <select
                            className="form-select"
                            value={addForm.categoryId}
                            onChange={(e) =>
                              setAddForm({
                                ...addForm,
                                categoryId: e.target.value,
                              })
                            }
                          >
                            <option value="">اختر التخصص</option>
                            {categories.map((c) => (
                              <option key={c.id} value={c.id}>
                                {c.name}
                              </option>
                            ))}
                          </select>
                        </div>
                        <div className="col-md-4">
                          <label className="form-label">سنوات الخبرة</label>
                          <input
                            type="number"
                            className="form-control"
                            value={addForm.experienceYears}
                            onChange={(e) =>
                              setAddForm({
                                ...addForm,
                                experienceYears: e.target.value,
                              })
                            }
                          />
                        </div>
                        <div className="col-md-4">
                          <label className="form-label">سعر المعاينة</label>
                          <input
                            type="number"
                            className="form-control"
                            value={addForm.price}
                            onChange={(e) =>
                              setAddForm({ ...addForm, price: e.target.value })
                            }
                          />
                        </div>
                      </>
                    )}
                    <div className="col-md-4">
                      <label className="form-label">المحافظة</label>
                      <select
                        className="form-select"
                        value={addForm.governorate}
                        onChange={(e) => {
                          setAddForm({
                            ...addForm,
                            governorate: e.target.value,
                            city: "",
                          });
                          loadCitiesForGovernorate(e.target.value);
                        }}
                        required
                      >
                        <option value="">اختر المحافظة</option>
                        {governorates.map((g) => (
                          <option key={g.id} value={g.arabicName}>
                            {g.arabicName}
                          </option>
                        ))}
                      </select>
                    </div>
                    <div className="col-md-4">
                      <label className="form-label">المدينة</label>
                      <select
                        className="form-select"
                        value={addForm.city}
                        onChange={(e) =>
                          setAddForm({ ...addForm, city: e.target.value })
                        }
                        required
                      >
                        <option value="">اختر المدينة</option>
                        {cities.map((c) => (
                          <option key={c.id} value={c.arabicName}>
                            {c.arabicName}
                          </option>
                        ))}
                      </select>
                    </div>
                    <div className="col-md-4">
                      <label className="form-label">الشارع</label>
                      <input
                        type="text"
                        className="form-control"
                        value={addForm.street}
                        onChange={(e) =>
                          setAddForm({ ...addForm, street: e.target.value })
                        }
                        required
                      />
                    </div>
                  </div>
                </div>
                <div className="modal-footer">
                  <button type="submit" className="btn btn-primary">
                    إضافة المستخدم
                  </button>
                  <button
                    type="button"
                    className="btn btn-secondary"
                    onClick={() => setShowAddModal(false)}
                  >
                    إغلاق
                  </button>
                </div>
              </form>
            </div>
          </div>
        </div>
      )}

      {/* Edit User Modal */}
      {editUser && (
        <div
          className="modal fade show d-block"
          style={{ backgroundColor: "rgba(0,0,0,0.5)" }}
        >
          <div className="modal-dialog modal-dialog-centered modal-lg">
            <div className="modal-content">
              <div className="modal-header">
                <h5 className="modal-title">
                  تعديل بيانات {editUser.userName}
                </h5>
                <button
                  type="button"
                  className="btn-close btn-close-white"
                  onClick={() => setEditUser(null)}
                ></button>
              </div>
              <form onSubmit={handleUpdateUser}>
                <div className="modal-body">
                  <div className="row g-3">
                    <div className="col-md-6">
                      <label className="form-label">الاسم الأول</label>
                      <input
                        type="text"
                        className="form-control"
                        value={editForm.firstName}
                        onChange={(e) =>
                          setEditForm({ ...editForm, firstName: e.target.value })
                        }
                        required
                      />
                    </div>
                    <div className="col-md-6">
                      <label className="form-label">الاسم الأخير</label>
                      <input
                        type="text"
                        className="form-control"
                        value={editForm.lastName}
                        onChange={(e) =>
                          setEditForm({ ...editForm, lastName: e.target.value })
                        }
                        required
                      />
                    </div>
                    <div className="col-md-6">
                      <label className="form-label">رقم الهاتف</label>
                      <input
                        type="text"
                        className="form-control"
                        value={editForm.phoneNumber}
                        onChange={(e) =>
                          setEditForm({
                            ...editForm,
                            phoneNumber: e.target.value,
                          })
                        }
                        required
                      />
                    </div>
                    <div className="col-md-6">
                      <label className="form-label">الدور</label>
                      <select
                        className="form-select"
                        value={editForm.role}
                        onChange={(e) =>
                          setEditForm({ ...editForm, role: e.target.value })
                        }
                      >
                        {rolesList.map((r) => (
                          <option key={r} value={r}>
                            {r}
                          </option>
                        ))}
                      </select>
                    </div>
                    <div className="col-md-4">
                      <label className="form-label">المحافظة</label>
                      <select
                        className="form-select"
                        value={editForm.governorate}
                        onChange={(e) => {
                          setEditForm({
                            ...editForm,
                            governorate: e.target.value,
                            city: "",
                          });
                          loadCitiesForGovernorate(e.target.value);
                        }}
                      >
                        <option value="">اختر المحافظة</option>
                        {governorates.map((g) => (
                          <option key={g.id} value={g.arabicName}>
                            {g.arabicName}
                          </option>
                        ))}
                      </select>
                    </div>
                    <div className="col-md-4">
                      <label className="form-label">المدينة</label>
                      <select
                        className="form-select"
                        value={editForm.city}
                        onChange={(e) =>
                          setEditForm({ ...editForm, city: e.target.value })
                        }
                      >
                        <option value="">اختر المدينة</option>
                        {cities.map((c) => (
                          <option key={c.id} value={c.arabicName}>
                            {c.arabicName}
                          </option>
                        ))}
                      </select>
                    </div>
                    <div className="col-md-4">
                      <label className="form-label">الشارع</label>
                      <input
                        type="text"
                        className="form-control"
                        value={editForm.street}
                        onChange={(e) =>
                          setEditForm({ ...editForm, street: e.target.value })
                        }
                      />
                    </div>
                    {editForm.role === "Technician" && (
                      <>
                        <div className="col-md-4">
                          <label className="form-label">التخصص</label>
                          <select
                            className="form-select"
                            value={editForm.categoryId}
                            onChange={(e) =>
                              setEditForm({
                                ...editForm,
                                categoryId: e.target.value,
                              })
                            }
                          >
                            <option value="">اختر التخصص</option>
                            {categories.map((c) => (
                              <option key={c.id} value={c.id}>
                                {c.name}
                              </option>
                            ))}
                          </select>
                        </div>
                        <div className="col-md-4">
                          <label className="form-label">سنوات الخبرة</label>
                          <input
                            type="number"
                            className="form-control"
                            value={editForm.experienceYears}
                            onChange={(e) =>
                              setEditForm({
                                ...editForm,
                                experienceYears: e.target.value,
                              })
                            }
                          />
                        </div>
                        <div className="col-md-4">
                          <label className="form-label">سعر المعاينة</label>
                          <input
                            type="number"
                            className="form-control"
                            value={editForm.price}
                            onChange={(e) =>
                              setEditForm({ ...editForm, price: e.target.value })
                            }
                          />
                        </div>
                      </>
                    )}
                    <div className="col-md-12">
                      <label className="form-label">نبذة</label>
                      <textarea
                        className="form-control"
                        rows={2}
                        value={editForm.bio}
                        onChange={(e) =>
                          setEditForm({ ...editForm, bio: e.target.value })
                        }
                      />
                    </div>
                  </div>
                </div>
                <div className="modal-footer">
                  <button type="submit" className="btn btn-primary">
                    حفظ التعديلات
                  </button>
                  <button
                    type="button"
                    className="btn btn-secondary"
                    onClick={() => setEditUser(null)}
                  >
                    إغلاق
                  </button>
                </div>
              </form>
            </div>
          </div>
        </div>
      )}

      {/* Delete User Modal */}
      {deleteUser && (
        <div
          className="modal fade show d-block"
          style={{ backgroundColor: "rgba(0,0,0,0.5)" }}
        >
          <div className="modal-dialog modal-dialog-centered">
            <div className="modal-content">
              <div className="modal-header bg-danger text-white">
                <h5 className="modal-title">تأكيد الحذف</h5>
                <button
                  type="button"
                  className="btn-close btn-close-white"
                  onClick={() => setDeleteUser(null)}
                ></button>
              </div>
              <div className="modal-body">
                <p className="mb-0">
                  هل أنت متأكد أنك تريد حذف المستخدم{" "}
                  <strong>{deleteUser.userName}</strong>؟
                </p>
              </div>
              <div className="modal-footer">
                <button
                  type="button"
                  className="btn btn-secondary"
                  onClick={() => setDeleteUser(null)}
                >
                  إلغاء
                </button>
                <button
                  type="button"
                  className="btn btn-danger"
                  onClick={handleDeleteUser}
                >
                  تأكيد الحذف
                </button>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
