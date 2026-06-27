# Questions for the team

Questions raised while building Part 1. For each I've noted the assumption I shipped against, so the
build is usable today and these are about *confirming or correcting* — not blockers.

Routing: **Product → Luke Allera (lukea@)**, **Engineering → David (davidh@)**.

---

## Product / clinical semantics (Luke)

### 1. Whose "day" is a daily adherence day? (timezone)
I bucket doses by **UTC calendar day**. For a patient in, say, NZ, a 9pm local dose lands on the
*next* UTC day, which shifts that dose between daily figures and can misattribute a missed evening.
- **Q:** Should daily adherence use the **patient's local day**? If so, is a timezone available per
  patient (or per prescription/device), and how should DST transition days be handled?
- *Why it matters:* this changes the data model (a timezone field) and the numbers clinicians see.

### 2. Should missed days appear as 0%, or not at all?
Today I only emit a summary for days that **have** events. A patient who took nothing produces no
row — so a fully non-adherent stretch looks like "no data" rather than "0%". For an adherence
product that inverts the most important signal.
- **Q:** For an active prescription, should we emit **0%-adherence rows for prescribed days with no
  actuations** (up to "today" / prescription end)? This is especially relevant given prescription
  200 in the sample is open-ended but its device was reassigned away on 03-07 — should it keep
  accruing missed days while it has no device?
- *Why it matters:* it's arguably the core of adherence monitoring; it needs a clock/"as-of" date and
  a definition of when a prescription is considered active.

### 3. Adherence by puff count, or by scheduled administration?
`dailyPrescribedDoses = dosesPerAdmin × timesPerDay`, and I count each `Actuation` (puff) toward it.
So 4 morning puffs and 0 in the evening scores **100%** by puff count, even though the evening
**administration** was missed.
- **Q:** Should adherence be evaluated at the **administration level** (did they take the morning and
  evening dose, roughly on time) rather than raw daily puff totals? Is dose *timing/spacing* in scope?
- *Why it matters:* clinically these are very different; it changes the calculation, not just the API.

### 4. What does a reliever with a schedule mean? (sample data)
Prescription 300 is a **reliever** (PRN rescue) yet carries `dosesPerAdmin:1, timesPerDay:2`. I treat
relievers as usage-only (no adherence %), so that schedule is currently ignored.
- **Q:** For a reliever, is `timesPerDay` a **max/expected baseline** (so we can flag *overuse*), or
  is it meaningless and just an artifact? Reliever overuse (e.g. SABA) is itself a key clinical
  signal — do you want a usage/overuse metric rather than an adherence %?

### 5. How should over-use (>100%) be represented?
I deliberately **don't cap** the rate (e.g. 5 of 4 puffs → 125%) since over-use is meaningful.
- **Q:** Does the consumer want it capped at 100% for a clean "compliance" figure, with over-use
  surfaced **separately** as a flag? Or is the uncapped rate fine as-is?

---

## Engineering / data integrity (David)

### 6. What actually triggers "recalculation on reassignment"?
The title is *Recalculation on Device Reassignment*, but assignments are static seed data and my API
recalculates on **event ingestion**. In production, the recalc-worthy event is often the
**reassignment itself** — or a *backdated correction* to assignment history — which should re-attribute
events that were already stored.
- **Q:** How do assignment changes enter the system, and should a new/corrected assignment trigger
  **re-attribution and recalculation of historical events** (not just newly-arriving ones)?
- *Why it matters:* this is the heart of the brief; it determines whether attribution must be
  re-runnable over stored events, which affects the storage/processing design.

### 7. Is `deviceLogId` a safe idempotency key forever?
I dedupe on `(deviceSerial, deviceLogId)`. The sample README hints at sequence resets after
refurbishment, which would let an old and a new event **collide** on the same key.
- **Q:** Is `deviceLogId` strictly monotonic per device for its whole lifetime, or can it reset? Is
  there a server-assigned event id or a device "epoch" we should incorporate into the key?

### 8. Events that fall in an assignment gap
Event `logId 40` (03-08) lands in the gap between assignments (200 ends 03-07, 300 starts 03-10), so
I mark it **Unattributed**. That's safe, but it may also be a real dose during a brief unassigned
window.
- **Q:** Should gap events be left unattributed (current), or attached to the **nearest** window
  within some tolerance? Is there an expected SLA for how stale assignment data can be?

### 9. Prescription validity vs. assignment window
Attribution resolves via the **assignment** window, then looks up the prescription — but I don't also
check the prescription's own `startUtc/endUtc`. An event inside an assignment window but outside the
prescription's active dates would still attribute.
- **Q:** Should the prescription's own validity dates **also gate** attribution, or is the assignment
  window the single source of truth?

### 10. Read path / consumer shape
Right now adherence is only returned from the **POST /recalculate** response; there's no way to query
existing adherence without re-posting.
- **Q:** Who consumes this (clinician dashboard, patient app, analytics pipeline), and do you want a
  **GET** to query stored adherence by patient/prescription/date range? That drives pagination and
  the persistence model.
