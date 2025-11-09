import { Link, useNavigate } from "react-router-dom";
import { getUserName, isAdmin, clearToken, isLoggedIn } from "../auth";

export default function Navbar() {
  const navigate = useNavigate();
  const user = getUserName();
  const admin = isAdmin();

  const logout = () => {
    clearToken();
    navigate("/login");
  };

  return (
    <header className="navbar">
      <Link to="/" className="navbar-brand">
        logitrack
      </Link>

      <nav className="navbar-actions">
        {admin && <Link to="/admin">Admin page</Link>}
        {isLoggedIn() && (
          <>
            <span className="navbar-username">{user}</span>
            <button className="btn danger" onClick={logout}>
              Logout
            </button>
          </>
        )}
      </nav>
    </header>
  );
}
