import { createSignal, createEffect, onMount } from "solid-js";

// --- Token Storage ---

function getStoredTokens(): { accessToken: string | null; refreshToken: string | null } {
  try {
    const access = sessionStorage.getItem("passport-access-token");
    const refresh = sessionStorage.getItem("passport-refresh-token");
    return { accessToken: access, refreshToken: refresh };
  } catch {
    return { accessToken: null, refreshToken: null };
  }
}

function storeTokens(accessToken: string, refreshToken: string): void {
  try {
    sessionStorage.setItem("passport-access-token", accessToken);
    sessionStorage.setItem("passport-refresh-token", refreshToken);
  } catch {
    // sessionStorage unavailable
  }
}

function clearStoredTokens(): void {
  try {
    sessionStorage.removeItem("passport-access-token");
    sessionStorage.removeItem("passport-refresh-token");
  } catch {
    // sessionStorage unavailable
  }
}

// --- JWT Decode ---

function decodeJwtPayload(token: string): Record<string, unknown> | null {
  try {
    const parts = token.split(".");
    if (parts.length !== 3) return null;
    const payload = parts[1];
    const decoded = atob(payload.replace(/-/g, "+").replace(/_/g, "/"));
    return JSON.parse(decoded);
  } catch {
    return null;
  }
}

function getEmailFromToken(token: string): string | null {
  const payload = decodeJwtPayload(token);
  if (!payload) return null;
  return (payload.email as string) ?? null;
}

function isTokenExpired(token: string): boolean {
  const payload = decodeJwtPayload(token);
  if (!payload) return true;
  const exp = payload.exp as number;
  if (!exp) return true;
  return Date.now() >= exp * 1000;
}

// --- Auth Signals ---

const [accessToken, setAccessToken] = createSignal<string | null>(null);
const [refreshToken, setRefreshToken] = createSignal<string | null>(null);
const [currentEmail, setCurrentEmail] = createSignal<string | null>(null);
const [isAuthenticated, setIsAuthenticated] = createSignal(false);

export function loginFromSession(newAccessToken: string, newRefreshToken: string): void {
  storeTokens(newAccessToken, newRefreshToken);
  setAccessToken(newAccessToken);
  setRefreshToken(newRefreshToken);
  setCurrentEmail(getEmailFromToken(newAccessToken));
  setIsAuthenticated(true);
}

export function logout(): void {
  clearStoredTokens();
  setAccessToken(null);
  setRefreshToken(null);
  setCurrentEmail(null);
  setIsAuthenticated(false);
  window.location.href = "/auth/login";
}

export function restoreSession(): boolean {
  const { accessToken: storedAccess, refreshToken: storedRefresh } = getStoredTokens();
  if (!storedAccess || !storedRefresh) return false;
  if (isTokenExpired(storedAccess) && isTokenExpired(storedRefresh)) {
    clearStoredTokens();
    return false;
  }
  setAccessToken(storedAccess);
  setRefreshToken(storedRefresh);
  setCurrentEmail(getEmailFromToken(storedAccess));
  setIsAuthenticated(true);
  return true;
}

export function getAccessToken(): string | null {
  return accessToken();
}

export function getRefreshToken(): string | null {
  return refreshToken();
}

export { currentEmail, isAuthenticated, accessToken, refreshToken };