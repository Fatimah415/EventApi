# Event Board User Acceptance Test Plan

## 1. Purpose

This plan validates that Event Board meets its business requirements from an end-user and administrator perspective. It complements automated unit and API integration tests by checking complete workflows, understandable feedback, authorization, data integrity, file safety, and the weather integration.

## 2. Scope

Included: administrator login, rejected login, event listing, event creation, editing, deletion, image upload validation, role-based API authorization, and event weather retrieval. Automated test code is not itself part of UAT; the deployed application behavior is.

Excluded: production load testing, disaster recovery, unsupported file types other than those explicitly tested, and third-party weather accuracy beyond confirming that the displayed city and response are plausible.

## 3. Test environment and data

- Application: current EventApi build in the Development environment.
- Browser: current Chrome or Edge desktop release.
- Database: dedicated SQL Server/LocalDB UAT database created from the latest migrations. Never use production data.
- API base URL: use the HTTPS URL printed by `dotnet run` (normally `https://localhost:<port>`).
- API client: Swagger UI or an equivalent HTTP client.
- Seeded administrator: `admin@eventboard.com`; obtain its password through the approved local test-data channel.
- Seeded standard user: `sara@eventboard.com`.
- Image fixtures: one genuine PNG/JPEG under 5 MB, one text file renamed `.jpg`, and one image over 5 MB.
- Defect record fields: script ID, build, browser, steps, expected/actual result, screenshot, severity, and owner.

Before execution, apply migrations, start the API, confirm the database is disposable, and verify that the weather API key is configured without recording it in evidence.

## 4. Risk-based priority

| Rank | Area | Impact | Likelihood | Priority | Reason |
|---:|---|---|---|---|---|
| 1 | Authorization | Critical | Medium | P0 | A failure could allow a standard or anonymous user to change data. |
| 2 | Event deletion | High | Medium | P0 | Deletion causes permanent data loss and has no undo. |
| 3 | Image upload | High | High | P0 | Uploaded content and paths create security and storage risks. |
| 4 | Event creation | High | Medium | P1 | Incorrect ownership or fields corrupt core business data. |
| 5 | Event editing | High | Medium | P1 | Updates must retain identity/ownership and correctly replace related data. |
| 6 | Login | High | Low | P1 | Administrators cannot work if authentication fails. |
| 7 | Event listing | Medium | Medium | P2 | Users depend on accurate, ordered event discovery. |
| 8 | Weather integration | Low | High | P2 | A third-party outage must not expose internals or break unrelated workflows. |

Execute P0 scripts first, followed by P1 and P2.

## 5. Entry, exit, schedule, and roles

Entry criteria: the application builds; automated tests pass; the UAT database is available; seeded accounts work; no known blocker prevents execution.

Exit criteria:

- 100% of P0 scripts pass.
- At least 95% of all executed scripts pass.
- No open Critical or High defect remains.
- Medium defects have an accepted workaround and named owner.
- Failed scripts are rerun after fixes, and evidence is attached.
- Product owner or designated learner records final acceptance.

Suggested schedule: Day 1 environment smoke check and P0; Day 2 P1; Day 3 P2, regression, and sign-off. The learner/tester executes and records evidence; the developer fixes defects; the product owner (or internship supervisor) approves exit criteria.

## 6. UAT scripts

### UAT-AUTH-001 — Administrator signs in

**Priority:** P1  
**Preconditions:** Application and UAT database are running; seeded administrator is active; user is signed out.

| Step | Action | Expected result |
|---:|---|---|
| 1 | Open `/Admin/Login`. | The login page loads over HTTPS. |
| 2 | Enter the valid administrator email and password. | Values are accepted without exposing the password. |
| 3 | Submit the form. | User is redirected to the admin dashboard. |
| 4 | Open the Events page. | Events are visible and administrative actions are available. |

**Actual result:** ____________________  
**Status:** Not Executed / Pass / Fail  
**Evidence/defect:** ____________________

### UAT-AUTH-002 — Invalid login is rejected

**Priority:** P1  
**Preconditions:** User is signed out and on `/Admin/Login`.

| Step | Action | Expected result |
|---:|---|---|
| 1 | Enter `admin@eventboard.com` with an incorrect password. | Form remains safe to submit; password is masked. |
| 2 | Submit the form. | Access is denied with a clear, non-technical message. |
| 3 | Browse directly to `/Admin/Events`. | User is redirected to login and protected data is not shown. |

**Actual result:** ____________________  
**Status:** Not Executed / Pass / Fail  
**Evidence/defect:** ____________________

### UAT-AUTH-003 — Non-admin cannot modify events through the API

**Priority:** P0  
**Preconditions:** A valid standard-user JWT is available in Swagger; seeded event 1 exists.

| Step | Action | Expected result |
|---:|---|---|
| 1 | Authorize Swagger with the standard-user token. | Token is accepted for authenticated requests. |
| 2 | Submit a valid `POST /api/events`. | API returns 403 Forbidden; no event is added. |
| 3 | Submit `DELETE /api/events/1`. | API returns 403 Forbidden; event 1 still exists. |
| 4 | Remove the token and repeat either operation. | API returns 401 Unauthorized. |

**Actual result:** ____________________  
**Status:** Not Executed / Pass / Fail  
**Evidence/defect:** ____________________

### UAT-EVENT-001 — User views the event list and details

**Priority:** P2  
**Preconditions:** Seed events and categories exist.

