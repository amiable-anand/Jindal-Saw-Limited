using Jindal.Models;
using Jindal.Services;
using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using ClosedXML.Excel;
using Microsoft.Maui.Storage;

namespace Jindal.Views
{
    public partial class ReportPage : ContentPage
    {
        private List<CheckInOut> allCheckedOutRecords = new();
        private bool isRoomAscending = true;
        private bool isGuestNameAscending = true;
        private bool isCheckOutDateAscending = false;

        public ReportPage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            try
            {
                await LoadCheckedOutData();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }

        private async Task LoadCheckedOutData()
        {
            await DatabaseService.Init();
            
            // Get only checked out guests
            allCheckedOutRecords = (await DatabaseService.GetCheckedOutGuests())
                .OrderByDescending(r => r.CheckOutDate)
                .ToList();

            RoomFilterPicker.ItemsSource = allCheckedOutRecords
                .Select(r => r.RoomNumber)
                .Where(r => !string.IsNullOrWhiteSpace(r))
                .Distinct()
                .OrderBy(r => r)
                .ToList();

            FromDatePicker.Date = DateTime.Today.AddDays(-7);
            ToDatePicker.Date = DateTime.Today;

            ApplyFilters();
        }

        private void ApplyFilters()
        {
            string? selectedRoom = RoomFilterPicker.SelectedItem as string;
            string search = SearchEntry.Text?.ToLower() ?? "";
            DateTime fromDate = FromDatePicker.Date.Date;
            DateTime toDate = ToDatePicker.Date.Date;

            var filtered = allCheckedOutRecords
                .Where(r =>
                    (string.IsNullOrEmpty(selectedRoom) || r.RoomNumber == selectedRoom) &&
                    (string.IsNullOrEmpty(search) || r.GuestName?.ToLower().Contains(search) == true) &&
                    r.CheckOutDate?.Date >= fromDate &&
                    r.CheckOutDate?.Date <= toDate)
                .ToList();

            PopulateTable(filtered);
        }

        private void PopulateTable(List<CheckInOut> records)
        {
            while (ReportTable.RowDefinitions.Count > 1)
                ReportTable.RowDefinitions.RemoveAt(ReportTable.RowDefinitions.Count - 1);

            var oldContent = ReportTable.Children.Skip(10).ToList();
            foreach (var view in oldContent)
                ReportTable.Children.Remove(view);

            int row = 1;

            foreach (var r in records)
            {
                ReportTable.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                var textColor = Color.FromArgb("#374151"); // Dark text for white background
                
                AddToGrid(new Label { Text = r.RoomNumber ?? "-", TextColor = textColor }, 0, row);
                AddToGrid(new Label { Text = r.GuestName ?? "-", TextColor = textColor }, 1, row);
                AddToGrid(new Label { Text = r.GuestIdNumber ?? "-", TextColor = textColor }, 2, row);
                AddToGrid(new Label { Text = r.CheckInDate.ToString("dd-MM-yyyy"), TextColor = textColor }, 3, row);
                AddToGrid(new Label { Text = r.CheckInTime.ToString(@"hh\:mm"), TextColor = textColor }, 4, row);
                AddToGrid(new Label { Text = r.CheckOutDate?.ToString("dd-MM-yyyy") ?? "-", TextColor = textColor }, 5, row);
                AddToGrid(new Label { Text = r.CheckOutTime?.ToString(@"hh\:mm") ?? "-", TextColor = textColor }, 6, row);
                AddToGrid(new Label { Text = r.Department ?? "-", TextColor = textColor }, 7, row);
                AddToGrid(new Label { Text = r.Purpose ?? "-", TextColor = textColor }, 8, row);
                AddToGrid(new Label { Text = r.MailReceivedDate.ToString("dd-MM-yyyy"), TextColor = textColor }, 9, row);

                row++;
            }

            RowCountLabel.Text = $"Total guests: {records.Count}";
        }

        private void AddToGrid(View view, int col, int row)
        {
            Grid.SetColumn(view, col);
            Grid.SetRow(view, row);
            ReportTable.Children.Add(view);
        }

        private void OnRoomFilterChanged(object sender, EventArgs e) => ApplyFilters();
        private void OnSearchTextChanged(object sender, TextChangedEventArgs e) => ApplyFilters();
        private void OnDateFilterClicked(object sender, EventArgs e) => ApplyFilters();

