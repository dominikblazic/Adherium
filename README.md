# Adherium — Adherence Recalculation on Device Reassignment

This is a small .NET 10 service that ingests a batch of device sensor-log
events, attributes each one to the prescription that was active **for that device at the time of
the event**, and returns recalculated daily adherence plus a per-event audit trail.

## Running the service

The service can be run via terminal with:

```bash
dotnet run --project src/Adherium.Adherence.Api
```

or opened through an IDE of choice and run immediately without prior configurations. 

The API seeds its in-memory data from [`docs/sample-batch.json`](docs/sample-batch.json) at
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

Or via Bruno/Postman at the same endpoint.

The response has three parts: a `summary` (received / processed / duplicates / unattributed),
the recalculated `dailyAdherence`, and an `outcomes` array explaining what happened to every event.

## Tests

Unit tests for the service can be run via terminal with:

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
  doesn't double-count with duplicates being reported as `DuplicateIgnored`, and a re-sent batch yields
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
- **In-memory storage**, single process as per the brief. The repository interfaces are the seam for a
  real database.

## Assumptions (pending product/clinical confirmation)

These points were raised through an email with questions with the product team. While
waiting for the answers, v1 ships the assumptions below. Each notes where it would change.

- **Assignments are authoritative going forward.** Each event is attributed and stamped with its
  prescription at ingest; if assignment history later changes (a backdated reassignment, or a
  corrected date), already-stored events are **not** re-attributed. Safe while assignments are only
  ever set correctly forward. To support retroactive changes, store the raw events and re-derive
  attribution on demand rather than stamping once.
- **Only days with activity are reported.** A day with no actuations produces no row, so a
  fully-missed scheduled day is currently indistinguishable from "no data" rather than shown as 0%.
  Emitting explicit 0% rows for prescribed (controller) days would need a defined "as-of" date and
  the prescription's active range.
- **Adherence is puff-count per day, not per scheduled administration.** Doses are counted against
  `DosesPerAdmin × TimesPerDay` for the day, so 4 morning puffs and none in the evening still scores
  100%. Assessing dose timing/spacing would require evaluating each administration separately.
- **Attribution is gated by the device-assignment window only.** A prescription's own
  `StartUtc`/`EndUtc` do not additionally gate attribution — an event inside the assignment window
  but outside the prescription's active dates still counts. Adding that check would make the
  prescription dates a second gate.
- **Idempotency assumes a stable `deviceLogId`** — see the *Sequence resets* caveat above.

## API versioning

URL-segment versioning (`/api/v{version}/…`) via `Asp.Versioning`, defaulting to v1. This is the
migration-friendly shape: new versions are added side-by-side without breaking existing device
fleets in the field.

## Fitting into a migration (Part 2)

This is intentionally a thin **vertical slice** with just the recalculation responsibility. It can
be introduced alongside the existing system using strangler-fig pattern rather than as a big-bang rewrite:

- **Additive, versioned surface.** The new `/api/v1` endpoint is new so nothing the legacy system
  exposes changes. Existing callers are unaffected.
- **Shadow before cutover.** Because recalculation is deterministic and idempotent, the new service
  can run in parallel on the same events and have its output compared against the
  legacy result before any traffic is switched over.
- **Safe dual-delivery.** The `(deviceSerial, deviceLogId)` idempotency key means events can be
  delivered to both the old and new paths during the transition without double-counting on retries
  or replays.
- **One source of truth.** The repository interfaces are the seam: the in-memory repositories can be
  swapped for adapters over the legacy database (or a read replica), so the slice reads/writes the
  same data rather than forking it.
- **Reversible.** Cutover can be done per-route or behind a feature flag and rolled back by routing
  away which means no schema break is required.

Together these let the slice be deployed incrementally and prove itself against real data which
demonstrates not just the calculation, but how this slice would coexist with a live legacy system
without breaking it.
