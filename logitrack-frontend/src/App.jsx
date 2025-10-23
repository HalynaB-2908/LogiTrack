import ShipmentsForm from "./components/ShipmentsForm";
import "./App.css";

export default function App() {
  return (
    <div
      style={{ maxWidth: 1100, margin: "2rem auto", fontFamily: "system-ui" }}
    >
      <h1>LogiTrack</h1>
      <ShipmentsForm />
    </div>
  );
}
