"use client";

import React, { useEffect, useState } from "react";
import Link from "next/link";
import { usePathname } from "next/navigation";
import { useAuth } from "@/context/AuthContext";

export function Navbar() {
  const pathname = usePathname();
  const { user, isAuthenticated, isAdmin, isTechnician, isClient, isShop, logout } =
    useAuth();
  const [scrolled, setScrolled] = useState(false);
  const [mobileOpen, setMobileOpen] = useState(false);
  const [dropdownOpen, setDropdownOpen] = useState(false);

  // Admin panel uses its own dedicated layout
  if (pathname?.startsWith("/admin")) {
    return null;
  }

  const isHomePage = pathname === "/";

  useEffect(() => {
    const handleScroll = () => {
      setScrolled(window.scrollY > 50);
    };
    handleScroll();
    window.addEventListener("scroll", handleScroll);
    return () => window.removeEventListener("scroll", handleScroll);
  }, []);

  useEffect(() => {
    setMobileOpen(false);
    setDropdownOpen(false);
  }, [pathname]);

  const navClass = isHomePage
    ? `navbar-transparent ${scrolled ? "scrolled" : ""}`
    : "navbar-solid shadow-sm";

  return (
    <header>
      <nav
        className={`navbar navbar-expand-lg fixed-top sany3y-navbar ${navClass}`}
        id="mainNav"
      >
        <div className="container">
          <Link className="navbar-brand" href="/">
            <i className="fas fa-tools ms-2 text-warning"></i>Sany3y
          </Link>

          <button
            className="navbar-toggler border-0"
            type="button"
            onClick={() => setMobileOpen(!mobileOpen)}
            aria-label="Toggle navigation"
          >
            <i
              className={`fas ${mobileOpen ? "fa-times" : "fa-bars"} ${
                isHomePage && !scrolled ? "text-white" : "text-dark"
              } fs-4`}
            ></i>
          </button>

          <div
            className={`collapse navbar-collapse ${mobileOpen ? "show bg-white p-3 rounded-3 shadow mt-2" : ""}`}
            id="navbarContent"
          >
            <ul className="navbar-nav mx-auto mb-2 mb-lg-0">
              <li className="nav-item">
                <Link
                  className={`nav-link ${pathname === "/" ? "active" : ""}`}
                  href="/"
                >
                  الرئيسية
                </Link>
              </li>
              <li className="nav-item">
                <Link
                  className={`nav-link ${pathname?.startsWith("/services") ? "active" : ""}`}
                  href="/services"
                >
                  خدماتنا
                </Link>
              </li>
              <li className="nav-item">
                <Link
                  className={`nav-link ${pathname?.startsWith("/shops") ? "active" : ""}`}
                  href="/shops"
                >
                  المحلات
                </Link>
              </li>

              {isAdmin && (
                <li className="nav-item">
                  <Link className="nav-link" href="/admin">
                    لوحة التحكم
                  </Link>
                </li>
              )}

              {isAuthenticated && isClient && !isAdmin && (
                <li className="nav-item">
                  <Link
                    className={`nav-link ${pathname === "/dashboard/customer" ? "active" : ""}`}
                    href="/dashboard/customer"
                  >
                    لوحتي
                  </Link>
                </li>
              )}

              {isAuthenticated && isTechnician && (
                <li className="nav-item">
                  <Link
                    className={`nav-link ${
                      pathname?.startsWith("/dashboard/worker") ||
                      pathname?.startsWith("/dashboard/shop")
                        ? "active"
                        : ""
                    }`}
                    href={isShop ? "/dashboard/shop" : "/dashboard/worker"}
                  >
                    لوحتي
                  </Link>
                </li>
              )}

              <li className="nav-item">
                <Link
                  className={`nav-link ${pathname === "/contact" ? "active" : ""}`}
                  href="/contact"
                >
                  تواصل معنا
                </Link>
              </li>
            </ul>

            <ul className="navbar-nav align-items-lg-center">
              {isAuthenticated && user ? (
                <li className="nav-item dropdown position-relative">
                  <button
                    type="button"
                    className="nav-link dropdown-toggle fw-bold btn btn-link text-decoration-none border-0"
                    onClick={() => setDropdownOpen(!dropdownOpen)}
                  >
                    <i className="fas fa-user-circle ms-1"></i> {user.userName}
                  </button>
                  {dropdownOpen && (
                    <ul
                      className="dropdown-menu dropdown-menu-end show border-0 shadow"
                      style={{ position: "absolute", left: 0, right: "auto", minWidth: 180 }}
                    >
                      <li>
                        <Link className="dropdown-item" href="/profile">
                          <i className="fas fa-cog ms-2 text-muted"></i> الإعدادات
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
                          <i className="fas fa-sign-out-alt ms-2"></i> تسجيل الخروج
                        </button>
                      </li>
                    </ul>
                  )}
                </li>
              ) : (
                <>
                  <li className="nav-item ms-2 mb-2 mb-lg-0">
                    <Link
                      className="btn btn-outline-light rounded-pill px-4 nav-btn-login"
                      href="/login"
                    >
                      دخول
                    </Link>
                  </li>
                  <li className="nav-item ms-2">
                    <Link
                      className="btn btn-warning text-dark fw-bold rounded-pill px-4"
                      href="/register"
                    >
                      حساب جديد
                    </Link>
                  </li>
                </>
              )}
            </ul>
          </div>
        </div>
      </nav>
    </header>
  );
}
