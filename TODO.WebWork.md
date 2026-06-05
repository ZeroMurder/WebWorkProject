# WebWork — план исправлений по терминальным ошибкам

- [ ] Исправить зависимости EF Core/Identity в `WebWork/WebWork.csproj` (добавить нужные PackageReference).
- [ ] Добавить enum `EmploymentType` в `WebWork/Enums/EmploymentType.cs` (или исправить использование в `Executor.cs`, если имя/смысл другой).
- [ ] Найти причину дубля `ProjectCalculationService` (второе объявление класса/метода в проекте) и удалить/объединить.
- [ ] Повторно запустить `dotnet build` и убедиться, что компиляция проходит.
- [ ] (После сборки) при необходимости запустить `dotnet run` и проверить инициализацию миграций/seed.

