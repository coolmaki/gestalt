import { getAccessToken, getRefreshToken, loginFromSession, logout } from "../signals/auth";

const BASE_URL = "";

function friendlyError(response: Response, path: string): string {
  const { status } = response;

  if (status === 401) return "Session expired. Please sign in again.";

  if (path.includes("/login")) return "Could not sign in. Please try again.";
  if (path.includes("/register")) return "Could not create your account. Please try again.";
  if (path.includes("/recovery")) return "Recovery failed. Please try again.";
  if (path.includes("/verify")) return "Verification failed. Please check your code and try again.";
  if (path.includes("/credentials")) return "Could not update passkeys. Please try again.";
  if (path.includes("/token")) return "Session error. Please sign in again.";

  return "Something went wrong. Please try again.";
}

async function handleResponse<T>(response: Response): Promise<T> {
  if (response.status === 204 || response.headers.get("Content-Length") === "0") {
    return undefined as unknown as T;
  }

  const body = await response.json();

  if (!response.ok) {
    const detail = body.errors?.[0]?.message ?? body.detail ?? body.title ?? response.statusText;
    console.error(`[API] ${response.status} on ${response.url}:`, detail, body);
    throw new Error(friendlyError(response, response.url));
  }

  if (body.success !== undefined) {
    if (!body.success) {
      const detail = body.errors?.[0]?.message ?? "Request failed";
      console.error("[API] unsuccessful response:", detail, body);
      throw new Error(friendlyError(response, response.url));
    }
    return body.data as T;
  }

  return body as T;
}

async function refreshAccessToken(): Promise<boolean> {
  const token = getRefreshToken();
  if (!token) return false;

  try {
    const response = await fetch(`${BASE_URL}/api/v1/auth/token/refresh`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ refreshToken: token }),
    });

    if (!response.ok) return false;

    const body = await response.json();
    const result = body.success ? body.data : body;

    loginFromSession(result.accessToken, result.refreshToken);
    return true;
  } catch (err) {
    console.error("[API] Token refresh failed:", err);
    return false;
  }
}

async function request<T>(method: string, path: string, body?: unknown): Promise<T> {
  const headers: Record<string, string> = { "Content-Type": "application/json" };
  const token = getAccessToken();
  if (token) {
    headers["Authorization"] = `Bearer ${token}`;
  }

  let response = await fetch(`${BASE_URL}${path}`, {
    method,
    headers,
    body: body ? JSON.stringify(body) : undefined,
  });

  if (response.status === 401 && token) {
    const refreshed = await refreshAccessToken();
    if (refreshed) {
      headers["Authorization"] = `Bearer ${getAccessToken()}`;
      response = await fetch(`${BASE_URL}${path}`, {
        method,
        headers,
        body: body ? JSON.stringify(body) : undefined,
      });
    } else {
      logout();
      throw new Error("Session expired. Please log in again.");
    }
  }

  return handleResponse<T>(response);
}

export const api = {
  get: <T>(path: string) => request<T>("GET", path),
  post: <T>(path: string, body?: unknown) => request<T>("POST", path, body),
  del: <T>(path: string, body?: unknown) => request<T>("DELETE", path, body),
};