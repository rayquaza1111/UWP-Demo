using Windows.UI.Xaml.Controls;
using UWP_Demo.Views;
// Navigation System: Import WinUI 2 controls namespace
using Microsoft.UI.Xaml.Controls;  // Navigation System: Provides NavigationView and other modern controls
using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;  // Navigation System: For NavigationEventArgs
using System;
using System.Collections.Generic;
using UWP_Demo.Models;  // Navigation System: Import Customer model for navigation methods

namespace UWP_Demo
{
    /// <summary>
    /// Navigation System: MainPage with comprehensive multi-page NavigationView
    /// Navigation System: Features: Advanced navigation, breadcrumb, search, responsive design
    /// Navigation System: Pages: Home, Customers, Edit, Settings, File Operations, Reports, Help, About
    /// </summary>
    public sealed partial class MainPage : Page
    {
        // Navigation System: Enhanced properties for navigation management
        private string _currentPageTitle = "Customer Management";
        public string CurrentPageTitle 
        { 
            get => _currentPageTitle;
            set
            {
                _currentPageTitle = value;
                Bindings.Update();
                UpdateBreadcrumb(); // Navigation System: Update breadcrumb when title changes
            }
        }

        // 6. Suspension & Resume Handling: Properties to track app launch state - COMMENTED OUT
        // public bool WasResumedFromSuspension { get; private set; } = false;
        // public string LaunchInformation { get; private set; } = "";

        // Page type mapping for navigation
        private readonly Dictionary<string, Type> _pageTypeMap = new Dictionary<string, Type>
        {
            { "home", typeof(HomePage) },
            { "customers", typeof(CustomersPage) }, // Navigation System: Dedicated customer page
            { "edit", typeof(EditPage) },
            // { "network", typeof(NetworkPage) }, // Network functionality disabled
            { "fileops", typeof(FileOperationsPage) }, // Navigation System: Dedicated file operations page
            { "reports", typeof(ReportsPage) },
            { "mobile", typeof(MobilePage) },
            { "help", typeof(HelpPage) },
            { "about", typeof(AboutPage) }
        };

        // Page title mapping
        private readonly Dictionary<string, string> _pageTitleMap = new Dictionary<string, string>
        {
            { "home", "Home Dashboard" },
            { "customers", "Customer Management" },
            { "edit", "Customer Editor" },
            // { "network", "Network API Demo" }, // Network functionality disabled
            { "fileops", "File Operations" },
            { "reports", "Analytics & Reports" },
            { "mobile", "Mobile Synchronization" },
            { "help", "Help & Support" },
            { "about", "About Customer Manager" }
        };

        // Breadcrumb mapping
        private readonly Dictionary<string, string> _breadcrumbMap = new Dictionary<string, string>
        {
            { "home", "Home" },
            { "customers", "Home > Customers" },
            { "edit", "Home > Customers > Edit" },
            // { "network", "Home > Network API" }, // Network functionality disabled
            { "fileops", "Home > File Operations" },
            { "reports", "Home > Reports" },
            { "mobile", "Home > Mobile Sync" },
            { "help", "Home > Help" },
            { "about", "Home > About" }
        };

        public MainPage()
        {
            this.InitializeComponent();
            
            System.Diagnostics.Debug.WriteLine("Navigation System: MainPage with enhanced NavigationView initialized");
            
            // Navigation System: Navigate to HomePage when MainPage loads
            NavigateToPage("home");
            
            System.Diagnostics.Debug.WriteLine("Navigation System: Initial navigation to HomePage completed");
        }

        /// <summary>
        /// Navigation System: Handle NavigationView selection changes
        /// Navigation System: Enhanced navigation logic with comprehensive page routing
        /// Navigation System: Handles menu items, settings, and footer navigation
        /// </summary>
        private void NavigationView_SelectionChanged(Microsoft.UI.Xaml.Controls.NavigationView sender, Microsoft.UI.Xaml.Controls.NavigationViewSelectionChangedEventArgs args)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Navigation System: NavigationView selection changed");
                
                // Navigation System: Handle built-in settings navigation
                if (args.IsSettingsSelected)
                {
                    System.Diagnostics.Debug.WriteLine("Navigation System: Settings selected from NavigationView");
                    NavigateToPage("settings");
                }
                // Navigation System: Handle custom navigation menu items
                else if (args.SelectedItem is Microsoft.UI.Xaml.Controls.NavigationViewItem selectedItem)
                {
                    string tag = selectedItem.Tag?.ToString();
                    System.Diagnostics.Debug.WriteLine($"Navigation System: Navigation item selected: {tag}");
                    
                    if (!string.IsNullOrEmpty(tag))
                    {
                        NavigateToPage(tag); // Navigation System: Execute navigation to selected page
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Navigation System ERROR: Navigation selection error: {ex.Message}");
            }
        }

