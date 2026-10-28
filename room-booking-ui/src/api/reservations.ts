import { request } from "./client";
import type { CreateReservationRequest, Reservation, ReservationFilters } from "../types/reservation";

function queryString(filters: ReservationFilters) {
  const params = new URLSearchParams();
  if (filters.roomId !== undefined) params.set("roomId", String(filters.roomId));
  if (filters.organizerId !== undefined) params.set("organizerId", String(filters.organizerId));
  if (filters.status) params.set("status", filters.status);
  if (filters.from) params.set("from", filters.from);
  if (filters.to) params.set("to", filters.to);
  const query = params.toString();
  return query ? `?${query}` : "";
}

export function getReservations(filters: ReservationFilters = {}): Promise<Reservation[]> {
  return request<Reservation[]>(`/api/reservations${queryString(filters)}`);
}

export function createReservation(input: CreateReservationRequest): Promise<Reservation> {
  return request<Reservation>("/api/reservations", { method: "POST", body: JSON.stringify(input) });
}

export function cancelReservation(id: number): Promise<Reservation> {
  return request<Reservation>(`/api/reservations/${id}/cancel`, { method: "POST" });
}

export function endReservation(id: number): Promise<Reservation> {
  return request<Reservation>(`/api/reservations/${id}/end`, { method: "POST" });
}
