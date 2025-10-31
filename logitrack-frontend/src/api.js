const BASE_URL = "/api/v1";

const TOKEN_KEY = "token";

function getToken() {
  return localStorage.getItem(TOKEN_KEY);
}

function authHeaders(extra = {}) {
  const token = getToken();

  return {
    "Content-Type": "application/json",
    ...(token ? { Authorization: `Bearer ${token}` } : {}),
    ...extra,
  };
}

export async function loginRequest(emailOrUserName, password) {
  const res = await fetch(`${BASE_URL}/auth/login`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify({
      emailOrUserName,
      password,
    }),
  });

  if (!res.ok) {
    throw new Error(`Login failed (${res.status})`);
  }

  return res.json();
}

export async function getShipments() {
  const res = await fetch(`${BASE_URL}/shipments`, {
    headers: authHeaders(),
  });
  if (!res.ok) throw new Error(`Error ${res.status}`);
  return res.json();
}

export async function getShipmentById(id) {
  const res = await fetch(`${BASE_URL}/shipments/${id}`, {
    headers: authHeaders(),
  });
  if (!res.ok) throw new Error(`Error ${res.status}`);
  return res.json();
}

export async function searchShipments(q, status) {
  const params = new URLSearchParams();
  if (q) params.append("q", q);
  if (status) params.append("status", status);

  const res = await fetch(`${BASE_URL}/shipments/search?${params.toString()}`, {
    headers: authHeaders(),
  });
  if (!res.ok) throw new Error(`Error ${res.status}`);
  return res.json();
}

export async function createShipment(body) {
  const res = await fetch(`${BASE_URL}/shipments`, {
    method: "POST",
    headers: authHeaders(),
    body: JSON.stringify(body),
  });
  if (!res.ok) throw new Error(`Error ${res.status}`);
  return res.json();
}

export async function updateShipment(id, body) {
  const res = await fetch(`${BASE_URL}/shipments/${id}`, {
    method: "PUT",
    headers: authHeaders(),
    body: JSON.stringify(body),
  });
  if (!res.ok) throw new Error(`Error ${res.status}`);
  return true;
}

export async function deleteShipment(id) {
  const res = await fetch(`${BASE_URL}/shipments/${id}`, {
    method: "DELETE",
    headers: authHeaders(),
  });
  if (!res.ok) throw new Error(`Error ${res.status}`);
  return true;
}

export async function getCustomers() {
  const res = await fetch(`${BASE_URL}/customers`, {
    headers: authHeaders(),
  });
  if (!res.ok) throw new Error(`Error ${res.status}`);
  return res.json();
}

export async function createCustomer(body) {
  const res = await fetch(`${BASE_URL}/customers`, {
    method: "POST",
    headers: authHeaders(),
    body: JSON.stringify(body),
  });
  if (!res.ok) throw new Error(`Error ${res.status}`);
  return res.json();
}

export async function updateCustomer(id, body) {
  const res = await fetch(`${BASE_URL}/customers/${id}`, {
    method: "PUT",
    headers: authHeaders(),
    body: JSON.stringify(body),
  });
  if (!res.ok) throw new Error(`Error ${res.status}`);
  return true;
}

export async function deleteCustomer(id) {
  const res = await fetch(`${BASE_URL}/customers/${id}`, {
    method: "DELETE",
    headers: authHeaders(),
  });
  if (!res.ok) throw new Error(`Error ${res.status}`);
  return true;
}

export async function getVehicles() {
  const res = await fetch(`${BASE_URL}/vehicles`, {
    headers: authHeaders(),
  });
  if (!res.ok) throw new Error(`Error ${res.status}`);
  return res.json();
}

export async function createVehicle(body) {
  const res = await fetch(`${BASE_URL}/vehicles`, {
    method: "POST",
    headers: authHeaders(),
    body: JSON.stringify(body),
  });
  if (!res.ok) throw new Error(`Error ${res.status}`);
  return res.json();
}

export async function updateVehicle(id, body) {
  const res = await fetch(`${BASE_URL}/vehicles/${id}`, {
    method: "PUT",
    headers: authHeaders(),
    body: JSON.stringify(body),
  });
  if (!res.ok) throw new Error(`Error ${res.status}`);
  return true;
}

export async function deleteVehicle(id) {
  const res = await fetch(`${BASE_URL}/vehicles/${id}`, {
    method: "DELETE",
    headers: authHeaders(),
  });
  if (!res.ok) throw new Error(`Error ${res.status}`);
  return true;
}

export async function getDrivers() {
  const res = await fetch(`${BASE_URL}/drivers`, {
    headers: authHeaders(),
  });
  if (!res.ok) throw new Error(`Error ${res.status}`);
  return res.json();
}

export async function createDriver(body) {
  const res = await fetch(`${BASE_URL}/drivers`, {
    method: "POST",
    headers: authHeaders(),
    body: JSON.stringify(body),
  });
  if (!res.ok) throw new Error(`Error ${res.status}`);
  return res.json();
}

export async function updateDriver(id, body) {
  const res = await fetch(`${BASE_URL}/drivers/${id}`, {
    method: "PUT",
    headers: authHeaders(),
    body: JSON.stringify(body),
  });
  if (!res.ok) throw new Error(`Error ${res.status}`);
  return true;
}

export async function deleteDriver(id) {
  const res = await fetch(`${BASE_URL}/drivers/${id}`, {
    method: "DELETE",
    headers: authHeaders(),
  });
  if (!res.ok) throw new Error(`Error ${res.status}`);
  return true;
}