        /// <summary>
        /// Navigation System: Handle NavigationView back button requests
        /// Navigation System: Provides automatic back navigation functionality
        /// Navigation System: Shows when user navigates deeper into the app hierarchy
        /// </summary>
        private void NavigationView_BackRequested(Microsoft.UI.Xaml.Controls.NavigationView sender, Microsoft.UI.Xaml.Controls.NavigationViewBackRequestedEventArgs args)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Navigation System: Back button requested");
                
                // Navigation System: Navigate back if possible using Frame's navigation stack
                if (ContentFrame.CanGoBack)
                {
                    ContentFrame.GoBack();
                    UpdateNavigationFromFrame(); // Navigation System: Update UI after navigation
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Navigation System ERROR: Back navigation error: {ex.Message}");
            }
        }

        /// <summary>
        /// Navigation System: Handle frame navigation events
        /// Navigation System: Updates NavigationView state based on frame navigation
        /// </summary>
        private void ContentFrame_Navigated(object sender, NavigationEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"Navigation System: Frame navigated to {e.SourcePageType?.Name}");
                UpdateNavigationFromFrame(); // Navigation System: Sync NavigationView with frame state
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Navigation System ERROR: Frame navigation event error: {ex.Message}");
            }
        }

        /// <summary>
        /// Navigation System: Core navigation method
        /// Navigation System: Centralized page navigation with title and breadcrumb updates
        /// </summary>
        private void NavigateToPage(string pageTag)
        {
            try
            {
                Type pageType = null;
                
                // Navigation System: Special handling for settings
                if (pageTag == "settings")
                {
                    pageType = typeof(SettingsPage);
                    CurrentPageTitle = "Settings";
                    BreadcrumbText.Text = "Home > Settings";
                }
                // Navigation System: Handle mapped pages
                else if (_pageTypeMap.ContainsKey(pageTag))
                {
                    pageType = _pageTypeMap[pageTag];
                    CurrentPageTitle = _pageTitleMap.ContainsKey(pageTag) ? _pageTitleMap[pageTag] : "Customer Management";
                }

                // Navigation System: Navigate if page type found
                if (pageType != null)
                {
                    ContentFrame.Navigate(pageType);
                    System.Diagnostics.Debug.WriteLine($"Navigation System: Successfully navigated to {pageType.Name}");
                }
                else
                {
                    // Navigation System: Show placeholder for unimplemented pages
                    ShowPagePlaceholder(pageTag);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Navigation System ERROR: Failed to navigate to {pageTag} - {ex.Message}");
            }
        }

        /// <summary>
        /// Navigation System: Show placeholder for unimplemented pages
        /// Navigation System: Provides feedback for pages that don't exist yet
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
                
                // Navigation System: Navigate back to home
                NavigateToPage("home");
                HomeNavItem.IsSelected = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Navigation System ERROR: Failed to show placeholder - {ex.Message}");
            }
        }

        /// <summary>
        /// Handle navigation view item selection
        /// </summary>
        private void MainNavigationView_SelectionChanged(Microsoft.UI.Xaml.Controls.NavigationView sender, Microsoft.UI.Xaml.Controls.NavigationViewSelectionChangedEventArgs args)
        {
            try
            {
                if (args.SelectedItem is Microsoft.UI.Xaml.Controls.NavigationViewItem item)
                {
                    string tag = item.Tag?.ToString();
                    if (!string.IsNullOrEmpty(tag) && _pageTypeMap.ContainsKey(tag))
                    {
                        ContentFrame.Navigate(_pageTypeMap[tag]);
                        UpdatePageTitle(tag);
                        UpdateBreadcrumb();
                    }
                }
                else if (args.SelectedItem == sender.SettingsItem)
                {
                    ContentFrame.Navigate(typeof(SettingsPage));
                    UpdatePageTitle("settings");
                    UpdateBreadcrumb();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Navigation error: {ex.Message}");
            }
        }

        /// <summary>
        /// Handle back requested navigation
        /// </summary>
        private void MainNavigationView_BackRequested(Microsoft.UI.Xaml.Controls.NavigationView sender, Microsoft.UI.Xaml.Controls.NavigationViewBackRequestedEventArgs args)
        {
            if (ContentFrame.CanGoBack)
            {
                ContentFrame.GoBack();
                UpdateNavigationFromFrame();
            }
        }

        /// <summary>
        /// Update page title based on current selection
        /// </summary>
        private void UpdatePageTitle(string tag)
        {
            if (_pageTitleMap.ContainsKey(tag))
            {
                CurrentPageTitle = _pageTitleMap[tag];
            }
            else if (tag == "settings")
            {
                CurrentPageTitle = "Settings";
            }
            else
            {
                CurrentPageTitle = "Customer Manager";
            }
        }

        /// <summary>
        /// Update navigation selection based on current frame content
        /// </summary>
        private void UpdateNavigationFromFrame()
        {
            try
            {
                if (ContentFrame.Content != null)
                {
                    var currentPageType = ContentFrame.Content.GetType();
                    
                    foreach (var kvp in _pageTypeMap)
                    {
                        if (kvp.Value == currentPageType)
                        {
                            var navItem = FindNavigationItemByTag(kvp.Key);
                            if (navItem != null)
                            {
                                MainNavigationView.SelectedItem = navItem;
                                UpdatePageTitle(kvp.Key);
                                UpdateBreadcrumb();
                                return;
                            }
                        }
                    }
                    
                    if (currentPageType == typeof(SettingsPage))
                    {
                        MainNavigationView.SelectedItem = MainNavigationView.SettingsItem;
                        UpdatePageTitle("settings");
                        UpdateBreadcrumb();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to update navigation from frame - {ex.Message}");
            }
        }

        /// <summary>
        /// Find NavigationViewItem by tag, searching both menu items and footer items
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
        /// Update breadcrumb navigation showing current path
        /// </summary>
        private void UpdateBreadcrumb()
        {
            try
            {
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
                System.Diagnostics.Debug.WriteLine($"Failed to update breadcrumb - {ex.Message}");
            }
        }

        #region Header Action Handlers

        /// <summary>
        /// Handle search button click
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
                System.Diagnostics.Debug.WriteLine($"Search error - {ex.Message}");
            }
        }

        /// <summary>
        /// Handle notifications button click
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
                System.Diagnostics.Debug.WriteLine($"Notifications error - {ex.Message}");
            }
        }

        #endregion

        #region Public Navigation Methods

        /// <summary>
        /// Navigation System: Public method for external navigation to settings
        /// Navigation System: Maintains navigation consistency throughout the app
        /// </summary>
        public void NavigateToSettings()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Navigation System: External NavigateToSettings called");
                NavigateToPage("settings");
                MainNavigationView.SelectedItem = MainNavigationView.SettingsItem;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Navigation System ERROR: NavigateToSettings error: {ex.Message}");
            }
        }

        /// <summary>
        /// Navigation System: Public method for navigating to home
        /// Navigation System: Provides programmatic navigation with UI consistency
        /// </summary>
        public void NavigateToHome()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Navigation System: External NavigateToHome called");
                NavigateToPage("home");
                HomeNavItem.IsSelected = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Navigation System ERROR: NavigateToHome error: {ex.Message}");
            }
        }

        /// <summary>
        /// Navigation System: Public method for navigating to edit page
        /// Navigation System: Supports state management integration
        /// </summary>
        public void NavigateToEdit(object parameter = null)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Navigation System: External NavigateToEdit called");
                
                if (parameter != null)
                {
                    ContentFrame.Navigate(typeof(EditPage), parameter); // Navigation System: Navigate with parameter
                }
                else
                {
                    NavigateToPage("edit"); // Navigation System: Navigate without parameter
                }
                
                EditNavItem.IsSelected = true;
                CurrentPageTitle = "Customer Editor";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Navigation System ERROR: NavigateToEdit error: {ex.Message}");
            }
        }

        /// <summary>
        /// Navigation System: Public method for navigating to edit page with customer
        /// Navigation System: Enhanced: Direct navigation through NavigationView with customer parameter
        /// Navigation System: Called from ViewModels and other pages for customer editing
        /// </summary>
        public void NavigateToEditWithCustomer(Customer customer)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"Navigation System: NavigateToEditWithCustomer called for {customer?.FullName}");
                
                if (customer != null)
                {
                    // Navigation System: Store customer in state service
                    var stateService = UWP_Demo.Services.NavigationStateService.Instance;
                    stateService.SetSelectedCustomerForEdit(customer);
                    
                    // Navigation System: Navigate to EditPage with customer parameter
                    ContentFrame.Navigate(typeof(EditPage), customer);
                    
                    // Navigation System: Update NavigationView selection
                    EditNavItem.IsSelected = true;
                    CurrentPageTitle = $"Edit {customer.FullName}";
                    
                    System.Diagnostics.Debug.WriteLine("Navigation System: Successfully navigated to EditPage with customer");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Navigation System ERROR: NavigateToEditWithCustomer error: {ex.Message}");
            }
        }

        /// <summary>
        /// Navigation System: Public method for navigating to new customer page
        /// Navigation System: Enhanced: Direct navigation through NavigationView for new customer
        /// Navigation System: Called from ViewModels and other pages for customer creation
        /// </summary>
        public void NavigateToNewCustomer()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Navigation System: NavigateToNewCustomer called");
                
                // Navigation System: Clear any existing state
                var stateService = UWP_Demo.Services.NavigationStateService.Instance;
                stateService.ClearAllState();
                
                // Navigation System: Navigate to EditPage without customer (new customer mode)
                ContentFrame.Navigate(typeof(EditPage), null);
                
                // Navigation System: Update NavigationView selection
                EditNavItem.IsSelected = true;
                CurrentPageTitle = "New Customer";
                
                System.Diagnostics.Debug.WriteLine("Navigation System: Successfully navigated to new customer EditPage");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Navigation System ERROR: NavigateToNewCustomer error: {ex.Message}");
            }
        }

        #endregion
    }
}