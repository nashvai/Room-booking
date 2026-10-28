import { ApiRequestError, type ApiError } from "../types/apiError";

const API_URL = (import.meta.env.VITE_API_URL || "http://localhost:5148").replace(/\/$/, "");

function isApiError(value: unknown): value is ApiError {
  return typeof value === "object" && value !== null &&
    "code" in value && typeof value.code === "string" &&
    "message" in value && typeof value.message === "string";
}

export async function request<T>(path: string, init: RequestInit = {}): Promise<T> {
  let res: Response;

  try {
    const headers = new Headers(init.headers);
    if (!headers.has("Accept")) headers.set("Accept", "application/json");
    if (init.body && !headers.has("Content-Type")) headers.set("Content-Type", "application/json");
    res = await fetch(`${API_URL}${path}`, {
      ...init,
      headers,
    });
  } catch {
    throw new ApiRequestError(0, null);
  }

  if (!res.ok) {
    let apiError: ApiError | null = null;
    try {
      const body: unknown = await res.json();
      if (isApiError(body)) apiError = body;
    } catch {
      // The response may be empty or non-JSON; use the status fallback.
    }
    throw new ApiRequestError(res.status, apiError);
  }

  if (res.status === 204) return undefined as T;
  return (await res.json()) as T;
}
