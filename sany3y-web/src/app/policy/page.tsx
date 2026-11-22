import React from "react";

export default function PolicyPage() {
  const email = "sany3y.system@gmail.com";

  return (
    <div className="container py-5 mt-5" dir="rtl">
      <div className="text-center mb-5">
        <h1 className="fw-bold text-success">سياسة الخصوصية</h1>
        <p className="text-muted">آخر تحديث: أكتوبر 2025</p>
      </div>

      {[
        {
          title: "1. مقدمة",
          content: (
            <p className="text-secondary">
              مرحبًا بك في <strong>صنــايعي</strong>! نحن نقدر ثقتك ونلتزم بحماية
              بياناتك الشخصية. توضح هذه السياسة كيفية جمع بياناتك واستخدامها
              وحمايتها عند استخدامك لموقعنا وخدماتنا.
            </p>
          ),
        },
        {
          title: "2. المعلومات التي نقوم بجمعها",
          content: (
            <>
              <h5 className="fw-semibold mt-3">2.1 معلومات يتم جمعها تلقائيًا</h5>
              <ul className="text-secondary">
                <li>عنوان الـ IP ونوع المتصفح</li>
                <li>نوع الجهاز ونظام التشغيل</li>
                <li>الصفحات التي تزورها ونشاطك داخل الموقع</li>
                <li>ملفات تعريف الارتباط (Cookies) وبيانات التحليل</li>
              </ul>
              <h5 className="fw-semibold mt-3">2.2 المعلومات التي تقوم بتقديمها</h5>
              <ul className="text-secondary">
                <li>الاسم الكامل والبريد الإلكتروني ورقم الهاتف</li>
                <li>العنوان أو مكان الخدمة</li>
                <li>بيانات الدفع للمعاملات الإلكترونية عبر بوابات دفع آمنة (Stripe)</li>
              </ul>
            </>
          ),
        },
        {
          title: "3. كيف نستخدم بياناتك",
          content: (
            <ul className="text-secondary">
              <li>تسهيل عملية التواصل والحجز بين العملاء والفنيين</li>
              <li>تقديم وتحسين خدماتنا ومعالجة المدفوعات بشكل آمن</li>
              <li>اكتشاف ومنع الاحتيال أو سوء الاستخدام</li>
              <li>إرسال الإشعارات والتحديثات المتعلقة بحجوزاتك</li>
            </ul>
          ),
        },
        {
          title: "4. تواصل معنا",
          content: (
            <p className="text-secondary mb-0">
              إذا كان لديك أي سؤال أو استفسار حول سياسة الخصوصية، يمكنك التواصل
              معنا عبر البريد الإلكتروني:{" "}
              <a href={`mailto:${email}`} className="text-success fw-bold">
                {email}
              </a>
            </p>
          ),
        },
      ].map((sec) => (
        <div
          key={sec.title}
          className="bg-white rounded-3 shadow-sm p-4 mb-4"
        >
          <h3 className="fw-bold text-success mb-3">{sec.title}</h3>
          {sec.content}
        </div>
      ))}
    </div>
  );
}
