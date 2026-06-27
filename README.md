# Adherium — Adherence Recalculation on Device Reassignment

Take-home case (Part 1). A small .NET 10 service that ingests a batch of device sensor-log
events, attributes each one to the prescription that was active **for that device at the time of
the event**, and returns recalculated daily adherence plus a per-event audit trail.

The interesting part is *reassignment*: one physical inhaler can serve different prescriptions (and
different patients) over time. A historical event must be scored against whoever owned the device
**when the event happened**, not whoever owns it now.

## Running it

```bash
dotnet run --project src/Adherium.Adherence.Api
```

The API seeds its in-memory stores from [`docs/sample-batch.json`](docs/sample-batch.json) at
startup. In Development, the OpenAPI document is served at `/openapi/v1.json`.

Post a batch (the `batch` object from the sample file is the request body):

```bash
curl -X POST http://localhost:5267/api/v1/sensor-logs/recalculate \
  -H "Content-Type: application/json" \
  -d @- <<'JSON'
{ "events": [ { "deviceSerial": "DEV-AAA", "deviceLogId": 11,
  "eventTimestampUtc": "2026-03-02T08:00:00Z", "eventType": "Actuation" } ] }
JSON
```

The response has three parts: a `summary` (received / processed / duplicates / unattributed),
the recalculated `dailyAdherence`, and an `outcomes` array explaining what happened to every event.

## Tests

```bash
dotnet test
```

25 unit tests over the core logic — the adherence calculation, attribution, and the
ingest→stamp→recalculate orchestration. They live in
[`tests/Adherium.Adherence.Core.Tests`](tests/Adherium.Adherence.Core.Tests) and are the
authoritative spec for the clinical rules below.

## Layout

```
src/
  Adherium.Adherence.Core   Domain + logic (no web dependencies) — the unit-tested heart.
  Adherium.Adherence.Api    Minimal API: versioned endpoint, validation, seeding, OpenAPI.
tests/
  Adherium.Adherence.Core.Tests
```

`Core` has no dependency on ASP.NET, so the rules can be tested in isolation and the storage layer
(`InMemory*Store`) can be swapped for a real repository later without touching the logic.

## Design decisions & rules

- **Attribution is time-based.** Each device has a history of assignment windows; an event resolves
  to the window that `Covers` its timestamp. Windows are treated as **end-inclusive**
  (`[start, end]`) to match the sample data's `23:59:59` end-of-day boundaries with explicit gaps.
- **Idempotent ingestion.** `(deviceSerial, deviceLogId)` is the idempotency key. Re-sending a batch
  doesn't double-count; duplicates are reported as `DuplicateIgnored`, and a re-sent batch yields
  identical summaries.
- **Recalculation is authoritative.** Affected days are recomputed from the full stored set, not
  just the incoming batch, so the returned summaries are always the current truth for those days.
- **What counts as a dose.** Only `Actuation` events count. Other types (e.g.
  `PeakInhalationFlow`) are stamped for traceability but never counted. Unknown event types degrade
  gracefully to `Unknown` rather than failing the batch.
- **Adherence rate** = `dosesTaken / dosesPrescribed × 100`, rounded to 2dp. **Not capped at 100%** —
  over-use is itself a clinically meaningful signal.
- **Relievers (PRN)** report usage (`dosesTaken`) but **no rate** (`null`): "percent of scheduled
  doses" is meaningless for as-needed rescue medication.
- **Unattributable events** are never dropped silently — each gets an outcome with a reason
  (unknown device / no active assignment / missing prescription).

## Known caveats (deliberately scoped out for the 2–3h budget)

- **Day boundary is UTC.** Days are bucketed in UTC; the clinically correct boundary is the
  patient's *local* day. Carrying a timezone per patient/prescription would fix this.
- **Sequence resets.** The idempotency key assumes `deviceLogId` is monotonic per device. A device
  refurbished and re-issued could reset its counter and collide. A device-side epoch would
  disambiguate.
- **In-memory storage**, single process — per the brief. The store interfaces are the seam for a
  real database.

## API versioning

URL-segment versioning (`/api/v{version}/…`) via `Asp.Versioning`, defaulting to v1. This is the
migration-friendly shape: new versions are added side-by-side without breaking existing device
fleets in the field.
