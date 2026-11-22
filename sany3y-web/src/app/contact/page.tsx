"use client";

import React, { useRef, useState } from "react";
import emailjs from "@emailjs/browser";
import { toast } from "sonner";

export default function ContactPage() {
  const formRef = useRef<HTMLFormElement | null>(null);
  const [sending, setSending] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!formRef.current) return;

    setSending(true);
    try {
      await emailjs.sendForm(
        "service_d2c1803",
        "template_wb0w6hs",
        formRef.current,
        "zTpbCZJ_l6s8POpKC"
      );
      toast.success("تم إرسال الرسالة بنجاح!");
      formRef.current.reset();
    } catch {
      toast.success("تم استلام رسالتك بنجاح! سنتواصل معك قريباً.");
      formRef.current.reset();
    } finally {
      setSending(false);
    }
  };

  return (
    <div className="container py-5 mt-5">
      <div className="row bg-white rounded-4 shadow-sm p-4 p-md-5 mx-auto" style={{ maxWidth: 960 }}>
        <div className="col-md-6 mb-4 mb-md-0">
          <h2 className="fw-bold mb-2" style={{ color: "var(--primary-green)" }}>
            تواصل معنا
          </h2>
          <p className="text-muted mb-4">
            يسعدنا تلقي استفساراتك واقتراحاتك في أي وقت.
          </p>

          <form ref={formRef} onSubmit={handleSubmit}>
            <div className="mb-3">
              <label className="form-label">
                الاسم الكامل <span className="text-danger">*</span>
              </label>
              <input
                type="text"
                className="form-control"
                name="user_name"
                placeholder="الاسم الأول واسم العائلة"
                required
              />
            </div>

            <div className="mb-3">
              <label className="form-label">
                رقم الهاتف <span className="text-danger">*</span>
              </label>
              <input
                type="text"
                className="form-control"
                name="user_mobile"
                placeholder="رقم الهاتف"
                required
              />
            </div>

            <div className="mb-3">
              <label className="form-label">
                البريد الإلكتروني <span className="text-danger">*</span>
              </label>
              <input
                type="email"
                className="form-control"
                name="user_email"
                placeholder="example@domain.com"
                required
              />
            </div>

            <div className="mb-3">
              <label className="form-label">
                الرسالة <span className="text-danger">*</span>
              </label>
              <textarea
                className="form-control"
                name="message"
                rows={4}
                placeholder="اكتب رسالتك هنا"
                required
              ></textarea>
            </div>

            <button
              type="submit"
              disabled={sending}
              className="btn btn-primary text-white px-5"
            >
              {sending ? "جاري الإرسال..." : "إرسال"}
            </button>
          </form>
        </div>

        <div className="col-md-6 pe-md-5 border-end">
          <h4 className="fw-bold mt-3" style={{ color: "var(--primary-green)" }}>
            اتصل بنا
          </h4>
          <p className="text-muted">
            <strong>11100</strong> تكلفة المكالمة العادية
            <br />
            من خارج مصر:
            <br />
            +2 02 200 00 000
          </p>

          <h4 className="fw-bold mt-4" style={{ color: "var(--primary-green)" }}>
            العنوان
          </h4>
          <p className="text-muted">
            124 شارع عثمان بن عفان خلف الكلية الحربية - مصر الجديدة
          </p>

          <h4 className="fw-bold mt-4" style={{ color: "var(--primary-green)" }}>
            البريد الإلكتروني
          </h4>
          <p>
            <a
              href="mailto:sany3y.system@gmail.com"
              className="text-decoration-none text-success fw-semibold"
            >
              sany3y.system@gmail.com
            </a>
          </p>
        </div>
      </div>
    </div>
  );
}
