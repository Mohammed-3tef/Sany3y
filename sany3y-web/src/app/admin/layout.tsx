"use client";

import React, { useEffect, useState } from "react";
import Link from "next/link";
import { usePathname, useRouter } from "next/navigation";
import { useAuth } from "@/context/AuthContext";
import { downloadApiFile } from "@/lib/api";
import { toast } from "sonner";

export default function AdminLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  const pathname = usePathname();
  const router = useRouter();
  const { user, isAuthenticated, isAdmin, isLoading, logout } = useAuth();
  const [sidebarOpen, setSidebarOpen] = useState(false);
  const [dropdownOpen, setDropdownOpen] = useState(false);

  useEffect(() => {
    if (!isLoading) {
      if (!isAuthenticated) {
        router.push("/login");
      } else if (!isAdmin) {
        router.push("/forbidden");
      }
    }
  }, [isLoading, isAuthenticated, isAdmin]);

  if (isLoading || !isAuthenticated || !isAdmin) {
    return (
      <div className="container py-5 text-center">
        <div className="spinner-border text-success" />
        <p className="mt-2 text-muted">جاري التحقق من صلاحيات المسؤول...</p>
      </div>
    );
  }

  return (
    <div>
      {/* Sidebar */}
      <aside
        className={`admin-sidebar ${sidebarOpen ? "active" : ""}`}
        id="adminSidebar"
      >
        <div
          className="d-flex align-items-center px-4 border-bottom border-secondary border-opacity-25"
          style={{ height: 70 }}
        >
          <Link
            href="/"
            className="text-white text-decoration-none fw-bolder fs-4 d-flex align-items-center gap-2"
          >
            <i className="fas fa-tools text-warning"></i>
            <span>Sany3y Admin</span>
          </Link>
          <button
            type="button"
            className="btn text-white d-lg-none me-auto"
            onClick={() => setSidebarOpen(false)}
          >
            <i className="fas fa-times"></i>
          </button>
        </div>

        <div className="py-3">
          <nav className="nav flex-column">
            <div
              className="px-4 mb-2 text-uppercase text-white-50 small fw-bold"
              style={{ fontSize: "0.75rem", letterSpacing: 1 }}
            >
              الرئيسية
            </div>

            <Link
              href="/admin"
              className={`nav-link ${pathname === "/admin" ? "active" : ""}`}
            >
              <i className="fas fa-th-large"></i>
              <span>لوحة التحكم</span>
            </Link>

            <div
              className="px-4 mt-4 mb-2 text-uppercase text-white-50 small fw-bold"
              style={{ fontSize: "0.75rem", letterSpacing: 1 }}
            >
              الإدارة
            </div>

            <Link
              href="/admin/users"
              className={`nav-link ${
                pathname === "/admin/users" ? "active" : ""
              }`}
            >
              <i className="fas fa-users"></i>
              <span>المستخدمين</span>
            </Link>

            <Link
              href="/admin/categories"
              className={`nav-link ${
                pathname === "/admin/categories" ? "active" : ""
              }`}
            >
              <i className="fas fa-tags"></i>
              <span>التصنيفات</span>
            </Link>

            <Link
              href="/admin/shops"
              className={`nav-link ${
                pathname === "/admin/shops" ? "active" : ""
              }`}
            >
              <i className="fas fa-store"></i>
              <span>المحلات</span>
            </Link>

            <div
              className="px-4 mt-4 mb-2 text-uppercase text-white-50 small fw-bold"
              style={{ fontSize: "0.75rem", letterSpacing: 1 }}
            >
              التقارير
            </div>

            <button
              type="button"
              onClick={() =>
                downloadApiFile(
                  "/api/Admin/export/users/pdf",
                  "Sany3y_Users.pdf"
                ).catch(() => toast.error("فشل تصدير ملف PDF"))
              }
              className="nav-link btn btn-link text-start border-0 w-100"
            >
              <i className="fas fa-file-pdf"></i>
              <span>تقرير المستخدمين (PDF)</span>
            </button>

            <button
              type="button"
              onClick={() =>
                downloadApiFile(
                  "/api/Admin/export/categories/csv",
                  "Sany3y_Categories.csv"
                ).catch(() => toast.error("فشل تصدير ملف CSV"))
              }
              className="nav-link btn btn-link text-start border-0 w-100"
            >
              <i className="fas fa-file-csv"></i>
              <span>تقرير التصنيفات (CSV)</span>
            </button>
          </nav>
        </div>
      </aside>

      {/* Main Content */}
      <div className="admin-main">
        <header
          className="bg-white border-bottom d-flex align-items-center justify-content-between px-4 sticky-top"
          style={{ height: 70 }}
        >
          <button
            type="button"
            className="btn btn-link text-dark d-lg-none"
            onClick={() => setSidebarOpen(!sidebarOpen)}
          >
            <i className="fas fa-bars fa-lg"></i>
          </button>

          <div className="d-flex align-items-center gap-3 me-auto position-relative">
            <button
              type="button"
              onClick={() => setDropdownOpen(!dropdownOpen)}
              className="btn btn-link d-flex align-items-center text-dark text-decoration-none dropdown-toggle border-0"
            >
              <div
                className="bg-light rounded-circle d-flex align-items-center justify-content-center ms-2"
                style={{ width: 40, height: 40 }}
              >
                <i className="fas fa-user"></i>
              </div>
              <span className="fw-bold d-none d-md-block">
                {user?.userName || "Admin"}
              </span>
            </button>

            {dropdownOpen && (
              <ul
                className="dropdown-menu show border-0 shadow"
                style={{ position: "absolute", top: "100%", left: 0 }}
              >
                <li>
                  <Link className="dropdown-item" href="/">
                    العودة للموقع
                  </Link>
                </li>
                <li>
                  <hr className="dropdown-divider" />
                </li>
                <li>
                  <button
                    type="button"
                    onClick={logout}
                    className="dropdown-item text-danger"
                  >
                    تسجيل الخروج
                  </button>
                </li>
              </ul>
            )}
          </div>
        </header>

        <div className="p-4">{children}</div>
      </div>
    </div>
  );
}
