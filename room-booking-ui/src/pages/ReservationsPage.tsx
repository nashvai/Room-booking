import { useEffect, useState, type FormEvent } from "react";
import { cancelReservation, createReservation, endReservation, getReservations } from "../api/reservations";
import { getRooms } from "../api/rooms";
import { getUsers } from "../api/users";
import ConfirmDialog from "../components/ConfirmDialog";
import ErrorAlert from "../components/ErrorAlert";
import StatusChip from "../components/StatusChip";
import type { Reservation, ReservationFilters, ReservationStatus } from "../types/reservation";
import type { Room } from "../types/room";
import type { User } from "../types/user";

type ReservationAction = { id: number; action: "cancel" | "end"; title: string };
type ReservationForm = { roomId: string; organizerId: string; title: string; start: string; end: string; attendees: string };
const emptyForm: ReservationForm = { roomId: "", organizerId: "", title: "", start: "", end: "", attendees: "1" };
const statuses: ReservationStatus[] = ["Booked", "Completed", "Cancelled"];

function toIso(value: string) {
  return value ? new Date(value).toISOString() : undefined;
}

function localTime(value: string) {
  return new Date(value).toLocaleString();
}

export default function ReservationsPage() {
  const [reservations, setReservations] = useState<Reservation[]>([]);
  const [rooms, setRooms] = useState<Room[]>([]);
  const [users, setUsers] = useState<User[]>([]);
  const [draft, setDraft] = useState({ roomId: "", organizerId: "", status: "" as "" | ReservationStatus, from: "", to: "" });
  const [filters, setFilters] = useState<ReservationFilters>({});
  const [loadingResources, setLoadingResources] = useState(true);
  const [resourceError, setResourceError] = useState<unknown>(null);
  const [resourceReload, setResourceReload] = useState(0);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<unknown>(null);
  const [actionError, setActionError] = useState<unknown>(null);
  const [reload, setReload] = useState(0);
  const [createOpen, setCreateOpen] = useState(false);
  const [form, setForm] = useState<ReservationForm>(emptyForm);
  const [formError, setFormError] = useState<unknown>(null);
  const [saving, setSaving] = useState(false);
  const [pendingAction, setPendingAction] = useState<ReservationAction | null>(null);
  const [acting, setActing] = useState(false);

  useEffect(() => {
    let current = true;
    setLoadingResources(true);
    setResourceError(null);
    Promise.all([getRooms(), getUsers()])
      .then(([roomRows, userRows]) => {
        if (!current) return;
        setRooms(roomRows);
        setUsers(userRows);
      })
      .catch((cause: unknown) => { if (current) setResourceError(cause); })
      .finally(() => { if (current) setLoadingResources(false); });
    return () => { current = false; };
  }, [resourceReload]);

  useEffect(() => {
    let current = true;
    setLoading(true);
    setError(null);
    getReservations(filters)
      .then((data) => { if (current) setReservations(data); })
      .catch((cause: unknown) => { if (current) setError(cause); })
      .finally(() => { if (current) setLoading(false); });
    return () => { current = false; };
  }, [filters, reload]);

  function applyFilters(event: FormEvent) {
    event.preventDefault();
    const next: ReservationFilters = {};
    if (draft.roomId) next.roomId = Number(draft.roomId);
    if (draft.organizerId) next.organizerId = Number(draft.organizerId);
    if (draft.status) next.status = draft.status;
    const from = toIso(draft.from);
    const to = toIso(draft.to);
    if (from) next.from = from;
    if (to) next.to = to;
    setFilters(next);
  }

  function clearFilters() {
    setDraft({ roomId: "", organizerId: "", status: "", from: "", to: "" });
    setFilters({});
  }

  async function submitReservation(event: FormEvent) {
    event.preventDefault();
    setSaving(true);
    setFormError(null);
    try {
      await createReservation({
        roomId: Number(form.roomId),
        organizerId: Number(form.organizerId),
        title: form.title,
        start: new Date(form.start).toISOString(),
        end: new Date(form.end).toISOString(),
        attendees: Number(form.attendees),
      });
      setCreateOpen(false);
      setForm(emptyForm);
      setReload((value) => value + 1);
    } catch (cause) {
      setFormError(cause);
    } finally {
      setSaving(false);
    }
  }

  async function confirmAction() {
    if (!pendingAction) return;
    setActing(true);
    setActionError(null);
    try {
      if (pendingAction.action === "cancel") await cancelReservation(pendingAction.id);
      else await endReservation(pendingAction.id);
      setPendingAction(null);
      setReload((value) => value + 1);
    } catch (cause) {
      setActionError(cause);
      setPendingAction(null);
    } finally {
      setActing(false);
    }
  }

  return (
    <main className="page-shell">
      <header className="page-header"><div><p className="eyebrow">ROOM BOOKING</p><h1>Reservations</h1><p className="intro">Review bookings, filter the schedule, and manage meetings.</p></div><button className="button primary" type="button" disabled={loadingResources || !!resourceError || rooms.length === 0 || users.length === 0} onClick={() => { setFormError(null); setCreateOpen(true); }}>New reservation</button></header>

      <form className="filter-panel reservation-filters" onSubmit={applyFilters}>
        <label className="field-label">Room<select className="text-input" value={draft.roomId} onChange={(event) => setDraft({ ...draft, roomId: event.target.value })}><option value="">All rooms</option>{rooms.map((room) => <option key={room.id} value={room.id}>{room.name}</option>)}</select></label>
        <label className="field-label">Organizer<select className="text-input" value={draft.organizerId} onChange={(event) => setDraft({ ...draft, organizerId: event.target.value })}><option value="">All organizers</option>{users.map((user) => <option key={user.id} value={user.id}>{user.firstName} {user.lastName}</option>)}</select></label>
        <label className="field-label">Status<select className="text-input" value={draft.status} onChange={(event) => setDraft({ ...draft, status: event.target.value as "" | ReservationStatus })}><option value="">Any status</option>{statuses.map((status) => <option key={status}>{status}</option>)}</select></label>
        <label className="field-label">From<input className="text-input" type="datetime-local" value={draft.from} onChange={(event) => setDraft({ ...draft, from: event.target.value })} /></label>
        <label className="field-label">To<input className="text-input" type="datetime-local" value={draft.to} onChange={(event) => setDraft({ ...draft, to: event.target.value })} /></label>
        <div className="filter-actions"><button className="button primary" type="submit">Apply filters</button><button className="button text-button" type="button" onClick={clearFilters}>Clear</button></div>
      </form>

      <ErrorAlert error={resourceError} onDismiss={() => setResourceError(null)} onRetry={() => setResourceReload((value) => value + 1)} />
      <ErrorAlert error={error} onDismiss={() => setError(null)} onRetry={() => setReload((value) => value + 1)} />
      <ErrorAlert error={actionError} onDismiss={() => setActionError(null)} />
      {loadingResources && <p className="notice" role="status">Loading rooms and organizers…</p>}
      {loading && <p className="notice" role="status">Loading reservations…</p>}
      {!loading && !error && reservations.length === 0 && <p className="notice">No reservations match these filters.</p>}
      {!loading && !error && reservations.length > 0 && <div className="table-wrap"><table className="reservations-table">
        <thead><tr><th>Room</th><th>Organizer</th><th>Title</th><th>Start</th><th>End</th><th>Attendees</th><th>Status</th><th>Actions</th></tr></thead>
        <tbody>{reservations.map((reservation) => {
          const now = Date.now();
          const canCancel = reservation.status === "Booked" && new Date(reservation.start).getTime() > now;
          const canEnd = reservation.status === "Booked" && new Date(reservation.start).getTime() <= now && new Date(reservation.end).getTime() > now;
          return <tr key={reservation.id}>
            <td>{reservation.roomName}</td><td>{reservation.organizerName}</td><td className="strong-cell">{reservation.title}</td><td>{localTime(reservation.start)}</td><td>{localTime(reservation.end)}</td><td>{reservation.attendees}</td><td><StatusChip status={reservation.status} /></td>
            <td><div className="row-actions"><button className="button small secondary" type="button" disabled={!canCancel} onClick={() => setPendingAction({ id: reservation.id, action: "cancel", title: reservation.title })}>Cancel</button><button className="button small secondary" type="button" disabled={!canEnd} onClick={() => setPendingAction({ id: reservation.id, action: "end", title: reservation.title })}>End early</button></div></td>
          </tr>;
        })}</tbody>
      </table></div>}

      {createOpen && <div className="dialog-backdrop" role="presentation"><section className="dialog-card" role="dialog" aria-modal="true" aria-labelledby="create-title">
        <div className="dialog-heading"><div><p className="eyebrow">NEW BOOKING</p><h2 id="create-title">Reserve a room</h2></div><button className="icon-button" type="button" aria-label="Close" disabled={saving} onClick={() => setCreateOpen(false)}>×</button></div>
        <ErrorAlert error={formError} onDismiss={() => setFormError(null)} />
        <form className="create-form" onSubmit={submitReservation}>
          <label className="field-label">Room<select className="text-input" required value={form.roomId} onChange={(event) => setForm({ ...form, roomId: event.target.value })}><option value="">Choose an active room</option>{rooms.filter((room) => room.isActive).map((room) => <option key={room.id} value={room.id}>{room.name} · {room.capacity} people</option>)}</select></label>
          <label className="field-label">Organizer<select className="text-input" required value={form.organizerId} onChange={(event) => setForm({ ...form, organizerId: event.target.value })}><option value="">Choose organizer</option>{users.map((user) => <option key={user.id} value={user.id}>{user.firstName} {user.lastName}</option>)}</select></label>
          <label className="field-label full-field">Title<input className="text-input" required maxLength={200} value={form.title} onChange={(event) => setForm({ ...form, title: event.target.value })} /></label>
          <label className="field-label">Start<input className="text-input" type="datetime-local" required value={form.start} onChange={(event) => setForm({ ...form, start: event.target.value })} /></label>
          <label className="field-label">End<input className="text-input" type="datetime-local" required value={form.end} onChange={(event) => setForm({ ...form, end: event.target.value })} /></label>
          <label className="field-label">Attendees<input className="text-input" type="number" min="1" required value={form.attendees} onChange={(event) => setForm({ ...form, attendees: event.target.value })} /></label>
          <p className="form-hint full-field">Times use your local time zone and are sent to the server in UTC.</p>
          <div className="dialog-actions full-field"><button className="button secondary" type="button" disabled={saving} onClick={() => setCreateOpen(false)}>Close</button><button className="button primary" type="submit" disabled={saving || loadingResources}>{saving ? "Creating…" : "Create reservation"}</button></div>
        </form>
      </section></div>}

      {pendingAction && <ConfirmDialog title={pendingAction.action === "cancel" ? "Cancel reservation?" : "End meeting early?"} message={`Confirm this action for “${pendingAction.title}”.`} confirmLabel={pendingAction.action === "cancel" ? "Cancel reservation" : "End meeting"} busy={acting} onConfirm={confirmAction} onCancel={() => setPendingAction(null)} />}
    </main>
  );
}
