
const API_BASE_URL = (
  import.meta.env.VITE_API_URL || "http://localhost:5148"
).replace(/\/$/, "");

export interface ApiError {
  code: string;
  message: string;
}

export class ApiRequestError extends Error {
  code: string;
  status: number;

  constructor(status: number, error: ApiError) {
    super(error.message);
    this.name = "ApiRequestError";
    this.status = status;
    this.code = error.code;
  }
}

export async function apiRequest<T>(
  path: string,
  options: RequestInit = {}
): Promise<T> {
  let response: Response;

  try {
    response = await fetch(`${API_BASE_URL}${path}`, {
      ...options,
      headers: {
        Accept: "application/json",
        ...options.headers,
        ...(options.body ? { "Content-Type": "application/json" } : {}),
      },
    });
  } catch {
    throw new Error(
      "Could not connect to the API. Check that the backend is running."
    );
  }

  if (!response.ok) {
    let apiError: ApiError = {
      code: "HTTP_ERROR",
      message: `Request failed with status ${response.status}.`,
    };

    try {
      const body: unknown = await response.json();

      if (
        typeof body === "object" &&
        body !== null &&
        "code" in body &&
        "message" in body &&
        typeof body.code === "string" &&
        typeof body.message === "string"
      ) {
        apiError = {
          code: body.code,
          message: body.message,
        };
      }
    } catch {
      // Keep the fallback error if the response isn't JSON.
    }

    throw new ApiRequestError(response.status, apiError);
  }

  if (response.status === 204) {
    return undefined as T;
  }

  return (await response.json()) as T;
}
