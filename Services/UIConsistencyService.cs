using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Controls.Shapes;
using System;
using System.Collections.Generic;

namespace Jindal.Services
{
    /// <summary>
    /// Service to maintain UI consistency across all pages
    /// </summary>
    public static class UIConsistencyService
    {
        // Standard Colors
        public static readonly Color PrimaryColor = Color.FromArgb("#1E3A8A");
        public static readonly Color SecondaryColor = Color.FromArgb("#10B981");
        public static readonly Color ErrorColor = Color.FromArgb("#EF4444");
        public static readonly Color WarningColor = Color.FromArgb("#F59E0B");
        public static readonly Color SuccessColor = Color.FromArgb("#10B981");
        public static readonly Color InfoColor = Color.FromArgb("#3B82F6");
        
        // Text Colors
        public static readonly Color OnSurfaceColor = Color.FromArgb("#1E293B");
        public static readonly Color OnSurfaceVariantColor = Color.FromArgb("#64748B");
        public static readonly Color OnBackgroundColor = Color.FromArgb("#0F172A");
        public static readonly Color OnPrimaryColor = Color.FromArgb("#FFFFFF");
        
        // Background Colors
        public static readonly Color SurfaceColor = Color.FromArgb("#FFFFFF");
        public static readonly Color SurfaceVariantColor = Color.FromArgb("#F8FAFC");
        public static readonly Color BackgroundColor = Color.FromArgb("#FFFFFF");
        public static readonly Color InputBackgroundColor = Color.FromArgb("#F8FAFC");
        public static readonly Color InputBorderColor = Color.FromArgb("#E2E8F0");
        
        // Standard Dimensions
        public static readonly double StandardCornerRadius = 12;
        public static readonly double StandardPadding = 16;
        public static readonly double StandardSpacing = 16;
        public static readonly double StandardButtonHeight = 44;
        public static readonly double StandardFontSize = 14;
        public static readonly double HeaderFontSize = 24;
        public static readonly double SubHeaderFontSize = 16;
        public static readonly double CaptionFontSize = 12;

        /// <summary>
        /// Create a standardized Entry control
        /// </summary>
        public static Entry CreateStandardEntry(string placeholder, string text = "")
        {
            return new Entry
            {
                Placeholder = placeholder,
                Text = text,
                FontSize = StandardFontSize,
                BackgroundColor = InputBackgroundColor,
                TextColor = OnSurfaceColor,
                PlaceholderColor = OnSurfaceVariantColor,
                MinimumHeightRequest = StandardButtonHeight
            };
        }

        /// <summary>
        /// Create a standardized Label control
        /// </summary>
        public static Label CreateStandardLabel(string text, bool isHeader = false, bool isCaption = false)
        {
            var fontSize = isHeader ? HeaderFontSize : (isCaption ? CaptionFontSize : StandardFontSize);
            var fontAttributes = isHeader ? FontAttributes.Bold : FontAttributes.None;
            
            return new Label
            {
                Text = text,
                FontSize = fontSize,
                FontAttributes = fontAttributes,
                TextColor = OnSurfaceColor
            };
        }

        /// <summary>
        /// Create a standardized Button control
        /// </summary>
        public static Button CreateStandardButton(string text, ButtonStyle style = ButtonStyle.Primary)
        {
            var button = new Button
            {
                Text = text,
                FontSize = StandardFontSize,
                FontAttributes = FontAttributes.Bold,
                CornerRadius = (int)StandardCornerRadius,
                MinimumHeightRequest = StandardButtonHeight,
                Padding = new Thickness(StandardPadding, 8)
            };

            switch (style)
            {
                case ButtonStyle.Primary:
                    button.BackgroundColor = PrimaryColor;
                    button.TextColor = OnPrimaryColor;
                    break;
                case ButtonStyle.Secondary:
                    button.BackgroundColor = SecondaryColor;
                    button.TextColor = OnPrimaryColor;
                    break;
                case ButtonStyle.Success:
                    button.BackgroundColor = SuccessColor;
                    button.TextColor = OnPrimaryColor;
                    break;
                case ButtonStyle.Warning:
                    button.BackgroundColor = WarningColor;
                    button.TextColor = OnPrimaryColor;
                    break;
                case ButtonStyle.Error:
                    button.BackgroundColor = ErrorColor;
                    button.TextColor = OnPrimaryColor;
                    break;
                case ButtonStyle.Outline:
                    button.BackgroundColor = Colors.Transparent;
                    button.TextColor = PrimaryColor;
                    button.BorderColor = PrimaryColor;
                    button.BorderWidth = 1;
                    break;
            }

            return button;
        }

        /// <summary>
        /// Create a standardized Border control (Frame replacement for .NET 9)
        /// </summary>
        public static Border CreateStandardBorder(bool hasShadow = true)
        {
            var border = new Border
            {
                BackgroundColor = SurfaceColor,
                StrokeThickness = 1,
                Stroke = InputBorderColor,
                Padding = new Thickness(StandardPadding)
            };
            
            border.StrokeShape = new RoundRectangle
            {
                CornerRadius = new CornerRadius(StandardCornerRadius)
            };
            
            if (hasShadow)
            {
                border.Shadow = new Shadow
                {
                    Brush = new SolidColorBrush(Colors.Black),
                    Opacity = 0.1f,
                    Radius = 8,
                    Offset = new Point(0, 2)
                };
            }
            
            return border;
        }

