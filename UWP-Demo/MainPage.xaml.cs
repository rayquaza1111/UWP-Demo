using Windows.UI.Xaml.Controls;
using UWP_Demo.Views;
// 🎨 WINUI 2 UI ENHANCEMENT: Import WinUI 2 controls namespace
using Microsoft.UI.Xaml.Controls;  // 🎯 WINUI 2: Provides NavigationView and other modern controls
using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;  // 🔄 SUSPENSION & RESUME: For NavigationEventArgs
using System;
using System.Collections.Generic;
using UWP_Demo.Models;  // 🚀 NAVIGATION: Import Customer model for navigation methods

namespace UWP_Demo
{
    /// <summary>
    /// 🚀 NAVIGATION SYSTEM: MainPage with comprehensive multi-page NavigationView
    /// 📱 Features: Advanced navigation, breadcrumb, search, responsive design
    /// 🎯 Pages: Home, Customers, Edit, Settings, File Operations, Reports, Help, About
    /// </summary>
    public sealed partial class MainPage : Page
    {
        // 🚀 NAVIGATION: Enhanced properties for navigation management
        private string _currentPageTitle = "Customer Management";
        public string CurrentPageTitle 
        { 
            get => _currentPageTitle;
            set
            {
                _currentPageTitle = value;
                Bindings.Update();
                UpdateBreadcrumb();
            }
        }

        // 🔄 SUSPENSION & RESUME: Properties to track app launch state
        public bool WasResumedFromSuspension { get; private set; } = false;
        public string LaunchInformation { get; private set; } = "";

        // 🚀 NAVIGATION: Page type mapping for navigation
        private readonly Dictionary<string, Type> _pageTypeMap = new Dictionary<string, Type>
        {
            { "home", typeof(HomePage) },
            { "customers", typeof(CustomersPage) }, // ✅ NOW: Dedicated customer page
            { "edit", typeof(EditPage) },
            { "fileops", typeof(FileOperationsPage) }, // ✅ NOW: Dedicated file operations page
            { "reports", typeof(ReportsPage) },
            { "mobile", typeof(MobilePage) },
            { "help", typeof(HelpPage) },
            { "about", typeof(AboutPage) }
        };

        // 🚀 NAVIGATION: Page title mapping
        private readonly Dictionary<string, string> _pageTitleMap = new Dictionary<string, string>
        {
            { "home", "Home Dashboard" },
            { "customers", "Customer Management" },
            { "edit", "Customer Editor" },
            { "fileops", "File Operations" },
            { "reports", "Analytics & Reports" },
            { "mobile", "Mobile Synchronization" },
            { "help", "Help & Support" },
            { "about", "About Customer Manager" }
        };

        // 🚀 NAVIGATION: Breadcrumb mapping
        private readonly Dictionary<string, string> _breadcrumbMap = new Dictionary<string, string>
        {
            { "home", "Home" },
            { "customers", "Home > Customers" },
            { "edit", "Home > Customers > Edit" },
            { "fileops", "Home > File Operations" },
            { "reports", "Home > Reports" },
            { "mobile", "Home > Mobile Sync" },
            { "help", "Home > Help" },
            { "about", "Home > About" }
        };

        public MainPage()
        {
            this.InitializeComponent();
            
            System.Diagnostics.Debug.WriteLine("🚀 NAVIGATION SYSTEM: MainPage with enhanced NavigationView initialized");
            
            // 🚀 NAVIGATION: Navigate to HomePage when MainPage loads
            NavigateToPage("home");
            
            System.Diagnostics.Debug.WriteLine("🚀 NAVIGATION SYSTEM: Initial navigation to HomePage completed");
        }

