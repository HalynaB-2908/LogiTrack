import CustomersForm from "../components/CustomersForm";
import DriversForm from "../components/DriversForm";
import VehiclesForm from "../components/VehiclesForm";

export default function AdminPage() {
  return (
    <div className="container">
      <h2 style={{ marginBottom: "0.5rem" }}>Admin panel</h2>

      <section className="card wide">
        <h3>Customers</h3>
        <CustomersForm />
      </section>

      <section className="card wide">
        <h3>Drivers</h3>
        <DriversForm />
      </section>

      <section className="card wide">
        <h3>Vehicles</h3>
        <VehiclesForm />
      </section>
    </div>
  );
}
