using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Jindal.Services
{
    /// <summary>
    /// Service to provide professional features like loading indicators, confirmations, and enhanced UX
    /// </summary>
    public static class ProfessionalFeaturesService
    {
        /// <summary>
        /// Show a loading dialog while executing an async operation
        /// </summary>
        public static async Task<T> ExecuteWithLoading<T>(Func<Task<T>> operation, string message = "Loading...")
        {
            var loadingPage = new LoadingPage(message);
            var mainPage = GetMainPage();
            
            if (mainPage == null) return await operation();
            
            try
            {
                // Show loading page
                await mainPage.Navigation.PushModalAsync(loadingPage);
                
                // Execute operation
                var result = await operation();
                
                return result;
            }
            finally
            {
                // Hide loading page
                await mainPage.Navigation.PopModalAsync();
            }
        }

        /// <summary>
        /// Show a loading dialog while executing an async operation (void return)
        /// </summary>
        public static async Task ExecuteWithLoading(Func<Task> operation, string message = "Loading...")
        {
            var loadingPage = new LoadingPage(message);
            var mainPage = GetMainPage();
            
            if (mainPage == null)
            {
                await operation();
                return;
            }
            
            try
            {
                // Show loading page
                await mainPage.Navigation.PushModalAsync(loadingPage);
                
                // Execute operation
                await operation();
            }
            finally
            {
                // Hide loading page
                await mainPage.Navigation.PopModalAsync();
            }
        }

        /// <summary>
        /// Show a professional confirmation dialog
        /// </summary>
        public static async Task<bool> ShowConfirmationDialog(string title, string message, string acceptText = "Yes", string cancelText = "No")
        {
            var mainPage = GetMainPage();
            return mainPage != null && await mainPage.DisplayAlert(title, message, acceptText, cancelText);
        }

        /// <summary>
        /// Show a professional alert dialog
        /// </summary>
        public static async Task ShowAlert(string title, string message, string buttonText = "OK")
        {
            var mainPage = GetMainPage();
            if (mainPage != null)
                await mainPage.DisplayAlert(title, message, buttonText);
        }

        /// <summary>
        /// Show a success message
        /// </summary>
        public static async Task ShowSuccess(string message)
        {
            await ShowAlert("Success", message, "Great!");
        }

        /// <summary>
        /// Show an error message
        /// </summary>
        public static async Task ShowError(string message)
        {
            await ShowAlert("Error", message, "OK");
        }

        /// <summary>
        /// Show a warning message
        /// </summary>
        public static async Task ShowWarning(string message)
        {
            await ShowAlert("Warning", message, "OK");
        }

        /// <summary>
        /// Show an info message
        /// </summary>
        public static async Task ShowInfo(string message)
        {
            await ShowAlert("Information", message, "OK");
        }

        /// <summary>
        /// Show a delete confirmation dialog
        /// </summary>
        public static async Task<bool> ShowDeleteConfirmation(string itemName)
        {
            return await ShowConfirmationDialog(
                "Confirm Delete",
                $"Are you sure you want to delete {itemName}? This action cannot be undone.",
                "Delete",
                "Cancel"
            );
        }

        /// <summary>
        /// Show a save confirmation dialog
        /// </summary>
        public static async Task<bool> ShowSaveConfirmation(string itemName)
        {
            return await ShowConfirmationDialog(
                "Confirm Save",
                $"Are you sure you want to save changes to {itemName}?",
                "Save",
                "Cancel"
            );
        }

        /// <summary>
        /// Show a checkout confirmation dialog
        /// </summary>
        public static async Task<bool> ShowCheckoutConfirmation(string guestName, string roomNumber)
        {
            return await ShowConfirmationDialog(
                "Confirm Check-Out",
                $"Are you sure you want to check out {guestName} from Room {roomNumber}?",
                "Check Out",
                "Cancel"
            );
        }

        /// <summary>
        /// Show validation errors in a professional way
        /// </summary>
        public static async Task ShowValidationErrors(ValidationResult validationResult)
        {
            if (!validationResult.IsValid)
            {
                var errorMessage = string.Join("\n• ", validationResult.Errors);
                await ShowError($"Please fix the following issues:\n\n• {errorMessage}");
            }
        }

        /// <summary>
        /// Show a toast-like message (using DisplayAlert for now, can be enhanced with custom implementations)
        /// </summary>
        public static async Task ShowToast(string message, int durationMs = 3000)
        {
            // For now, use DisplayAlert. In a real app, you'd implement a custom toast
            var mainPage = GetMainPage();
            if (mainPage != null)
                await mainPage.DisplayAlert("", message, "OK");
        }
        
        /// <summary>
        /// Get the current MainPage using the modern .NET 9 approach.
        /// </summary>
        private static Page? GetMainPage() =>
            Application.Current?.Windows?.FirstOrDefault()?.Page;
    }

    /// <summary>
    /// Loading page to show during async operations
    /// </summary>
    public class LoadingPage : ContentPage
    {
        public LoadingPage(string message = "Loading...")
        {
            Title = "Loading";
            BackgroundColor = Color.FromArgb("#80000000"); // Semi-transparent black
            
            Content = new StackLayout
            {
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center,
                Children =
                {
                    new Border
                    {
                        BackgroundColor = Colors.White,
                        StrokeShape = new RoundRectangle { CornerRadius = 20 },
                        Padding = 30,
                        Shadow = new Shadow { Brush = Brush.Black, Opacity = 0.2f, Offset = new Point(0, 4) },
                        Content = new StackLayout
                        {
                            Spacing = 20,
                            Children =
                            {
                                new ActivityIndicator
                                {
                                    IsRunning = true,
                                    Color = Color.FromArgb("#1E3A8A"),
                                    HorizontalOptions = LayoutOptions.Center
                                },
                                new Label
                                {
                                    Text = message,
                                    HorizontalOptions = LayoutOptions.Center,
                                    FontSize = 16,
                                    TextColor = Color.FromArgb("#1E293B")
                                }
                            }
                        }
                    }
                }
            };
        }
    }
}