        /// <summary>
        /// 🔄 SUSPENSION & RESUME: Handle navigation with suspension state parameter
        /// 📊 Processes launch information and suspension state from App.xaml.cs
        /// 🎯 Enables welcome message display based on app lifecycle events
        /// </summary>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            try
            {
                // 🔄 SUSPENSION & RESUME: Check if we have suspension state information
                if (e.Parameter != null)
                {
                    System.Diagnostics.Debug.WriteLine($"🔄 SUSPENSION & RESUME: MainPage received parameter: {e.Parameter}");
                    
                    // 🔄 SUSPENSION & RESUME: Process launch information if available
                    var launchInfo = e.Parameter.ToString();
                    LaunchInformation = launchInfo;
                    
                    // 🔄 SUSPENSION & RESUME: Log the launch information
                    System.Diagnostics.Debug.WriteLine($"🔄 SUSPENSION & RESUME: Launch information: {LaunchInformation}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"🔄 SUSPENSION & RESUME ERROR: Failed to process navigation parameter - {ex.Message}");
            }
        }

        /// <summary>
        /// 🚀 NAVIGATION: Handle NavigationView selection changes
        /// 🎯 Enhanced navigation logic with comprehensive page routing
        /// 📱 Handles menu items, settings, and footer navigation
        /// </summary>
        private void NavigationView_SelectionChanged(Microsoft.UI.Xaml.Controls.NavigationView sender, Microsoft.UI.Xaml.Controls.NavigationViewSelectionChangedEventArgs args)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("🚀 NAVIGATION: NavigationView selection changed");
                
                // 🚀 NAVIGATION: Handle built-in settings navigation
                if (args.IsSettingsSelected)
                {
                    System.Diagnostics.Debug.WriteLine("🚀 NAVIGATION: Settings selected from NavigationView");
                    NavigateToPage("settings");
                }
                // 🚀 NAVIGATION: Handle custom navigation menu items
                else if (args.SelectedItem is Microsoft.UI.Xaml.Controls.NavigationViewItem selectedItem)
                {
                    string tag = selectedItem.Tag?.ToString();
                    System.Diagnostics.Debug.WriteLine($"🚀 NAVIGATION: Navigation item selected: {tag}");
                    
                    if (!string.IsNullOrEmpty(tag))
                    {
                        NavigateToPage(tag);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"🚀 NAVIGATION ERROR: Navigation selection error: {ex.Message}");
            }
        }

        /// <summary>
        /// 🚀 NAVIGATION: Handle NavigationView back button requests
        /// 🔄 Provides automatic back navigation functionality
        /// 📱 Shows when user navigates deeper into the app hierarchy
        /// </summary>
        private void NavigationView_BackRequested(Microsoft.UI.Xaml.Controls.NavigationView sender, Microsoft.UI.Xaml.Controls.NavigationViewBackRequestedEventArgs args)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("🚀 NAVIGATION: Back button requested");
                
