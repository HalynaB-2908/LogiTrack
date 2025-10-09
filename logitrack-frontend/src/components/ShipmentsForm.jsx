import { useState } from 'react';
import { getShipmentById, searchShipments, createShipment } from '../api';

export default function ShipmentsForm({ onResult, onError }) {
  const [id, setId] = useState('');
  const [q, setQ] = useState('');
  const [status, setStatus] = useState('');
  const [reference, setReference] = useState('');
  const [distanceKm, setDistanceKm] = useState('');
  const [weightKg, setWeightKg] = useState('');

  const run = async (fn) => {
    onError?.(null); onResult?.(null);
    try {
      const data = await fn();
      onResult ? onResult(data) : console.log(data);
    } catch (e) {
      onError ? onError(String(e)) : console.error(e);
    }
  };

  return (
    <section className="card">
      <div className="card-header">
        <h2>Shipments</h2>
        <a className="btn ghost" href="/api/v1/shipments/export">
          Download JSON
        </a>
      </div>

      <div className="row">
        <h3>Get by ID</h3>
        <div className="controls">
          <input
            className="input"
            placeholder="Shipment ID"
            value={id}
            onChange={e => setId(e.target.value)}
          />
          <button className="btn" onClick={() => run(() => getShipmentById(id))}>
            GET /shipments/{'{id}'}
          </button>
        </div>
      </div>

      <div className="row">
        <h3>Search</h3>
        <div className="controls">
          <input className="input" placeholder="q" value={q} onChange={e => setQ(e.target.value)} />
          <input className="input" placeholder="status" value={status} onChange={e => setStatus(e.target.value)} />
          <button className="btn" onClick={() => run(() => searchShipments(q, status))}>
            GET /shipments/search
          </button>
        </div>
      </div>

      <div className="row">
        <h3>Create</h3>
        <div className="controls">
          <input className="input" placeholder="Reference" value={reference} onChange={e => setReference(e.target.value)} />
          <input className="input" placeholder="DistanceKm" type="number" value={distanceKm} onChange={e => setDistanceKm(e.target.value)} />
          <input className="input" placeholder="WeightKg" type="number" value={weightKg} onChange={e => setWeightKg(e.target.value)} />
          <button
            className="btn primary"
            onClick={() => run(() => createShipment({
              reference,
              distanceKm: Number(distanceKm),
              weightKg: Number(weightKg)
            }))}
          >
            POST /shipments
          </button>
        </div>
      </div>
    </section>
  );
}
