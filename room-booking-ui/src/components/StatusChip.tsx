import type { ReservationStatus } from "../types/reservation";

export default function StatusChip({ status }: { status: ReservationStatus }) {
  return <span className={`status-chip status-${status.toLowerCase()}`}>{status}</span>;
}
