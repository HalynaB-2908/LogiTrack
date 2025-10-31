import { useState } from "react";
import { useNavigate } from "react-router-dom";

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
        body: JSON.stringify({
          emailOrUserName,
          password,
        }),
      });

      if (!res.ok) {
        setErr("Login failed");
        return;
      }

      const data = await res.json();

      localStorage.setItem("token", data.token);
      localStorage.setItem("userName", data.userName || "");
      localStorage.setItem("roles", (data.roles || []).join(","));

      navigate("/");
    } catch (e) {
      console.error(e);
      setErr("Network error");
    }
  }

  return (
    <div
      style={{ maxWidth: 400, margin: "2rem auto", fontFamily: "system-ui" }}
    >
      <h2>Sign in</h2>
      <form
        onSubmit={handleLogin}
        style={{
          display: "grid",
          gap: "0.75rem",
          background: "#1f2937",
          padding: "1rem",
          borderRadius: "0.75rem",
          color: "#f9fafb",
        }}
      >
        <input
          style={{
            padding: "0.5rem 0.75rem",
            borderRadius: "0.5rem",
            border: "1px solid #4b5563",
            background: "#374151",
            color: "#f9fafb",
          }}
          placeholder="Email or username"
          value={emailOrUserName}
          onChange={(e) => setEmailOrUserName(e.target.value)}
        />

        <input
          type="password"
          style={{
            padding: "0.5rem 0.75rem",
            borderRadius: "0.5rem",
            border: "1px solid #4b5563",
            background: "#374151",
            color: "#f9fafb",
          }}
          placeholder="Password"
          value={password}
          onChange={(e) => setPassword(e.target.value)}
        />

        <button
          type="submit"
          style={{
            padding: "0.6rem 0.75rem",
            borderRadius: "0.5rem",
            border: "0",
            background: "#10b981",
            fontWeight: 600,
            color: "#111827",
            cursor: "pointer",
          }}
        >
          Sign in
        </button>

        {err && (
          <div style={{ color: "#f87171", fontSize: "0.9rem" }}>{err}</div>
        )}
      </form>
    </div>
  );
}
