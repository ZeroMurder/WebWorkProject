Диагностика Create(Project model) в ProjectsController

На время отладки в ProjectsController.Create добавлены:
- чтение postedTitle/postedStart/postedEnd/postedCustomerId/postedTaxRate/postedStatus из Request.Form
- подстановка model.Title из postedTitle, если binder не заполнил
- проверка model.Title / StartDate / EndDate и возврат View(model) при ошибках
- временный редирект после успешного Create на Edit(id), чтобы проверить загрузку сохранённого проекта

После завершения отладки нужно удалить временные блоки ViewBag.Debug и редирект на Edit.

