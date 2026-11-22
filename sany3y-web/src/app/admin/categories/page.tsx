"use client";

import React, { useEffect, useState } from "react";
import { apiFetch, downloadApiFile } from "@/lib/api";
import { Category } from "@/types";
import { toast } from "sonner";

export default function AdminCategoriesPage() {
  const [categories, setCategories] = useState<Category[]>([]);
  const [loading, setLoading] = useState(true);
  const [search, setSearch] = useState("");

  // Create Modal
  const [showCreateModal, setShowCreateModal] = useState(false);
  const [newName, setNewName] = useState("");
  const [newDescription, setNewDescription] = useState("");

  // Edit Modal
  const [editCat, setEditCat] = useState<Category | null>(null);
  const [editName, setEditName] = useState("");
  const [editDescription, setEditDescription] = useState("");

  // Delete Modal
  const [deleteCat, setDeleteCat] = useState<Category | null>(null);

  const loadCategories = async () => {
    setLoading(true);
    try {
      const data = await apiFetch<Category[]>("/api/Category/GetAll");
      setCategories(data || []);
    } catch {
      toast.error("فشل تحميل التصنيفات");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadCategories();
  }, []);

  const handleCreate = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      await apiFetch("/api/Category/Create", {
        method: "POST",
        body: JSON.stringify({ name: newName, description: newDescription }),
      });
      toast.success("تم إنشاء التصنيف بنجاح");
      setShowCreateModal(false);
      setNewName("");
      setNewDescription("");
      loadCategories();
    } catch (err: any) {
      toast.error(err.message || "فشل إنشاء التصنيف");
    }
  };

  const handleUpdate = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!editCat) return;
    try {
      await apiFetch(`/api/Category/Update/${editCat.id}`, {
        method: "PUT",
        body: JSON.stringify({
          id: editCat.id,
          name: editName,
          description: editDescription,
        }),
      });
      toast.success("تم تحديث التصنيف بنجاح");
      setEditCat(null);
      loadCategories();
    } catch (err: any) {
      toast.error(err.message || "فشل تحديث التصنيف");
    }
  };

  const handleDelete = async () => {
    if (!deleteCat) return;
    try {
      await apiFetch(`/api/Category/Delete/${deleteCat.id}`, {
        method: "DELETE",
      });
      toast.success("تم حذف التصنيف بنجاح");
      setDeleteCat(null);
      loadCategories();
    } catch (err: any) {
      toast.error(err.message || "فشل حذف التصنيف");
    }
  };

  const filtered = categories.filter(
    (c) =>
      !search.trim() ||
      c.name?.toLowerCase().includes(search.toLowerCase()) ||
      c.description?.toLowerCase().includes(search.toLowerCase())
  );

  return (
    <div>
      <div className="d-flex flex-column flex-lg-row justify-content-between align-items-center mb-4">
        <h2 className="fw-bold mb-3 mb-lg-0">إدارة التصنيفات</h2>

        <div className="d-flex flex-wrap gap-2">
          <button
            type="button"
            className="btn btn-outline-primary"
            onClick={() => setShowCreateModal(true)}
          >
            <i className="fa-solid fa-plus ms-1"></i> إنشاء تصنيف جديد
          </button>
          <button
            type="button"
            onClick={() =>
              downloadApiFile(
                "/api/Admin/export/categories/pdf",
                "Sany3y_Categories.pdf"
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
                "/api/Admin/export/categories/csv",
                "Sany3y_Categories.csv"
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
              placeholder="بحث في التصنيفات..."
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
                  <th>رقم</th>
                  <th>الاسم</th>
                  <th>الوصف</th>
                  <th>الإجراءات</th>
                </tr>
              </thead>
              <tbody>
                {filtered.map((item) => (
                  <tr key={item.id}>
                    <td>{item.id}</td>
                    <td className="fw-bold">{item.name}</td>
                    <td>
                      {item.description || (
                        <span className="text-muted">لا يوجد وصف</span>
                      )}
                    </td>
                    <td>
                      <div className="d-flex justify-content-center gap-2">
                        <button
                          type="button"
                          className="btn btn-outline-warning btn-sm"
                          onClick={() => {
                            setEditCat(item);
                            setEditName(item.name);
                            setEditDescription(item.description || "");
                          }}
                        >
                          <i className="fa-solid fa-pen"></i>
                        </button>
                        <button
                          type="button"
                          className="btn btn-outline-danger btn-sm"
                          onClick={() => setDeleteCat(item)}
                        >
                          <i className="fa-solid fa-trash"></i>
                        </button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>

      {/* Create Modal */}
      {showCreateModal && (
        <div
          className="modal fade show d-block"
          style={{ backgroundColor: "rgba(0,0,0,0.5)" }}
        >
          <div className="modal-dialog modal-dialog-centered">
            <div className="modal-content">
              <div className="modal-header bg-primary text-white">
                <h5 className="modal-title">إنشاء تصنيف جديد</h5>
                <button
                  type="button"
                  className="btn-close btn-close-white"
                  onClick={() => setShowCreateModal(false)}
                ></button>
              </div>
              <form onSubmit={handleCreate}>
                <div className="modal-body">
                  <div className="mb-3">
                    <label className="form-label">اسم التصنيف</label>
                    <input
                      type="text"
                      className="form-control"
                      value={newName}
                      onChange={(e) => setNewName(e.target.value)}
                      required
                    />
                  </div>
                  <div className="mb-3">
                    <label className="form-label">الوصف</label>
                    <textarea
                      className="form-control"
                      rows={3}
                      value={newDescription}
                      onChange={(e) => setNewDescription(e.target.value)}
                    />
                  </div>
                </div>
                <div className="modal-footer">
                  <button
                    type="button"
                    className="btn btn-secondary"
                    onClick={() => setShowCreateModal(false)}
                  >
                    إلغاء
                  </button>
                  <button type="submit" className="btn btn-primary">
                    إنشاء
                  </button>
                </div>
              </form>
            </div>
          </div>
        </div>
      )}

      {/* Edit Modal */}
      {editCat && (
        <div
          className="modal fade show d-block"
          style={{ backgroundColor: "rgba(0,0,0,0.5)" }}
        >
          <div className="modal-dialog modal-dialog-centered">
            <div className="modal-content">
              <div className="modal-header bg-warning text-dark">
                <h5 className="modal-title">تعديل التصنيف</h5>
                <button
                  type="button"
                  className="btn-close"
                  onClick={() => setEditCat(null)}
                ></button>
              </div>
              <form onSubmit={handleUpdate}>
                <div className="modal-body">
                  <div className="mb-3">
                    <label className="form-label">اسم التصنيف</label>
                    <input
                      type="text"
                      className="form-control"
                      value={editName}
                      onChange={(e) => setEditName(e.target.value)}
                      required
                    />
                  </div>
                  <div className="mb-3">
                    <label className="form-label">الوصف</label>
                    <textarea
                      className="form-control"
                      rows={3}
                      value={editDescription}
                      onChange={(e) => setEditDescription(e.target.value)}
                    />
                  </div>
                </div>
                <div className="modal-footer">
                  <button
                    type="button"
                    className="btn btn-secondary"
                    onClick={() => setEditCat(null)}
                  >
                    إلغاء
                  </button>
                  <button type="submit" className="btn btn-warning">
                    حفظ التعديل
                  </button>
                </div>
              </form>
            </div>
          </div>
        </div>
      )}

      {/* Delete Modal */}
      {deleteCat && (
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
                  onClick={() => setDeleteCat(null)}
                ></button>
              </div>
              <div className="modal-body">
                <p className="mb-0">
                  هل أنت متأكد أنك تريد حذف التصنيف{" "}
                  <strong>{deleteCat.name}</strong>؟
                </p>
              </div>
              <div className="modal-footer">
                <button
                  type="button"
                  className="btn btn-secondary"
                  onClick={() => setDeleteCat(null)}
                >
                  إلغاء
                </button>
                <button
                  type="button"
                  className="btn btn-danger"
                  onClick={handleDelete}
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
