import { useEffect, useState, type FormEvent } from "react";
import { getRooms } from "../api/rooms";
import ErrorAlert from "../components/ErrorAlert";
import type { Room, RoomFilters } from "../types/room";

const equipmentOptions = [
  { label: "Projector", value: 1 },
  { label: "TV screen", value: 2 },
  { label: "Video conferencing", value: 4 },
  { label: "Whiteboard", value: 8 },
];

const floors = ["1st Floor", "2nd Floor", "3rd Floor"];

function displayEquipment(value: string) {
  if (!value || value === "None" || value === "0") return "None";
  return value.split(",").map((item) => {
    const name = item.trim();
    return name === "TvScreen" ? "TV screen" : name === "VideoConferencing" ? "Video conferencing" : name;
  }).join(", ");
}

export default function RoomsPage() {
  const [rooms, setRooms] = useState<Room[]>([]);
  const [draftCapacity, setDraftCapacity] = useState("");
  const [draftEquipment, setDraftEquipment] = useState<number[]>([]);
  const [draftFloor, setDraftFloor] = useState("");
  const [filters, setFilters] = useState<RoomFilters>({});
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<unknown>(null);
  const [reload, setReload] = useState(0);

  useEffect(() => {
    let current = true;
    setLoading(true);
    setError(null);
    getRooms(filters)
      .then((data) => { if (current) setRooms(data); })
      .catch((cause: unknown) => { if (current) setError(cause); })
      .finally(() => { if (current) setLoading(false); });
    return () => { current = false; };
  }, [filters, reload]);

  function applyFilters(event: FormEvent) {
    event.preventDefault();
    const next: RoomFilters = {};
    if (draftCapacity) next.minCapacity = Number(draftCapacity);
    if (draftEquipment.length) next.equipment = draftEquipment.reduce((mask, flag) => mask | flag, 0);
    if (draftFloor) next.floor = draftFloor;
    setFilters(next);
  }

  function clearFilters() {
    setDraftCapacity("");
    setDraftEquipment([]);
    setDraftFloor("");
    setFilters({});
  }

  return (
    <main className="page-shell">
      <header className="page-header">
        <div><p className="eyebrow">ROOM BOOKING</p><h1>Rooms</h1><p className="intro">Browse spaces and find a room with the features you need.</p></div>
        <span className="room-total">{rooms.length} {rooms.length === 1 ? "room" : "rooms"}</span>
      </header>

      <form className="filter-panel" onSubmit={applyFilters}>
        <label className="field-label">Minimum capacity<input className="text-input" type="number" min="1" inputMode="numeric" placeholder="Any size" value={draftCapacity} onChange={(event) => setDraftCapacity(event.target.value)} /></label>
        <label className="field-label">Floor<select className="text-input" value={draftFloor} onChange={(event) => setDraftFloor(event.target.value)}><option value="">Any floor</option>{floors.map((floor) => <option key={floor}>{floor}</option>)}</select></label>
        <fieldset className="equipment-options"><legend className="field-label">Required equipment</legend>{equipmentOptions.map((item) => <label key={item.value}><input type="checkbox" checked={draftEquipment.includes(item.value)} onChange={(event) => setDraftEquipment((current) => event.target.checked ? [...current, item.value] : current.filter((value) => value !== item.value))} />{item.label}</label>)}</fieldset>
        <div className="filter-actions"><button className="button primary" type="submit">Apply filters</button><button className="button text-button" type="button" onClick={clearFilters}>Clear</button></div>
      </form>

      <ErrorAlert error={error} onDismiss={() => setError(null)} onRetry={() => setReload((value) => value + 1)} />
      {loading && <p className="notice" role="status">Loading rooms…</p>}
      {!loading && !error && rooms.length === 0 && <p className="notice">No rooms match these filters.</p>}
      {!loading && !error && rooms.length > 0 && (
        <div className="table-wrap"><table>
          <thead><tr><th>ID</th><th>Room</th><th>Capacity</th><th>Floor</th><th>Equipment</th><th>Status</th></tr></thead>
          <tbody>{rooms.map((room) => <tr key={room.id} className={!room.isActive ? "row-inactive" : ""}>
            <td className="muted">{room.id}</td><td className="strong-cell">{room.name}</td><td>{room.capacity} people</td><td>{room.floor}</td><td>{displayEquipment(room.equipment)}</td>
            <td><span className={`status-chip ${room.isActive ? "status-active" : "status-inactive"}`}>{room.isActive ? "Active" : "Unavailable"}</span></td>
          </tr>)}</tbody>
        </table></div>
      )}
    </main>
  );
}
