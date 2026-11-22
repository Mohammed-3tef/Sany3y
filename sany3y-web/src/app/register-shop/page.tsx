"use client";

import React, { useEffect, useState } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { apiFetch } from "@/lib/api";
import { Category } from "@/types";
import { useGovernoratesAndCities } from "@/components/GovernorateCitySelect";
import { toast } from "sonner";

export default function RegisterShopPage() {
  const router = useRouter();
  const { governorates, cities, loadCitiesForGovernorate } =
    useGovernoratesAndCities();
  const [categories, setCategories] = useState<Category[]>([]);

  const [firstName, setFirstName] = useState("");
  const [lastName, setLastName] = useState("");
  const [nationalId, setNationalId] = useState("");
  const [isMale, setIsMale] = useState(true);
  const [shopName, setShopName] = useState("");
  const [categoryId, setCategoryId] = useState("");
  const [birthDate, setBirthDate] = useState("");
  const [email, setEmail] = useState("");
  const [phoneNumber, setPhoneNumber] = useState("");
  const [governorate, setGovernorate] = useState("");
  const [city, setCity] = useState("");
  const [street, setStreet] = useState("");
  const [userName, setUserName] = useState("");
  const [password, setPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [nationalIdImage, setNationalIdImage] = useState<File | null>(null);
  const [imagePreview, setImagePreview] = useState<string | null>(null);

  const [errors, setErrors] = useState<string[]>([]);
  const [submitting, setSubmitting] = useState(false);

  useEffect(() => {
    apiFetch<Category[]>("/api/Category/GetAll")
      .then((data) => setCategories(data || []))
      .catch(() => setCategories([]));
  }, []);

  const handleImageChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (file) {
      setNationalIdImage(file);
      const reader = new FileReader();
      reader.onload = () => setImagePreview(reader.result as string);
      reader.readAsDataURL(file);
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setErrors([]);

    if (password !== confirmPassword) {
      setErrors(["كلمة المرور وتأكيد كلمة المرور غير متطابقين."]);
      return;
    }

    if (!/^\d{14}$/.test(nationalId)) {
      setErrors(["الرقم القومي للمالك يجب أن يتكون من 14 رقماً."]);
      return;
    }

    if (!nationalIdImage) {
      setErrors(["يرجى رفع صورة بطاقة مالك المحل."]);
      return;
    }

    setSubmitting(true);
    try {
      const formData = new FormData();
      formData.append("NationalId", nationalId);
      formData.append("FirstName", firstName);
      formData.append("LastName", lastName);
      formData.append("UserName", userName);
      formData.append("Email", email);
      formData.append("PhoneNumber", phoneNumber);
      formData.append("BirthDate", birthDate);
      formData.append("IsMale", String(isMale));
      formData.append("Governorate", governorate);
      formData.append("City", city);
      formData.append("Street", street);
      formData.append("Password", password);
      formData.append("ConfirmPassword", confirmPassword);
      formData.append("IsClient", "false");
      formData.append("IsShop", "true");
      formData.append("ShopName", shopName);
      formData.append("CategoryId", categoryId);
      formData.append("NationalIdImage", nationalIdImage);

      await apiFetch("/api/Auth/register", {
        method: "POST",
        body: formData,
      });

      toast.success("تم إنشاء حساب المحل بنجاح! يرجى تأكيد بريدك الإلكتروني.");
      router.push(`/confirm-email-notice?email=${encodeURIComponent(email)}`);
    } catch (err: any) {
      const errList = err.data?.errors;
      if (Array.isArray(errList) && errList.length > 0) {
        setErrors(errList);
      } else {
        setErrors([err.message || "حدث خطأ أثناء إنشاء حساب المحل."]);
      }
      window.scrollTo({ top: 0, behavior: "smooth" });
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div className="py-5 mt-5" style={{ backgroundColor: "#f8f9fa", minHeight: "100vh" }}>
      <div className="container">
        <div className="row justify-content-center">
          <div className="col-lg-8">
            <div className="card border-0 rounded-4 shadow overflow-hidden bg-white">
              <div
                className="p-4 text-center text-white"
                style={{
                  background: "linear-gradient(135deg, var(--primary-green), #006400)",
                }}
              >
                <h2 className="fw-bold mb-2">إنشاء حساب محل جديد</h2>
                <p className="mb-0 opacity-75">
                  انضم إلينا الآن وأضف محلك إلى صنايعي
                </p>
              </div>

              <div className="p-3 p-md-5">
                {errors.length > 0 && (
                  <div className="alert alert-danger mb-4">
                    <ul className="mb-0">
                      {errors.map((err, i) => (
                        <li key={i}>{err}</li>
                      ))}
                    </ul>
                  </div>
                )}

                <form onSubmit={handleSubmit}>
                  {/* Owner Data */}
                  <h5
                    className="fw-bold pb-2 mb-3 border-bottom d-flex align-items-center gap-2"
                    style={{ color: "var(--primary-green)" }}
                  >
                    <i className="fas fa-user text-warning"></i> بيانات مالك المحل
                  </h5>

                  <div className="row g-3 mb-4">
                    <div className="col-md-6">
                      <label className="form-label">الاسم الأول</label>
                      <input
                        type="text"
                        className="form-control"
                        value={firstName}
                        onChange={(e) => setFirstName(e.target.value)}
                        required
                      />
                    </div>
                    <div className="col-md-6">
                      <label className="form-label">الاسم الأخير</label>
                      <input
                        type="text"
                        className="form-control"
                        value={lastName}
                        onChange={(e) => setLastName(e.target.value)}
                        required
                      />
                    </div>
                    <div className="col-md-6">
                      <label className="form-label">الرقم القومي للمالك</label>
                      <input
                        type="text"
                        maxLength={14}
                        className="form-control"
                        value={nationalId}
                        onChange={(e) => setNationalId(e.target.value)}
                        required
                      />
                    </div>
                    <div className="col-md-6 d-flex align-items-end gap-3">
                      <span className="fw-semibold mb-2">جنس المالك:</span>
                      <div className="form-check mb-2">
                        <input
                          className="form-check-input"
                          type="radio"
                          id="ownerMale"
                          checked={isMale}
                          onChange={() => setIsMale(true)}
                        />
                        <label className="form-check-label" htmlFor="ownerMale">
                          ذكر
                        </label>
                      </div>
                      <div className="form-check mb-2">
                        <input
                          className="form-check-input"
                          type="radio"
                          id="ownerFemale"
                          checked={!isMale}
                          onChange={() => setIsMale(false)}
                        />
                        <label className="form-check-label" htmlFor="ownerFemale">
                          أنثى
                        </label>
                      </div>
                    </div>
                  </div>

                  {/* Shop Data */}
                  <h5
                    className="fw-bold pb-2 mb-3 border-bottom d-flex align-items-center gap-2"
                    style={{ color: "var(--primary-green)" }}
                  >
                    <i className="fas fa-store text-warning"></i> بيانات المحل
                  </h5>

                  <div className="row g-3 mb-4">
                    <div className="col-md-12">
                      <label className="form-label">اسم المحل</label>
                      <input
                        type="text"
                        className="form-control"
                        value={shopName}
                        onChange={(e) => setShopName(e.target.value)}
                        required
                      />
                    </div>
                    <div className="col-md-6">
                      <label className="form-label">نوع النشاط</label>
                      <select
                        className="form-select"
                        value={categoryId}
                        onChange={(e) => setCategoryId(e.target.value)}
                        required
                      >
                        <option value="">اختر نوع النشاط</option>
                        {categories.map((c) => (
                          <option key={c.id} value={c.id}>
                            {c.name}
                          </option>
                        ))}
                      </select>
                    </div>
                    <div className="col-md-6">
                      <label className="form-label">تاريخ الإنشاء</label>
                      <input
                        type="date"
                        className="form-control"
                        value={birthDate}
                        onChange={(e) => setBirthDate(e.target.value)}
                        required
                      />
                    </div>
                    <div className="col-md-6">
                      <label className="form-label">البريد الإلكتروني</label>
                      <input
                        type="email"
                        className="form-control"
                        value={email}
                        onChange={(e) => setEmail(e.target.value)}
                        required
                      />
                    </div>
                    <div className="col-md-6">
                      <label className="form-label">رقم الهاتف</label>
                      <input
                        type="text"
                        className="form-control"
                        value={phoneNumber}
                        onChange={(e) => setPhoneNumber(e.target.value)}
                        required
                      />
                    </div>
                  </div>

                  {/* Address */}
                  <h5
                    className="fw-bold pb-2 mb-3 border-bottom d-flex align-items-center gap-2"
                    style={{ color: "var(--primary-green)" }}
                  >
                    <i className="fas fa-map-marker-alt text-warning"></i> عنوان المحل
                  </h5>

                  <div className="row g-3 mb-4">
                    <div className="col-md-6">
                      <label className="form-label">المحافظة</label>
                      <select
                        className="form-select"
                        value={governorate}
                        onChange={(e) => {
                          setGovernorate(e.target.value);
                          setCity("");
                          loadCitiesForGovernorate(e.target.value);
                        }}
                        required
                      >
                        <option value="">اختر المحافظة</option>
                        {governorates.map((g) => (
                          <option key={g.id} value={g.id}>
                            {g.arabicName}
                          </option>
                        ))}
                      </select>
                    </div>
                    <div className="col-md-6">
                      <label className="form-label">المدينة</label>
                      <select
                        className="form-select"
                        value={city}
                        onChange={(e) => setCity(e.target.value)}
                        required
                      >
                        <option value="">اختر المدينة</option>
                        {cities.map((c) => (
                          <option key={c.id} value={c.id}>
                            {c.arabicName}
                          </option>
                        ))}
                      </select>
                    </div>
                    <div className="col-md-12">
                      <label className="form-label">الشارع / المنطقة</label>
                      <input
                        type="text"
                        className="form-control"
                        value={street}
                        onChange={(e) => setStreet(e.target.value)}
                        required
                      />
                    </div>
                  </div>

                  {/* Account Credentials */}
                  <h5
                    className="fw-bold pb-2 mb-3 border-bottom d-flex align-items-center gap-2"
                    style={{ color: "var(--primary-green)" }}
                  >
                    <i className="fas fa-lock text-warning"></i> بيانات الحساب
                  </h5>

                  <div className="row g-3 mb-4">
                    <div className="col-md-12">
                      <label className="form-label">اسم المستخدم (للدخول)</label>
                      <input
                        type="text"
                        className="form-control"
                        value={userName}
                        onChange={(e) => setUserName(e.target.value)}
                        required
                      />
                    </div>
                    <div className="col-md-6">
                      <label className="form-label">كلمة المرور</label>
                      <input
                        type="password"
                        className="form-control"
                        value={password}
                        onChange={(e) => setPassword(e.target.value)}
                        required
                      />
                    </div>
                    <div className="col-md-6">
                      <label className="form-label">تأكيد كلمة المرور</label>
                      <input
                        type="password"
                        className="form-control"
                        value={confirmPassword}
                        onChange={(e) => setConfirmPassword(e.target.value)}
                        required
                      />
                    </div>
                  </div>

                  {/* Verification */}
                  <h5
                    className="fw-bold pb-2 mb-3 border-bottom d-flex align-items-center gap-2"
                    style={{ color: "var(--primary-green)" }}
                  >
                    <i className="fas fa-camera text-warning"></i> التحقق من الهوية / الرخصة
                  </h5>

                  <div className="mb-4">
                    <label className="form-label mb-2">صورة بطاقة مالك المحل</label>
                    <label
                      className="d-block w-100 p-4 text-center rounded-3"
                      style={{
                        border: "2px dashed #ccc",
                        backgroundColor: "#fafafa",
                        cursor: "pointer",
                      }}
                    >
                      <input
                        type="file"
                        accept="image/*"
                        style={{ display: "none" }}
                        onChange={handleImageChange}
                      />
                      <i className="fas fa-cloud-upload-alt fa-3x text-muted mb-2"></i>
                      <h6 className="text-muted">اضغط هنا لرفع الصورة</h6>
                      {imagePreview && (
                        <div className="mt-3">
                          <img
                            src={imagePreview}
                            alt="Preview"
                            style={{ maxHeight: 150, borderRadius: 10 }}
                          />
                        </div>
                      )}
                    </label>
                  </div>

                  <div className="d-grid gap-2 mt-4">
                    <button
                      type="submit"
                      disabled={submitting}
                      className="btn btn-warning btn-lg fw-bold text-dark rounded-pill shadow-sm"
                    >
                      {submitting ? "جاري إنشاء حساب المحل..." : "إنشاء الحساب"}
                    </button>
                  </div>
                </form>

                <div className="text-center mt-4">
                  <Link href="/register" className="text-muted small">
                    العودة لتسجيل الأفراد (عميل / فني)
                  </Link>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
