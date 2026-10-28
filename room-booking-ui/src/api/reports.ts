import { request } from "./client";
import type { RoomUsage, UsageFilters } from "../types/report";

export function getRoomUsage(filters: UsageFilters = {}): Promise<RoomUsage[]> {
  const params = new URLSearchParams();
  if (filters.from) params.set("from", filters.from);
  if (filters.to) params.set("to", filters.to);
  const query = params.toString();
  return request<RoomUsage[]>(`/api/reports/room-usage${query ? `?${query}` : ""}`);
}
