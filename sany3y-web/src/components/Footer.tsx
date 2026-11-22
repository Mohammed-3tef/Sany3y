"use client";

import React from "react";
import Link from "next/link";
import { usePathname } from "next/navigation";

export function Footer() {
  const pathname = usePathname();
  if (pathname?.startsWith("/admin")) {
    return null;
  }

  return (
    <footer className="footer-section">
      <div className="container">
        <div className="footer-content pt-5 pb-5">
          <div className="row">
            <div className="col-md-6 mb-5">
              <div className="footer-widget">
                <div className="footer-logo">
                  <Link href="/" id="logo-text">
                    Sany3y<span className="text-gold">.com</span>
                  </Link>
                </div>
                <div className="footer-text">
                  <p>
                    منصتك الأولى للوصول لأمهر الفنيين والصنايعية في مصر. جودة،
                    ثقة، وسرعة في التنفيذ.
                  </p>
                </div>
                <div className="footer-social-icon">
                  <span>تابعنا</span>
                  <a href="#" aria-label="Facebook">
                    <i className="fa-brands fa-facebook-f"></i>
                  </a>
                  <a href="#" aria-label="Instagram">
                    <i className="fa-brands fa-instagram"></i>
                  </a>
                  <a href="#" aria-label="Twitter">
                    <i className="fa-brands fa-twitter"></i>
                  </a>
                </div>
              </div>
            </div>

            <div className="col-md-3 mb-4">
              <div className="footer-widget">
                <div className="footer-widget-heading">
                  <h3>روابط سريعة</h3>
                </div>
                <ul>
                  <li>
                    <Link href="/">الرئيسية</Link>
                  </li>
                  <li>
                    <Link href="/about">من نحن</Link>
                  </li>
                  <li>
                    <Link href="/services">خدماتنا</Link>
                  </li>
                  <li>
                    <Link href="/contact">تواصل معنا</Link>
                  </li>
                  <li>
                    <Link href="/team">فريق العمل</Link>
                  </li>
                  <li>
                    <Link href="/policy">سياسة الخصوصية</Link>
                  </li>
                </ul>
              </div>
            </div>

            <div className="col-md-3 mb-4">
              <div className="footer-widget">
                <div className="footer-widget-heading">
                  <h3>أهم الخدمات</h3>
                </div>
                <ul>
                  <li>
                    <Link href="/services?serviceType=كهربا">كهرباء</Link>
                  </li>
                  <li>
                    <Link href="/services?serviceType=سباكه">سباكة</Link>
                  </li>
                  <li>
                    <Link href="/services?serviceType=نجاره">نجارة</Link>
                  </li>
                  <li>
                    <Link href="/services?serviceType=نقاشه">نقاشة</Link>
                  </li>
                  <li>
                    <Link href="/services?serviceType=تكييف وتبريد">تكييف</Link>
                  </li>
                  <li>
                    <Link href="/services?serviceType=صيانه اجهزه">
                      صيانة أجهزة
                    </Link>
                  </li>
                </ul>
              </div>
            </div>
          </div>
        </div>
      </div>

      <div className="copyright-area">
        <div className="container">
          <div className="row">
            <div className="col-xl-6 col-lg-6 text-center text-lg-end">
              <div className="copyright-text">
                <p>
                  حقوق النشر &copy; {new Date().getFullYear()} محفوظة لـ{" "}
                  <Link href="/">Sany3y</Link>
                </p>
              </div>
            </div>
            <div className="col-xl-6 col-lg-6 d-none d-lg-block text-start">
              <div className="footer-menu">
                <ul>
                  <li>
                    <Link href="/">الرئيسية</Link>
                  </li>
                  <li>
                    <Link href="/policy">سياسة الخصوصية</Link>
                  </li>
                  <li>
                    <Link href="/contact">تواصل</Link>
                  </li>
                  <li>
                    <Link href="/team">فريق العمل</Link>
                  </li>
                </ul>
              </div>
            </div>
          </div>
        </div>
      </div>
    </footer>
  );
}
