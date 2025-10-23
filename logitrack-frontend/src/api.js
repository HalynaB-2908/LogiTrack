const BASE_URL = "/api/v1";

export async function getShipments() {
  const res = await fetch(`${BASE_URL}/shipments`);
  if (!res.ok) throw new Error(`Error ${res.status}`);
  return res.json();
}

export async function getShipmentById(id) {
  const res = await fetch(`${BASE_URL}/shipments/${id}`);
  if (!res.ok) throw new Error(`Error ${res.status}`);
  return res.json();
}

export async function searchShipments(q, status) {
  const params = new URLSearchParams();
  if (q) params.append("q", q);
  if (status) params.append("status", status);
  const res = await fetch(`${BASE_URL}/shipments/search?${params.toString()}`);
  if (!res.ok) throw new Error(`Error ${res.status}`);
  return res.json();
}

export async function createShipment(body) {
  const res = await fetch(`${BASE_URL}/shipments`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(body),
  });
  if (!res.ok) throw new Error(`Error ${res.status}`);
  return res.json();
}

export async function updateShipment(id, body) {
  const res = await fetch(`${BASE_URL}/shipments/${id}`, {
    method: "PUT",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(body),
  });
  if (!res.ok) throw new Error(`Error ${res.status}`);
  return true;
}

export async function deleteShipment(id) {
  const res = await fetch(`${BASE_URL}/shipments/${id}`, { method: "DELETE" });
  if (!res.ok) throw new Error(`Error ${res.status}`);
  return true;
}

// Dictionaries
export async function getCustomers() {
  const res = await fetch(`${BASE_URL}/customers`);
  if (!res.ok) throw new Error(`Error ${res.status}`);
  return res.json();
}

export async function getVehicles() {
  const res = await fetch(`${BASE_URL}/vehicles`);
  if (!res.ok) throw new Error(`Error ${res.status}`);
  return res.json();
}
