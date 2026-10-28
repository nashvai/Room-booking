
import { request } from "./client";
import type { Room, RoomFilters } from "../types/room";

export async function getRooms(filters: RoomFilters = {}): Promise<Room[]> {
  const params = new URLSearchParams();

  if (filters.minCapacity !== undefined) params.set("minCapacity", String(filters.minCapacity));
  if (filters.equipment !== undefined) params.set("equipment", String(filters.equipment));
  if (filters.floor) params.set("floor", filters.floor);

  const query = params.toString();
  return request<Room[]>(`/api/rooms${query ? `?${query}` : ""}`);
}
