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
        <span className="navbar-brand-logi">Logi</span>
        <span className="navbar-brand-track">Track</span>
      </Link>

      <nav className="navbar-actions">
        {admin && <Link to="/admin">Admin page</Link>}
        {isLoggedIn() && (
          <>
            <span className="navbar-username">{user}</span>
            <button
              type="button"
              className="btn danger navbar-logout"
              onClick={logout}
              aria-label="Logout"
              title="Logout"
            >
              <svg
                viewBox="0 0 24 24"
                fill="none"
                stroke="currentColor"
                strokeWidth="2"
                strokeLinecap="round"
                strokeLinejoin="round"
                aria-hidden="true"
              >
                <path d="M10 17l5-5-5-5" />
                <path d="M15 12H3" />
                <path d="M15 3h4a2 2 0 0 1 2 2v14a2 2 0 0 1-2 2h-4" />
              </svg>
            </button>
          </>
        )}
      </nav>
    </header>
  );
}
