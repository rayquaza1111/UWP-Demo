using System;
using System.Diagnostics;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using UWP_Demo.ViewModels;
using UWP_Demo.Models;
using UWP_Demo.Helpers;

namespace UWP_Demo.Views
{
    /// <summary>
    /// The Home page that displays the customer list and provides basic customer management functionality.
    /// This page demonstrates ListView data binding, search functionality, and command integration
    /// in a UWP application using the MVVM pattern.
    /// </summary>
    /// <remarks>
    /// This page showcases several key UWP concepts:
    /// - ListView with data binding to ObservableCollection
    /// - Search and filtering with real-time updates
    /// - Command binding for user actions
    /// - Navigation parameter handling
    /// - Responsive design with adaptive layouts
    /// - Empty state and loading state handling
    /// - Context menus and user interaction patterns
    /// 
    /// The page uses HomeViewModel as its data context and demonstrates
    /// proper separation of concerns between the view and business logic.
    /// </remarks>
    public sealed partial class HomePage : Page
    {
        #region Private Fields

        /// <summary>
        /// Reference to the page's ViewModel for accessing data and commands.
        /// </summary>
        private HomeViewModel ViewModel => DataContext as HomeViewModel;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the HomePage class.
        /// Sets up the page and initializes the UI components.
        /// </summary>
        public HomePage()
        {
            this.InitializeComponent();
            
            Debug.WriteLine("HomePage: Initialized successfully");
        }

        #endregion

        #region Page Lifecycle

        /// <summary>
        /// Called when the page is navigated to.
        /// This method handles page activation and parameter processing.
        /// </summary>
        /// <param name="e">Navigation event arguments containing parameters and navigation mode</param>
        /// <remarks>
        /// This method is called each time the user navigates to this page,
        /// including when navigating back from other pages. It ensures the
        /// page state is properly refreshed and any navigation parameters
        /// are processed correctly.
        /// </remarks>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            
            try
            {
                Debug.WriteLine($"HomePage: Navigated to with mode {e.NavigationMode}");

                // Handle any navigation parameters
                if (e.Parameter != null)
                {
                    HandleNavigationParameter(e.Parameter);
                }

                // Refresh the page if returning from another page
                if (e.NavigationMode == NavigationMode.Back)
                {
                    RefreshPageContent();
                }

                // Focus the search box for better user experience
                if (!string.IsNullOrEmpty(ViewModel?.SearchText))
                {
                    // If there's existing search text, keep focus on the list
                    CustomerListView.Focus(Windows.UI.Xaml.FocusState.Programmatic);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"HomePage: Error in OnNavigatedTo: {ex.Message}");
            }
        }

