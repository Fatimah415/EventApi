# Loom Video Scripts — EventApi

Two scripts, as required by the assessment:

1. **Video 1 — Technical Walkthrough** (~5–6 minutes): architecture choices, for a technical reviewer.
2. **Video 2 — Platform Demo** (~2–3 minutes): non-technical, for a stakeholder.

**Before recording (both videos):**

- Start the API: `dotnet run --launch-profile http` (runs on `http://localhost:5289`).
- Have Postman (or the `EventApi.http` file in Visual Studio) open with the requests ready.
- Demo accounts (already in the database):
  - Admin: `admin@demo.com` / `AdminPass123!`
  - Regular user: `user@demo.com` / `UserPass123!`
- Close extra tabs, mute notifications.

---

## Video 1 — Technical Walkthrough (5–6 min)

> Screen: start on the Solution Explorer showing the folder structure.

### 1. Introduction (30 sec)

"Hi, I'm Fatimah. In this video I'll walk through the architecture of my Event
Board API — a .NET Web API with JWT authentication and role-based
authorization, built with AI assistance. I'll cover the layered architecture,
how authentication works end to end, and the security decisions I made."

### 2. Layered architecture (1 min)

> Screen: expand Controllers, Service, Repositories, Data, Models folders.

"The project follows a clean layered architecture with three layers.

The **Controllers** are the presentation layer. They're deliberately thin —
they only validate input, call a service, and translate the result into an
HTTP status code. For example, `EventsController` never touches the database.

The **Service layer** holds the business logic. `AuthService` decides what
'register' and 'login' mean; `EventService` owns the event rules.

The **Repository layer** is the only place that talks to Entity Framework
Core. `UserRepository` and `EventRepository` wrap the `AppDbContext`, so if
we ever switch databases, the services don't change.

This separation also made AI prompting more reliable: I could ask for one
class at a time — like 'an AuthService that depends on IUserRepository and
IJwtTokenService' — and the generated code fit the structure instead of
hallucinating its own."

### 3. Data model and EF Core (45 sec)

> Screen: open Models/User.cs, then Models/Event.cs, then Data/AppDbContext.cs.

"There are two entities. `User` has a `PasswordHash` — never a plain
password — and a `Role`, which defaults to 'User'. `Event` belongs to a user
through a foreign key, so one user can create many events.

`AppDbContext` also enforces a unique index on Email at the database level,
so duplicate accounts are impossible even under concurrent requests.

The schema is built entirely from code-first migrations — three of them:
the initial Users table, the Events table, and the Role column."

### 4. Authentication flow (1.5 min)

> Screen: open Controllers/AuthController.cs, then Service/AuthService.cs.

"Authentication lives in `AuthController` with two endpoints: register and
login.

On **register**, `AuthService` checks whether the email already exists via
the repository, then hashes the password with **BCrypt** — an adaptive
hashing algorithm designed for passwords, much stronger than something like
SHA256 — and saves the user with the 'User' role.

On **login**, we look the user up by email and verify the password against
the stored hash. One security detail: whether the email is unknown or the
password is wrong, the response is the same '401 Invalid credentials' — so
an attacker can't use the login endpoint to discover which emails exist.

> Screen: open Service/JwtTokenService.cs.

If the credentials are valid, `JwtTokenService` issues a JWT signed with
HS256. The token carries three claims — the user's id, email, and role — and
expires after 60 minutes, configured in appsettings. I used `ClaimTypes.Role`
specifically because ASP.NET maps it to its default role claim, which is what
makes role-based authorization work without extra configuration."

### 5. Middleware and role-based authorization (1 min)

> Screen: open Program.cs, scroll to the JWT section.

"In `Program.cs`, the JWT bearer middleware validates every incoming token:
the signature, the issuer, the audience, and the lifetime — all four checks
are on.

Order matters here: `UseAuthentication` runs **before** `UseAuthorization`,
because the framework has to know *who* you are before it can decide *what*
you're allowed to do.

