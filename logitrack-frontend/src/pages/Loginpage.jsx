import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { saveSession } from "../auth";

export default function LoginPage() {
  const [emailOrUserName, setEmailOrUserName] = useState("");
  const [password, setPassword] = useState("");
  const [err, setErr] = useState(null);
  const navigate = useNavigate();

  async function handleLogin(e) {
    e.preventDefault();
    setErr(null);
    try {
      const res = await fetch("/api/v1/auth/login", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ emailOrUserName, password }),
      });
      if (!res.ok) {
        setErr("Login failed");
        return;
      }
      const data = await res.json();
      saveSession({
        token: data.token,
        userName: data.userName || "",
        roles: data.roles || [],
      });
      navigate("/");
    } catch (e) {
      console.error(e);
      setErr("Network error");
    }
  }

  return (
    <div className="auth-page">
      <div className="auth-card">
        <h2 className="auth-title">Sign in</h2>
        <form className="auth-form" onSubmit={handleLogin}>
          <input
            className="input"
            placeholder="Email or username"
            value={emailOrUserName}
            onChange={(e) => setEmailOrUserName(e.target.value)}
          />
          <input
            className="input"
            type="password"
            placeholder="Password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
          />
          <button className="btn primary" type="submit">
            Sign in
          </button>
          {err && <div className="auth-error">{err}</div>}
        </form>
      </div>
    </div>
  );
}
