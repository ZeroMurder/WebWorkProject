$outputFile = "project_export.txt"
$rootPath = "."

if (Test-Path $outputFile) { Remove-Item $outputFile -Force }

function Write-FileContent {
    param($filePath, $relativePath)
    Add-Content -Path $outputFile -Value ("`n" + "="*80)
    Add-Content -Path $outputFile -Value "FILE: $relativePath"
    Add-Content -Path $outputFile -Value ("="*80 + "`n")
    try {
        $content = Get-Content $filePath -Raw -ErrorAction Stop
        Add-Content -Path $outputFile -Value $content
    } catch {
        Add-Content -Path $outputFile -Value "ERROR: Cannot read file"
    }
}

Add-Content -Path $outputFile -Value "="*80
Add-Content -Path $outputFile -Value "PROJECT EXPORT: WebWorkNew"
Add-Content -Path $outputFile -Value ("Date: " + (Get-Date -Format "yyyy-MM-dd HH:mm:ss"))
Add-Content -Path $outputFile -Value "="*80

# Models
Add-Content -Path $outputFile -Value "`n" + "-"*80
Add-Content -Path $outputFile -Value "MODELS"
Add-Content -Path $outputFile -Value "-"*80
$models = Get-ChildItem -Path "$rootPath\Models" -Filter "*.cs" | Sort-Object Name
foreach ($m in $models) {
    Write-FileContent $m.FullName "Models\$($m.Name)"
}

# Controllers
Add-Content -Path $outputFile -Value "`n" + "-"*80
Add-Content -Path $outputFile -Value "CONTROLLERS"
Add-Content -Path $outputFile -Value "-"*80
$controllers = Get-ChildItem -Path "$rootPath\Controllers" -Filter "*.cs" | Sort-Object Name
foreach ($c in $controllers) {
    Write-FileContent $c.FullName "Controllers\$($c.Name)"
}

# MD-Files
Add-Content -Path $outputFile -Value "`n" + "-"*80
Add-Content -Path $outputFile -Value "MDFILES"
Add-Content -Path $outputFile -Value "-"*80
$mdi = Get-ChildItem -Path "$rootPath" -Filter "*.md" | Sort-Object Name
foreach ($c in $mdi) {
    Write-FileContent $c.FullName "$($c.Name)"
}

# Services
Add-Content -Path $outputFile -Value "`n" + "-"*80
Add-Content -Path $outputFile -Value "SERVICES"
Add-Content -Path $outputFile -Value "-"*80
$services = Get-ChildItem -Path "$rootPath\Services" -Filter "*.cs" | Sort-Object Name
foreach ($s in $services) {
    Write-FileContent $s.FullName "Services\$($s.Name)"
}

# Data
Add-Content -Path $outputFile -Value "`n" + "-"*80
Add-Content -Path $outputFile -Value "DATA"
Add-Content -Path $outputFile -Value "-"*80
$dataFiles = Get-ChildItem -Path "$rootPath\Data" -Filter "*.cs" | Sort-Object Name
foreach ($d in $dataFiles) {
    Write-FileContent $d.FullName "Data\$($d.Name)"
}

# Enums
Add-Content -Path $outputFile -Value "`n" + "-"*80
Add-Content -Path $outputFile -Value "ENUMS"
Add-Content -Path $outputFile -Value "-"*80
$enums = Get-ChildItem -Path "$rootPath\Enums" -Filter "*.cs" | Sort-Object Name
foreach ($e in $enums) {
    Write-FileContent $e.FullName "Enums\$($e.Name)"
}

# Program.cs
if (Test-Path "$rootPath\Program.cs") {
    Write-FileContent "$rootPath\Program.cs" "Program.cs"
}

# ALL VIEWS - получаем все папки из списка Views
Add-Content -Path $outputFile -Value "`n" + "-"*80
Add-Content -Path $outputFile -Value "ALL VIEWS"
Add-Content -Path $outputFile -Value "-"*80

# Список всех папок Views из вашего списка
$allViewFolders = @(
    "Account",
    "Adminuser",
    "AdminUsers", 
    "Customers",
    "Employees",
    "Equipments",
    "Executors",
    "Profiles",
    "Projects",
    "Register",
    "Settings",
    "Shared",
    "Subcontractors",
    "TechnicalTasks",
    "Workspaces"
)

foreach ($folder in $allViewFolders) {
    $path = Join-Path $rootPath "Views" $folder
    if (Test-Path $path) {
        $files = Get-ChildItem -Path $path -Filter "*.cshtml" | Sort-Object Name
        foreach ($f in $files) {
            Write-FileContent $f.FullName "Views\$folder\$($f.Name)"
        }
    } else {
        Add-Content -Path $outputFile -Value "`nWARNING: Folder not found - Views\$folder"
    }
}

Add-Content -Path $outputFile -Value "`n" + "="*80
Add-Content -Path $outputFile -Value "EXPORT COMPLETE"
Add-Content -Path $outputFile -Value "="*80

Write-Host "Done! File: $outputFile"