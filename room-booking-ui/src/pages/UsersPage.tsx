import { useEffect, useState } from "react";
import { getUsers } from "../api/users";
import ErrorAlert from "../components/ErrorAlert";
import type { User } from "../types/user";

export default function UsersPage() {
  const [users, setUsers] = useState<User[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<unknown>(null);
  const [reload, setReload] = useState(0);

  useEffect(() => {
    let current = true;
    setLoading(true);
    setError(null);
    getUsers()
      .then((data) => { if (current) setUsers(data); })
      .catch((cause: unknown) => { if (current) setError(cause); })
      .finally(() => { if (current) setLoading(false); });
    return () => { current = false; };
  }, [reload]);

  return (
    <main className="page-shell">
      <header className="page-header"><div><p className="eyebrow">ROOM BOOKING</p><h1>Users</h1><p className="intro">Organizers and their current booked reservations.</p></div><button className="button secondary" type="button" onClick={() => setReload((value) => value + 1)}>Refresh</button></header>
      <ErrorAlert error={error} onRetry={() => setReload((value) => value + 1)} onDismiss={() => setError(null)} />
      {loading && <p className="notice" role="status">Loading users…</p>}
      {!loading && !error && users.length === 0 && <p className="notice">No users found.</p>}
      {!loading && !error && users.length > 0 && <div className="table-wrap"><table>
        <thead><tr><th>ID</th><th>First name</th><th>Last name</th><th>Active reservations</th></tr></thead>
        <tbody>{users.map((user) => <tr key={user.id}><td className="muted">{user.id}</td><td>{user.firstName}</td><td className="strong-cell">{user.lastName}</td><td>{user.activeReservations}</td></tr>)}</tbody>
      </table></div>}
    </main>
  );
}
