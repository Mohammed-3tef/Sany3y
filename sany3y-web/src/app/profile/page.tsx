"use client";

import React, { useEffect, useState } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { apiFetch } from "@/lib/api";
import { useAuth } from "@/context/AuthContext";
import { Category } from "@/types";
import { useGovernoratesAndCities } from "@/components/GovernorateCitySelect";
import { toast } from "sonner";

export default function ProfilePage() {
  const router = useRouter();
  const { user, isAuthenticated, isLoading, updateUser } = useAuth();
  const { governorates, cities, loadCitiesForGovernorate } =
    useGovernoratesAndCities();
  const [categories, setCategories] = useState<Category[]>([]);

  const [loadingProfile, setLoadingProfile] = useState(true);
  const [saving, setSaving] = useState(false);

  const [firstName, setFirstName] = useState("");
  const [lastName, setLastName] = useState("");
  const [userName, setUserName] = useState("");
  const [birthDate, setBirthDate] = useState("");
  const [email, setEmail] = useState("");
  const [phoneNumber, setPhoneNumber] = useState("");
  const [governorate, setGovernorate] = useState("");
  const [city, setCity] = useState("");
  const [street, setStreet] = useState("");
  const [bio, setBio] = useState("");
  const [profilePicture, setProfilePicture] = useState(
    "https://placehold.co/150x150?text=Profile"
  );
  const [uploadedImage, setUploadedImage] = useState<File | null>(null);
  const [categoryId, setCategoryId] = useState<string>("");
  const [experienceYears, setExperienceYears] = useState<string>("");
  const [price, setPrice] = useState<string>("");
  const [isShop, setIsShop] = useState(false);
  const [shopName, setShopName] = useState("");
  const [isTechnician, setIsTechnician] = useState(false);

  useEffect(() => {
    if (!isLoading && !isAuthenticated) {
      router.push("/login");
      return;
    }

    if (!isAuthenticated) return;

    Promise.all([
      apiFetch<any>("/api/Auth/profile"),
      apiFetch<Category[]>("/api/Category/GetAll").catch(() => []),
    ])
      .then(([prof, cats]) => {
        setCategories(cats || []);
        setFirstName(prof.firstName || "");
        setLastName(prof.lastName || "");
        setUserName(prof.userName || "");
        setBirthDate(
          prof.birthDate ? String(prof.birthDate).split("T")[0] : ""
        );
        setEmail(prof.email || "");
        setPhoneNumber(prof.phoneNumber || "");
        setGovernorate(prof.governorate || "");
        setCity(prof.city || "");
        setStreet(prof.street || "");
        setBio(prof.bio || "");
        setProfilePicture(
          prof.profilePicture || "https://placehold.co/150x150?text=Profile"
        );
        setCategoryId(prof.categoryId ? String(prof.categoryId) : "");
        setExperienceYears(
          prof.experienceYears != null ? String(prof.experienceYears) : ""
        );
        setPrice(prof.price != null ? String(prof.price) : "");
        setIsShop(Boolean(prof.isShop));
        setShopName(prof.shopName || "");
        setIsTechnician(Boolean(prof.isTechnician));

        if (prof.governorate) {
          loadCitiesForGovernorate(prof.governorate);
        }
      })
      .catch(() => {
        toast.error("فشل تحميل بيانات الملف الشخصي.");
      })
      .finally(() => setLoadingProfile(false));
  }, [isLoading, isAuthenticated]);

  const handleImageSelect = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (file) {
      setUploadedImage(file);
      const reader = new FileReader();
      reader.onload = () => setProfilePicture(reader.result as string);
      reader.readAsDataURL(file);
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setSaving(true);

    try {
      const formData = new FormData();
      formData.append("FirstName", firstName);
      formData.append("LastName", lastName);
      formData.append("BirthDate", birthDate);
      formData.append("Email", email);
      formData.append("PhoneNumber", phoneNumber);
      formData.append("Governorate", governorate);
      formData.append("City", city);
      formData.append("Street", street);
      if (bio) formData.append("Bio", bio);
      if (categoryId) formData.append("CategoryId", categoryId);
      if (experienceYears) formData.append("ExperienceYears", experienceYears);
      if (price) formData.append("Price", price);
      if (shopName) formData.append("ShopName", shopName);
      if (uploadedImage) formData.append("UploadedImage", uploadedImage);

      const res = await apiFetch<{ message: string; user: any }>(
        "/api/Auth/profile",
        {
          method: "PUT",
          body: formData,
        }
      );

      if (res.user) {
        updateUser(res.user);
      }
      toast.success(res.message || "تم تحديث الملف الشخصي بنجاح.");
    } catch (err: any) {
      toast.error(err.message || "فشل تحديث الملف الشخصي.");
    } finally {
      setSaving(false);
    }
  };

  if (loadingProfile) {
    return (
      <div className="container py-5 mt-5 text-center">
        <div className="spinner-border text-success" />
        <p className="mt-3 text-muted">جاري تحميل الملف الشخصي...</p>
      </div>
    );
  }

  return (
    <div className="container py-5 mt-5">
      <div className="row justify-content-center">
        <div className="col-lg-10">
          <div className="card border-0 rounded-4 shadow-sm overflow-hidden bg-white">
            <div
              className="p-4 text-white d-flex justify-content-between align-items-center"
              style={{
                background: "linear-gradient(135deg, #004d00 0%, #006400 100%)",
              }}
            >
              <div className="d-flex align-items-center">
                <i className="fas fa-user-edit fa-2x ms-3"></i>
                <h3 className="mb-0 fw-bold">تعديل الملف الشخصي</h3>
              </div>
              <Link
                href={`/reset-password?email=${encodeURIComponent(email)}`}
                className="btn btn-light btn-sm rounded-pill fw-bold"
              >
                <i className="fas fa-key ms-1"></i> تغيير كلمة المرور
              </Link>
            </div>

            <div className="card-body p-4 p-md-5">
              <form onSubmit={handleSubmit}>
                <div className="row g-4">
                  {/* Profile Picture */}
                  <div className="col-12 text-center">
                    <label className="form-label d-block mb-2 fw-bold">
                      صورة الملف الشخصي
                    </label>
                    <div className="mb-3">
                      <img
                        src={profilePicture}
                        alt="Profile"
                        className="rounded-circle"
                        style={{
                          width: 150,
                          height: 150,
                          objectFit: "cover",
                          border: "3px solid #004d00",
                        }}
                        onError={(e) => {
                          (e.target as HTMLImageElement).src =
                            "https://placehold.co/150x150?text=Profile";
                        }}
                      />
                    </div>
                    <input
                      type="file"
                      accept="image/*"
                      onChange={handleImageSelect}
                      className="form-control form-control-sm w-auto mx-auto"
                    />
                  </div>

                  {isShop ? (
                    <>
                      <div className="col-12">
                        <h5 className="text-muted border-bottom pb-2 mt-3">
                          <i className="fas fa-store ms-2"></i> بيانات المحل
                        </h5>
                      </div>

                      <div className="col-md-6">
                        <label className="form-label">اسم المحل</label>
                        <input
                          type="text"
                          className="form-control"
                          value={shopName}
                          onChange={(e) => setShopName(e.target.value)}
                        />
                      </div>

                      <div className="col-md-6">
                        <label className="form-label">نوع النشاط</label>
                        <select
                          className="form-select"
                          value={categoryId}
                          onChange={(e) => setCategoryId(e.target.value)}
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
                        <label className="form-label">اسم المستخدم</label>
                        <input
                          type="text"
                          className="form-control bg-light"
                          value={userName}
                          readOnly
                        />
                      </div>

                      <div className="col-md-6">
                        <label className="form-label">تاريخ الإنشاء</label>
                        <input
                          type="date"
                          className="form-control"
                          value={birthDate}
                          onChange={(e) => setBirthDate(e.target.value)}
                        />
                      </div>

                      <div className="col-12">
                        <h5 className="text-muted border-bottom pb-2 mt-3">
                          <i className="fas fa-user ms-2"></i> بيانات مالك المحل
                        </h5>
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
                        <label className="form-label">اسم العائلة</label>
                        <input
                          type="text"
                          className="form-control"
                          value={lastName}
                          onChange={(e) => setLastName(e.target.value)}
                          required
                        />
                      </div>
                    </>
                  ) : (
                    <>
                      <div className="col-12">
                        <h5 className="text-muted border-bottom pb-2 mt-3">
                          <i className="fas fa-id-card ms-2"></i> المعلومات الشخصية
                        </h5>
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
                        <label className="form-label">اسم العائلة</label>
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
                          className="form-control bg-light"
                          value={userName}
                          readOnly
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
                    </>
                  )}

                  {/* Contact Info */}
                  <div className="col-12 mt-4">
                    <h5 className="text-muted border-bottom pb-2">
                      <i className="fas fa-address-book ms-2"></i> معلومات الاتصال
                    </h5>
                  </div>

                  <div className="col-md-6">
                    <label className="form-label">البريد الإلكتروني</label>
                    <input
                      type="email"
                      className="form-control bg-light"
                      value={email}
                      readOnly
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

                  {/* Address */}
                  <div className="col-12 mt-4">
                    <h5 className="text-muted border-bottom pb-2">
                      <i className="fas fa-map-marker-alt ms-2"></i> العنوان
                    </h5>
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
                        <option key={g.id} value={g.arabicName}>
                          {g.arabicName}
                        </option>
                      ))}
                    </select>
                  </div>

                  <div className="col-md-6">
                    <label className="form-label">المدينة / المنطقة</label>
                    <select
                      className="form-select"
                      value={city}
                      onChange={(e) => setCity(e.target.value)}
                      required
                    >
                      <option value="">اختر المدينة / المنطقة</option>
                      {cities.map((c) => (
                        <option key={c.id} value={c.arabicName}>
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

                  {/* Technician specialty */}
                  {isTechnician && !isShop && (
                    <>
                      <div className="col-12 mt-4">
                        <h5 className="text-muted border-bottom pb-2">
                          <i className="fas fa-layer-group ms-2"></i> التخصص
                        </h5>
                      </div>

                      <div className="col-md-12">
                        <label className="form-label">التخصص</label>
                        <select
                          className="form-select"
                          value={categoryId}
                          onChange={(e) => setCategoryId(e.target.value)}
                        >
                          <option value="">اختر التخصص</option>
                          {categories.map((c) => (
                            <option key={c.id} value={c.id}>
                              {c.name}
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
                        />
                      </div>

                      <div className="col-md-6">
                        <label className="form-label">سعر الخدمة (ج.م)</label>
                        <input
                          type="number"
                          className="form-control"
                          value={price}
                          onChange={(e) => setPrice(e.target.value)}
                        />
                      </div>
                    </>
                  )}

                  <div className="col-12 mt-3">
                    <label className="form-label">
                      <i className="fas fa-info-circle ms-2"></i> نبذة تعريفية
                    </label>
                    <textarea
                      className="form-control"
                      rows={4}
                      placeholder="اكتب نبذة مختصرة..."
                      value={bio}
                      onChange={(e) => setBio(e.target.value)}
                    />
                  </div>
                </div>

                <div className="row mt-5 pt-3 border-top">
                  <div className="col-md-6 mb-2 mb-md-0">
                    <button
                      type="submit"
                      disabled={saving}
                      className="btn btn-success w-100 py-2 fw-bold"
                    >
                      <i className="fas fa-save ms-2"></i>
                      {saving ? "جاري الحفظ..." : "حفظ التعديلات"}
                    </button>
                  </div>
                  <div className="col-md-6">
                    <Link
                      href="/"
                      className="btn btn-outline-secondary w-100 py-2"
                    >
                      <i className="fas fa-times ms-2"></i> إلغاء
                    </Link>
                  </div>
                </div>
              </form>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
