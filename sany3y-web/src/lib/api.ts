export const API_BASE_URL = (
  process.env.NEXT_PUBLIC_API_URL || "https://localhost:7178"
).replace(/\/$/, "");

export function getStoredToken(): string | null {
  if (typeof window === "undefined") return null;
  return localStorage.getItem("sany3y_token");
}

export async function apiFetch<T = any>(
  endpoint: string,
  options: RequestInit = {}
): Promise<T> {
  const url = endpoint.startsWith("http")
    ? endpoint
    : `${API_BASE_URL}${endpoint.startsWith("/") ? "" : "/"}${endpoint}`;

  const token = getStoredToken();
  const headers = new Headers(options.headers || {});

  if (token && !headers.has("Authorization")) {
    headers.set("Authorization", `Bearer ${token}`);
  }

  if (
    options.body &&
    !(options.body instanceof FormData) &&
    !headers.has("Content-Type")
  ) {
    headers.set("Content-Type", "application/json");
  }

  const response = await fetch(url, {
    ...options,
    headers,
  });

  if (!response.ok) {
    let errorData: any = null;
    const text = await response.text();
    try {
      errorData = JSON.parse(text);
    } catch {
      errorData = { message: text || `HTTP Error ${response.status}` };
    }
    const err: any = new Error(
      errorData?.message ||
        (Array.isArray(errorData?.errors) ? errorData.errors[0] : null) ||
        `حدث خطأ (${response.status})`
    );
    err.status = response.status;
    err.data = errorData;
    throw err;
  }

  if (response.status === 204) {
    return null as T;
  }

  const contentType = response.headers.get("content-type") || "";
  if (contentType.includes("application/json")) {
    return response.json();
  }
  return (await response.text()) as unknown as T;
}

export async function downloadApiFile(endpoint: string, defaultFilename: string) {
  const url = `${API_BASE_URL}${endpoint.startsWith("/") ? "" : "/"}${endpoint}`;
  const token = getStoredToken();
  const headers = new Headers();
  if (token) {
    headers.set("Authorization", `Bearer ${token}`);
  }

  const res = await fetch(url, { headers });
  if (!res.ok) {
    throw new Error("فشل تحميل الملف");
  }

  const blob = await res.blob();
  const objectUrl = window.URL.createObjectURL(blob);
  const a = document.createElement("a");
  a.href = objectUrl;
  a.download = defaultFilename;
  document.body.appendChild(a);
  a.click();
  a.remove();
  window.URL.revokeObjectURL(objectUrl);
}