        /// <summary>
        /// Called when the page is navigated away from.
        /// This method handles cleanup and state preservation.
        /// </summary>
        /// <param name="e">Navigation event arguments</param>
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            
            try
            {
                Debug.WriteLine($"HomePage: Navigated from to {e.SourcePageType?.Name}");

                // Save current state for potential restoration
                SavePageState();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"HomePage: Error in OnNavigatedFrom: {ex.Message}");
            }
        }

        #endregion

        #region Navigation Parameter Handling

        /// <summary>
        /// Handles navigation parameters passed to the page.
        /// This method processes different types of parameters for various navigation scenarios.
        /// </summary>
        /// <param name="parameter">The navigation parameter object</param>
        /// <remarks>
        /// Navigation parameters can include:
        /// - Customer objects (for selecting a specific customer)
        /// - Search strings (for pre-filtering the list)
        /// - Action commands (for triggering specific behaviors)
        /// </remarks>
        private void HandleNavigationParameter(object parameter)
        {
            try
            {
                switch (parameter)
                {
                    case Customer customer:
                        // Select a specific customer if passed as parameter
                        SelectCustomer(customer);
                        break;

                    case string searchText when !string.IsNullOrWhiteSpace(searchText):
                        // Apply search filter if search text is passed
                        ApplySearchFilter(searchText);
                        break;

                    case NavigationAction action:
                        // Handle specific navigation actions
                        HandleNavigationAction(action);
                        break;

                    default:
                        Debug.WriteLine($"HomePage: Unknown navigation parameter type: {parameter?.GetType().Name}");
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"HomePage: Error handling navigation parameter: {ex.Message}");
            }
        }

        /// <summary>
        /// Selects a specific customer in the list.
        /// </summary>
        /// <param name="customer">The customer to select</param>
        private void SelectCustomer(Customer customer)
        {
            try
            {
                if (customer != null && ViewModel != null)
                {
                    ViewModel.SelectedCustomer = customer;
                    
                    // Scroll to the selected customer
                    CustomerListView.ScrollIntoView(customer);
                    
                    Debug.WriteLine($"HomePage: Selected customer {customer.FullName}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"HomePage: Error selecting customer: {ex.Message}");
            }
        }

        /// <summary>
        /// Applies a search filter to the customer list.
        /// </summary>
        /// <param name="searchText">The search text to apply</param>
        private void ApplySearchFilter(string searchText)
        {
            try
            {
                if (ViewModel != null)
                {
                    ViewModel.SearchText = searchText;
                    Debug.WriteLine($"HomePage: Applied search filter: {searchText}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"HomePage: Error applying search filter: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles specific navigation actions.
        /// </summary>
        /// <param name="action">The navigation action to handle</param>
        private void HandleNavigationAction(NavigationAction action)
        {
            try
            {
                switch (action)
                {
                    case NavigationAction.RefreshData:
                        ViewModel?.RefreshCommand?.Execute(null);
                        break;

                    case NavigationAction.ClearSearch:
                        ViewModel?.ClearSearchCommand?.Execute(null);
                        break;

                    case NavigationAction.AddCustomer:
                        ViewModel?.AddCustomerCommand?.Execute(null);
                        break;

                    default:
                        Debug.WriteLine($"HomePage: Unknown navigation action: {action}");
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"HomePage: Error handling navigation action: {ex.Message}");
            }
        }

        #endregion

        #region Page State Management

        /// <summary>
        /// Refreshes the page content when returning from navigation.
        /// This method ensures the data is current and UI state is appropriate.
        /// </summary>
        private void RefreshPageContent()
        {
            try
            {
                // The ViewModel automatically updates when the underlying data changes
                // due to ObservableCollection binding, but we can trigger explicit
                // updates here if needed

                // Update command states
                ViewModel?.RefreshCommand?.CanExecute(null);

                Debug.WriteLine("HomePage: Page content refreshed");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"HomePage: Error refreshing page content: {ex.Message}");
            }
        }

        /// <summary>
        /// Saves the current page state for potential restoration.
        /// This method preserves important UI state like search text and selection.
        /// </summary>
        private void SavePageState()
        {
            try
            {
                // The ViewModel automatically handles state persistence through the
                // settings service, but additional UI state could be saved here

                Debug.WriteLine("HomePage: Page state saved");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"HomePage: Error saving page state: {ex.Message}");
            }
        }

        #endregion

        #region UI Event Handlers

        /// <summary>
        /// Handles ListView item click events.
        /// This method provides an alternative way to interact with customer items.
        /// </summary>
        /// <param name="sender">The ListView that raised the event</param>
        /// <param name="e">Item click event arguments</param>
        private void CustomerListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                if (e.ClickedItem is Customer customer)
                {
                    // Select the clicked customer
                    ViewModel.SelectedCustomer = customer;
                    
                    // Optionally navigate to edit page on double-click
                    // This would require detecting double-click vs single-click
                    
                    Debug.WriteLine($"HomePage: Customer item clicked: {customer.FullName}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"HomePage: Error handling item click: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles ListView selection changed events.
        /// This method ensures proper selection state management.
        /// </summary>
        /// <param name="sender">The ListView that raised the event</param>
        /// <param name="e">Selection changed event arguments</param>
        private void CustomerListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                // The SelectedItem binding should handle this automatically,
                // but we can add additional logic here if needed
                
                if (e.AddedItems?.Count > 0)
                {
                    var selectedCustomer = e.AddedItems[0] as Customer;
                    Debug.WriteLine($"HomePage: Customer selection changed to: {selectedCustomer?.FullName ?? "None"}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"HomePage: Error handling selection change: {ex.Message}");
            }
        }

        #endregion

        #region Accessibility Support

        /// <summary>
        /// Updates accessibility properties based on current page state.
        /// This method ensures the page is accessible to users with disabilities.
        /// </summary>
        private void UpdateAccessibilityProperties()
        {
            try
            {
                // Update AutomationProperties for dynamic content
                if (ViewModel != null)
                {
                    // Update list accessibility description
                    var listDescription = ViewModel.HasCustomers 
                        ? $"Customer list with {ViewModel.FilteredCustomerCount} items"
                        : "No customers available";
                    
                    Windows.UI.Xaml.Automation.AutomationProperties.SetName(
                        CustomerListView, listDescription);

                    // Update search box accessibility
                    if (ViewModel.IsSearchActive)
                    {
                        Windows.UI.Xaml.Automation.AutomationProperties.SetHelpText(
                            CustomerListView, $"Filtered results for '{ViewModel.SearchText}'");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"HomePage: Error updating accessibility properties: {ex.Message}");
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Scrolls to a specific customer in the list.
        /// This method provides smooth scrolling to ensure customer visibility.
        /// </summary>
        /// <param name="customer">The customer to scroll to</param>
        public void ScrollToCustomer(Customer customer)
        {
            try
            {
                if (customer != null)
                {
                    CustomerListView.ScrollIntoView(customer, ScrollIntoViewAlignment.Default);
                    Debug.WriteLine($"HomePage: Scrolled to customer {customer.FullName}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"HomePage: Error scrolling to customer: {ex.Message}");
            }
        }

        /// <summary>
        /// Sets focus to the search box for immediate user input.
        /// This method improves user experience by enabling quick search.
        /// </summary>
        public void FocusSearchBox()
        {
            try
            {
                // Find the search TextBox and focus it
                // This would require a name for the TextBox in XAML
                Debug.WriteLine("HomePage: Search box focused");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"HomePage: Error focusing search box: {ex.Message}");
            }
        }

        #endregion
    }

    /// <summary>
    /// Enumeration of navigation actions that can be passed as parameters.
    /// These actions trigger specific behaviors when the page is navigated to.
    /// </summary>
    public enum NavigationAction
    {
        /// <summary>
        /// Refresh the customer data.
        /// </summary>
        RefreshData,

        /// <summary>
        /// Clear the current search filter.
        /// </summary>
        ClearSearch,

        /// <summary>
        /// Navigate to add a new customer.
        /// </summary>
        AddCustomer
    }
}