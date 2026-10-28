import { useEffect, useState, type FormEvent } from "react";
import { getRoomUsage } from "../api/reports";
import ErrorAlert from "../components/ErrorAlert";
import type { RoomUsage } from "../types/report";

export default function UsagePage() {
  const [rows, setRows] = useState<RoomUsage[]>([]);
  const [draftFrom, setDraftFrom] = useState("");
  const [draftTo, setDraftTo] = useState("");
  const [filters, setFilters] = useState<{ from?: string; to?: string }>({});
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<unknown>(null);
  const [reload, setReload] = useState(0);

  useEffect(() => {
    let current = true;
    setLoading(true);
    setError(null);
    getRoomUsage(filters)
      .then((data) => { if (current) setRows(data); })
      .catch((cause: unknown) => { if (current) setError(cause); })
      .finally(() => { if (current) setLoading(false); });
    return () => { current = false; };
  }, [filters, reload]);

  function applyFilters(event: FormEvent) {
    event.preventDefault();
    setFilters({
      ...(draftFrom ? { from: new Date(draftFrom).toISOString() } : {}),
      ...(draftTo ? { to: new Date(draftTo).toISOString() } : {}),
    });
  }

  function clearFilters() {
    setDraftFrom("");
    setDraftTo("");
    setFilters({});
  }

  const sortedRows = [...rows].sort((a, b) => b.usageRatePercent - a.usageRatePercent || a.roomName.localeCompare(b.roomName));

  return (
    <main className="page-shell">
      <header className="page-header"><div><p className="eyebrow">ROOM BOOKING</p><h1>Room usage</h1><p className="intro">Usage rate compares actual hours with scheduled hours for finished meetings. Occupancy uses actual occupied time over a 24-hour room day.</p></div></header>

      <form className="filter-panel date-filters" onSubmit={applyFilters}>
        <label className="field-label">From<input className="text-input" type="datetime-local" value={draftFrom} onChange={(event) => setDraftFrom(event.target.value)} /></label>
        <label className="field-label">To<input className="text-input" type="datetime-local" value={draftTo} onChange={(event) => setDraftTo(event.target.value)} /></label>
        <div className="filter-actions"><button className="button primary" type="submit">Apply dates</button><button className="button text-button" type="button" onClick={clearFilters}>Use last 30 days</button></div>
      </form>

      <ErrorAlert error={error} onDismiss={() => setError(null)} onRetry={() => setReload((value) => value + 1)} />
      {loading && <p className="notice" role="status">Loading room usage…</p>}
      {!loading && !error && sortedRows.length === 0 && <p className="notice">No room usage data is available.</p>}
      {!loading && !error && sortedRows.length > 0 && <div className="table-wrap"><table>
        <thead><tr><th>Room</th><th>Bookings</th><th>Cancelled</th><th>Booked hours</th><th>Actual hours</th><th>Occupancy</th><th>Usage rate</th></tr></thead>
        <tbody>{sortedRows.map((row) => <tr key={row.roomId}><td className="strong-cell">{row.roomName}</td><td>{row.bookingCount}</td><td>{row.cancelledCount}</td><td>{row.bookedHours.toFixed(2)} h</td><td>{row.actualHours.toFixed(2)} h</td><td><strong>{row.occupancyPercent.toFixed(2)}%</strong></td><td><strong>{row.usageRatePercent.toFixed(2)}%</strong></td></tr>)}</tbody>
      </table></div>}
    </main>
  );
}
