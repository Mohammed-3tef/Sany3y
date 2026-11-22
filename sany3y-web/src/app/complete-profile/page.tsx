"use client";

import React, { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { apiFetch } from "@/lib/api";
import { Category } from "@/types";
import { useGovernoratesAndCities } from "@/components/GovernorateCitySelect";
import { useAuth } from "@/context/AuthContext";
import { toast } from "sonner";

export default function CompleteProfilePage() {
  const router = useRouter();
  const { setSession } = useAuth();
  const { governorates, cities, loadCitiesForGovernorate } =
    useGovernoratesAndCities();
  const [categories, setCategories] = useState<Category[]>([]);

  const [isClient, setIsClient] = useState(true);
  const [firstName, setFirstName] = useState("");
  const [lastName, setLastName] = useState("");
  const [userName, setUserName] = useState("");
  const [email, setEmail] = useState("");
  const [picture, setPicture] = useState("");
  const [nationalId, setNationalId] = useState("");
  const [birthDate, setBirthDate] = useState("");
  const [isMale, setIsMale] = useState(true);
  const [phoneNumber, setPhoneNumber] = useState("");
  const [governorate, setGovernorate] = useState("");
  const [city, setCity] = useState("");
  const [street, setStreet] = useState("");
  const [password, setPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [categoryId, setCategoryId] = useState("");
  const [experienceYears, setExperienceYears] = useState("");
  const [price, setPrice] = useState("");
  const [nationalIdImage, setNationalIdImage] = useState<File | null>(null);
  const [errors, setErrors] = useState<string[]>([]);
  const [submitting, setSubmitting] = useState(false);

  useEffect(() => {
    const raw = sessionStorage.getItem("sany3y_google_prefill");
    if (raw) {
      try {
        const prefill = JSON.parse(raw);
        setFirstName(prefill.firstName || "");
        setLastName(prefill.lastName || "");
        setEmail(prefill.email || "");
        setUserName(prefill.userName || "");
        setPicture(prefill.picture || "");
      } catch {
        // Ignore parse error
      }
    }

    apiFetch<Category[]>("/api/Category/GetAll")
      .then((data) => setCategories(data || []))
      .catch(() => setCategories([]));
  }, []);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setErrors([]);

    if (!nationalIdImage) {
      setErrors(["يرجى رفع صورة البطاقة الشخصية."]);
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
      if (picture) formData.append("Picture", picture);
      if (!isClient) {
        if (categoryId) formData.append("CategoryId", categoryId);
        if (experienceYears) formData.append("ExperienceYears", experienceYears);
        if (price) formData.append("Price", price);
      }
      formData.append("NationalIdImage", nationalIdImage);

      const res = await apiFetch<{ token: string; user: any }>(
        "/api/Auth/complete-profile",
        {
          method: "POST",
          body: formData,
        }
      );

      sessionStorage.removeItem("sany3y_google_prefill");
      setSession(res.token, res.user);
      toast.success("تم استكمال بيانات الحساب بنجاح!");
      router.push("/");
    } catch (err: any) {
      setErrors(err.data?.errors || [err.message || "فشل استكمال الملف الشخصي."]);
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div className="container py-5 mt-5">
      <div className="row justify-content-center">
        <div className="col-lg-8">
          <div className="card shadow border-0 rounded-4 p-4 p-md-5 bg-white">
            <h3 className="fw-bold text-success mb-2">استكمال بيانات الحساب</h3>
            <p className="text-muted mb-4">
              يرجى استكمال بياناتك والرقم القومي لتفعيل حسابك المرتبط بجوجل
            </p>

            {errors.length > 0 && (
              <div className="alert alert-danger">
                <ul className="mb-0">
                  {errors.map((err, i) => (
                    <li key={i}>{err}</li>
                  ))}
                </ul>
              </div>
            )}

            <form onSubmit={handleSubmit}>
              <div className="row g-3">
                <div className="col-md-6">
                  <label className="form-label">نوع الحساب</label>
                  <select
                    className="form-select"
                    value={isClient ? "client" : "tech"}
                    onChange={(e) => setIsClient(e.target.value === "client")}
                  >
                    <option value="client">عميل</option>
                    <option value="tech">فني</option>
                  </select>
                </div>
                <div className="col-md-6">
                  <label className="form-label">البريد الإلكتروني</label>
                  <input
                    type="email"
                    className="form-control bg-light"
                    value={email}
                    onChange={(e) => setEmail(e.target.value)}
                    required
                  />
                </div>
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
                  <label className="form-label">اسم المستخدم</label>
                  <input
                    type="text"
                    className="form-control"
                    value={userName}
                    onChange={(e) => setUserName(e.target.value)}
                    required
                  />
                </div>
                <div className="col-md-6">
                  <label className="form-label">الرقم القومي (14 رقم)</label>
                  <input
                    type="text"
                    maxLength={14}
                    className="form-control"
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
                  <label className="form-label">الشارع</label>
                  <input
                    type="text"
                    className="form-control"
                    value={street}
                    onChange={(e) => setStreet(e.target.value)}
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

                {!isClient && (
                  <>
                    <div className="col-md-4">
                      <label className="form-label">التخصص</label>
                      <select
                        className="form-select"
                        value={categoryId}
                        onChange={(e) => setCategoryId(e.target.value)}
                        required
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
                        value={experienceYears}
                        onChange={(e) => setExperienceYears(e.target.value)}
                        required
                      />
                    </div>
                    <div className="col-md-4">
                      <label className="form-label">سعر المعاينة</label>
                      <input
                        type="number"
                        className="form-control"
                        value={price}
                        onChange={(e) => setPrice(e.target.value)}
                        required
                      />
                    </div>
                  </>
                )}

                <div className="col-md-12">
                  <label className="form-label">صورة البطاقة الشخصية (OCR)</label>
                  <input
                    type="file"
                    accept="image/*"
                    className="form-control"
                    onChange={(e) =>
                      setNationalIdImage(e.target.files?.[0] || null)
                    }
                    required
                  />
                </div>
              </div>

              <button
                type="submit"
                disabled={submitting}
                className="btn btn-primary w-100 py-3 fw-bold mt-4"
              >
                {submitting ? "جاري الحفظ..." : "إكمال التسجيل"}
              </button>
            </form>
          </div>
        </div>
      </div>
    </div>
  );
}
