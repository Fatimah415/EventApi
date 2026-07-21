# AI-Assisted Testing Prompt Transcript

## Provenance and accuracy note

This transcript records only prompt text that is attributable to the supplied course material or to the Codex conversation used to complete and audit this testing assignment. It does not invent separate conversations that did not happen.

`EventServiceTests.cs`, `FileServiceTests.cs`, and `FileValidatorTests.cs` already existed when the first repository audit began. Their original granular generation prompts were not stored in the repository. The broad remediation prompt below was then used to review, repair, and complete the test implementation. The course prompts below were supplied as part of the assignment context and describe the requested testing approach.

## 1. EventService unit tests

**Source:** Course assignment supplied to Codex.

> I have an EventService that depends on IEventRepository. Write xUnit tests with Moq for CreateEvent, GetAllEvents, GetEventById, DeleteEvent. Cover happy paths, null arguments, non-existent ids, and verify repository calls. Use Arrange-Act-Assert pattern and meaningful names.

**Actual follow-up remediation prompt from the project conversation:**

> fix all these issues and make sure it matches with task description 100/100%

**How AI used it:** AI audited the existing `EventServiceTests`, retained the meaningful tests, added missing validation and cleanup-path coverage, and reran the suite and Coverlet report to locate remaining uncovered branches.

## 2. FileService unit tests

**Source:** Existing tests were present before the recorded audit; their original standalone generation prompt is unavailable.

**Actual prompt used to audit and preserve this test area:**

> Re-evaluate the project strictly against this exact assignment requirement: "Write unit and integration tests for at least two services, achieving >80% coverage."

The prompt further required Codex to identify two actual service classes with meaningful unit tests and calculate their combined line and branch coverage.

**How AI used it:** AI inspected `FileServiceTests`, verified tests for valid saves, invalid files, directory creation, PNG handling, null/empty paths, successful deletion, missing files, and path traversal, then measured `FileService` independently with Coverlet. No additional FileService tests were invented during the final deliverable phase.

## 3. EventsController unit tests

**Source:** Actual remediation prompt from the project conversation.

> fix all these issues and make sure it matches with task description 100/100%

**How AI used it:** After the first coverage run showed controller branches unexecuted, AI generated focused Moq-based controller tests for weather success, missing events, missing locations, invalid model state, and missing owners. These tests exercise controller decisions without invoking external services.

## 4. Events API integration tests

**Source:** Course assignment supplied to Codex.

> Generate integration tests for all CRUD endpoints. Include seeding of test data, assertions on status codes, response bodies, and database state. Use the factory to create an HTTP client. Make sure you test both authorised and unauthorised scenarios if your API requires authentication.

**Actual follow-up remediation prompt:**

> fix all these issues and make sure it matches with task description 100/100%

**How AI used it:** AI expanded the existing `WebApplicationFactory` suite to cover collection/detail GET, POST, PUT, DELETE, model validation, 404 responses, response bodies, `Location` headers, ordering, and persisted database state using an isolated EF Core InMemory database.

## 5. JWT and authorization testing

**Source:** Course assignment supplied to Codex.

> Modify the CustomWebApplicationFactory to support test authentication. Add a TestAuthHandler that returns a successful authentication result with a test user claim. Override the JWT middleware configuration in ConfigureWebHost. The test client should then automatically send authorized requests.

**Implementation refinement made by AI:** The project did not bypass authentication with `TestAuthHandler`. AI instead configured deterministic test issuer/audience/key values and generated real signed JWTs with Admin or User role claims. The resulting integration tests verify 401 for anonymous callers, 403 for authenticated non-admin users, and successful CRUD operations for administrators.

## 6. Coverage-gap analysis

**Source:** Course assignment supplied to Codex.

> Given this C# controller action and the following Coverlet report summary, identify the uncovered branches and suggest specific unit tests to achieve >80% line coverage.

**Actual strict audit prompt:**

> Do not assume whole-application coverage is required. Identify at least TWO actual service classes that have meaningful unit tests. Calculate the combined line coverage and branch coverage for those two services. Confirm whether their combined line coverage is greater than 80%. Ensure the coverage configuration does not falsely claim whole-application coverage.

**How AI used it:** AI ran Coverlet without restrictive filters for the audit, aggregated unique executable source lines and branches from `EventService.cs` and `FileService.cs`, and verified 97.94% combined line coverage and 94.12% combined branch coverage. A dedicated submission profile now measures exactly those two service class families.

## 7. Final submission-deliverable prompt

**Source:** Actual project conversation.

> Complete only the remaining submission deliverables: create a dedicated Coverlet submission profile that measures exactly EventApi.Services.EventService* and EventApi.Services.FileService*; create/update a testing-specific AI prompt transcript; review the repository and identify every required test/UAT/coverage documentation file that must be committed; do not create fake Loom URLs or fake PR URLs; and do not modify unrelated production code.

This final prompt changed documentation and the submission coverage profile only. It did not generate or modify production code or add tests.
