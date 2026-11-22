"use client";

import React, { Suspense, useEffect, useState } from "react";
import Link from "next/link";
import { useSearchParams } from "next/navigation";
import { apiFetch } from "@/lib/api";
import { Category, TechnicianCardItem } from "@/types";
import { useGovernoratesAndCities } from "@/components/GovernorateCitySelect";

function ServicesDirectoryContent() {
  const searchParams = useSearchParams();
  const initialServiceType = searchParams.get("serviceType") || "";
  const initialCategoryId = searchParams.get("categoryId") || "";

  const { governorates, cities, loadCitiesForGovernorate } =
    useGovernoratesAndCities();
  const [categories, setCategories] = useState<Category[]>([]);

  const [categoryId, setCategoryId] = useState(initialCategoryId);
  const [governorate, setGovernorate] = useState("");
  const [city, setCity] = useState("");
  const [name, setName] = useState("");
  const [serviceType, setServiceType] = useState(initialServiceType);
  const [sortBy, setSortBy] = useState("");

  const [technicians, setTechnicians] = useState<TechnicianCardItem[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    apiFetch<Category[]>("/api/Category/GetAll")
      .then((data) => setCategories(data || []))
      .catch(() => setCategories([]));
  }, []);

  const fetchTechnicians = async (overrideParams?: {
    catId?: string;
    gov?: string;
    cty?: string;
    nm?: string;
    srvType?: string;
    srt?: string;
  }) => {
    setLoading(true);
    try {
      const params = new URLSearchParams();
      params.set("isShop", "false");

      const cId = overrideParams?.catId ?? categoryId;
      const g = overrideParams?.gov ?? governorate;
      const c = overrideParams?.cty ?? city;
      const n = overrideParams?.nm ?? name;
      const st = overrideParams?.srvType ?? serviceType;
      const s = overrideParams?.srt ?? sortBy;

      if (cId) params.set("categoryId", cId);
      if (g) params.set("governorate", g);
      if (c) params.set("city", c);
      if (n) params.set("name", n);
      if (st && !cId) params.set("serviceType", st);
      if (s) params.set("sortBy", s);

      const data = await apiFetch<TechnicianCardItem[]>(
        `/api/Technician/Search?${params.toString()}`
      );
      setTechnicians(data || []);
    } catch {
      setTechnicians([]);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    setServiceType(initialServiceType);
    setCategoryId(initialCategoryId);
    fetchTechnicians({
      catId: initialCategoryId,
      srvType: initialServiceType,
    });
  }, [initialServiceType, initialCategoryId]);

  const handleFilterSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    fetchTechnicians();
  };

  return (
    <div className="pt-5 pb-5 mt-4">
      {/* Top Search Section */}
      <div className="directory-search-section">
        <div className="container">
          <h2 className="mb-3 text-center fw-bold">
            احجز مع أفضل الصنايعية في مصر
          </h2>
          <form onSubmit={handleFilterSubmit}>
            <div className="directory-search-container">
              <div className="directory-search-item">
                <label className="directory-search-label">
                  <i className="fas fa-tools"></i> التخصص
                </label>
                <select
                  className="directory-search-select"
                  value={categoryId}
                  onChange={(e) => {
                    setCategoryId(e.target.value);
                    setServiceType("");
                  }}
                >
                  <option value="">اختر التخصص</option>
                  {categories.map((cat) => (
                    <option key={cat.id} value={cat.id}>
                      {cat.name}
                    </option>
                  ))}
                </select>
              </div>

              <div className="directory-search-item">
                <label className="directory-search-label">
                  <i className="fas fa-map-marker-alt"></i> المحافظة
                </label>
                <select
                  className="directory-search-select"
                  value={governorate}
                  onChange={(e) => {
                    const val = e.target.value;
                    setGovernorate(val);
                    setCity("");
                    loadCitiesForGovernorate(val);
                  }}
                >
                  <option value="">اختر المحافظة</option>
                  {governorates.map((gov) => (
                    <option key={gov.id} value={gov.arabicName}>
                      {gov.arabicName}
                    </option>
                  ))}
                </select>
              </div>

              <div className="directory-search-item">
                <label className="directory-search-label">
                  <i className="fas fa-map-pin"></i> المنطقة
                </label>
                <select
                  className="directory-search-select"
                  value={city}
                  onChange={(e) => setCity(e.target.value)}
                >
                  <option value="">اختر المنطقة</option>
                  {cities.map((c) => (
                    <option key={c.id} value={c.arabicName}>
                      {c.arabicName}
                    </option>
                  ))}
                </select>
              </div>

              <div className="directory-search-item">
                <label className="directory-search-label">
                  <i className="fas fa-user"></i> اسم الصنايعي
                </label>
                <input
                  type="text"
                  className="directory-search-select"
                  placeholder="ابحث بالاسم..."
                  style={{ borderBottom: "1px solid #eee" }}
                  value={name}
                  onChange={(e) => setName(e.target.value)}
                />
              </div>

              <button type="submit" className="directory-search-btn">
                <i className="fas fa-search ms-1"></i> ابحث
              </button>
            </div>
          </form>
        </div>
      </div>

      {/* Results List */}
      <div className="container">
        <div className="d-flex justify-content-between align-items-center mb-3">
          <h5 className="fw-bold mb-0">
            كل التخصصات{" "}
            <span className="text-muted fs-6">
              ({technicians.length} صنايعي)
            </span>
          </h5>
          <div className="d-flex align-items-center gap-2">
            <label className="small text-muted mb-0">الترتيب:</label>
            <select
              className="form-select form-select-sm w-auto"
              value={sortBy}
              onChange={(e) => {
                setSortBy(e.target.value);
                fetchTechnicians({ srt: e.target.value });
              }}
            >
              <option value="">الأكثر تطابقاً</option>
              <option value="rating">الأعلى تقييماً</option>
              <option value="price_asc">الأقل سعراً</option>
            </select>
          </div>
        </div>

        {loading ? (
          <div className="text-center py-5">
            <div className="spinner-border text-success" />
            <p className="mt-2 text-muted">جاري البحث عن الصنايعية...</p>
          </div>
        ) : technicians.length === 0 ? (
          <div className="alert alert-info text-center py-4">
            لا يوجد صنايعية متاحين حالياً في هذا التصنيف.
          </div>
        ) : (
          technicians.map((user) => (
            <div key={user.id} className="directory-card">
              <Link
                href={`/services/${user.id}`}
                className="directory-info-section"
              >
                <div className="d-flex gap-3 mb-3 align-items-center">
                  <img
                    src={
                      user.profilePictureUrl ||
                      "https://placehold.co/100x100?text=User"
                    }
                    alt={user.firstName}
                    style={{
                      width: 80,
                      height: 80,
                      borderRadius: "50%",
                      objectFit: "cover",
                      border: "2px solid #eee",
                    }}
                    onError={(e) => {
                      (e.target as HTMLImageElement).src =
                        "https://placehold.co/100x100?text=User";
                    }}
                  />
                  <div>
                    <h4
                      className="fw-bold mb-1 fs-5"
                      style={{ color: "var(--primary-green)" }}
                    >
                      {user.firstName} {user.lastName}
                    </h4>
                    <p className="text-muted small mb-1">
                      صنايعي عام ومحترف في الصيانة المنزلية
                    </p>
                    <div className="text-warning small">
                      <i className="fas fa-star"></i>
                      <i className="fas fa-star"></i>
                      <i className="fas fa-star"></i>
                      <i className="fas fa-star"></i>
                      <i className="fas fa-star-half-alt"></i>
                      <span className="text-dark me-1 fw-bold">
                        {user.rating || 4.5}
                      </span>
                      <span className="text-muted me-1">
                        ({user.reviewCount || 0} تقييم)
                      </span>
                    </div>
                  </div>
                </div>

                <div className="d-flex flex-column gap-2 small text-secondary">
                  <div>
                    <i
                      className="fas fa-tools ms-2"
                      style={{ color: "var(--primary-green)" }}
                    ></i>
                    متخصص في:{" "}
                    <span className="fw-bold text-dark">
                      {user.categoryName}
                    </span>
                  </div>
                  <div>
                    <i
                      className="fas fa-map-marker-alt ms-2"
                      style={{ color: "var(--primary-green)" }}
                    ></i>
                    العنوان: {user.governorate}، {user.city}، {user.street}
                  </div>
                  <div>
                    <i
                      className="fas fa-money-bill-alt ms-2"
                      style={{ color: "var(--primary-green)" }}
                    ></i>
                    سعر المعاينة:{" "}
                    <span className="fw-bold text-success">
                      {user.price} ج.م
                    </span>
                  </div>
                  <div>
                    <i
                      className="fas fa-phone-alt ms-2"
                      style={{ color: "var(--primary-green)" }}
                    ></i>
                    {user.phoneNumber} - سعر مكالمة عادية
                  </div>
                </div>
              </Link>

              <div className="directory-action-section">
                <div className="text-center mb-2 text-muted small">
                  <i className="fas fa-bolt text-warning"></i> حجز فوري
                </div>
                <Link
                  href={`/services/${user.id}`}
                  className="btn btn-danger fw-bold w-100 px-4"
                >
                  احجز الآن
                </Link>
                <div
                  className="mt-2 text-muted small"
                  style={{ fontSize: "0.75rem" }}
                >
                  متاح الدفع أونلاين
                </div>
              </div>
            </div>
          ))
        )}
      </div>
    </div>
  );
}

export default function ServicesPage() {
  return (
    <Suspense
      fallback={
        <div className="container py-5 mt-5 text-center">جاري التحميل...</div>
      }
    >
      <ServicesDirectoryContent />
    </Suspense>
  );
}