                // 🚀 NAVIGATION: Navigate back if possible using Frame's navigation stack
                if (ContentFrame.CanGoBack)
                {
                    ContentFrame.GoBack();
                    UpdateNavigationFromFrame();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"🚀 NAVIGATION ERROR: Back navigation error: {ex.Message}");
            }
        }

        /// <summary>
        /// 🚀 NAVIGATION: Handle frame navigation events
        /// 📍 Updates NavigationView state based on frame navigation
        /// </summary>
        private void ContentFrame_Navigated(object sender, NavigationEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"🚀 NAVIGATION: Frame navigated to {e.SourcePageType?.Name}");
                UpdateNavigationFromFrame();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"🚀 NAVIGATION ERROR: Frame navigation event error: {ex.Message}");
            }
        }

        /// <summary>
        /// 🚀 NAVIGATION: Core navigation method
        /// 🎯 Centralized page navigation with title and breadcrumb updates
        /// </summary>
        private void NavigateToPage(string pageTag)
        {
            try
            {
                Type pageType = null;
                
                // 🚀 NAVIGATION: Special handling for settings
                if (pageTag == "settings")
                {
                    pageType = typeof(SettingsPage);
                    CurrentPageTitle = "Settings";
                    BreadcrumbText.Text = "Home > Settings";
                }
                // 🚀 NAVIGATION: Handle mapped pages
                else if (_pageTypeMap.ContainsKey(pageTag))
                {
                    pageType = _pageTypeMap[pageTag];
                    CurrentPageTitle = _pageTitleMap.ContainsKey(pageTag) ? _pageTitleMap[pageTag] : "Customer Management";
                }

                // 🚀 NAVIGATION: Navigate if page type found
                if (pageType != null)
                {
                    ContentFrame.Navigate(pageType);
                    System.Diagnostics.Debug.WriteLine($"🚀 NAVIGATION: Successfully navigated to {pageType.Name}");
                }
                else
                {
                    // 🚀 NAVIGATION: Show placeholder for unimplemented pages
                    ShowPagePlaceholder(pageTag);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"🚀 NAVIGATION ERROR: Failed to navigate to {pageTag} - {ex.Message}");
            }
        }

        /// <summary>
        /// 🚀 NAVIGATION: Show placeholder for unimplemented pages
        /// 🎯 Provides feedback for pages that don't exist yet
        /// </summary>
        private async void ShowPagePlaceholder(string pageTag)
        {
            try
            {
                var dialog = new ContentDialog
                {
                    Title = "Page Coming Soon",
                    Content = $"The {_pageTitleMap.GetValueOrDefault(pageTag, pageTag)} page is not implemented yet.\n\nThis would be a great place to add more features to your app!",
                    CloseButtonText = "OK",
                    DefaultButton = ContentDialogButton.Close
                };

                await dialog.ShowAsync();
                
                // Navigate back to home
                NavigateToPage("home");
                HomeNavItem.IsSelected = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"🚀 NAVIGATION ERROR: Failed to show placeholder - {ex.Message}");
            }
        }

        /// <summary>
        /// 🚀 NAVIGATION: Update NavigationView selection based on current frame content
        /// 🎯 Keeps NavigationView synchronized with actual page state
        /// </summary>
        private void UpdateNavigationFromFrame()
        {
            try
            {
                var currentPageType = ContentFrame.CurrentSourcePageType;
                if (currentPageType == null) return;

                // 🚀 NAVIGATION: Find corresponding navigation item
                string pageTag = null;
                
                if (currentPageType == typeof(SettingsPage))
                {
                    // Handle settings specially
                    MainNavigationView.SelectedItem = MainNavigationView.SettingsItem;
                    CurrentPageTitle = "Settings";
                    BreadcrumbText.Text = "Home > Settings";
                    return;
                }
                else
                {
                    // Find by page type
                    foreach (var kvp in _pageTypeMap)
                    {
                        if (kvp.Value == currentPageType)
                        {
                            pageTag = kvp.Key;
                            break;
                        }
                    }
                }

                // 🚀 NAVIGATION: Update selected item
                if (!string.IsNullOrEmpty(pageTag))
                {
                    var navItem = FindNavigationItemByTag(pageTag);
                    if (navItem != null)
                    {
                        MainNavigationView.SelectedItem = navItem;
                        CurrentPageTitle = _pageTitleMap.GetValueOrDefault(pageTag, "Customer Management");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"🚀 NAVIGATION ERROR: Failed to update navigation from frame - {ex.Message}");
            }
        }

        /// <summary>
        /// 🚀 NAVIGATION: Find NavigationViewItem by tag
        /// 🔍 Searches both menu items and footer items
        /// </summary>
        private Microsoft.UI.Xaml.Controls.NavigationViewItem FindNavigationItemByTag(string tag)
        {
            // Search menu items
            foreach (var item in MainNavigationView.MenuItems)
            {
                if (item is Microsoft.UI.Xaml.Controls.NavigationViewItem navItem && navItem.Tag?.ToString() == tag)
                    return navItem;
            }

            // Search footer items
            foreach (var item in MainNavigationView.FooterMenuItems)
            {
                if (item is Microsoft.UI.Xaml.Controls.NavigationViewItem navItem && navItem.Tag?.ToString() == tag)
                    return navItem;
            }

            return null;
        }

        /// <summary>
        /// 🚀 NAVIGATION: Update breadcrumb navigation
        /// 📍 Shows current navigation path to user
        /// </summary>
        private void UpdateBreadcrumb()
        {
            try
            {
                // Find current page tag
                string currentTag = null;
                var selectedItem = MainNavigationView.SelectedItem as Microsoft.UI.Xaml.Controls.NavigationViewItem;
                
                if (selectedItem != null)
                {
                    currentTag = selectedItem.Tag?.ToString();
                }
                else if (MainNavigationView.SelectedItem == MainNavigationView.SettingsItem)
                {
                    currentTag = "settings";
                }

                // Update breadcrumb
                if (!string.IsNullOrEmpty(currentTag) && _breadcrumbMap.ContainsKey(currentTag))
                {
                    BreadcrumbText.Text = _breadcrumbMap[currentTag];
                }
                else
                {
                    BreadcrumbText.Text = "Home";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"🚀 NAVIGATION ERROR: Failed to update breadcrumb - {ex.Message}");
            }
        }

        #region Header Action Handlers

        /// <summary>
        /// 🚀 NAVIGATION: Handle search button click
        /// 🔍 Opens search functionality
        /// </summary>
        private async void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new ContentDialog
                {
                    Title = "Customer Search",
                    Content = "Search functionality coming soon!\n\nThis would allow you to quickly find customers by name, email, or company.",
                    CloseButtonText = "OK",
                    DefaultButton = ContentDialogButton.Close
                };

                await dialog.ShowAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"🚀 NAVIGATION ERROR: Search error - {ex.Message}");
            }
        }

        /// <summary>
        /// 🚀 NAVIGATION: Handle notifications button click
        /// 📢 Shows notifications panel
        /// </summary>
        private async void NotificationsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new ContentDialog
                {
                    Title = "Notifications",
                    Content = "No new notifications.\n\nThis would show important updates about your customer data, system status, and feature announcements.",
                    CloseButtonText = "OK",
                    DefaultButton = ContentDialogButton.Close
                };

                await dialog.ShowAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"🚀 NAVIGATION ERROR: Notifications error - {ex.Message}");
            }
        }

        #endregion

        #region Public Navigation Methods

        /// <summary>
        /// 🚀 NAVIGATION: Public method for external navigation to settings
        /// 🎯 Maintains navigation consistency throughout the app
        /// </summary>
        public void NavigateToSettings()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("🚀 NAVIGATION: External NavigateToSettings called");
                NavigateToPage("settings");
                MainNavigationView.SelectedItem = MainNavigationView.SettingsItem;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"🚀 NAVIGATION ERROR: NavigateToSettings error: {ex.Message}");
            }
        }

        /// <summary>
        /// 🚀 NAVIGATION: Public method for navigating to home
        /// 🎯 Provides programmatic navigation with UI consistency
        /// </summary>
        public void NavigateToHome()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("🚀 NAVIGATION: External NavigateToHome called");
                NavigateToPage("home");
                HomeNavItem.IsSelected = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"🚀 NAVIGATION ERROR: NavigateToHome error: {ex.Message}");
            }
        }

        /// <summary>
        /// 🚀 NAVIGATION: Public method for navigating to edit page
        /// 🎯 Supports state management integration
        /// </summary>
        public void NavigateToEdit(object parameter = null)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("🚀 NAVIGATION: External NavigateToEdit called");
                
                if (parameter != null)
                {
                    ContentFrame.Navigate(typeof(EditPage), parameter);
                }
                else
                {
                    NavigateToPage("edit");
                }
                
                EditNavItem.IsSelected = true;
                CurrentPageTitle = "Customer Editor";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"🚀 NAVIGATION ERROR: NavigateToEdit error: {ex.Message}");
            }
        }

        /// <summary>
        /// 🚀 NAVIGATION: Public method for navigating to edit page with customer
        /// 🎯 Enhanced: Direct navigation through NavigationView with customer parameter
        /// 📱 Called from ViewModels and other pages for customer editing
        /// </summary>
        public void NavigateToEditWithCustomer(Customer customer)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"🚀 NAVIGATION: NavigateToEditWithCustomer called for {customer?.FullName}");
                
                if (customer != null)
                {
                    // Store customer in state service
                    var stateService = UWP_Demo.Services.NavigationStateService.Instance;
                    stateService.SetSelectedCustomerForEdit(customer);
                    
                    // Navigate to EditPage with customer parameter
                    ContentFrame.Navigate(typeof(EditPage), customer);
                    
                    // Update NavigationView selection
                    EditNavItem.IsSelected = true;
                    CurrentPageTitle = $"Edit {customer.FullName}";
                    
                    System.Diagnostics.Debug.WriteLine("🚀 NAVIGATION: Successfully navigated to EditPage with customer");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"🚀 NAVIGATION ERROR: NavigateToEditWithCustomer error: {ex.Message}");
            }
        }

        /// <summary>
        /// 🚀 NAVIGATION: Public method for navigating to new customer page
        /// 🎯 Enhanced: Direct navigation through NavigationView for new customer
        /// 📱 Called from ViewModels and other pages for customer creation
        /// </summary>
        public void NavigateToNewCustomer()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("🚀 NAVIGATION: NavigateToNewCustomer called");
                
                // Clear any existing state
                var stateService = UWP_Demo.Services.NavigationStateService.Instance;
                stateService.ClearAllState();
                
                // Navigate to EditPage without customer (new customer mode)
                ContentFrame.Navigate(typeof(EditPage), null);
                
                // Update NavigationView selection
                EditNavItem.IsSelected = true;
                CurrentPageTitle = "New Customer";
                
                System.Diagnostics.Debug.WriteLine("🚀 NAVIGATION: Successfully navigated to new customer EditPage");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"🚀 NAVIGATION ERROR: NavigateToNewCustomer error: {ex.Message}");
            }
        }

        #endregion
    }
}