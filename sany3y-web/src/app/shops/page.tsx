"use client";

import React, { useEffect, useState } from "react";
import Link from "next/link";
import { apiFetch } from "@/lib/api";
import { Category, TechnicianCardItem } from "@/types";
import { useGovernoratesAndCities } from "@/components/GovernorateCitySelect";
import { useSignalR } from "@/context/SignalRContext";

export default function ShopsDirectoryPage() {
  const { governorates, cities, loadCitiesForGovernorate } =
    useGovernoratesAndCities();
  const { onlineUsersById, onlineUsersByName } = useSignalR();
  const [categories, setCategories] = useState<Category[]>([]);

  const [categoryId, setCategoryId] = useState("");
  const [governorate, setGovernorate] = useState("");
  const [city, setCity] = useState("");
  const [name, setName] = useState("");

  const [shops, setShops] = useState<TechnicianCardItem[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    apiFetch<Category[]>("/api/Category/GetAll")
      .then((data) => setCategories(data || []))
      .catch(() => setCategories([]));
  }, []);

  const fetchShops = async () => {
    setLoading(true);
    try {
      const params = new URLSearchParams();
      params.set("isShop", "true");
      if (categoryId) params.set("categoryId", categoryId);
      if (governorate) params.set("governorate", governorate);
      if (city) params.set("city", city);
      if (name) params.set("name", name);

      const data = await apiFetch<TechnicianCardItem[]>(
        `/api/Technician/Search?${params.toString()}`
      );
      setShops(data || []);
    } catch {
      setShops([]);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchShops();
  }, []);

  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault();
    fetchShops();
  };

  return (
    <div className="pt-5 pb-5 mt-4">
      <div className="directory-search-section">
        <div className="container">
          <h2 className="mb-3 text-center fw-bold">
            استعرض أفضل المحلات في مصر
          </h2>
          <form onSubmit={handleSearch}>
            <div className="directory-search-container">
              <div className="directory-search-item">
                <label className="directory-search-label">
                  <i className="fas fa-store"></i> التخصص
                </label>
                <select
                  className="directory-search-select"
                  value={categoryId}
                  onChange={(e) => setCategoryId(e.target.value)}
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
                  <i className="fas fa-user"></i> اسم المحل
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

      <div className="container">
        <div className="d-flex justify-content-between align-items-center mb-3">
          <h5 className="fw-bold">
            كل التخصصات{" "}
            <span className="text-muted fs-6">({shops.length} محل)</span>
          </h5>
        </div>

        {loading ? (
          <div className="text-center py-5">
            <div className="spinner-border text-success" />
            <p className="mt-2 text-muted">جاري تحميل المحلات...</p>
          </div>
        ) : shops.length === 0 ? (
          <div className="alert alert-info text-center py-4">
            لا يوجد محلات متاحة حالياً في هذا التصنيف.
          </div>
        ) : (
          shops.map((shop) => {
            const isOnline =
              onlineUsersById[shop.id] ??
              onlineUsersByName[shop.userName] ??
              shop.isOnline;

            return (
              <div key={shop.id} className="directory-card">
                <Link
                  href={`/shops/${shop.id}`}
                  className="directory-info-section"
                >
                  <div className="d-flex gap-3 mb-3 align-items-center">
                    <img
                      src={
                        shop.profilePictureUrl ||
                        "https://placehold.co/100x100?text=Shop"
                      }
                      alt={shop.shopName || shop.firstName}
                      style={{
                        width: 80,
                        height: 80,
                        borderRadius: "50%",
                        objectFit: "cover",
                        border: "2px solid #eee",
                      }}
                      onError={(e) => {
                        (e.target as HTMLImageElement).src =
                          "https://placehold.co/100x100?text=Shop";
                      }}
                    />
                    <div>
                      <h4
                        className="fw-bold mb-1 fs-5"
                        style={{ color: "var(--primary-green)" }}
                      >
                        {shop.shopName || `${shop.firstName} ${shop.lastName}`}
                      </h4>
                      <p className="text-muted small mb-0">
                        {shop.categoryName || "محل عام"}
                      </p>
                    </div>
                  </div>

                  <div className="d-flex flex-column gap-2 small text-secondary">
                    <div>
                      <i
                        className="fas fa-store ms-2"
                        style={{ color: "var(--primary-green)" }}
                      ></i>
                      نوع النشاط:{" "}
                      <span className="fw-bold text-dark">
                        {shop.categoryName}
                      </span>
                    </div>
                    <div>
                      <i
                        className="fas fa-map-marker-alt ms-2"
                        style={{ color: "var(--primary-green)" }}
                      ></i>
                      العنوان: {shop.governorate}، {shop.city}، {shop.street}
                    </div>
                    <div>
                      <i
                        className="fas fa-info-circle ms-2"
                        style={{ color: "var(--primary-green)" }}
                      ></i>
                      {shop.bio || "لا يوجد وصف للمحل"}
                    </div>
                  </div>
                </Link>

                <div className="directory-action-section">
                  <div className="text-center mb-2 text-muted small">
                    <i
                      className={`fas fa-circle ms-1 ${
                        isOnline ? "text-success" : "text-secondary"
                      }`}
                    ></i>
                    {isOnline ? "متاح الآن" : "غير متاح"}
                  </div>
                  <Link
                    href={`/shops/${shop.id}`}
                    className="btn btn-danger fw-bold w-100"
                  >
                    عرض المحل
                  </Link>
                  <div
                    className="mt-2 text-muted small"
                    style={{ fontSize: "0.75rem" }}
                  >
                    تواصل مباشر
                  </div>
                </div>
              </div>
            );
          })
        )}
      </div>
    </div>
  );
}
