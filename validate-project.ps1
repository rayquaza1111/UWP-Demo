#!/usr/bin/env pwsh
# UWP Demo Application - Project Structure Validation Script
# Run this script to verify that all required files are present

Write-Host "UWP Demo Application - Project Structure Validation" -ForegroundColor Cyan
Write-Host "====================================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "Checking project structure..." -ForegroundColor Yellow
Write-Host ""

$errors = 0

# Check for main project files
$files = @(
    "UWPDemo.csproj",
    "Package.appxmanifest", 
    "App.xaml",
    "App.xaml.cs",
    "MainWindow.xaml",
    "MainWindow.xaml.cs"
)

foreach ($file in $files) {
    if (Test-Path $file) {
        Write-Host "[OK] $file found" -ForegroundColor Green
    } else {
        Write-Host "[ERROR] $file not found" -ForegroundColor Red
        $errors++
    }
}

Write-Host ""
Write-Host "Checking Views folder..." -ForegroundColor Yellow

$viewFiles = @(
    "Views\MainPage.xaml",
    "Views\MainPage.xaml.cs",
    "Views\ControlsDemoPage.xaml", 
    "Views\ControlsDemoPage.xaml.cs",
    "Views\DataBindingDemoPage.xaml",
    "Views\DataBindingDemoPage.xaml.cs",
    "Views\SettingsPage.xaml",
    "Views\SettingsPage.xaml.cs"
)

foreach ($file in $viewFiles) {
    if (Test-Path $file) {
        Write-Host "[OK] $file found" -ForegroundColor Green
    } else {
        Write-Host "[ERROR] $file not found" -ForegroundColor Red
        $errors++
    }
}

Write-Host ""
Write-Host "Checking ViewModels folder..." -ForegroundColor Yellow

$viewModelFiles = @(
    "ViewModels\BaseViewModel.cs",
    "ViewModels\ControlsDemoViewModel.cs",
    "ViewModels\DataBindingDemoViewModel.cs", 
    "ViewModels\SettingsViewModel.cs"
)

foreach ($file in $viewModelFiles) {
    if (Test-Path $file) {
        Write-Host "[OK] $file found" -ForegroundColor Green
    } else {
        Write-Host "[ERROR] $file not found" -ForegroundColor Red
        $errors++
    }
}

Write-Host ""
Write-Host "Checking Models folder..." -ForegroundColor Yellow

$modelFiles = @(
    "Models\Person.cs"
)

foreach ($file in $modelFiles) {
    if (Test-Path $file) {
        Write-Host "[OK] $file found" -ForegroundColor Green
    } else {
        Write-Host "[ERROR] $file not found" -ForegroundColor Red
        $errors++
    }
}

Write-Host ""
Write-Host "Checking Assets folder..." -ForegroundColor Yellow

if (Test-Path "Assets") {
    Write-Host "[OK] Assets folder found" -ForegroundColor Green
} else {
    Write-Host "[ERROR] Assets folder not found" -ForegroundColor Red
    $errors++
}

Write-Host ""
Write-Host "====================================================" -ForegroundColor Cyan

if ($errors -eq 0) {
    Write-Host "[SUCCESS] All project files are present!" -ForegroundColor Green
    Write-Host "You can now open the project in Visual Studio 2022." -ForegroundColor White
    Write-Host ""
    Write-Host "To build and run:" -ForegroundColor Yellow
    Write-Host "1. Open UWPDemo.csproj in Visual Studio 2022" -ForegroundColor White
    Write-Host "2. Ensure you have the UWP development workload installed" -ForegroundColor White
    Write-Host "3. Set the target platform (x64, x86, or ARM64)" -ForegroundColor White
    Write-Host "4. Press F5 to build and run" -ForegroundColor White
} else {
    Write-Host "[FAILED] $errors errors found in project structure!" -ForegroundColor Red
    Write-Host "Please check that all files are present and try again." -ForegroundColor White
}

Write-Host ""
Write-Host "Press any key to continue..." -ForegroundColor DarkGray
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")