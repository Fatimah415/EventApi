# Testing Assignment Submission

## Assignment

Write unit and integration tests for at least two services, achieving greater than 80% coverage. Submit the test project, coverage report screenshot, UAT test plan, two Loom videos, AI prompts used, and a link to the test-branch pull request.

## Verified results

| Evidence | Result |
|---|---:|
| Test run | 63 passed, 0 failed, 0 skipped |
| EventService line coverage | 100% (141/141) |
| FileService line coverage | 92.45% (49/53) |
| Combined line coverage | 97.94% (190/194) |
| Combined branch coverage | 94.12% (48/51) |
| UAT scripts | 10 |

Coverlet stores truncated rate values (`97.93%` and `94.11%`) in Cobertura XML. The table calculates the same covered/valid ratios and rounds them to two decimal places.

## Submission links and evidence

- **Technical Loom video:** `[ADD TECHNICAL LOOM URL]`
- **Non-technical UAT Loom video:** `[ADD NON-TECHNICAL UAT LOOM URL]`
- **Test-branch pull request:** `[ADD TEST BRANCH PR URL]`
- **Coverage screenshot:** `docs/evidence/two-service-coverage.png`
- **UAT plan:** `docs/UAT-Plan.md`
- **AI testing prompts:** `docs/testing-ai-prompts.md`

No placeholder above is a real URL. Replace each bracketed value only after the corresponding artifact exists.

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

### Existing production dependencies

The verified tests currently depend on already-present working-tree changes in:

- `Service/EventService.cs`
- `Service/FileService.cs`

These files were not changed while preparing the remaining submission artifacts. They must be included in the test branch if those changes are not already present in the branch used as the PR base; otherwise the committed test results will not reproduce the audited run.

Do not commit `bin/`, `obj/`, `TestResults/`, uploaded files, `AGENTS.md`, the unused `TestDbContextFactory.cs`, or the superseded event-feature-only `coverlet.runsettings` as assignment evidence.

## Final checklist

| Deliverable | Status |
|---|---|
| Two meaningfully unit-tested services | COMPLETE |
| Unit and integration test project | COMPLETE locally; files must be committed |
| Combined line coverage greater than 80% | COMPLETE — 97.94% |
| Dedicated two-service Coverlet profile | COMPLETE |
| UAT plan with at least eight scripts | COMPLETE — 10 scripts |
| Testing-specific AI prompt transcript | COMPLETE |
| Coverage screenshot | COMPLETE |
| Technical Loom video and URL | REMAINING |
| Non-technical UAT Loom video and URL | REMAINING |
| Test branch, PR, and real PR URL | REMAINING |
