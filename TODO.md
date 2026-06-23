# TODO (WebWorkNew)

## Этап 4: API + ViewModels/UI динамика
- [x] 1) Создать папку `Controllers/Api/`
- [x] 2) Добавить `ResourceApiController.cs` (employees/executors/subcontractors/equipment + by-type)
- [x] 3) Добавить `ProjectApiController.cs` (update-margin + summary)
- [x] 4) Создать `wwwroot/js/resource-api.js` клиент для загрузки и обновления
- [x] 5) Подключить `resource-api.js` в `Views/Shared/_Layout.cshtml` (или точечно в `Views/Projects/Resources.cshtml`)
- [x] 6) Обновить `Views/Projects/Resources.cshtml`: добавить onchange для маржи через API (частично: без замены ввода ID на select)
- [ ] 7) Прогнать сборку/запуск и проверить end-to-end (селекты + пересчет + обновление FinalCost/итогов)

## Исправление: Create проекта возвращает пустые данные / несинхронизация
- [x] 8) В `Controllers/ProjectsController.cs` добавить диагностику входных данных в POST Create (Title/StartDate/EndDate/CustomerId/TaxRate/Status)
- [x] 9) В `Controllers/ProjectsController.cs` сделать серверную валидацию и при необходимости подставлять Title из `Request.Form`
- [x] 10) Временно редиректить после POST Create на `Edit` вместо `Index` для проверки сохранения и загрузки данных
- [ ] 11) После подтверждения убрать временные диагностики/редирект и вернуть нормальный редирект на `Index`


