import { useEffect, useState } from "react";
import { ApiRequestError } from "../api/client";
import { getRooms, type Room, type RoomFilters } from "../api/rooms";

const equipmentOptions = [
  { label: "Projector", value: 1 },
  { label: "TV screen", value: 2 },
  { label: "Video conferencing", value: 4 },
  { label: "Whiteboard", value: 8 },
];

function formatEquipment(equipment: string) {
  if (!equipment || equipment === "None" || equipment === "0") return "None";
  return equipment.split(",").map((item) => item.trim()).join(", ");
}

export default function RoomsPage() {
  const [rooms, setRooms] = useState<Room[]>([]);
  const [minCapacity, setMinCapacity] = useState("");
  const [equipment, setEquipment] = useState("");
  const [floor, setFloor] = useState("");
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [reload, setReload] = useState(0);

  useEffect(() => {
    const filters: RoomFilters = {};
    const capacity = Number(minCapacity);
    if (minCapacity && Number.isFinite(capacity)) filters.minCapacity = capacity;
    if (equipment) filters.equipment = Number(equipment);
    if (floor) filters.floor = floor;

    let current = true;
    setLoading(true);
    setError("");
    getRooms(filters)
      .then((data) => { if (current) setRooms(data); })
      .catch((cause: unknown) => {
        if (!current) return;
        setError(cause instanceof ApiRequestError ? cause.message : cause instanceof Error ? cause.message : "Could not load rooms.");
      })
      .finally(() => { if (current) setLoading(false); });
    return () => { current = false; };
  }, [minCapacity, equipment, floor, reload]);

  return (
    <main className="page-shell">
      <header className="page-header">
        <div>
          <p className="eyebrow">ROOM BOOKING</p>
          <h1>Find a room</h1>
          <p className="intro">Browse available spaces and filter by the features you need.</p>
        </div>
        <span className="room-total">{rooms.length} {rooms.length === 1 ? "room" : "rooms"}</span>
      </header>

      <section className="filters" aria-label="Filter rooms">
        <label>Minimum capacity
          <input type="number" min="1" inputMode="numeric" placeholder="Any size" value={minCapacity} onChange={(event) => setMinCapacity(event.target.value)} />
        </label>
        <label>Equipment
          <select value={equipment} onChange={(event) => setEquipment(event.target.value)}>
            <option value="">Any equipment</option>
            {equipmentOptions.map((item) => <option key={item.value} value={item.value}>{item.label}</option>)}
          </select>
        </label>
        <label>Floor
          <select value={floor} onChange={(event) => setFloor(event.target.value)}>
            <option value="">Any floor</option>
            {["1st Floor", "2nd Floor", "3rd Floor"].map((item) => <option key={item} value={item}>{item}</option>)}
          </select>
        </label>
        <button className="reset-button" type="button" onClick={() => { setMinCapacity(""); setEquipment(""); setFloor(""); }}>Clear filters</button>
      </section>

      {error && <div className="notice error" role="alert">{error} <button type="button" onClick={() => setReload((value) => value + 1)}>Retry</button></div>}
      {loading ? <p className="notice" role="status">Loading rooms…</p> : !error && rooms.length === 0 ? <p className="notice">No rooms match these filters. Try changing your search.</p> : null}

      {!loading && rooms.length > 0 && (
        <section className="room-grid" aria-label="Rooms">
          {rooms.map((room) => (
            <article className={`room-card${room.isActive ? "" : " inactive"}`} key={room.id}>
              <div className="card-topline"><span className="room-id">ROOM {String(room.id).padStart(2, "0")}</span><span className={`status ${room.isActive ? "active" : "inactive-status"}`}>{room.isActive ? "Active" : "Inactive"}</span></div>
              <h2>{room.name}</h2>
              <div className="room-facts"><span><strong>{room.capacity}</strong> people</span><span>{room.floor}</span></div>
              <div className="equipment-block"><span className="detail-label">EQUIPMENT</span><p>{formatEquipment(room.equipment)}</p></div>
              {!room.isActive && <p className="unavailable">This room cannot be booked</p>}
            </article>
          ))}
        </section>
      )}
    </main>
  );
}