| Step | Action | Expected result |
|---:|---|---|
| 1 | Request `GET /api/events`. | Response is 200 and contains the seeded events. |
| 2 | Compare the first and last dates. | Events are ordered chronologically. |
| 3 | Request an ID returned by the list. | Response is 200 with matching title, category, location, and date. |
| 4 | Request `GET /api/events/99999`. | Response is 404 with a useful not-found message. |

**Actual result:** ____________________  
**Status:** Not Executed / Pass / Fail  
**Evidence/defect:** ____________________

### UAT-EVENT-002 — Administrator creates an event

**Priority:** P1  
**Preconditions:** Administrator is logged in; valid owner and category exist.

| Step | Action | Expected result |
|---:|---|---|
| 1 | Open Admin Events and choose Create. | Create form displays all required fields. |
| 2 | Enter title `UAT Workshop`, a future date, location `Lahore`, valid category, owner, and description. | Form accepts the values. |
| 3 | Save. | A success outcome is shown and the user returns to the event workflow. |
| 4 | Find the event in the UI and `GET /api/events`. | One new event appears with exactly the submitted business fields and a server-generated ID/creation time. |

**Actual result:** ____________________  
**Status:** Not Executed / Pass / Fail  
**Evidence/defect:** ____________________

### UAT-EVENT-003 — Invalid event is rejected

**Priority:** P1  
**Preconditions:** Administrator is on the Create Event form; note the current event count.

| Step | Action | Expected result |
|---:|---|---|
| 1 | Leave Title and Location empty and submit. | Clear validation messages identify both required fields. |
| 2 | Enter a title longer than 200 characters and submit. | Length validation is shown. |
| 3 | Refresh the event list. | Event count is unchanged; no partial record was saved. |

**Actual result:** ____________________  
**Status:** Not Executed / Pass / Fail  
**Evidence/defect:** ____________________

### UAT-EVENT-004 — Administrator edits an event

**Priority:** P1  
**Preconditions:** Administrator is logged in; `UAT Workshop` exists.

| Step | Action | Expected result |
|---:|---|---|
| 1 | Open Edit for `UAT Workshop`. | Existing values are populated. |
| 2 | Change title to `UAT Workshop Updated`, location to `Karachi`, and select a different category. | New values appear in the form. |
| 3 | Save. | Update succeeds without creating a duplicate. |
| 4 | View the event through the UI/API. | Title, location, and category changed; ID, owner, and creation time did not. |

**Actual result:** ____________________  
**Status:** Not Executed / Pass / Fail  
**Evidence/defect:** ____________________

### UAT-EVENT-005 — Administrator deletes an event

**Priority:** P0  
**Preconditions:** Administrator is logged in; `UAT Workshop Updated` exists; its ID is recorded.

| Step | Action | Expected result |
|---:|---|---|
| 1 | Choose Delete for the recorded event. | Destructive intent is clear before execution. |
| 2 | Confirm deletion. | Event disappears from the admin list. |
| 3 | Request the recorded ID from the API. | API returns 404. |
| 4 | Delete the same ID again through the API. | API returns 404 and other events remain unchanged. |

**Actual result:** ____________________  
**Status:** Not Executed / Pass / Fail  
**Evidence/defect:** ____________________

### UAT-FILE-001 — Event image upload is safe and consistent

**Priority:** P0  
**Preconditions:** Administrator is creating an event; valid and invalid image fixtures are available.

| Step | Action | Expected result |
|---:|---|---|
| 1 | Attach a genuine PNG/JPEG under 5 MB and save. | Event is created and its image loads from a unique `/uploads/` URL. |
| 2 | Edit the event and upload a different valid image. | New image is displayed and the old file is removed after the database update succeeds. |
| 3 | Attempt upload of renamed text content with `.jpg`. | Request is rejected as invalid; spoofed content is not stored. |
| 4 | Attempt upload of an image over 5 MB. | Request is rejected with a useful size message. |
| 5 | Delete the event. | Database record and current image are removed; no unrelated file is affected. |

**Actual result:** ____________________  
**Status:** Not Executed / Pass / Fail  
**Evidence/defect:** ____________________

### UAT-API-001 — Event weather is displayed and failures are safe

**Priority:** P2  
**Preconditions:** Valid weather API configuration; administrator JWT; one event has location `Lahore` and one test event has a blank/invalid location where permitted by setup.

| Step | Action | Expected result |
|---:|---|---|
| 1 | Request `GET /api/events/{lahoreId}/weather` with the admin JWT. | Response is 200 and contains plausible weather data for Lahore without exposing upstream JSON internals or the API key. |
| 2 | Repeat without a JWT. | Response is 401. |
| 3 | Request weather for a nonexistent event. | Response is 404. |
| 4 | Simulate invalid location/upstream failure in the UAT environment. | API returns a controlled Problem Details error; event listing continues to work. |

**Actual result:** ____________________  
**Status:** Not Executed / Pass / Fail  
**Evidence/defect:** ____________________

## 7. Execution summary and sign-off

| Metric | Result |
|---|---|
| Build/version | ____________________ |
| Execution date | ____________________ |
| Scripts passed / total | ____________________ |
| P0 passed / total | ____________________ |
| Open Critical/High defects | ____________________ |
| Exit criteria met? | Yes / No |

**Tester:** ____________________  
**Product owner/supervisor:** ____________________  
**Decision:** Accepted / Rejected / Accepted with conditions  
**Comments:** ____________________
