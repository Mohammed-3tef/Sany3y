"use client";

import React, { useState } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { apiFetch } from "@/lib/api";
import { useAuth } from "@/context/AuthContext";
import { toast } from "sonner";

export default function LoginPage() {
  const router = useRouter();
  const { setSession } = useAuth();
  const [userName, setUserName] = useState("");
  const [password, setPassword] = useState("");
  const [rememberMe, setRememberMe] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  // Google Quick Sign-In Modal State
  const [showGoogleModal, setShowGoogleModal] = useState(false);
  const [googleEmail, setGoogleEmail] = useState("");
  const [googleFirstName, setGoogleFirstName] = useState("");
  const [googleLastName, setGoogleLastName] = useState("");
  const [googleLoading, setGoogleLoading] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setLoading(true);

    try {
      const res = await apiFetch<{ token: string; user: any }>("/api/Auth/login", {
        method: "POST",
        body: JSON.stringify({ userName, password, rememberMe }),
      });

      setSession(res.token, res.user);
      toast.success(`مرحباً بعودتك، ${res.user.firstName}!`);

      if (res.user.roles?.includes("Admin") || res.user.role === "Admin") {
        router.push("/admin");
      } else {
        router.push("/");
      }
    } catch (err: any) {
      if (err.status === 403 && err.data?.requiresEmailConfirmation) {
        toast.info(err.message);
        router.push(
          `/confirm-email-notice?email=${encodeURIComponent(err.data.email || "")}`
        );
        return;
      }
      if (err.status === 423 && err.data?.isLockedOut) {
        router.push("/lockout");
        return;
      }
      setError(err.message || "اسم المستخدم أو كلمة المرور غير صحيحة.");
    } finally {
      setLoading(false);
    }
  };

  const handleGoogleSignIn = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!googleEmail) return;
    setGoogleLoading(true);
    try {
      const res = await apiFetch<any>("/api/Auth/google", {
        method: "POST",
        body: JSON.stringify({
          email: googleEmail,
          firstName: googleFirstName,
          lastName: googleLastName,
        }),
      });

      if (!res.isNewUser && res.token && res.user) {
        setSession(res.token, res.user);
        setShowGoogleModal(false);
        toast.success("تم تسجيل الدخول باستخدام جوجل بنجاح!");
        if (res.isProfileIncomplete) {
          router.push("/profile");
        } else {
          router.push("/");
        }
      } else if (res.isNewUser && res.prefill) {
        sessionStorage.setItem("sany3y_google_prefill", JSON.stringify(res.prefill));
        setShowGoogleModal(false);
        router.push("/complete-profile");
      }
    } catch (err: any) {
      toast.error(err.message || "فشل تسجيل الدخول عبر جوجل.");
    } finally {
      setGoogleLoading(false);
    }
  };

  return (
    <div className="container-fluid p-0 overflow-hidden bg-white" style={{ minHeight: "100vh" }}>
      <div className="row g-0" style={{ minHeight: "100vh" }}>
        {/* Right Side: Image */}
        <div
          className="col-lg-7 d-none d-lg-block position-relative"
          style={{
            backgroundImage: "url('/img/login-bg.png')",
            backgroundSize: "cover",
            backgroundPosition: "center",
            minHeight: "100vh",
          }}
        >
          <div
            style={{
              position: "absolute",
              inset: 0,
              background: "linear-gradient(135deg, rgba(0,0,0,0.6), rgba(0,100,0,0.4))",
            }}
          />
          <div
            style={{
              position: "absolute",
              bottom: 50,
              right: 50,
              color: "white",
              zIndex: 1,
            }}
          >
            <h1 className="display-4 fw-bold mb-3">مرحباً بك في صنايعي</h1>
            <p className="lead opacity-75">
              منصتك الأولى لخدمات الصيانة المنزلية الموثوقة
            </p>
          </div>
        </div>

        {/* Left Side: Form */}
        <div className="col-lg-5 d-flex align-items-center justify-content-center p-4 p-md-5">
          <div className="w-100" style={{ maxWidth: 450 }}>
            <div className="text-center mb-4">
              <Link
                href="/"
                className="text-decoration-none fw-bolder fs-2 d-inline-block mb-3"
                style={{ color: "var(--primary-green)" }}
              >
                <i className="fas fa-tools ms-2 text-warning"></i>Sany3y
              </Link>
              <h3 className="fw-bold mb-2">تسجيل الدخول</h3>
              <p className="text-muted">أهلاً بعودتك! الرجاء إدخال بياناتك</p>
            </div>

            {error && (
              <div className="alert alert-danger mb-4" role="alert">
                <i className="fas fa-exclamation-circle ms-2"></i>
                {error}
              </div>
            )}

            <form onSubmit={handleSubmit}>
              <div className="mb-3">
                <label className="form-label">اسم المستخدم</label>
                <input
                  type="text"
                  className="form-control form-control-lg bg-light"
                  placeholder="أدخل اسم المستخدم أو البريد الإلكتروني"
                  value={userName}
                  onChange={(e) => setUserName(e.target.value)}
                  required
                />
              </div>

              <div className="mb-3">
                <label className="form-label">كلمة المرور</label>
                <input
                  type="password"
                  className="form-control form-control-lg bg-light"
                  placeholder="أدخل كلمة المرور"
                  value={password}
                  onChange={(e) => setPassword(e.target.value)}
                  required
                />
              </div>

              <div className="d-flex justify-content-between align-items-center mb-4">
                <div className="form-check">
                  <input
                    className="form-check-input"
                    type="checkbox"
                    id="rememberMe"
                    checked={rememberMe}
                    onChange={(e) => setRememberMe(e.target.checked)}
                  />
                  <label className="form-check-label text-muted" htmlFor="rememberMe">
                    تذكرني
                  </label>
                </div>
                <Link
                  href="/forgot-password"
                  className="text-success text-decoration-none small fw-bold"
                >
                  نسيت كلمة المرور؟
                </Link>
              </div>

              <button
                type="submit"
                disabled={loading}
                className="btn btn-primary w-100 py-3 fw-bold fs-5 mb-3"
              >
                {loading ? (
                  <>
                    <span className="spinner-border spinner-border-sm ms-2"></span>
                    جاري تسجيل الدخول...
                  </>
                ) : (
                  "تسجيل الدخول"
                )}
              </button>
            </form>

            <div className="d-flex align-items-center my-4 text-muted">
              <div className="flex-grow-1 border-bottom"></div>
              <span className="px-3 small">أو</span>
              <div className="flex-grow-1 border-bottom"></div>
            </div>

            <button
              type="button"
              onClick={() => setShowGoogleModal(true)}
              className="btn btn-outline-secondary w-100 py-2 fw-bold mb-4 bg-white text-dark border"
            >
              <img
                src="https://www.svgrepo.com/show/475656/google-color.svg"
                alt="Google"
                width="20"
                className="ms-2"
              />
              الدخول باستخدام جوجل
            </button>

            <div className="text-center">
              <p className="text-muted mb-0">
                ليس لديك حساب؟{" "}
                <Link
                  href="/register"
                  className="text-success fw-bold text-decoration-none"
                >
                  أنشئ حسابك الآن
                </Link>
              </p>
            </div>
          </div>
        </div>
      </div>

      {/* Google OAuth Modal */}
      {showGoogleModal && (
        <div
          className="modal fade show d-block"
          style={{ backgroundColor: "rgba(0,0,0,0.5)" }}
        >
          <div className="modal-dialog modal-dialog-centered">
            <div className="modal-content">
              <div className="modal-header">
                <h5 className="modal-title">
                  <i className="fa-brands fa-google ms-2"></i> الدخول باستخدام حساب جوجل
                </h5>
                <button
                  type="button"
                  className="btn-close btn-close-white"
                  onClick={() => setShowGoogleModal(false)}
                ></button>
              </div>
              <form onSubmit={handleGoogleSignIn}>
                <div className="modal-body">
                  <div className="mb-3">
                    <label className="form-label">البريد الإلكتروني لحساب جوجل</label>
                    <input
                      type="email"
                      className="form-control"
                      placeholder="yourname@gmail.com"
                      value={googleEmail}
                      onChange={(e) => setGoogleEmail(e.target.value)}
                      required
                    />
                  </div>
                  <div className="row">
                    <div className="col-6 mb-3">
                      <label className="form-label">الاسم الأول (للحسابات الجديدة)</label>
                      <input
                        type="text"
                        className="form-control"
                        value={googleFirstName}
                        onChange={(e) => setGoogleFirstName(e.target.value)}
                      />
                    </div>
                    <div className="col-6 mb-3">
                      <label className="form-label">الاسم الأخير (للحسابات الجديدة)</label>
                      <input
                        type="text"
                        className="form-control"
                        value={googleLastName}
                        onChange={(e) => setGoogleLastName(e.target.value)}
                      />
                    </div>
                  </div>
                </div>
                <div className="modal-footer">
                  <button
                    type="button"
                    className="btn btn-secondary"
                    onClick={() => setShowGoogleModal(false)}
                  >
                    إلغاء
                  </button>
                  <button
                    type="submit"
                    disabled={googleLoading}
                    className="btn btn-primary"
                  >
                    {googleLoading ? "جاري المتابعة..." : "متابعة"}
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
