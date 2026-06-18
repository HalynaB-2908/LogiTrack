import { useEffect, useState } from "react";
import { getDrivers, createDriver, updateDriver, deleteDriver } from "../api";
import { isAdmin } from "../auth";

export default function DriversForm() {
  const admin = isAdmin();
  const [items, setItems] = useState([]);
  const [loading, setLoading] = useState(false);

  const initialForm = { fullName: "", phone: "" };
  const [form, setForm] = useState(initialForm);
  const [editingId, setEditingId] = useState(null);

  const [error, setError] = useState(null);
  const [note, setNote] = useState(null);

  const [showModal, setShowModal] = useState(false);

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
      () => getDrivers(),
      (data) => setItems(data)
    );

  useEffect(() => {
    refresh();
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
      fullName: row.fullName ?? "",
      phone: row.phone ?? "",
    });
    setShowModal(true);
  };

  const closeModal = () => {
    setShowModal(false);
    setForm(initialForm);
    setEditingId(null);
  };

  const submit = () => {
    if (!form.fullName.trim()) {
      setError("Full name is required");
      return;
    }
    if (!editingId) {
      run(
        () => createDriver(form),
        () => {
          setNote("Driver created");
          closeModal();
          refresh();
        }
      );
    } else {
      run(
        () => updateDriver(editingId, form),
        () => {
          setNote("Driver updated");
          closeModal();
          refresh();
        }
      );
    }
  };

  const remove = (id) => {
    if (!confirm(`Delete driver #${id}?`)) return;
    run(
      () => deleteDriver(id),
      () => {
        setNote("Driver deleted");
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
              Add driver
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
                <th>Full name</th>
                <th style={{ width: 160 }}>Phone</th>
                <th style={{ width: 96 }}></th>
              </tr>
            </thead>
            <tbody>
              {items.length === 0 && (
                <tr>
                  <td colSpan={4} style={{ textAlign: "center" }}>
                    {loading ? "Loading…" : "No data"}
                  </td>
                </tr>
              )}
              {items.map((row) => (
                <tr key={row.id}>
                  <td>{row.id}</td>
                  <td>{row.fullName}</td>
                  <td>{row.phone || "-"}</td>
                  <td className="table-actions-cell">
                    {admin && (
                      <div className="table-actions">
                        <button
                          type="button"
                          className="icon-btn icon-btn-edit"
                          onClick={() => openEdit(row)}
                          disabled={loading}
                          aria-label="Edit driver"
                          title="Edit"
                        >
                          <svg
                            viewBox="0 0 24 24"
                            fill="none"
                            stroke="currentColor"
                            strokeWidth="2"
                            strokeLinecap="round"
                            strokeLinejoin="round"
                            aria-hidden="true"
                          >
                            <path d="M12 20h9" />
                            <path d="M16.5 3.5a2.1 2.1 0 0 1 3 3L8 18l-4 1 1-4Z" />
                          </svg>
                        </button>

                        <button
                          type="button"
                          className="icon-btn icon-btn-delete"
                          onClick={() => remove(row.id)}
                          disabled={loading}
                          aria-label="Delete driver"
                          title="Delete"
                        >
                          <svg
                            viewBox="0 0 24 24"
                            fill="none"
                            stroke="currentColor"
                            strokeWidth="2"
                            strokeLinecap="round"
                            strokeLinejoin="round"
                            aria-hidden="true"
                          >
                            <path d="M3 6h18" />
                            <path d="M8 6V4h8v2" />
                            <path d="M19 6l-1 14H6L5 6" />
                            <path d="M10 11v5" />
                            <path d="M14 11v5" />
                          </svg>
                        </button>
                      </div>
                    )}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>

      {showModal && (
        <div className="modal-backdrop" onClick={closeModal}>
          <div className="modal" onClick={(e) => e.stopPropagation()}>
            <h3 style={{ marginBottom: 12 }}>
              {editingId ? `Edit #${editingId}` : "Add driver"}
            </h3>
            <div className="controls" style={{ marginTop: 12 }}>
              <input
                className="input"
                placeholder="Full name"
                value={form.fullName}
                onChange={(e) =>
                  setForm((f) => ({ ...f, fullName: e.target.value }))
                }
              />
              <input
                className="input"
                placeholder="Phone"
                value={form.phone}
                onChange={(e) =>
                  setForm((f) => ({ ...f, phone: e.target.value }))
                }
              />
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
