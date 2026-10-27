
import { apiRequest } from "./client";

export interface Room {
  id: number;
  name: string;
  capacity: number;
  floor: string;
  equipment: string;
  isActive: boolean;
}

export interface RoomFilters {
  minCapacity?: number;
  equipment?: number;
  floor?: string;
}

export async function getRooms(filters: RoomFilters = {}): Promise<Room[]> {
  const params = new URLSearchParams();

  if (filters.minCapacity !== undefined) params.set("minCapacity", String(filters.minCapacity));
  if (filters.equipment !== undefined) params.set("equipment", String(filters.equipment));
  if (filters.floor) params.set("floor", filters.floor);

  const query = params.toString();
  return apiRequest<Room[]>(`/api/rooms${query ? `?${query}` : ""}`);
}
