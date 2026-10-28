import { request } from "./client";
import type { User } from "../types/user";

export function getUsers(): Promise<User[]> {
  return request<User[]>("/api/users");
}
