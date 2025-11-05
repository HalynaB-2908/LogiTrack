const TOKEN_KEY = "token";
const USERNAME_KEY = "userName";
const ROLES_KEY = "roles"; 

export function saveSession({ token, userName, roles = [] }) {
  if (token) localStorage.setItem(TOKEN_KEY, token);
  if (userName != null) localStorage.setItem(USERNAME_KEY, userName);
  if (Array.isArray(roles)) {
    localStorage.setItem(ROLES_KEY, roles.join(","));
  } else if (typeof roles === "string") {
    localStorage.setItem(ROLES_KEY, roles);
  }
}

export function saveToken(token) {
  localStorage.setItem(TOKEN_KEY, token);
}

export function clearSession() {
  localStorage.removeItem(TOKEN_KEY);
  localStorage.removeItem(USERNAME_KEY);
  localStorage.removeItem(ROLES_KEY);
}

export const clearToken = clearSession;

export function getToken() {
  return localStorage.getItem(TOKEN_KEY);
}

export function getUserName() {
  return localStorage.getItem(USERNAME_KEY) || "";
}

export function getRoles() {
  const raw = localStorage.getItem(ROLES_KEY) || "";
  return raw
    .split(",")
    .map((s) => s.trim())
    .filter(Boolean);
}

export function isLoggedIn() {
  return !!getToken();
}

export function hasRole(role) {
  return getRoles().includes(role);
}

export function isAdmin() {
  return hasRole("Admin");
}
