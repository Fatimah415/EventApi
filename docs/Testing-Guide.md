# EventApi Automated Testing and Coverage

## Assessment scope

The assignment requires meaningful tests for at least two services and greater than 80% combined coverage. The submission coverage profile measures exactly:

- `EventService` — validation, mapping, file lifecycle, error cleanup, and repository coordination.
- `FileService` — validation delegation, secure file creation, deletion guards, and path safety.

The profile is deliberately named `coverlet.submission.runsettings` and does not claim whole-application coverage. Controller and repository tests still run, but their production classes are not included in the two-service coverage percentage.

## Tooling and test design

- xUnit is the test runner.
- Moq isolates service and controller dependencies for fast unit tests.
- `WebApplicationFactory<Program>` hosts the real HTTP pipeline.
- EF Core InMemory provides a unique database per integration-test instance and loads deterministic `HasData` records.
- Integration tests use real signed JWTs for anonymous, standard-user, and administrator authorization outcomes.
- Coverlet Collector produces Cobertura XML using `coverlet.submission.runsettings`.

Unit tests follow Arrange–Act–Assert and cover successful, negative, boundary, exception, cleanup, and validation paths. Integration tests cover GET collection/detail, POST, PUT, DELETE, model validation, 404 responses, 401/403 authorization, JSON bodies, `Location` headers, ordering, and database state.

## Commands

Run all tests:

```powershell
dotnet test EventApi.Tests\EventApi.Tests.csproj --no-restore
```

Run the assessed two-service coverage profile:

```powershell
dotnet test EventApi.Tests\EventApi.Tests.csproj --no-restore --settings coverlet.submission.runsettings --collect:"XPlat Code Coverage" --results-directory TestResults\SubmissionTwoServices
```

Open the generated `coverage.cobertura.xml` in Visual Studio Fine Code Coverage, ReportGenerator, or VS Code Coverage Gutters. The submission passes when combined line coverage is greater than 80% and the test run has zero failures.

## Verified submission result

Final local verification on 21 July 2026:

- 63 tests passed; 0 failed; 0 skipped.
- `EventService`: 100% line coverage (141/141 unique executable lines).
- `FileService`: 92.45% line coverage (49/53 unique executable lines).
- Combined: 97.94% line coverage (190/194) and 94.12% branch coverage (48/51).
- Coverlet's XML rate fields are truncated to 97.93% and 94.11%; the displayed percentages above are the same ratios rounded to two decimal places.
- Cobertura evidence: `TestResults/SubmissionTwoServices/<run-id>/coverage.cobertura.xml` after running the command above.

## UAT

Automated coverage proves code execution; it does not replace user acceptance. Execute the scripts in `docs/UAT-Plan.md` against a disposable SQL Server UAT environment, record Actual Result and Status, attach evidence, and obtain sign-off only when its exit criteria are met.
