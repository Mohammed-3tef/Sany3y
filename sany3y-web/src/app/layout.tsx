import type { Metadata } from "next";
import "bootstrap/dist/css/bootstrap.rtl.min.css";
import "@fortawesome/fontawesome-free/css/all.min.css";
import "./globals.css";
import { Providers } from "@/components/Providers";
import { Navbar } from "@/components/Navbar";
import { Footer } from "@/components/Footer";
import { ScrollToTopButton } from "@/components/ScrollToTopButton";

export const metadata: Metadata = {
  title: "Sany3y - صنايعي | منصة الخدمات والصيانة المنزلية",
  description:
    "منصتك الأولى للوصول لأمهر الفنيين والصنايعية والمحلات في مصر. جودة، ثقة، وسرعة في التنفيذ.",
  icons: {
    icon: "/favicon.png",
  },
};

export default function RootLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <html lang="ar" dir="rtl">
      <head>
        <link
          href="https://fonts.googleapis.com/css2?family=Cairo:wght@300;400;600;700;800;900&display=swap"
          rel="stylesheet"
        />
      </head>
      <body dir="rtl">
        <Providers>
          <Navbar />
          <main role="main">{children}</main>
          <ScrollToTopButton />
          <Footer />
        </Providers>
      </body>
    </html>
  );
}
