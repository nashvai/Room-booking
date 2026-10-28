# Room Booking / Room Reservation System

A small room reservation application with an ASP.NET Core Web API and a React + TypeScript frontend.

## Run locally

Requirements: .NET 10 SDK and Node.js with npm.

1. Start the API:

   ```powershell
   cd RoomBooking.Api
   dotnet run --launch-profile http
   ```

   The API listens at `http://localhost:5148` and seeds an in-memory database when it starts.

2. In another terminal, start the UI:

   ```powershell
   cd room-booking-ui
   Copy-Item .env.example .env
   npm.cmd install
   npm.cmd run dev
   ```

   Vite uses `http://localhost:5173`. The backend CORS policy allows that origin.

3. Open `http://localhost:5173`. The root route redirects to `/rooms`.

The API client defaults to `http://localhost:5148`; `VITE_API_URL` can override it in the UI environment.

## Architecture

- `RoomBooking.Api` is an ASP.NET Core controller API backed by EF Core InMemory.
- Booking validation runs through ordered rule classes, including time range, future start, horizon, capacity, duration, active room, room overlap, and organizer overlap rules.
- `TimeProvider` supplies current time for reservation status, validation, and reports.
- A reservation stored as `Booked` is reported as `Completed` when its scheduled end is at or before now. Reads do not mutate stored status.
- The React UI uses a shared API request helper and separate API modules for rooms, users, reservations, and reports. Filters are sent to the API; the UI does not filter or count records locally.
- OpenAPI is available at `/openapi/v1.json` in Development.

## Pages and API routes

| UI page | API route | Description |
| --- | --- | --- |
| Rooms | `GET /api/rooms` | List rooms; filter by `minCapacity`, `equipment`, and `floor`. Equipment flags can be combined. |
| Users | `GET /api/users` | List organizers and active reservation counts. |
| Reservations | `GET`, `POST /api/reservations` | Filter and list bookings or create one. |
| Reservations | `POST /api/reservations/{id}/cancel` | Cancel a future booking. |
| Reservations | `POST /api/reservations/{id}/end` | End an in-progress booking early. |
| Room usage | `GET /api/reports/room-usage` | Per-room usage summary with optional `from` and `to` query values. |

Booking validation errors use an `{ "code": "...", "message": "..." }` response. The UI displays the server message, including conflict responses.

## Usage report definitions

- **Bookings** counts non-cancelled reservations in the selected report window. Cancelled bookings are counted separately and add no hours.
- **Booked hours** sums scheduled durations clipped to the report window.
- **Actual hours** sums finished meeting durations, using `ActualEndTime` when present and otherwise the scheduled end.
- **Usage rate** is actual hours divided by scheduled hours for the same finished meetings. In-progress reservations do not enter this ratio.
- **Occupancy** is actual occupied duration divided by the full report-window duration. The current model assumes rooms can be occupied 24 hours a day, every day.

The report page sorts rooms by usage rate and shows both usage rate and occupancy so readers can compare how booked a room was with how much scheduled time meetings used.

## Known limitations

- EF Core InMemory is temporary; data resets when the API process restarts. It is suitable for this assignment/demo, not durable production data.
- The report's occupancy denominator assumes 24-hour availability and does not model room opening hours, holidays, or maintenance closures.
- The current UI covers room browsing, users, reservations, and the room usage report. It does not include a room timeline or PDF export.
- The local `.env` file is ignored by Git. Start from `.env.example` when setting up a checkout.
