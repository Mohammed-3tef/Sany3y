"use client";

import React, { useEffect, useRef, useState } from "react";
import Link from "next/link";
import {
  Chart as ChartJS,
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  ArcElement,
  Title,
  Tooltip,
  Legend,
  Filler,
} from "chart.js";
import { Line, Doughnut } from "react-chartjs-2";
import { apiFetch } from "@/lib/api";
import { useAuth } from "@/context/AuthContext";
import { AdminStats } from "@/types";

ChartJS.register(
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  ArcElement,
  Title,
  Tooltip,
  Legend,
  Filler
);

export default function AdminOverviewPage() {
  const { user } = useAuth();
  const [stats, setStats] = useState<AdminStats | null>(null);
  const [monthlyData, setMonthlyData] = useState<
    { month: string; userCount: number }[]
  >([]);
  const [genderData, setGenderData] = useState<
    { gender: string; count: number }[]
  >([]);
  const [loading, setLoading] = useState(true);

  const lineChartRef = useRef<any>(null);
  const doughnutChartRef = useRef<any>(null);

  useEffect(() => {
    Promise.all([
      apiFetch<AdminStats>("/api/Admin/stats"),
      apiFetch<{ month: string; userCount: number }[]>(
        "/api/Admin/charts/monthly-users"
      ),
      apiFetch<{ gender: string; count: number }[]>(
        "/api/Admin/charts/gender-distribution"
      ),
    ])
      .then(([s, m, g]) => {
        setStats(s);
        setMonthlyData(m || []);
        setGenderData(g || []);
      })
      .finally(() => setLoading(false));
  }, []);

  const downloadChart = (chartRef: any, fileName: string) => {
    const chart = chartRef.current;
    if (!chart) return;
    const url = chart.toBase64Image();
    const a = document.createElement("a");
    a.href = url;
    a.download = `${fileName}.png`;
    a.click();
  };

  if (loading) {
    return (
      <div className="text-center py-5">
        <div className="spinner-border text-success" />
        <p className="mt-2 text-muted">جاري تحميل الإحصائيات...</p>
      </div>
    );
  }

  return (
    <div>
      <div className="mb-4">
        <h2 className="fw-bold text-dark">نظرة عامة</h2>
        <p className="text-muted">
          مرحبًا {user?.userName || "Admin"}، إليك ملخص لما يحدث اليوم.
        </p>
      </div>

      {/* Stats Row */}
      <div className="row g-4 mb-4">
        {[
          {
            label: "إجمالي المستخدمين",
            value: stats?.totalUsers ?? 0,
            icon: "fa-users",
            bg: "rgba(13, 110, 253, 0.1)",
            color: "#0d6efd",
          },
          {
            label: "عدد الفنيين",
            value: stats?.totalTaskers ?? 0,
            icon: "fa-tools",
            bg: "rgba(25, 135, 84, 0.1)",
            color: "#198754",
          },
          {
            label: "عدد المحلات",
            value: stats?.totalStores ?? 0,
            icon: "fa-store",
            bg: "rgba(25, 135, 84, 0.1)",
            color: "#198754",
          },
          {
            label: "التصنيفات",
            value: stats?.totalCategories ?? 0,
            icon: "fa-tags",
            bg: "rgba(255, 193, 7, 0.1)",
            color: "#ffc107",
          },
        ].map((card) => (
          <div key={card.label} className="col-md-3">
            <div className="bg-white rounded-4 p-4 shadow-sm border d-flex align-items-center justify-content-between h-100">
              <div>
                <h6 className="text-muted mb-2">{card.label}</h6>
                <h3 className="fw-bold mb-0">{card.value}</h3>
              </div>
              <div
                className="rounded-3 d-flex align-items-center justify-content-center"
                style={{
                  width: 60,
                  height: 60,
                  backgroundColor: card.bg,
                  color: card.color,
                  fontSize: "1.8rem",
                }}
              >
                <i className={`fas ${card.icon}`}></i>
              </div>
            </div>
          </div>
        ))}
      </div>

      {/* Charts Row */}
      <div className="row g-4 mb-4">
        <div className="col-lg-8">
          <div className="bg-white rounded-4 p-4 shadow-sm border h-100">
            <div className="d-flex justify-content-between align-items-center mb-3">
              <h5 className="fw-bold mb-0">
                نمو المستخدمين شهريًا ({new Date().getFullYear()})
              </h5>
              <button
                type="button"
                className="btn btn-sm btn-light rounded-circle"
                onClick={() => downloadChart(lineChartRef, "Users-Growth")}
              >
                <i className="fas fa-download text-muted"></i>
              </button>
            </div>
            <div style={{ height: 300 }}>
              <Line
                ref={lineChartRef}
                data={{
                  labels: monthlyData.map((d) => d.month),
                  datasets: [
                    {
                      label: "المستخدمين الجدد",
                      data: monthlyData.map((d) => d.userCount),
                      borderColor: "#28a745",
                      backgroundColor: "rgba(40, 167, 69, 0.1)",
                      borderWidth: 3,
                      tension: 0.4,
                      fill: true,
                    },
                  ],
                }}
                options={{
                  responsive: true,
                  maintainAspectRatio: false,
                  plugins: { legend: { display: false } },
                  scales: {
                    y: { beginAtZero: true },
                    x: { grid: { display: false } },
                  },
                }}
              />
            </div>
          </div>
        </div>

        <div className="col-lg-4">
          <div className="bg-white rounded-4 p-4 shadow-sm border h-100">
            <div className="d-flex justify-content-between align-items-center mb-3">
              <h5 className="fw-bold mb-0">توزيع المستخدمين</h5>
              <button
                type="button"
                className="btn btn-sm btn-light rounded-circle"
                onClick={() => downloadChart(doughnutChartRef, "Gender-Dist")}
              >
                <i className="fas fa-download text-muted"></i>
              </button>
            </div>
            <div
              style={{
                height: 300,
                display: "flex",
                alignItems: "center",
                justifyContent: "center",
              }}
            >
              <Doughnut
                ref={doughnutChartRef}
                data={{
                  labels: genderData.map((d) => d.gender),
                  datasets: [
                    {
                      data: genderData.map((d) => d.count),
                      backgroundColor: ["#0d6efd", "#e83e8c"],
                      borderWidth: 0,
                    },
                  ],
                }}
                options={{
                  responsive: true,
                  maintainAspectRatio: false,
                  cutout: "70%",
                  plugins: {
                    legend: { position: "bottom" },
                  },
                }}
              />
            </div>
          </div>
        </div>
      </div>

      {/* Top Categories Table */}
      {stats?.topCategories && stats.topCategories.length > 0 && (
        <div className="bg-white rounded-4 p-4 shadow-sm border">
          <div className="d-flex justify-content-between align-items-center mb-3">
            <h5 className="fw-bold mb-0">أكثر التصنيفات طلبًا</h5>
            <Link
              href="/admin/categories"
              className="btn btn-sm btn-outline-primary rounded-pill px-3"
            >
              عرض الكل
            </Link>
          </div>
          <div className="table-responsive">
            <table className="table table-hover align-middle">
              <thead className="bg-light">
                <tr>
                  <th>اسم التصنيف</th>
                  <th>عدد المهام</th>
                  <th className="text-start">الحالة</th>
                </tr>
              </thead>
              <tbody>
                {stats.topCategories.map((cat) => (
                  <tr key={cat.id}>
                    <td className="fw-bold">{cat.name}</td>
                    <td>{cat.count}</td>
                    <td className="text-start">
                      <span className="badge bg-success bg-opacity-10 text-success rounded-pill">
                        نشط
                      </span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}
    </div>
  );
}
