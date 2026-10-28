export interface RoomUsage {
  roomId: number;
  roomName: string;
  bookingCount: number;
  cancelledCount: number;
  bookedHours: number;
  actualHours: number;
  occupancyPercent: number;
  usageRatePercent: number;
}

export interface UsageFilters {
  from?: string;
  to?: string;
}
