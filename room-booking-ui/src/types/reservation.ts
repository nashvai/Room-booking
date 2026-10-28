export type ReservationStatus = "Booked" | "Completed" | "Cancelled";

export interface Reservation {
  id: number;
  roomId: number;
  roomName: string;
  organizerId: number;
  organizerName: string;
  title: string;
  start: string;
  end: string;
  attendees: number;
  status: ReservationStatus;
}

export interface ReservationFilters {
  roomId?: number;
  organizerId?: number;
  status?: ReservationStatus;
  from?: string;
  to?: string;
}

export interface CreateReservationRequest {
  roomId: number;
  organizerId: number;
  title: string;
  start: string;
  end: string;
  attendees: number;
}
