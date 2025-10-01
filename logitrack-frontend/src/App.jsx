import { useState } from 'react';
import CustomersForm from './components/CustomersForm';
import ShipmentsForm from './components/ShipmentsForm';
import VehiclesForm from './components/VehiclesForm';
import './App.css';

export default function App() {
  const [result, setResult] = useState(null);
  const [error, setError] = useState(null);

  return (
    <div style={{ maxWidth: 1000, margin: '2rem auto', fontFamily: 'system-ui' }}>
      <h1>LogiTrack</h1>

      <CustomersForm onResult={setResult} onError={setError} />
      <ShipmentsForm onResult={setResult} onError={setError} />
      <VehiclesForm onResult={setResult} onError={setError} />

      <section style={{ marginTop: 16 }}>
        {error && <pre style={{ color: 'crimson' }}>{error}</pre>}
        {result && (
          <pre style={{ background: '#f7f7f7', padding: 12, borderRadius: 8 }}>
            {JSON.stringify(result, null, 2)}
          </pre>
        )}
      </section>
    </div>
  );
}

