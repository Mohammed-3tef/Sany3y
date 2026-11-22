import React from "react";
import Link from "next/link";

export default function AboutPage() {
  return (
    <div className="pt-5 pb-5 mt-5">
      <div className="container card shadow-sm border-0 my-4">
        <div className="card-body p-4 p-md-5">
          <h2 className="fw-bold text-success mb-4">من نحن</h2>

          <p className="fs-5 text-secondary">
            <strong>صنَّاعي</strong> هي منصة رقمية تهدف لربط أصحاب الحرف والمهنيين
            (مثل النجارين، السباكين، الدهّانين، الكهربائيين، وغيرهم) مع الأشخاص
            الذين يحتاجون خدمات موثوقة وسريعة وبأسعار عادلة في منطقتهم. نساعد
            العملاء على إيجاد الفني المناسب، الاطلاع على التقييمات الحقيقية،
            مقارنة الأسعار، وحجز الخدمة خلال دقائق — مع التركيز على الجودة
            والشفافية.
          </p>

          <p className="fs-5 text-secondary">
            يحصل كل حرفي على صفحة تعريفية بسيطة تعرض صور أعماله، الخدمات التي
            يقدمها، المناطق التي يغطيها، الأسعار الابتدائية، وتقييمات العملاء. كما
            تعتمد المنصة على مراجعات حقيقية وموثّقة لضمان بناء مجتمع موثوق وشفاف.
          </p>

          <p className="fs-5 text-secondary">
            ومع تزايد التقييمات والمراجعات، ومع وجود معايير واضحة لقبول الحرفيين،
            تهدف منصة صنَّاعي إلى بناء مجتمع آمن، عملي، وسهل الاستخدام للجميع.
          </p>

          <Link
            href="/contact"
            className="btn btn-outline-success mt-3 fw-semibold"
          >
            تواصل معنا
          </Link>
        </div>
      </div>
    </div>
  );
}
