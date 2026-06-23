# check.ps1 - проверка базы данных

Write-Host "ПРОВЕРКА БАЗЫ ДАННЫХ" -ForegroundColor Cyan
Write-Host "="*50

$dbPath = "WebWorkNew.db"

if (Test-Path $dbPath) {
    Write-Host "✅ База данных найдена!" -ForegroundColor Green
    Write-Host "📊 Размер: $([math]::Round((Get-Item $dbPath).Length / 1KB, 2)) KB"
    Write-Host ""
    Write-Host "📝 ДЛЯ ПРОСМОТРА ПОЛЬЗОВАТЕЛЕЙ:" -ForegroundColor Yellow
    Write-Host "   1. Откройте браузер: http://localhost:5253/AdminUsers" -ForegroundColor White
    Write-Host "   2. Войдите: admin@local / Admin123!" -ForegroundColor White
} else {
    Write-Host "No!" -ForegroundColor Red
}