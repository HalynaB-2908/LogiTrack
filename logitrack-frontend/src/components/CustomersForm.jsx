import { useState } from 'react';
import { getCustomerById } from '../api';

export default function CustomersForm({ onResult, onError }) {
  const [id, setId] = useState('');

  const fetchCustomer = async () => {
    onError?.(null); onResult?.(null);
    try {
      const data = await getCustomerById(id);
      onResult ? onResult(data) : console.log(data);
    } catch (e) {
      onError ? onError(String(e)) : console.error(e);
    }
  };

  return (
    <section style={box}>
      <h2>Customers</h2>
      <div style={row}>
        <input
          placeholder="Customer ID"
          value={id}
          onChange={e => setId(e.target.value)}
        />
        <button onClick={fetchCustomer} style={{ marginLeft: 8 }}>
          GET /customers/{'{id}'}
        </button>
      </div>
    </section>
  );
}

const box = { padding: 12, border: '1px solid #ddd', borderRadius: 8, marginBottom: 12 };
const row = { marginTop: 8 };
