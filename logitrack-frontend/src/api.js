const BASE_URL = '/api/v1';

// Shipments
export async function getShipmentById(id) {
  const res = await fetch(`${BASE_URL}/shipments/${id}`);
  if (!res.ok) throw new Error(`Error ${res.status}`);
  return res.json();
}

export async function searchShipments(q, status) {
  const params = new URLSearchParams();
  if (q) params.append('q', q);
  if (status) params.append('status', status);
  const res = await fetch(`${BASE_URL}/shipments/search?` + params.toString());
  if (!res.ok) throw new Error(`Error ${res.status}`);
  return res.json();
}

export async function createShipment(body) {
  const res = await fetch(`${BASE_URL}/shipments`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(body)
  });
  if (!res.ok) throw new Error(`Error ${res.status}`);
  return res.json();
}

// Customers
export async function getCustomerById(id) {
  const res = await fetch(`${BASE_URL}/customers/${id}`);
  if (!res.ok) throw new Error(`Error ${res.status}`);
  return res.json();
}

// Vehicles
export async function getVehicleById(id) {
  const res = await fetch(`${BASE_URL}/vehicles/${id}`);
  if (!res.ok) throw new Error(`Error ${res.status}`);
  return res.json();
}

