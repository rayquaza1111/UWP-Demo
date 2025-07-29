@echo off
echo UWP Demo Application - Project Structure Validation
echo =====================================================
echo.

echo Checking project structure...
echo.

set "errors=0"

REM Check for main project files
if exist "UWPDemo.csproj" (
    echo [OK] UWPDemo.csproj found
) else (
    echo [ERROR] UWPDemo.csproj not found
    set /a errors+=1
)

if exist "Package.appxmanifest" (
    echo [OK] Package.appxmanifest found
) else (
    echo [ERROR] Package.appxmanifest not found
    set /a errors+=1
)

if exist "App.xaml" (
    echo [OK] App.xaml found
) else (
    echo [ERROR] App.xaml not found
    set /a errors+=1
)

if exist "MainWindow.xaml" (
    echo [OK] MainWindow.xaml found
) else (
    echo [ERROR] MainWindow.xaml not found
    set /a errors+=1
)

echo.
echo Checking Views folder...
if exist "Views\MainPage.xaml" (
    echo [OK] Views\MainPage.xaml found
) else (
    echo [ERROR] Views\MainPage.xaml not found
    set /a errors+=1
)

if exist "Views\ControlsDemoPage.xaml" (
    echo [OK] Views\ControlsDemoPage.xaml found
) else (
    echo [ERROR] Views\ControlsDemoPage.xaml not found
    set /a errors+=1
)

if exist "Views\DataBindingDemoPage.xaml" (
    echo [OK] Views\DataBindingDemoPage.xaml found
) else (
    echo [ERROR] Views\DataBindingDemoPage.xaml not found
    set /a errors+=1
)

if exist "Views\SettingsPage.xaml" (
    echo [OK] Views\SettingsPage.xaml found
) else (
    echo [ERROR] Views\SettingsPage.xaml not found
    set /a errors+=1
)

echo.
echo Checking ViewModels folder...
if exist "ViewModels\BaseViewModel.cs" (
    echo [OK] ViewModels\BaseViewModel.cs found
) else (
    echo [ERROR] ViewModels\BaseViewModel.cs not found
    set /a errors+=1
)

if exist "ViewModels\ControlsDemoViewModel.cs" (
    echo [OK] ViewModels\ControlsDemoViewModel.cs found
) else (
    echo [ERROR] ViewModels\ControlsDemoViewModel.cs not found
    set /a errors+=1
)

if exist "ViewModels\DataBindingDemoViewModel.cs" (
    echo [OK] ViewModels\DataBindingDemoViewModel.cs found
) else (
    echo [ERROR] ViewModels\DataBindingDemoViewModel.cs not found
    set /a errors+=1
)

if exist "ViewModels\SettingsViewModel.cs" (
    echo [OK] ViewModels\SettingsViewModel.cs found
) else (
    echo [ERROR] ViewModels\SettingsViewModel.cs not found
    set /a errors+=1
)

echo.
echo Checking Models folder...
if exist "Models\Person.cs" (
    echo [OK] Models\Person.cs found
) else (
    echo [ERROR] Models\Person.cs not found
    set /a errors+=1
)

echo.
echo Checking Assets folder...
if exist "Assets\" (
    echo [OK] Assets folder found
) else (
    echo [ERROR] Assets folder not found
    set /a errors+=1
)

echo.
echo =====================================================
if %errors%==0 (
    echo [SUCCESS] All project files are present!
    echo You can now open the project in Visual Studio 2022.
    echo.
    echo To build and run:
    echo 1. Open UWPDemo.csproj in Visual Studio 2022
    echo 2. Ensure you have the UWP development workload installed
    echo 3. Set the target platform (x64, x86, or ARM64)
    echo 4. Press F5 to build and run
) else (
    echo [FAILED] %errors% errors found in project structure!
    echo Please check that all files are present and try again.
)

echo.
pause