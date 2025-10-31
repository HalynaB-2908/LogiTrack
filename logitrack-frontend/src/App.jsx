import { Routes, Route, Navigate } from "react-router-dom";
import ShipmentsForm from "./components/ShipmentsForm";
import ProtectedRoute from "./ProtectedRoute";
import LoginPage from "./pages/Loginpage";
import "./App.css";

export default function App() {
  return (
    <div
      style={{ maxWidth: 1100, margin: "2rem auto", fontFamily: "system-ui" }}
    >
      <h1>LogiTrack</h1>

      <Routes>
        <Route
          path="/"
          element={
            <ProtectedRoute>
              <ShipmentsForm />
            </ProtectedRoute>
          }
        />

        <Route path="/login" element={<LoginPage />} />

        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
    </div>
  );
}
