# UWP Demo Application

A comprehensive Universal Windows Platform (UWP) demo application showcasing modern Windows development practices, built with .NET 6 and WinUI 3.

## Features

### 🎨 Modern UI Design
- **Fluent Design Principles**: Follows Microsoft's latest design guidelines
- **Responsive Layout**: Adapts to different screen sizes and orientations
- **Theme Support**: Light, Dark, and System theme options
- **Smooth Animations**: Fluid transitions and interactions

### 🏗️ Architecture
- **MVVM Pattern**: Model-View-ViewModel architecture for clean separation of concerns
- **Data Binding**: Comprehensive two-way and one-way data binding examples
- **Observable Collections**: Dynamic list management with real-time updates
- **Property Change Notifications**: Automatic UI updates when data changes

### 📱 Demo Sections

#### 1. **Controls Demo**
Showcases various UWP controls and their capabilities:
- Input controls (TextBox, ComboBox, Slider, DatePicker, TimePicker)
- Toggle controls (CheckBox, ToggleSwitch, RadioButton)
- Progress indicators (ProgressBar, ProgressRing)
- Button variants (Standard, Accent, Toggle, AppBar buttons)
- Media and content display

#### 2. **Data Binding Demo**
Demonstrates data binding patterns:
- Two-way binding with real-time updates
- One-way binding for display-only content
- Collection binding with ObservableCollection
- Value converters for data transformation
- Property change notifications

#### 3. **Settings Page**
Complete settings implementation:
- Appearance customization (theme, colors)
- Behavior configuration (animations, confirmations)
- Data management (cache, auto-save)
- System information display
- Import/export functionality

## 🛠️ Technical Requirements

### Prerequisites
- **Windows 10** version 1809 (build 17763) or later
- **Windows 11** (recommended)
- **Visual Studio 2022** with the following workloads:
  - .NET desktop development
  - Universal Windows Platform development
- **.NET 6.0 SDK** or later
- **Windows App SDK** 1.4 or later

### Development Environment Setup

1. **Install Visual Studio 2022**
   - Download from [Visual Studio website](https://visualstudio.microsoft.com/)
   - During installation, select:
     - ".NET desktop development" workload
     - "Universal Windows Platform development" workload

2. **Install Windows App SDK**
   ```bash
   # Install via NuGet Package Manager or use the project references
   ```

3. **Enable Developer Mode**
   - Go to Settings → Update & Security → For developers
   - Select "Developer mode"

> **Note**: This UWP application can only be built and run on Windows systems with the appropriate development tools installed. Linux and macOS environments are not supported for UWP development.

## 🚀 Getting Started

### Project Validation

Before building, you can verify that all project files are present:

**Windows Command Prompt:**
```cmd
validate-project.bat
```

**PowerShell (Cross-platform):**
```powershell
.\validate-project.ps1
```

### Clone and Build

1. **Clone the repository**
   ```bash
   git clone https://github.com/rayquaza1111/UWP-Demo.git
   cd UWP-Demo
   ```

2. **Open the solution**
   ```bash
   # Open in Visual Studio
   start UWPDemo.csproj
   
   # Or use Visual Studio Code with C# extension
   code .
   ```

3. **Restore packages**
   ```bash
   dotnet restore
   ```

4. **Build the application**
   ```bash
   dotnet build
   ```

5. **Run the application**
   ```bash
   dotnet run
   ```

### Alternative: Using Visual Studio

1. Open `UWPDemo.csproj` in Visual Studio 2022
2. Ensure the target platform matches your system (x64, x86, or ARM64)
3. Press F5 to build and run the application

## 📁 Project Structure

```
UWP-Demo/
├── Assets/                     # Application assets (icons, images)
├── Models/                     # Data models
│   └── Person.cs              # Sample person model with INotifyPropertyChanged
├── ViewModels/                 # MVVM ViewModels
│   ├── BaseViewModel.cs       # Base class with INotifyPropertyChanged implementation
│   ├── ControlsDemoViewModel.cs
│   ├── DataBindingDemoViewModel.cs
│   └── SettingsViewModel.cs
├── Views/                      # XAML pages
│   ├── MainPage.xaml          # Main navigation page
│   ├── ControlsDemoPage.xaml  # Controls demonstration
│   ├── DataBindingDemoPage.xaml # Data binding examples
│   └── SettingsPage.xaml     # Settings and configuration
├── App.xaml                   # Application resources and styling
├── App.xaml.cs               # Application entry point
├── MainWindow.xaml           # Main application window
├── MainWindow.xaml.cs        # Main window code-behind
├── Package.appxmanifest      # Application manifest
├── UWPDemo.csproj           # Project file
└── README.md                # This file
```

## 🎯 Key Learning Points

### MVVM Implementation
- **BaseViewModel**: Implements INotifyPropertyChanged with helper methods
- **Property Binding**: Two-way and one-way binding examples
- **Command Pattern**: Button actions through ViewModel methods
- **Data Validation**: Input validation and error handling

### UWP Controls Usage
- **Input Controls**: Proper handling of user input with validation
- **Layout Controls**: Responsive design with Grid, StackPanel, and adaptive layouts
- **Navigation**: Frame-based navigation between pages
- **Styling**: Custom styles and theme resources

### Data Management
- **ObservableCollection**: Dynamic collections that update the UI automatically
- **Property Change Notifications**: Automatic UI updates when data changes
- **Value Converters**: Transform data for display purposes
- **Settings Persistence**: Save and restore application settings

## 🔧 Customization

### Adding New Pages
1. Create a new XAML page in the `Views` folder
2. Create a corresponding ViewModel in the `ViewModels` folder
3. Add navigation logic to the MainPage
4. Update the application manifest if needed

### Modifying Styles
- Edit `App.xaml` for application-wide styles
- Use Theme Resources for consistency with system themes
- Follow Fluent Design guidelines for best user experience

### Adding Controls
- Reference the official [WinUI 3 documentation](https://docs.microsoft.com/en-us/windows/winui/)
- Use the Controls Demo page as a template for new control examples

## 📚 Learning Resources

- [UWP Documentation](https://docs.microsoft.com/en-us/windows/uwp/)
- [WinUI 3 Documentation](https://docs.microsoft.com/en-us/windows/winui/)
- [MVVM Pattern Guide](https://docs.microsoft.com/en-us/windows/uwp/data-binding/data-binding-and-mvvm)
- [Fluent Design System](https://www.microsoft.com/design/fluent/)

## 🤝 Contributing

Contributions are welcome! Please feel free to submit issues and enhancement requests.

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Submit a pull request

## 📄 License

This project is provided as a learning resource and demonstration. Feel free to use it as a reference for your own UWP applications.

## 🆘 Troubleshooting

### Common Issues

**Build Errors:**
- Ensure you have the correct Windows SDK version installed
- Check that Developer Mode is enabled in Windows Settings
- Verify that all NuGet packages are restored

**Runtime Errors:**
- Check the application manifest for correct configuration
- Ensure target platform matches your system architecture
- Verify that Windows App SDK is properly installed

**Debugging:**
- Use Visual Studio's debugging tools
- Check the Output window for detailed error messages
- Enable native code debugging if needed

### Getting Help

- Check the [Issues](https://github.com/rayquaza1111/UWP-Demo/issues) page for known problems
- Search the [UWP Community Forums](https://docs.microsoft.com/en-us/answers/topics/uwp.html)
- Review the [Windows Development Documentation](https://docs.microsoft.com/en-us/windows/uwp/)

---

**Happy coding!** 🚀