        private async void OnReloadClicked(object sender, EventArgs e)
        {
            RoomFilterPicker.SelectedIndex = -1;
            SearchEntry.Text = string.Empty;
            FromDatePicker.Date = DateTime.Today.AddDays(-7);
            ToDatePicker.Date = DateTime.Today;

            await LoadCheckedOutData();
        }

        private void OnSortByRoom(object sender, EventArgs e)
        {
            isRoomAscending = !isRoomAscending;
            allCheckedOutRecords = isRoomAscending
                ? allCheckedOutRecords.OrderBy(r => r.RoomNumber).ToList()
                : allCheckedOutRecords.OrderByDescending(r => r.RoomNumber).ToList();

            RoomHeader.Text = $"Room No {(isRoomAscending ? "?" : "?")}";
            ApplyFilters();
        }

        private void OnSortByGuestName(object sender, EventArgs e)
        {
            isGuestNameAscending = !isGuestNameAscending;
            allCheckedOutRecords = isGuestNameAscending
                ? allCheckedOutRecords.OrderBy(r => r.GuestName).ToList()
                : allCheckedOutRecords.OrderByDescending(r => r.GuestName).ToList();

            GuestNameHeader.Text = $"Guest Name {(isGuestNameAscending ? "?" : "?")}";
            ApplyFilters();
        }

        private void OnSortByCheckOutDate(object sender, EventArgs e)
        {
            isCheckOutDateAscending = !isCheckOutDateAscending;
            allCheckedOutRecords = isCheckOutDateAscending
                ? allCheckedOutRecords.OrderBy(r => r.CheckOutDate).ToList()
                : allCheckedOutRecords.OrderByDescending(r => r.CheckOutDate).ToList();

            CheckOutDateHeader.Text = $"Check Out Date {(isCheckOutDateAscending ? "?" : "?")}";
            ApplyFilters();
        }

        private async void OnExportToExcelClicked(object sender, EventArgs e)
        {
            try
            {
#if ANDROID
                var status = await Permissions.RequestAsync<Permissions.StorageWrite>();
                if (status != PermissionStatus.Granted)
                {
                    await DisplayAlert("Permission Denied", "Storage permission is required to export the Excel file.", "OK");
                    return;
                }
#endif
                var fromDate = FromDatePicker.Date;
                var toDate = ToDatePicker.Date;

                var filtered = allCheckedOutRecords
                    .Where(r =>
                        r.CheckOutDate != null &&
                        r.CheckOutDate.Value.Date >= fromDate &&
                        r.CheckOutDate.Value.Date <= toDate)
                    .ToList();

                var wb = new XLWorkbook();
                var ws = wb.Worksheets.Add("Guest Report");

                string[] headers = {
                    "Room No", "Guest Name", "Guest ID No", "Check In Date", "Check In Time",
                    "Check Out Date", "Check Out Time", "Department", "Purpose", "Mail Received Date"
                };

                for (int i = 0; i < headers.Length; i++)
                    ws.Cell(1, i + 1).Value = headers[i];

                for (int i = 0; i < filtered.Count; i++)
                {
                    var r = filtered[i];
                    int row = i + 2;
                    ws.Cell(row, 1).Value = r.RoomNumber;
                    ws.Cell(row, 2).Value = r.GuestName;
                    ws.Cell(row, 3).Value = r.GuestIdNumber;
                    ws.Cell(row, 4).Value = r.CheckInDate.ToString("dd-MM-yyyy");
                    ws.Cell(row, 5).Value = r.CheckInTime.ToString(@"hh\:mm");
                    ws.Cell(row, 6).Value = r.CheckOutDate?.ToString("dd-MM-yyyy");
                    ws.Cell(row, 7).Value = r.CheckOutTime?.ToString(@"hh\:mm");
                    ws.Cell(row, 8).Value = r.Department;
                    ws.Cell(row, 9).Value = r.Purpose;
                    ws.Cell(row, 10).Value = r.MailReceivedDate.ToString("dd-MM-yyyy");
                }

                ws.Columns().AdjustToContents();

                string fileName = $"Guest_Report_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                string filePath;

#if ANDROID
                var downloads = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads).AbsolutePath;
                filePath = Path.Combine(downloads, fileName);
#else
                filePath = Path.Combine(FileSystem.AppDataDirectory, fileName);
#endif

                wb.SaveAs(filePath);

                await DisplayAlert("Success", $"Excel file saved to:\n{filePath}", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }
    }
}