        /// <summary>
        /// Create a standardized DatePicker control
        /// </summary>
        public static DatePicker CreateStandardDatePicker(DateTime? date = null)
        {
            return new DatePicker
            {
                Date = date ?? DateTime.Today,
                BackgroundColor = InputBackgroundColor,
                TextColor = OnSurfaceColor,
                MinimumHeightRequest = StandardButtonHeight
            };
        }

        /// <summary>
        /// Create a standardized TimePicker control
        /// </summary>
        public static TimePicker CreateStandardTimePicker(TimeSpan? time = null)
        {
            return new TimePicker
            {
                Time = time ?? DateTime.Now.TimeOfDay,
                BackgroundColor = InputBackgroundColor,
                TextColor = OnSurfaceColor,
                MinimumHeightRequest = StandardButtonHeight
            };
        }

        /// <summary>
        /// Create a standardized Picker control
        /// </summary>
        public static Picker CreateStandardPicker(string title, IList<string> items)
        {
            var picker = new Picker
            {
                Title = title,
                BackgroundColor = InputBackgroundColor,
                TextColor = OnSurfaceColor,
                TitleColor = OnSurfaceVariantColor,
                MinimumHeightRequest = StandardButtonHeight
            };

            foreach (var item in items)
            {
                picker.Items.Add(item);
            }

            return picker;
        }

        /// <summary>
        /// Apply consistent styling to existing controls
        /// </summary>
        public static void ApplyStandardStyling(View view)
        {
            switch (view)
            {
                case Entry entry:
                    entry.BackgroundColor = InputBackgroundColor;
                    entry.TextColor = OnSurfaceColor;
                    entry.PlaceholderColor = OnSurfaceVariantColor;
                    entry.FontSize = StandardFontSize;
                    entry.MinimumHeightRequest = StandardButtonHeight;
                    break;
                    
                case Label label:
                    label.TextColor = OnSurfaceColor;
                    label.FontSize = StandardFontSize;
                    break;
                    
                case Button button:
                    button.BackgroundColor = PrimaryColor;
                    button.TextColor = OnPrimaryColor;
                    button.FontSize = StandardFontSize;
                    button.FontAttributes = FontAttributes.Bold;
                    button.CornerRadius = (int)StandardCornerRadius;
                    button.MinimumHeightRequest = StandardButtonHeight;
                    button.Padding = new Thickness(StandardPadding, 8);
                    break;
                    
                case Border border:
                    border.BackgroundColor = SurfaceColor;
                    border.StrokeThickness = 1;
                    border.Stroke = InputBorderColor;
                    border.Padding = new Thickness(StandardPadding);
                    border.StrokeShape = new RoundRectangle
                    {
                        CornerRadius = new CornerRadius(StandardCornerRadius)
                    };
                    border.Shadow = new Shadow
                    {
                        Brush = new SolidColorBrush(Colors.Black),
                        Opacity = 0.1f,
                        Radius = 8,
                        Offset = new Point(0, 2)
                    };
                    break;
                    
                case DatePicker datePicker:
                    datePicker.BackgroundColor = InputBackgroundColor;
                    datePicker.TextColor = OnSurfaceColor;
                    datePicker.MinimumHeightRequest = StandardButtonHeight;
                    break;
                    
                case TimePicker timePicker:
                    timePicker.BackgroundColor = InputBackgroundColor;
                    timePicker.TextColor = OnSurfaceColor;
                    timePicker.MinimumHeightRequest = StandardButtonHeight;
                    break;
                    
                case Picker picker:
                    picker.BackgroundColor = InputBackgroundColor;
                    picker.TextColor = OnSurfaceColor;
                    picker.TitleColor = OnSurfaceVariantColor;
                    picker.MinimumHeightRequest = StandardButtonHeight;
                    break;
            }
        }

        /// <summary>
        /// Create a standardized loading indicator
        /// </summary>
        public static ActivityIndicator CreateLoadingIndicator()
        {
            return new ActivityIndicator
            {
                Color = PrimaryColor,
                IsRunning = true,
                IsVisible = true,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            };
        }

        /// <summary>
        /// Create a standardized page header
        /// </summary>
        public static Label CreatePageHeader(string title)
        {
            return new Label
            {
                Text = title,
                FontSize = HeaderFontSize,
                FontAttributes = FontAttributes.Bold,
                TextColor = OnBackgroundColor,
                HorizontalOptions = LayoutOptions.Center,
                HorizontalTextAlignment = TextAlignment.Center
            };
        }

        /// <summary>
        /// Create a standardized section header
        /// </summary>
        public static Label CreateSectionHeader(string title)
        {
            return new Label
            {
                Text = title,
                FontSize = SubHeaderFontSize,
                FontAttributes = FontAttributes.Bold,
                TextColor = OnSurfaceColor,
                Margin = new Thickness(0, StandardSpacing, 0, 8)
            };
        }

        /// <summary>
        /// Create a standardized caption label
        /// </summary>
        public static Label CreateCaptionLabel(string text)
        {
            return new Label
            {
                Text = text,
                FontSize = CaptionFontSize,
                TextColor = OnSurfaceVariantColor
            };
        }
    }

    /// <summary>
    /// Button style enumeration
    /// </summary>
    public enum ButtonStyle
    {
        Primary,
        Secondary,
        Success,
        Warning,
        Error,
        Outline
    }
}
