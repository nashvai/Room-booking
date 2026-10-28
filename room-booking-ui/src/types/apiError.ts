export interface ApiError {
  code: string;
  message: string;
}

export class ApiRequestError extends Error {
  status: number;
  apiError: ApiError | null;

  constructor(status: number, apiError: ApiError | null) {
    super(
      apiError?.message ??
        (status > 0 ? `Request failed (${status})` : "Could not connect to the API."),
    );
    this.name = "ApiRequestError";
    this.status = status;
    this.apiError = apiError;
  }
}
