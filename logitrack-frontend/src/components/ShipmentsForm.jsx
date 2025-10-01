import { useState } from 'react';
import { getShipmentById, searchShipments, createShipment } from '../api';

export default function ShipmentsForm({ onResult, onError }) {
  // Get by ID
  const [id, setId] = useState('');
  // Search
  const [q, setQ] = useState('');
  const [status, setStatus] = useState('');
  // Create
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
    <section style={box}>
      <h2>Shipments</h2>

      {/* GET /shipments/{id} */}
      <div style={row}>
        <input
          placeholder="Shipment ID"
          value={id}
          onChange={e => setId(e.target.value)}
        />
        <button onClick={() => run(() => getShipmentById(id))} style={{ marginLeft: 8 }}>
          GET /shipments/{'{id}'}
        </button>
      </div>

      {/* GET /shipments/search */}
      <div style={row}>
        <input placeholder="q" value={q} onChange={e => setQ(e.target.value)} />
        <input placeholder="status" value={status} onChange={e => setStatus(e.target.value)} style={{ marginLeft: 8 }} />
        <button onClick={() => run(() => searchShipments(q, status))} style={{ marginLeft: 8 }}>
          GET /shipments/search
        </button>
      </div>

      {/* POST /shipments */}
      <div style={row}>
        <input placeholder="Reference" value={reference} onChange={e => setReference(e.target.value)} />
        <input
          placeholder="DistanceKm"
          type="number"
          value={distanceKm}
          onChange={e => setDistanceKm(e.target.value)}
          style={{ marginLeft: 8 }}
        />
        <input
          placeholder="WeightKg"
          type="number"
          value={weightKg}
          onChange={e => setWeightKg(e.target.value)}
          style={{ marginLeft: 8 }}
        />
        <button
          onClick={() => run(() => createShipment({
            reference,
            distanceKm: Number(distanceKm),
            weightKg: Number(weightKg)
          }))}
          style={{ marginLeft: 8 }}
        >
          POST /shipments
        </button>
      </div>
    </section>
  );
}

const box = { padding: 12, border: '1px solid #ddd', borderRadius: 8, marginBottom: 12 };
const row = { marginTop: 8 };
