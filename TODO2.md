# TODO2 (Project Create/Index correctness)

- [ ] Read Program.cs to verify seeding/initialization paths that may create an extra Project.
- [ ] Read full Controllers/ProjectsController.cs to confirm no other Project creation path.
- [ ] Identify code path that creates Projects row with default values (Id=2, Title empty, dates 0001-01-01).
- [ ] Fix root cause (seed or initialization) so no empty Project is inserted.
- [ ] Clean DB and verify: Projects/Index no longer shows 01.01.0001 - 01.01.0001.
- [ ] Re-test POST /Projects/Create (400 resolved) and verify saved StartDate/EndDate in UI.

