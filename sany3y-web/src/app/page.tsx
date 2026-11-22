"use client";

import React, { useEffect, useState } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { apiFetch } from "@/lib/api";
import { Category, TechnicianCardItem } from "@/types";

export default function HomePage() {
  const router = useRouter();
  const [searchQuery, setSearchQuery] = useState("");
  const [categories, setCategories] = useState<Category[]>([]);
  const [selectedCategory, setSelectedCategory] = useState<number>(0);
  const [technicians, setTechnicians] = useState<TechnicianCardItem[]>([]);
  const [loadingTechs, setLoadingTechs] = useState(true);

  useEffect(() => {
    apiFetch<Category[]>("/api/Category/GetAll")
      .then((data) => setCategories((data || []).slice(0, 8)))
      .catch(() => setCategories([]));
  }, []);

  useEffect(() => {
    setLoadingTechs(true);
    const query =
      selectedCategory > 0
        ? `/api/Technician/Search?isShop=false&categoryId=${selectedCategory}`
        : `/api/Technician/Search?isShop=false`;

    apiFetch<TechnicianCardItem[]>(query)
      .then((data) => setTechnicians((data || []).slice(0, 8)))
      .catch(() => setTechnicians([]))
      .finally(() => setLoadingTechs(false));
  }, [selectedCategory]);

  const handleSearchSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (searchQuery.trim()) {
      router.push(
        `/services?serviceType=${encodeURIComponent(searchQuery.trim())}`
      );
    } else {
      router.push("/services");
    }
  };

  return (
    <div className="pb-3">
      {/* Hero Section */}
      <section className="hero-section text-white text-center">
        <div className="hero-content">
          <h1 className="hero-title">دور على صنايعي في ثواني</h1>
          <p className="hero-subtitle">
            كل الخدمات في مكان واحد — سباكة، نجارة، كهرباء وغيرهم
          </p>

          <form className="mt-5" onSubmit={handleSearchSubmit}>
            <div className="hero-search-box">
              <i className="fas fa-search text-white align-self-center me-3 opacity-75"></i>
              <input
                type="text"
                className="hero-search-input"
                placeholder="ابحث عن خدمة (مثال: سباكة، كهرباء...)"
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
              />
              <button type="submit" className="hero-search-btn">
                ابحث الآن
              </button>
            </div>
          </form>
        </div>
      </section>

      {/* Categories & Technicians */}
      <section className="py-5">
        <div className="container">
          <div className="section-header">
            <h2 className="section-title">تصفح حسب التخصص</h2>
          </div>

          <div className="d-flex justify-content-center flex-wrap mb-5 gap-2">
            <button
              type="button"
              className={`filter-btn ${selectedCategory === 0 ? "active" : ""}`}
              onClick={() => setSelectedCategory(0)}
            >
              الكل
            </button>
            {categories.map((cat) => (
              <button
                key={cat.id}
                type="button"
                className={`filter-btn ${
                  selectedCategory === cat.id ? "active" : ""
                }`}
                onClick={() => setSelectedCategory(cat.id)}
              >
                {cat.name}
              </button>
            ))}
          </div>

          {loadingTechs ? (
            <div className="text-center py-5">
              <div className="spinner-border text-success" role="status" />
              <p className="mt-2 text-muted">جاري تحميل الفنيين...</p>
            </div>
          ) : technicians.length === 0 ? (
            <div className="alert alert-light text-center border py-4">
              لا يوجد فنيين في هذا التخصص حالياً.
            </div>
          ) : (
            <div className="row g-4">
              {technicians.map((t) => (
                <div key={t.id} className="col-lg-3 col-md-4 col-sm-6">
                  <div className="home-tech-card h-100 d-flex flex-column">
                    <div className="home-tech-img-wrapper">
                      <img
                        src={
                          t.profilePictureUrl ||
                          "https://placehold.co/300x200?text=Technician"
                        }
                        alt={`${t.firstName} ${t.lastName}`}
                        className="home-tech-img"
                        onError={(e) => {
                          (e.target as HTMLImageElement).src =
                            "https://placehold.co/300x200?text=Technician";
                        }}
                      />
                      <div className="home-tech-overlay">
                        <span className="badge bg-warning text-dark">
                          {t.categoryName}
                        </span>
                      </div>
                    </div>
                    <div className="card-body p-3 text-center d-flex flex-column flex-grow-1">
                      <h5 className="fw-bold mb-1">
                        {t.firstName} {t.lastName}
                      </h5>
                      <p className="text-muted small mb-3">
                        <i className="fas fa-map-marker-alt text-danger ms-1"></i>
                        {t.governorate} - {t.city}
                      </p>
                      <div className="mt-auto">
                        <Link
                          href={`/services/${t.id}`}
                          className="btn btn-outline-success btn-sm w-100 rounded-pill fw-bold"
                        >
                          عرض الملف الشخصي
                        </Link>
                      </div>
                    </div>
                  </div>
                </div>
              ))}
            </div>
          )}

          <div className="text-center mt-5">
            <Link
              href="/services"
              className="btn btn-primary rounded-pill px-5 py-2 fw-bold"
            >
              عرض جميع الفنيين
            </Link>
          </div>
        </div>
      </section>

      {/* How it Works */}
      <section className="py-5 bg-light">
        <div className="container">
          <div className="section-header">
            <h2 className="section-title">كيف تطلب صنايعي؟</h2>
          </div>

          <div className="row g-4">
            <div className="col-md-4">
              <div className="step-card">
                <div className="step-icon-box">
                  <i className="fas fa-search fa-2x text-primary"></i>
                </div>
                <h5
                  className="fw-bold mb-3"
                  style={{ color: "var(--primary-green)" }}
                >
                  1. ابحث عن الخدمة
                </h5>
                <p className="text-muted mb-0">
                  اختر نوع الخدمة التي تحتاجها من بين مجموعة واسعة من التخصصات.
                </p>
              </div>
            </div>
            <div className="col-md-4">
              <div className="step-card">
                <div className="step-icon-box">
                  <i className="fas fa-user-check fa-2x text-warning"></i>
                </div>
                <h5
                  className="fw-bold mb-3"
                  style={{ color: "var(--primary-green)" }}
                >
                  2. اختر الصنايعي
                </h5>
                <p className="text-muted mb-0">
                  قارن بين الفنيين بناءً على التقييمات والأسعار واختر الأنسب لك.
                </p>
              </div>
            </div>
            <div className="col-md-4">
              <div className="step-card">
                <div className="step-icon-box">
                  <i className="fas fa-calendar-check fa-2x text-success"></i>
                </div>
                <h5
                  className="fw-bold mb-3"
                  style={{ color: "var(--primary-green)" }}
                >
                  3. احجز الموعد
                </h5>
                <p className="text-muted mb-0">
                  تواصل مع الفني وحدد الموعد المناسب لإتمام المهمة بسهولة وأمان.
                </p>
              </div>
            </div>
          </div>
        </div>
      </section>

      {/* Testimonials */}
      <section className="py-5">
        <div className="container">
          <div className="section-header text-center mb-4">
            <h2 className="section-title">آراء عملائنا</h2>
          </div>

          <div className="row g-4">
            {[
              {
                name: "محمد علي",
                role: "عميل مميز - القاهرة",
                img: "https://randomuser.me/api/portraits/men/10.jpg",
                comment:
                  "تجربة ممتازة جداً، الفني وصل في الموعد وأنجز العمل باحترافية عالية. أنصح الجميع باستخدام الموقع.",
              },
              {
                name: "أحمد حسن",
                role: "عميل مميز - الجيزة",
                img: "https://randomuser.me/api/portraits/men/12.jpg",
                comment:
                  "سهولة في الحجز والدفع أونلاين، وأسعار واضحة ومناسبة جداً مقارنة بالسوق.",
              },
              {
                name: "كريم محمود",
                role: "عميل مميز - الإسكندرية",
                img: "https://randomuser.me/api/portraits/men/14.jpg",
                comment:
                  "منصة موثوقة والتواصل المباشر مع الصنايعي عبر الشات وفر علي وقت ومجهود كبير.",
              },
            ].map((item, idx) => (
              <div key={idx} className="col-md-4">
                <div className="bg-white text-center p-4 border rounded-4 shadow-sm h-100">
                  <img
                    src={item.img}
                    className="rounded-circle mb-3"
                    width="80"
                    height="80"
                    alt={item.name}
                  />
                  <h5 className="fw-bold mb-1">{item.name}</h5>
                  <p className="text-muted small mb-3">{item.role}</p>
                  <p className="fst-italic text-muted">"{item.comment}"</p>
                  <div className="text-warning">
                    <i className="fas fa-star"></i>
                    <i className="fas fa-star"></i>
                    <i className="fas fa-star"></i>
                    <i className="fas fa-star"></i>
                    <i className="fas fa-star"></i>
                  </div>
                </div>
              </div>
            ))}
          </div>
        </div>
      </section>
    </div>
  );
}
