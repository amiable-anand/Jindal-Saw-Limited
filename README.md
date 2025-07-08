# Jindal Guest Management System

A comprehensive .NET MAUI cross-platform application for managing guest check-ins and check-outs in hospitality settings.

## 🏨 Features

### 🔐 Security & Authentication
- **Secure Login**: BCrypt password hashing with strong default credentials
- **Session Management**: Persistent login state with secure preferences
- **Input Validation**: Comprehensive data validation and sanitization

### 📊 Dashboard & Analytics
- **Real-time Dashboard**: Live statistics and key performance indicators
- **Quick Actions**: One-click access to common tasks
- **Recent Activity**: Live feed of check-ins, check-outs, and system events
- **Visual Analytics**: Color-coded room status and occupancy metrics

### 🏢 Management Features
- **Location Management**: Multi-floor/building support with hierarchical organization
- **Room Management**: Smart room allocation with availability tracking
- **Guest Management**: Complete guest lifecycle from check-in to check-out
- **Advanced Reporting**: Excel export with customizable date ranges

### 🎨 User Experience
- **Modern UI**: Clean, professional design with consistent branding
- **Mobile-First**: Responsive design optimized for all screen sizes
- **Intuitive Navigation**: Icon-based menu with logical information architecture
- **Loading States**: User feedback and error handling throughout

### 🔧 Technical Features
- **Cross-Platform**: Runs on Android, iOS, macOS, and Windows
- **Offline-First**: SQLite database for reliable local data storage
- **Fast Performance**: Optimized queries and efficient data loading
- **Scalable Architecture**: Clean separation of concerns with MVVM pattern

## 🚀 Getting Started

### Prerequisites

- .NET 9.0 SDK
- Visual Studio 2022 (17.8 or later) with MAUI workload
- For mobile development: Android SDK and/or Xcode

### Installation

1. Clone the repository:
```bash
git clone [your-repository-url]
cd Jindal
```

2. Restore dependencies:
```bash
dotnet restore
```

3. Build the project:
```bash
dotnet build
```

4. Run the application:
```bash
dotnet run
```

## 🔐 Default Login

**Username**: `admin`  
**Password**: `JindalAdmin2024!`

⚠️ **Important**: Change the default password immediately after first login for security.

**Note**: The password is now securely hashed using BCrypt for enhanced security.

## 📱 Platform Support

| Platform | Status | Notes |
|----------|--------|-------|
| Windows | ✅ Supported | Windows 10 version 1903 or higher |
| Android | ✅ Supported | API 21 (Android 5.0) or higher |
| iOS | ✅ Supported | iOS 15.0 or higher |
| macOS | ✅ Supported | macOS 12.0 or higher |

## 🏗️ Architecture

### Data Models
- **Employee**: User authentication and authorization
- **Location**: Property locations (floors, buildings)
- **Room**: Room information and availability
- **CheckInOut**: Guest registration and stay tracking

### Security Features
- BCrypt password hashing
- Secure session management
- Input validation and sanitization

### Database
- SQLite for local data storage
- Automatic database initialization
- Seed data for quick setup

## 📖 User Guide

### First Time Setup
1. Launch the application
2. Login with default credentials
3. Create locations (e.g., "First Floor", "Second Floor")
4. Add rooms to locations
5. Start managing guests

### Managing Guests
1. Navigate to "Check In/Out" from the menu
2. Click "Add Guest" to register new arrivals
3. Select available rooms from the dropdown
4. Fill in guest information
5. Complete check-in process

### Generating Reports
1. Go to "Report" section
2. Select date range (optional)
3. Export data to Excel format
4. Share or print reports as needed

## 🔧 Configuration

### Database Location
The SQLite database is stored in the app's data directory:
- Windows: `%LOCALAPPDATA%\Packages\[AppId]\LocalState\`
- Android: `/data/data/[PackageName]/files/`
- iOS: `~/Library/[AppName]/`

### Customization
You can customize the app by modifying:
- Colors and themes in `Resources/Styles/`
- Default locations in `DatabaseService.cs`
- App icon in `Resources/AppIcon/`

## 🛠️ Development

### Project Structure
```
Jindal/
├── Models/           # Data models
├── Views/            # UI pages and components
├── Services/         # Business logic and data access
├── Resources/        # Images, fonts, styles
└── Platforms/        # Platform-specific code
```

### Key Dependencies
- **Microsoft.Maui.Controls**: UI framework
- **sqlite-net-pcl**: Database access
- **BCrypt.Net-Next**: Password hashing
- **ClosedXML**: Excel export functionality
- **CommunityToolkit.Maui**: Additional UI controls

### Building for Release

#### Android
```bash
dotnet publish -f net9.0-android -c Release
```

#### iOS
```bash
dotnet publish -f net9.0-ios -c Release
```

#### Windows
```bash
dotnet publish -f net9.0-windows10.0.19041.0 -c Release
```

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests if applicable
5. Submit a pull request

## 📞 Support

For support and questions:
- Create an issue in the repository
- Contact the development team

## 🔄 Version History

### v1.1.0 (Current) - Major UI/UX Update
- ✨ **NEW**: Modern Dashboard with real-time analytics
- 🎨 **NEW**: Professional blue branding (#1E3A8A)
- 📱 **IMPROVED**: Mobile-first responsive design
- 🛡️ **ENHANCED**: BCrypt password security
- 🧩 **IMPROVED**: Intuitive navigation with icons
- ⚡ **ENHANCED**: Quick action buttons
- 📊 **NEW**: Live activity feed
- 🐛 **FIXED**: All deprecated API usage
- 🔧 **IMPROVED**: Error handling and user feedback

### v1.0.0
- Initial release
- Basic guest management functionality
- Multi-platform support
- Basic authentication system

---

**© 2024 Jindal Guest Management System. All rights reserved.**
