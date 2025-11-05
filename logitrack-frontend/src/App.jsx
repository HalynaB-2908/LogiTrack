import { Routes, Route, Navigate } from "react-router-dom";
import Navbar from "./components/Navbar";
import ShipmentsForm from "./components/ShipmentsForm";
import ProtectedRoute from "./ProtectedRoute";
import LoginPage from "./pages/LoginPage";
import AdminPage from "./pages/AdminPage";
import { isAdmin } from "./auth";
import "./App.css";

export default function App() {
  return (
    <div className="container">
      <Navbar />

      <Routes>
        <Route
          path="/"
          element={
            <ProtectedRoute>
              <ShipmentsForm />
            </ProtectedRoute>
          }
        />

        <Route
          path="/admin"
          element={
            <ProtectedRoute>
              {isAdmin() ? <AdminPage /> : <Navigate to="/" replace />}
            </ProtectedRoute>
          }
        />

        <Route path="/login" element={<LoginPage />} />

        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
    </div>
  );
}