> Screen: open Controllers/EventsController.cs.

On the Events controller, reading is public — anyone can GET events. But
POST, PUT, and DELETE carry `[Authorize(Roles = "Admin")]`. No token gets a
401; a valid token with the wrong role gets a 403. I promoted my test admin
by updating the Role column directly in the database, which is the promotion
mechanism the lesson prescribes for local testing."

### 6. Live proof (1 min)

> Screen: Postman / the .http file. Run these in order.

"Let me prove it works.

First, I log in as a regular user and copy the token.
Now I try to create an event with that token — **403 Forbidden**, correct,
because they're not an admin.

Now I log in as the admin, use that token instead — **201 Created**.

And with no token at all — **401 Unauthorized**.

Reads stay public: GET events returns 200 without any token."

### 7. Trade-offs and next steps (30 sec)

"Three honest trade-offs. First, the JWT signing key sits in
appsettings.Development.json so the project runs straight after cloning; in
production it would live in Secret Manager or environment variables. Second,
user IDs are integers rather than GUIDs, to stay consistent with the existing
Week 1 schema. Third, refresh tokens are the natural next step — short-lived
access tokens with a rotating refresh token stored server-side.

That's the architecture. Thanks for watching."

---

## Video 2 — Platform Demo for a Stakeholder (2–3 min)

> Audience: non-technical. No code on screen — only Postman/Swagger responses
> and simple language. Avoid: JWT, BCrypt, middleware, endpoint, DTO.
> Screen: start on the running app / Postman with friendly request names.

### 1. What the platform is (30 sec)

"Hi! This is a quick demo of the Event Board platform — a system where an
organization can publish events, and anyone can browse them.

Think of it like a community notice board: everyone can *read* the board, but
only authorized staff can *pin* things to it or take them down. That's the
core idea I'll show you."

### 2. Anyone can browse (30 sec)

> Action: GET all events, show the JSON list — call it "the event list".

"First, the public view. Without signing in at all, anyone can open the
platform and see the list of upcoming events — here's a tech meetup in
Lahore with its date, location, and category.

This is exactly what visitors, students, or community members would see."

### 3. Signing in — and why accounts are safe (45 sec)

> Action: register a new account, then log in as the regular user.

"People can create an account with just a name, email, and password.

Two things happen behind the scenes that I want to highlight for you.
First, passwords are **never stored as readable text** — they're scrambled
with bank-grade one-way protection, so even if someone got hold of our
database, they could not read anyone's password.
Second, when you sign in, the system gives your device a **temporary secure
pass** that expires after one hour — like a visitor badge that stops working
at the end of the day."

### 4. The permission system (45 sec)

> Action: try to create an event as the regular user → show the "forbidden"
> response. Then log in as admin and create it successfully → show the new event.

"Now, the important business rule: not everyone can publish events.

Here I'm signed in as a **regular member**, and I try to publish a new event —
and the platform politely refuses. Members can browse, but they can't post.

Now I sign in as an **administrator** and do the same thing — and this time
the event is published instantly and appears on the public list.

Same action, different person, different outcome. That's role-based access:
the platform knows who you are and what your role allows."

### 5. Why this matters and what's next (30 sec)

"So to summarize what you've seen: an open event board anyone can browse,
safe password handling, hour-long secure sessions, and a permission system
that keeps publishing in the hands of authorized staff.

Next steps on the roadmap would be letting members request events for
approval, and email notifications. Happy to answer any questions — thanks
for watching!"

---

## Recording tips

- **Rehearse once with a timer.** Video 1 should land 5–6 min, Video 2 under 3.
- Run every request once *before* recording so nothing fails on camera.
- In Video 2, rename Postman requests to friendly names ("Browse events",
  "Sign in as admin") — stakeholders will read those, not URLs.
- If a request fails mid-recording, don't restart — say "let me try that
  again" and rerun. Reviewers value composure.
- Loom: use "Screen + Camera" for Video 2 (face builds stakeholder trust);
  screen-only is fine for Video 1.
