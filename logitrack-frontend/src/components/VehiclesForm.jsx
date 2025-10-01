import { useState } from 'react';
import { getVehicleById } from '../api';

export default function VehiclesForm({ onResult, onError }) {
  const [id, setId] = useState('');

  const fetchVehicle = async () => {
    onError?.(null); onResult?.(null);
    try {
      const data = await getVehicleById(id);
      onResult ? onResult(data) : console.log(data);
    } catch (e) {
      onError ? onError(String(e)) : console.error(e);
    }
  };

  return (
    <section style={box}>
      <h2>Vehicles</h2>
      <div style={row}>
        <input
          placeholder="Vehicle ID"
          value={id}
          onChange={e => setId(e.target.value)}
        />
        <button onClick={fetchVehicle} style={{ marginLeft: 8 }}>
          GET /vehicles/{'{id}'}
        </button>
      </div>
    </section>
  );
}

const box = { padding: 12, border: '1px solid #ddd', borderRadius: 8, marginBottom: 12 };
const row = { marginTop: 8 };
