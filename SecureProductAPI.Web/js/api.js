const BASE_URL = "https://localhost:56919/api"; 

function getAccessToken() {
  return localStorage.getItem("accessToken");
}

function getRefreshToken() {
  return localStorage.getItem("refreshToken");
}

function saveTokens(accessToken, refreshToken) {
  localStorage.setItem("accessToken", accessToken);
  localStorage.setItem("refreshToken", refreshToken);
}

function clearTokens() {
  localStorage.removeItem("accessToken");
  localStorage.removeItem("refreshToken");
}

function decodeToken(token) {
  try {
    const payload = token.split(".")[1];
    return JSON.parse(atob(payload));
  } catch {
    return null;
  }
}

function getUserRole() {
  const token = getAccessToken();
  if (!token) return null;
  const decoded = decodeToken(token);
  // role claim key from ClaimTypes.Role
  return decoded?.["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"] || decoded?.role || null;
}

async function apiFetch(endpoint, options = {}) {
  const token = getAccessToken();
  const headers = {
    "Content-Type": "application/json",
    ...(options.headers || {}),
    ...(token ? { Authorization: `Bearer ${token}` } : {})
  };

  let response = await fetch(`${BASE_URL}${endpoint}`, { ...options, headers });

  // auto-refresh once on 401
  if (response.status === 401 && getRefreshToken()) {
    const refreshed = await tryRefreshToken();
    if (refreshed) {
      const newToken = getAccessToken();
      response = await fetch(`${BASE_URL}${endpoint}`, {
        ...options,
        headers: { ...headers, Authorization: `Bearer ${newToken}` }
      });
    } else {
      clearTokens();
      window.location.href = "login.html";
    }
  }

  return response;
}

async function tryRefreshToken() {
  try {
    const res = await fetch(`${BASE_URL}/auth/refresh-token`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ refreshToken: getRefreshToken() })
    });
    if (!res.ok) return false;
    const data = await res.json();
    saveTokens(data.accessToken, data.refreshToken);
    return true;
  } catch {
    return false;
  }
}