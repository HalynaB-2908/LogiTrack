import { useEffect, useMemo, useState } from "react";
import {
  getVehicles,
  createVehicle,
  updateVehicle,
  deleteVehicle,
  getDrivers,
} from "../api";
import { isAdmin } from "../auth";

export default function VehiclesForm() {
  const admin = isAdmin();
  const [items, setItems] = useState([]);
  const [drivers, setDrivers] = useState([]);
  const [loading, setLoading] = useState(false);

  const initialForm = {
    plateNumber: "",
    model: "",
    capacityKg: "",
    driverId: "",
  };
  const [form, setForm] = useState(initialForm);
  const [editingId, setEditingId] = useState(null);

  const [error, setError] = useState(null);
  const [note, setNote] = useState(null);

  const [showModal, setShowModal] = useState(false);

  const driverById = useMemo(() => {
    const map = new Map();
    drivers.forEach((d) => map.set(d.id, d));
    return map;
  }, [drivers]);

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

  const refresh = () =>
    run(
      () => getVehicles(),
      (data) => setItems(data)
    );

  useEffect(() => {
    refresh();
    run(
      () => getDrivers(),
      (data) => setDrivers(data)
    );
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const openCreate = () => {
    setEditingId(null);
    setForm(initialForm);
    setShowModal(true);
  };

  const openEdit = (row) => {
    setEditingId(row.id);
    setForm({
      plateNumber: row.plateNumber ?? "",
      model: row.model ?? "",
      capacityKg: row.capacityKg ?? "",
      driverId: row.driverId ?? "",
    });
    setShowModal(true);
  };

  const closeModal = () => {
    setShowModal(false);
    setForm(initialForm);
    setEditingId(null);
  };

  const submit = () => {
    if (!form.plateNumber.trim() || !form.model.trim()) {
      setError("Plate number and Model are required");
      return;
    }
    const payload = {
      plateNumber: form.plateNumber.trim(),
      model: form.model.trim(),
      capacityKg: Number(form.capacityKg || 0),
      driverId: form.driverId ? Number(form.driverId) : null,
    };

    if (!editingId) {
      run(
        () => createVehicle(payload),
        () => {
          setNote("Vehicle created");
          closeModal();
          refresh();
        }
      );
    } else {
      run(
        () => updateVehicle(editingId, payload),
        () => {
          setNote("Vehicle updated");
          closeModal();
          refresh();
        }
      );
    }
  };

  const remove = (id) => {
    if (!confirm(`Delete vehicle #${id}?`)) return;
    run(
      () => deleteVehicle(id),
      () => {
        setNote("Vehicle deleted");
        refresh();
      }
    );
  };

  return (
    <>
      <div className="row" style={{ marginTop: 0 }}>
        <div className="header-actions">
          {admin && (
            <button
              className="btn primary"
              onClick={openCreate}
              disabled={loading}
            >
              Add vehicle
            </button>
          )}
        </div>
      </div>

      {error && <div className="alert danger">{error}</div>}
      {note && <div className="alert success">{note}</div>}

      <div className="row">
        <div className="table-wrap">
          <table className="table">
            <thead>
              <tr>
                <th style={{ width: 70 }}>ID</th>
                <th>Plate</th>
                <th>Model</th>
                <th style={{ width: 120 }}>Capacity, kg</th>
                <th>Driver</th>
                <th style={{ width: 160 }}></th>
              </tr>
            </thead>
            <tbody>
              {items.length === 0 && (
                <tr>
                  <td colSpan={6} style={{ textAlign: "center" }}>
                    {loading ? "Loading…" : "No data"}
                  </td>
                </tr>
              )}
              {items.map((row) => {
                const d = driverById.get(row.driverId);
                return (
                  <tr key={row.id}>
                    <td>{row.id}</td>
                    <td>{row.plateNumber}</td>
                    <td>{row.model}</td>
                    <td>{row.capacityKg}</td>
                    <td>{d ? d.fullName : row.driverId || "-"}</td>
                    <td style={{ textAlign: "right" }}>
                      {admin && (
                        <>
                          <button
                            className="btn"
                            onClick={() => openEdit(row)}
                            disabled={loading}
                          >
                            Edit
                          </button>
                          <button
                            className="btn danger"
                            onClick={() => remove(row.id)}
                            disabled={loading}
                            style={{ marginLeft: 8 }}
                          >
                            Delete
                          </button>
                        </>
                      )}
                    </td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        </div>
      </div>

      {showModal && (
        <div className="modal-backdrop" onClick={closeModal}>
          <div className="modal" onClick={(e) => e.stopPropagation()}>
            <h3 style={{ marginBottom: 12 }}>
              {editingId ? `Edit #${editingId}` : "Add vehicle"}
            </h3>
            <div className="controls" style={{ marginTop: 12 }}>
              <input
                className="input"
                placeholder="Plate number"
                value={form.plateNumber}
                onChange={(e) =>
                  setForm((f) => ({ ...f, plateNumber: e.target.value }))
                }
              />
              <input
                className="input"
                placeholder="Model"
                value={form.model}
                onChange={(e) =>
                  setForm((f) => ({ ...f, model: e.target.value }))
                }
              />
              <input
                className="input"
                placeholder="Capacity, kg"
                type="number"
                value={form.capacityKg}
                onChange={(e) =>
                  setForm((f) => ({ ...f, capacityKg: e.target.value }))
                }
              />
              <select
                className="input"
                value={form.driverId}
                onChange={(e) =>
                  setForm((f) => ({ ...f, driverId: e.target.value }))
                }
              >
                <option value="">No driver</option>
                {drivers.map((d) => (
                  <option key={d.id} value={d.id}>
                    {d.fullName}
                  </option>
                ))}
              </select>
            </div>
            <div className="modal-actions">
              <button className="btn" onClick={closeModal} disabled={loading}>
                Cancel
              </button>
              <button
                className="btn primary"
                onClick={submit}
                disabled={loading}
              >
                {editingId ? "Save" : "Create"}
              </button>
            </div>
          </div>
        </div>
      )}
    </>
  );
}
