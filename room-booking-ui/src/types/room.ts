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
