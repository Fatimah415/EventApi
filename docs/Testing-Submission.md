# Testing Assignment Submission

## Assignment

Write unit and integration tests for at least two services, achieving greater than 80% coverage. Submit the test project, coverage report screenshot, UAT test plan, two Loom videos, AI prompts used, and a link to the test-branch pull request.

## Verified results

| Evidence | Result |
|---|---:|
| Test run | 78 passed, 0 failed, 0 skipped |
| EventService line coverage | 100% (141/141) |
| AdminService line coverage | 100% (29/29) |
| Combined line coverage | 100% (170/170) |
| Combined branch coverage | 100% (39/39) |
| UAT scripts | 10 |

These figures come from the mentor-required `EventService` + `AdminService` Coverlet profile. They do not claim whole-application coverage.

## Submission links and evidence

- **Technical Loom video:** `[ADD TECHNICAL LOOM URL]`
- **Non-technical UAT Loom video:** `[ADD NON-TECHNICAL UAT LOOM URL]`
- **Prerequisite/base PR:** [Fatimah415/EventApi#2](https://github.com/Fatimah415/EventApi/pull/2)
- **Mentor-remediation PR:** `[ADD MENTOR-REMEDIATION PR URL]`
- **Coverage screenshot:** `docs/evidence/two-service-coverage.png`
- **UAT plan:** `docs/UAT-Plan.md`
- **AI testing prompts:** `docs/testing-ai-prompts.md`

The two Loom placeholders and mentor-remediation PR placeholder are not real URLs. Replace each one only after the corresponding artifact exists.

### Stacked PR scope

- **Base:** `feature/ai-testing`
- **Compare:** `feature/uat-testing`

This stacked PR contains only the mentor-remediation changes. It does not target `main` and does not add the prerequisite Auth/DB, admin, file-upload, or other application changes to the remediation diff.

## Generate the final coverage report

From the repository root:

```powershell
dotnet test EventApi.Tests\EventApi.Tests.csproj --no-restore
dotnet test EventApi.Tests\EventApi.Tests.csproj --no-restore --settings coverlet.submission.runsettings --collect:"XPlat Code Coverage" --results-directory TestResults\SubmissionTwoServices
```

The second command creates:

```text
TestResults/SubmissionTwoServices/<run-id>/coverage.cobertura.xml
```

Open that XML in a Cobertura-compatible viewer such as Visual Studio Fine Code Coverage or ReportGenerator. Capture a screenshot that visibly shows the two-service scope and combined coverage above 80%, then save it as:

```text
docs/evidence/two-service-coverage.png
```

The generated `TestResults` directory and raw `coverage.cobertura.xml` are ignored build artifacts and do not need to be committed unless the assessor explicitly requests the raw XML.

## Files required in the test-branch PR

### Test project

- `EventApi.Tests/EventApi.Tests.csproj`
- `EventApi.Tests/AdminServiceTests.cs`
- `EventApi.Tests/EventServiceTests.cs`
- `EventApi.Tests/FileServiceTests.cs`
- `EventApi.Tests/FileValidatorTests.cs`
- `EventApi.Tests/EventsControllerTests.cs`
- `EventApi.Tests/EventsControllerIntegrationTests.cs`
- `EventApi.Tests/Infrastructure/CustomWebApplicationFactory.cs`

`EventApi.Tests/TestDbContextFactory.cs` is currently unused and is not required for this submission unless a test is changed to consume it.

### Coverage and documentation

- `coverlet.submission.runsettings`
- `docs/Testing-Guide.md`
- `docs/Testing-Submission.md`
- `docs/testing-ai-prompts.md`
- `docs/UAT-Plan.md`
- `docs/evidence/two-service-coverage.png` after the screenshot is captured

`FileServiceTests.cs` and `FileValidatorTests.cs` are additional coverage. They are not a substitute for `AdminServiceTests.cs`.

Do not commit `bin/`, `obj/`, `TestResults/`, uploaded files, `AGENTS.md`, the unused `TestDbContextFactory.cs`, or the superseded event-feature-only `coverlet.runsettings` as assignment evidence.

## Final checklist

| Deliverable | Status |
|---|---|
| EventService and AdminService meaningfully unit tested | COMPLETE |
| Unit and integration test project | COMPLETE |
| Combined line coverage greater than 80% | COMPLETE — 100% |
| Dedicated two-service Coverlet profile | COMPLETE |
| UAT plan with at least eight scripts | COMPLETE — 10 scripts |
| AI prompt evidence and method traceability | COMPLETE — 42/68 methods (61.76%) have stored remediation-prompt evidence |
| Repository-owner human review confirmation | PENDING — owner must personally review and confirm three entries |
| Coverage screenshot | COMPLETE |
| Technical Loom video and genuine URL | PENDING |
| Non-technical UAT Loom video and genuine URL | PENDING |
| Assessment branch | COMPLETE — local `feature/uat-testing` worktree |
| Commit, push, and PR containing this mentor remediation | PENDING |
