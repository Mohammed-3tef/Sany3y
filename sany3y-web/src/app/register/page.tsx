"use client";

import React, { useEffect, useState } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { apiFetch } from "@/lib/api";
import { Category } from "@/types";
import { useGovernoratesAndCities } from "@/components/GovernorateCitySelect";
import { toast } from "sonner";

export default function RegisterPage() {
  const router = useRouter();
  const { governorates, cities, loadCitiesForGovernorate } =
    useGovernoratesAndCities();
  const [categories, setCategories] = useState<Category[]>([]);

  const [isClient, setIsClient] = useState(true);
  const [firstName, setFirstName] = useState("");
  const [lastName, setLastName] = useState("");
  const [nationalId, setNationalId] = useState("");
  const [birthDate, setBirthDate] = useState("");
  const [isMale, setIsMale] = useState(true);
  const [email, setEmail] = useState("");
  const [phoneNumber, setPhoneNumber] = useState("");
  const [governorate, setGovernorate] = useState("");
  const [city, setCity] = useState("");
  const [street, setStreet] = useState("");
  const [userName, setUserName] = useState("");
  const [password, setPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [categoryId, setCategoryId] = useState("");
  const [experienceYears, setExperienceYears] = useState("");
  const [price, setPrice] = useState("");
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
      setErrors(["الرقم القومي يجب أن يتكون من 14 رقماً."]);
      return;
    }

    if (!nationalIdImage) {
      setErrors(["يرجى رفع صورة البطاقة الشخصية للتحقق."]);
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
      formData.append("IsClient", String(isClient));
      formData.append("IsShop", "false");

      if (!isClient) {
        if (categoryId) formData.append("CategoryId", categoryId);
        if (experienceYears) formData.append("ExperienceYears", experienceYears);
        if (price) formData.append("Price", price);
      }

      formData.append("NationalIdImage", nationalIdImage);

      await apiFetch("/api/Auth/register", {
        method: "POST",
        body: formData,
      });

      toast.success("تم إنشاء الحساب بنجاح! يرجى تأكيد بريدك الإلكتروني.");
      router.push(`/confirm-email-notice?email=${encodeURIComponent(email)}`);
    } catch (err: any) {
      const errList = err.data?.errors;
      if (Array.isArray(errList) && errList.length > 0) {
        setErrors(errList);
      } else {
        setErrors([err.message || "حدث خطأ أثناء إنشاء الحساب."]);
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
                <h2 className="fw-bold mb-2">إنشاء حساب جديد</h2>
                <p className="mb-0 opacity-75">
                  انضم إلينا الآن وابدأ رحلتك مع صنايعي
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
                  {/* 1. Account Type */}
                  <div className="mb-5">
                    <h5 className="text-center mb-4 fw-bold text-muted">
                      اختر نوع الحساب
                    </h5>
                    <div className="row g-3 justify-content-center">
                      <div className="col-4 col-md-3">
                        <div
                          onClick={() => setIsClient(true)}
                          className={`p-3 rounded-4 text-center border-2 h-100 d-flex flex-column align-items-center justify-content-center ${
                            isClient
                              ? "border-success bg-success bg-opacity-10 text-success"
                              : "border bg-white"
                          }`}
                          style={{ cursor: "pointer" }}
                        >
                          <i className="fas fa-user fa-2x mb-2"></i>
                          <h6 className="fw-bold mb-1">عميل</h6>
                          <small className="text-muted">أبحث عن خدمات</small>
                        </div>
                      </div>

                      <div className="col-4 col-md-3">
                        <div
                          onClick={() => setIsClient(false)}
                          className={`p-3 rounded-4 text-center border-2 h-100 d-flex flex-column align-items-center justify-content-center ${
                            !isClient
                              ? "border-success bg-success bg-opacity-10 text-success"
                              : "border bg-white"
                          }`}
                          style={{ cursor: "pointer" }}
                        >
                          <i className="fas fa-tools fa-2x mb-2"></i>
                          <h6 className="fw-bold mb-1">فني</h6>
                          <small className="text-muted">أقدم خدمات</small>
                        </div>
                      </div>

                      <div className="col-4 col-md-3">
                        <Link
                          href="/register-shop"
                          className="p-3 rounded-4 text-center border bg-white text-dark text-decoration-none h-100 d-flex flex-column align-items-center justify-content-center"
                        >
                          <i className="fas fa-store fa-2x mb-2 text-secondary"></i>
                          <h6 className="fw-bold mb-1">محل</h6>
                          <small className="text-muted">يمثل محل/متجر</small>
                        </Link>
                      </div>
                    </div>
                  </div>

                  {/* 2. Personal Info */}
                  <h5
                    className="fw-bold pb-2 mb-3 border-bottom d-flex align-items-center gap-2"
                    style={{ color: "var(--primary-green)" }}
                  >
                    <span
                      className="rounded-circle d-inline-flex align-items-center justify-content-center"
                      style={{
                        width: 30,
                        height: 30,
                        backgroundColor: "var(--accent-yellow)",
                        color: "#222",
                      }}
                    >
                      <i className="fas fa-id-card small"></i>
                    </span>
                    البيانات الشخصية
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
                    <div className="col-md-12">
                      <label className="form-label">الرقم القومي (14 رقم)</label>
                      <input
                        type="text"
                        maxLength={14}
                        className="form-control"
                        placeholder="أدخل الرقم القومي المكون من 14 رقماً"
                        value={nationalId}
                        onChange={(e) => setNationalId(e.target.value)}
                        required
                      />
                    </div>
                    <div className="col-md-6">
                      <label className="form-label">تاريخ الميلاد</label>
                      <input
                        type="date"
                        className="form-control"
                        value={birthDate}
                        onChange={(e) => setBirthDate(e.target.value)}
                        required
                      />
                    </div>
                    <div className="col-md-6 d-flex align-items-end">
                      <div className="form-control d-flex align-items-center justify-content-around bg-light border-0 py-2">
                        <div className="form-check">
                          <input
                            className="form-check-input"
                            type="radio"
                            id="genderMale"
                            checked={isMale}
                            onChange={() => setIsMale(true)}
                          />
                          <label className="form-check-label" htmlFor="genderMale">
                            <i className="fas fa-male ms-1"></i> ذكر
                          </label>
                        </div>
                        <div className="form-check">
                          <input
                            className="form-check-input"
                            type="radio"
                            id="genderFemale"
                            checked={!isMale}
                            onChange={() => setIsMale(false)}
                          />
                          <label className="form-check-label" htmlFor="genderFemale">
                            <i className="fas fa-female ms-1"></i> أنثى
                          </label>
                        </div>
                      </div>
                    </div>
                  </div>

                  {/* 3. Contact Info */}
                  <h5
                    className="fw-bold pb-2 mb-3 border-bottom d-flex align-items-center gap-2"
                    style={{ color: "var(--primary-green)" }}
                  >
                    <span
                      className="rounded-circle d-inline-flex align-items-center justify-content-center"
                      style={{
                        width: 30,
                        height: 30,
                        backgroundColor: "var(--accent-yellow)",
                        color: "#222",
                      }}
                    >
                      <i className="fas fa-map-marker-alt small"></i>
                    </span>
                    بيانات التواصل
                  </h5>

                  <div className="row g-3 mb-4">
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

                  {/* 4. Security */}
                  <h5
                    className="fw-bold pb-2 mb-3 border-bottom d-flex align-items-center gap-2"
                    style={{ color: "var(--primary-green)" }}
                  >
                    <span
                      className="rounded-circle d-inline-flex align-items-center justify-content-center"
                      style={{
                        width: 30,
                        height: 30,
                        backgroundColor: "var(--accent-yellow)",
                        color: "#222",
                      }}
                    >
                      <i className="fas fa-lock small"></i>
                    </span>
                    بيانات الحساب
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

                  {/* 5. Category (Technicians only) */}
                  {!isClient && (
                    <div className="mb-4">
                      <h5
                        className="fw-bold pb-2 mb-3 border-bottom d-flex align-items-center gap-2"
                        style={{ color: "var(--primary-green)" }}
                      >
                        <span
                          className="rounded-circle d-inline-flex align-items-center justify-content-center"
                          style={{
                            width: 30,
                            height: 30,
                            backgroundColor: "var(--accent-yellow)",
                            color: "#222",
                          }}
                        >
                          <i className="fas fa-layer-group small"></i>
                        </span>
                        التخصص
                      </h5>

                      <div className="row g-3">
                        <div className="col-md-12">
                          <label className="form-label">التخصص</label>
                          <select
                            className="form-select"
                            value={categoryId}
                            onChange={(e) => setCategoryId(e.target.value)}
                            required={!isClient}
                          >
                            <option value="">اختر التخصص</option>
                            {categories.map((cat) => (
                              <option key={cat.id} value={cat.id}>
                                {cat.name}
                              </option>
                            ))}
                          </select>
                        </div>
                        <div className="col-md-6">
                          <label className="form-label">عدد سنين الخبرة</label>
                          <input
                            type="number"
                            className="form-control"
                            value={experienceYears}
                            onChange={(e) => setExperienceYears(e.target.value)}
                            required={!isClient}
                          />
                        </div>
                        <div className="col-md-6">
                          <label className="form-label">سعر المعاينة (ج.م)</label>
                          <input
                            type="number"
                            className="form-control"
                            value={price}
                            onChange={(e) => setPrice(e.target.value)}
                            required={!isClient}
                          />
                        </div>
                      </div>
                    </div>
                  )}

                  {/* 6. National ID OCR Verification */}
                  <h5
                    className="fw-bold pb-2 mb-3 border-bottom d-flex align-items-center gap-2"
                    style={{ color: "var(--primary-green)" }}
                  >
                    <span
                      className="rounded-circle d-inline-flex align-items-center justify-content-center"
                      style={{
                        width: 30,
                        height: 30,
                        backgroundColor: "var(--accent-yellow)",
                        color: "#222",
                      }}
                    >
                      <i className="fas fa-camera small"></i>
                    </span>
                    التحقق من الهوية بالذكاء الاصطناعي (OCR)
                  </h5>

                  <div className="mb-4">
                    <label className="form-label mb-2">
                      صورة البطاقة الشخصية (للتحقق من تطابق الرقم القومي)
                    </label>
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
                      <h6 className="text-muted">اضغط هنا لرفع صورة البطاقة</h6>
                      <p className="small text-muted mb-0">
                        يرجى التأكد من وضوح الصورة والأرقام
                      </p>
                      {imagePreview && (
                        <div className="mt-3">
                          <img
                            src={imagePreview}
                            alt="National ID Preview"
                            style={{ maxHeight: 150, borderRadius: 10 }}
                          />
                          <p className="text-success small mt-1 mb-0">
                            <i className="fas fa-check-circle"></i> تم اختيار الصورة
                          </p>
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
                      {submitting ? (
                        <>
                          <span className="spinner-border spinner-border-sm ms-2"></span>
                          جاري التحقق من البطاقة وإنشاء الحساب...
                        </>
                      ) : (
                        <>
                          <i className="fas fa-user-plus ms-2"></i> إنشاء الحساب
                        </>
                      )}
                    </button>
                  </div>
                </form>

                <div className="text-center mt-4">
                  <p className="text-muted">
                    لديك حساب بالفعل؟{" "}
                    <Link
                      href="/login"
                      className="text-success fw-bold text-decoration-none"
                    >
                      سجل الدخول الآن
                    </Link>
                  </p>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
