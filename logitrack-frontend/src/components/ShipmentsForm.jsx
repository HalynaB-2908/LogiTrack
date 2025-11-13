import { useEffect, useMemo, useState } from "react";
import {
  getShipments,
  searchShipments,
  createShipment,
  updateShipment,
  deleteShipment,
  getCustomers,
  getVehicles,
} from "../api";
import { isAdmin } from "../auth";

const statuses = ["Planned", "InTransit", "Delivered", "Cancelled"];
const deliveryModes = ["standard", "express", "eco"];

export default function ShipmentsForm() {
  const [items, setItems] = useState([]);
  const [loading, setLoading] = useState(false);

  const [customers, setCustomers] = useState([]);
  const [vehicles, setVehicles] = useState([]);

  const [q, setQ] = useState("");
  const [status, setStatus] = useState("");

  const initialForm = {
    reference: "",
    distanceKm: "",
    weightKg: "",
    customerId: "",
    vehicleId: "",
    deliveryMode: "standard",
  };
  const [form, setForm] = useState(initialForm);
  const [editingId, setEditingId] = useState(null);

  const [error, setError] = useState(null);
  const [note, setNote] = useState(null);

  const admin = isAdmin();

  const run = async (fn, after) => {
    setError(null);
    setNote(null);
    setLoading(true);
    try {
      const data = await fn();
      after?.(data);
    } catch (e) {
      setError(String(e));
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    refresh();
    run(
      () => getCustomers(),
      (data) => setCustomers(data)
    );
    run(
      () => getVehicles(),
      (data) => setVehicles(data)
    );
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const refresh = () =>
    run(
      () => getShipments(),
      (data) => setItems(data)
    );

  const customerById = useMemo(() => {
    const map = new Map();
    customers.forEach((c) => map.set(c.id, c));
    return map;
  }, [customers]);

  const vehicleById = useMemo(() => {
    const map = new Map();
    vehicles.forEach((v) => map.set(v.id, v));
    return map;
  }, [vehicles]);

  const handleCreate = () => {
    if (!admin) return;
    if (!form.reference || Number(form.distanceKm) <= 0) {
      setError("Reference required and DistanceKm must be > 0");
      return;
    }
    if (!form.customerId || !form.vehicleId) {
      setError("Select Customer and Vehicle");
      return;
    }
    const payload = {
      reference: form.reference.trim(),
      distanceKm: Number(form.distanceKm),
      weightKg: Number(form.weightKg || 0),
      customerId: Number(form.customerId),
      vehicleId: Number(form.vehicleId),
      deliveryMode: form.deliveryMode || "standard",
    };
    run(
      () => createShipment(payload),
      () => {
        setForm(initialForm);
        setNote("Shipment created");
        refresh();
      }
    );
  };

  const startEdit = (row) => {
    if (!admin) return;
    setEditingId(row.id);
    setForm({
      reference: row.reference ?? "",
      distanceKm: row.distanceKm ?? "",
      weightKg: row.weightKg ?? "",
      customerId: row.customerId ?? "",
      vehicleId: row.vehicleId ?? "",
      deliveryMode: "standard",
    });
  };

  const handleUpdate = () => {
    if (!admin || editingId == null) return;
    if (!form.reference || Number(form.distanceKm) <= 0) {
      setError("Reference required and DistanceKm must be > 0");
      return;
    }
    if (!form.customerId || !form.vehicleId) {
      setError("Select Customer and Vehicle");
      return;
    }
    const payload = {
      reference: form.reference.trim(),
      distanceKm: Number(form.distanceKm),
      weightKg: Number(form.weightKg || 0),
      customerId: Number(form.customerId),
      vehicleId: Number(form.vehicleId),
      deliveryMode: form.deliveryMode || "standard",
    };
    run(
      () => updateShipment(editingId, payload),
      () => {
        setEditingId(null);
        setForm(initialForm);
        setNote("Shipment updated");
        refresh();
      }
    );
  };

  const handleDelete = (id) => {
    if (!admin) return;
    if (!confirm(`Delete shipment #${id}?`)) return;
    run(
      () => deleteShipment(id),
      () => {
        setNote("Shipment deleted");
        refresh();
      }
    );
  };

  const handleSearch = () => {
    if (!q && !status) return refresh();
    run(
      () => searchShipments(q, status),
      (data) => setItems(data)
    );
  };

  return (
    <section className="card wide">
      <div className="card-header">
        <h2>Shipments</h2>
      </div>

      {error && <div className="alert danger">{error}</div>}
      {note && <div className="alert success">{note}</div>}

      {admin && (
        <div className="row">
          <h3>{editingId ? "Edit #${editingId}" : "Create"}</h3>
          <div className="controls grid-2">
            <input
              className="input"
              placeholder="Reference"
              value={form.reference}
              onChange={(e) =>
                setForm((f) => ({ ...f, reference: e.target.value }))
              }
            />

            <select
              className="input"
              value={form.customerId}
              onChange={(e) =>
                setForm((f) => ({ ...f, customerId: e.target.value }))
              }
            >
              <option value="">Select customer…</option>
              {customers.map((c) => (
                <option key={c.id} value={c.id}>
                  {c.name}
                </option>
              ))}
            </select>

            <input
              className="input"
              placeholder="DistanceKm"
              type="number"
              value={form.distanceKm}
              onChange={(e) =>
                setForm((f) => ({ ...f, distanceKm: e.target.value }))
              }
            />

            <select
              className="input"
              value={form.vehicleId}
              onChange={(e) =>
                setForm((f) => ({ ...f, vehicleId: e.target.value }))
              }
            >
              <option value="">Select vehicle…</option>
              {vehicles.map((v) => (
                <option key={v.id} value={v.id}>
                  {v.plateNumber} — {v.model}
                </option>
              ))}
            </select>

            <input
              className="input"
              placeholder="WeightKg"
              type="number"
              value={form.weightKg}
              onChange={(e) =>
                setForm((f) => ({ ...f, weightKg: e.target.value }))
              }
            />

            {/* NEW: Delivery mode select */}
            <select
              className="input"
              value={form.deliveryMode}
              onChange={(e) =>
                setForm((f) => ({ ...f, deliveryMode: e.target.value }))
              }
            >
              {deliveryModes.map((m) => (
                <option key={m} value={m}>
                  {m}
                </option>
              ))}
            </select>

            <div className="inline-actions">
              {!editingId ? (
                <button
                  className="btn primary"
                  onClick={handleCreate}
                  disabled={loading}
                >
                  Create
                </button>
              ) : (
                <>
                  <button
                    className="btn primary"
                    onClick={handleUpdate}
                    disabled={loading}
                  >
                    Save
                  </button>
                  <button
                    className="btn"
                    onClick={() => {
                      setEditingId(null);
                      setForm(initialForm);
                    }}
                  >
                    Cancel
                  </button>
                </>
              )}
            </div>
          </div>
        </div>
      )}

      <div className="row">
        <h3>Search</h3>
        <div className="controls">
          <input
            className="input"
            placeholder="q (in Reference)"
            value={q}
            onChange={(e) => setQ(e.target.value)}
          />
          <select
            className="input"
            value={status}
            onChange={(e) => setStatus(e.target.value)}
          >
            <option value="">status: any</option>
            {statuses.map((s) => (
              <option key={s} value={s}>
                {s}
              </option>
            ))}
          </select>
          <button className="btn" onClick={handleSearch} disabled={loading}>
            Search
          </button>
          <button
            className="btn ghost"
            onClick={() => {
              setQ("");
              setStatus("");
              refresh();
            }}
            disabled={loading}
          >
            Reset
          </button>
        </div>
      </div>

      <div className="row">
        <h3>List</h3>
        <div className="table-wrap">
          <table className="table">
            <thead>
              <tr>
                <th>ID</th>
                <th>Reference</th>
                <th>Status</th>
                <th>Customer</th>
                <th>Vehicle</th>
                <th>DistanceKm</th>
                <th>WeightKg</th>
                <th>CreatedUtc</th>
                <th>Price</th>
                <th>EstimatedTime (h)</th>
                {admin && <th></th>}
              </tr>
            </thead>
            <tbody>
              {items.length === 0 && (
                <tr>
                  <td
                    colSpan={admin ? 11 : 10}
                    style={{ textAlign: "center", color: "#6b7280" }}
                  >
                    {loading ? "Loading…" : "No data"}
                  </td>
                </tr>
              )}
              {items.map((row) => {
                const c = customerById.get(row.customerId);
                const v = vehicleById.get(row.vehicleId);
                return (
                  <tr key={row.id}>
                    <td>{row.id}</td>
                    <td>{row.reference}</td>
                    <td>{row.status}</td>
                    <td>{c ? c.name : row.customerId}</td>
                    <td>
                      {v ? "${v.plateNumber} — ${v.model}" : row.vehicleId}
                    </td>
                    <td>{row.distanceKm}</td>
                    <td>{row.weightKg}</td>
                    <td>{new Date(row.createdUtc).toLocaleString()}</td>
                    <td>
                      {row.estimatedPrice != null
                        ? `${row.estimatedPrice.toFixed(2)} ${
                            row.currency || ""
                          }`
                        : "-"}
                    </td>
                    <td>
                      {row.estimatedTimeHours != null
                        ? row.estimatedTimeHours.toFixed(2)
                        : "-"}
                    </td>
                    {admin && (
                      <td style={{ textAlign: "right" }}>
                        <button
                          className="btn"
                          onClick={() => startEdit(row)}
                          disabled={loading}
                        >
                          Edit
                        </button>
                        <button
                          className="btn danger"
                          onClick={() => handleDelete(row.id)}
                          disabled={loading}
                          style={{ marginLeft: 8 }}
                        >
                          Delete
                        </button>
                      </td>
                    )}
                  </tr>
                );
              })}
            </tbody>
          </table>
        </div>
      </div>
    </section>
  );
}
