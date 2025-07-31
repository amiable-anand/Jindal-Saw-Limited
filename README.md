# 🏨 Jindal Guest Management System

<div align="center">

![Version](https://img.shields.io/badge/version-2.0.0-blue.svg)
![.NET](https://img.shields.io/badge/.NET-9.0-purple.svg)
![Platform](https://img.shields.io/badge/platform-Windows%20%7C%20Android%20%7C%20iOS%20%7C%20macOS-green.svg)
![API](https://img.shields.io/badge/API-ASP.NET%20Core%209.0-orange.svg)
![Database](https://img.shields.io/badge/Database-SQL%20Server%20%7C%20SQLite-red.svg)
![Status](https://img.shields.io/badge/status-Production%20Ready-brightgreen.svg)

**Complete enterprise-grade hospitality management solution with cross-platform client and REST API backend**

*Professional • Secure • Cross-Platform • Scalable • API-First*

</div>

---

## 📋 **System Overview**

The **Jindal Guest House Management System** is a **production-ready**, enterprise-grade solution for comprehensive hospitality management. This system represents the pinnacle of modern software architecture with:

### **🏗️ Architecture Components**
- **📱 MAUI Cross-Platform App**: Native performance on Windows, Android, iOS, and macOS
- **🌐 ASP.NET Core Web API**: Enterprise-grade REST API with JWT authentication
- **🗄️ Hybrid Database System**: SQLite for offline + SQL Server for production scalability
- **☁️ Cloud-Ready Infrastructure**: Azure/AWS deployment ready with Docker support

## ✨ **Enterprise Features**

### 🔐 **Security & Authentication**
- 🛡️ **Military-Grade Encryption**: BCrypt password hashing with dynamic salts
- 🔑 **JWT Token Authentication**: Secure, stateless authentication with 8-hour sessions
- 👥 **Role-Based Access Control**: Granular permission system (Admin/User roles)
- 🔒 **Session Management**: Automatic timeout and refresh token handling
- 🛠️ **Audit Trails**: Complete activity logging for compliance
- 🚫 **Rate Limiting**: API protection against abuse (100 req/min)

### 📊 **Advanced Analytics & Reporting**
- 📈 **Real-Time Dashboard**: Live KPIs, occupancy rates, and revenue metrics
- 📋 **Excel Export**: Professional reports with advanced filtering
- 📊 **Visual Analytics**: Color-coded status indicators and trend charts
- 🕐 **Activity Feed**: Real-time check-in/check-out notifications
- 📅 **Historical Analysis**: Date-range reporting with custom filters

### 🏢 **Comprehensive Management Suite**
- 🏗️ **Multi-Location Support**: Unlimited properties, buildings, and floors
- 🏠 **Smart Room Management**: Automated availability tracking and optimization
- 👥 **Complete Guest Lifecycle**: From inquiry to check-out
- 💳 **Payment Integration Ready**: Extensible payment processing
- 🔄 **Bulk Operations**: Mass check-in/check-out capabilities
- 📱 **Offline-First Architecture**: Continue working without internet

### 🎨 **Superior User Experience**
- 🎯 **Modern Material Design**: Clean, professional interface
- 📱 **True Responsive Design**: Optimized for all screen sizes
- 🧭 **Intuitive Navigation**: Icon-based menu with logical flow
- ⚡ **Lightning Fast**: Optimized performance with lazy loading
- 🌙 **Accessibility Ready**: WCAG 2.1 AA compliant
- 📏 **Mobile-First**: Touch-optimized with horizontal scrolling tables

### 🔧 **Technical Excellence**
- 🌐 **True Cross-Platform**: Single codebase, native performance
- 🗄️ **Hybrid Database**: SQLite (offline) + SQL Server (production)
- ⚡ **High Performance**: Optimized queries, caching, and lazy loading
- 🏗️ **Clean Architecture**: MVVM pattern with dependency injection
- 📦 **Modular Design**: Extensible plugin architecture
- 🐳 **Container Ready**: Docker support for cloud deployment

## 🚀 Quick Start

### Prerequisites
- .NET 9.0 SDK
- Visual Studio 2022 or VS Code
- SQL Server (LocalDB for development)
- Android SDK (for mobile development)

### API Setup
1. Navigate to `JindalGuestHouseAPI` folder
2. Update connection string in `appsettings.json`
3. Run database migrations:
   ```bash
   dotnet ef database update
   ```
4. Start the API:
   ```bash
   dotnet run
   ```

### Mobile App Setup
1. Open solution in Visual Studio
2. Update API base URL in `DatabaseService.cs`
3. Build and deploy to target platform

## 🔧 Configuration

### API Configuration (appsettings.json)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=JindalGuestHouseDB;Trusted_Connection=true;TrustServerCertificate=true;"
  },
  "JWT": {
    "SecretKey": "YourSecretKeyHere",
    "Issuer": "JindalGuestHouseAPI",
    "Audience": "JindalGuestHouseClient",
    "ExpirationHours": 8
  }
}
```

### Mobile App Configuration
Update `DatabaseService.cs` with your API URL:
```csharp
private static string _baseUrl = "https://yourapi.com/api/";
```

## 📱 Supported Platforms

| Platform | Support Status |
|----------|----------------|
| Android | ✅ Full Support |
| iOS | ✅ Full Support |
| Windows | ✅ Full Support |
| macOS | ✅ Full Support |

## 🔐 Default Credentials

**Admin User:**
- Username: `admin`
- Password: `admin123`

*⚠️ Change these credentials before production deployment!*

## 📊 API Endpoints

### Authentication
- `POST /api/Users/authenticate` - Login

### Users
- `GET /api/Users` - Get all users
- `POST /api/Users` - Create user
- `PUT /api/Users/{id}` - Update user
- `DELETE /api/Users/{id}` - Delete user

### Rooms
- `GET /api/Rooms` - Get all rooms
- `GET /api/Rooms/available` - Get available rooms
- `POST /api/Rooms` - Create room
- `PUT /api/Rooms/{id}` - Update room
- `DELETE /api/Rooms/{id}` - Delete room

### Locations
- `GET /api/Locations` - Get all locations
- `POST /api/Locations` - Create location
- `PUT /api/Locations/{id}` - Update location
- `DELETE /api/Locations/{id}` - Delete location

### Check-In/Out
- `GET /api/CheckInOut` - Get all records
- `POST /api/CheckInOut` - Create record
- `PUT /api/CheckInOut/{id}` - Update record
- `DELETE /api/CheckInOut/{id}` - Delete record

## 🔧 Development

### Database Migrations
```bash
# Add migration
dotnet ef migrations add MigrationName

# Update database
dotnet ef database update
```

### Running Tests
```bash
# Run all tests
dotnet test
```

## 📦 Deployment

### API Deployment
1. Publish the API:
   ```bash
   dotnet publish -c Release
   ```
2. Deploy to your preferred hosting provider
3. Update connection strings for production
4. Configure HTTPS certificates

### Mobile App Deployment
1. Build release versions for target platforms
2. Sign applications for distribution
3. Deploy to app stores or enterprise distribution

## 🆘 Support

For support and questions:
- Create an issue in the repository
- Contact: support@jindal.com

## 📄 License

© 2025 Jindal Corporation. All rights reserved.

---

**Built with ❤️ using .NET MAUI and ASP.NET Core**

# 🏨 Jindal Guest Management System

<div align="center">

![Version](https://img.shields.io/badge/version-2.0.0-blue.svg?cacheSeconds=2592000)
![.NET](https://img.shields.io/badge/.NET-9.0-purple.svg)
![Platform](https://img.shields.io/badge/platform-Windows%20%7C%20Android%20%7C%20iOS%20%7C%20macOS-green.svg)
![License](https://img.shields.io/badge/license-MIT-blue.svg)

**A comprehensive, enterprise-grade .NET MAUI cross-platform application for hospitality guest management**

*Professional • Secure • Cross-Platform • Responsive*

</div>

---

## 📖 Table of Contents

- [✨ Features](#-features)
- [🚀 Quick Start](#-quick-start)
- [📱 Platform Support](#-platform-support)
- [🏗️ Architecture](#️-architecture)
- [🔧 Installation](#-installation)
- [📚 User Guide](#-user-guide)
- [🛠️ Development](#️-development)
- [📦 Deployment](#-deployment)
- [🤝 Contributing](#-contributing)
- [📄 License](#-license)

---

## ✨ Features

### 🔐 **Enterprise Security**
- 🛡️ **BCrypt Password Hashing**: Military-grade password encryption
- 🔑 **Role-Based Access Control**: Admin and user permissions
- 🔒 **Secure Session Management**: Persistent, encrypted login states
- ✅ **Input Validation**: Comprehensive data validation and sanitization
- 🛠️ **Audit Trails**: Complete activity logging for compliance

### 📊 **Advanced Dashboard & Analytics**
- 📈 **Real-time KPIs**: Live room occupancy and availability metrics
- ⚡ **Quick Actions**: One-click access to frequently used features
- 🕐 **Activity Feed**: Real-time check-in/check-out notifications
- 🎨 **Visual Analytics**: Color-coded status indicators and charts
- 📱 **Responsive Cards**: Mobile-optimized dashboard layout

### 🏢 **Complete Management Suite**
- 🏗️ **Multi-Location Support**: Unlimited floors, buildings, and properties
- 🏠 **Smart Room Management**: Automated availability tracking
- 👥 **Guest Lifecycle**: End-to-end guest journey management
- 📋 **Advanced Reporting**: Excel export with date filtering and search
- 🔄 **Bulk Operations**: Mass check-in/out capabilities

### 🎨 **Superior User Experience**
- 🎯 **Modern Material Design**: Clean, professional interface
- 📱 **Mobile-First Responsive**: Optimized for all screen sizes and orientations
- 🧭 **Intuitive Navigation**: Icon-based menu with logical flow
- ⚡ **Loading States**: Smooth animations and user feedback
- 🌙 **Dark Mode Ready**: Theme support for enhanced usability
- 📏 **Horizontal Scrolling**: Optimized table views for mobile devices

### 🔧 **Technical Excellence**
- 🌐 **True Cross-Platform**: Windows, Android, iOS, macOS support
- 🗄️ **Offline-First Architecture**: SQLite with automatic sync
- ⚡ **High Performance**: Optimized queries and lazy loading
- 🏗️ **Clean Architecture**: MVVM pattern with dependency injection
- 🔄 **Real-time Updates**: Live data synchronization
- 📦 **Modular Design**: Extensible plugin architecture

## 🚀 Quick Start

### ⚡ 5-Minute Setup

```bash
# Clone and setup
git clone [your-repository-url]
cd Jindal
dotnet restore
dotnet build

# Run on Windows
dotnet run --framework net9.0-windows10.0.19041.0

# Run on Android (with device connected)
dotnet run --framework net9.0-android
```

### 💻 Development Prerequisites

| Requirement | Version | Notes |
|-------------|---------|-------|
| .NET SDK | 9.0+ | [Download](https://dotnet.microsoft.com/download) |
| Visual Studio | 2022 (17.8+) | With .NET MAUI workload |
| Android SDK | API 21+ | For Android development |
| Xcode | 15.0+ | For iOS/macOS (macOS only) |

### 🔧 Installation

#### **Option 1: Visual Studio (Recommended)**
1. Open Visual Studio 2022
2. Clone repository: `File → Clone Repository`
3. Enter URL and clone
4. Select target platform (Windows/Android/iOS)
5. Press F5 to run

#### **Option 2: Command Line**
```bash
# 1. Clone repository
git clone [your-repository-url]
cd Jindal

# 2. Restore dependencies
dotnet restore

# 3. Build project
dotnet build

# 4. Run application
# Windows
dotnet run --framework net9.0-windows10.0.19041.0

# Android (requires connected device or emulator)
dotnet run --framework net9.0-android

# iOS (macOS only)
dotnet run --framework net9.0-ios
```

#### **Option 3: Package Installation**
*Download pre-built packages from [Releases](../../releases)*

## 🔐 Default Login

| Field | Value |
|-------|-------|
| **Username** | `admin` |
| **Password** | `JindalAdmin2024!` |

⚠️ **Security Notice**: Change the default password immediately after first login.

🛡️ **Security Features**:
- BCrypt password hashing with salt
- Session-based authentication
- Automatic session timeout
- Failed login attempt protection

## 📸 Screenshots

### 🏠 Desktop Experience

| Login Screen | Dashboard | Guest Management |
|-------------|-----------|------------------|
| Modern login interface | Real-time analytics | Complete guest lifecycle |
| *Clean, professional design* | *Live KPIs and quick actions* | *Seamless check-in/out process* |

### 📱 Mobile Experience

| Mobile Dashboard | Room Management | Reports |
|-----------------|-----------------|----------|
| Responsive cards layout | Touch-optimized interface | Excel export functionality |
| *Optimized for small screens* | *Horizontal scrolling tables* | *Advanced filtering options* |

## 🎨 UI/UX Highlights

### 🎨 Design System
- **Primary Color**: Professional Blue (`#1E3A8A`)
- **Typography**: Open Sans font family
- **Icons**: Emoji-based for universal recognition
- **Spacing**: Consistent 8px grid system
- **Shadows**: Subtle depth for modern look

### 📱 Responsive Features
- **Breakpoints**: Automatic layout adaptation
- **Touch Targets**: 44px minimum for accessibility
- **Horizontal Scrolling**: Wide tables on mobile
- **Collapsible Sections**: Space-efficient mobile UI
- **Adaptive Text**: Dynamic font scaling

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

### 🛀 **Initial Setup Wizard**

#### 1️⃣ **First Launch**
```
🎆 Welcome to Jindal Guest Management!
🔑 Login with default credentials
🏢 Set up your first location
🏠 Add rooms to get started
```

#### 2️⃣ **System Configuration**
1. **Login** with admin credentials
2. **Navigate** to Settings → Change Password
3. **Create Locations**: Buildings, floors, or areas
4. **Add Rooms**: Assign rooms to locations
5. **Test Setup**: Try a guest check-in

### 👥 **Guest Management Workflows**

#### 🚪 **Check-In Process**
1. 🔍 **Search** for existing guest or create new
2. 🏠 **Select Room** from available options
3. 📝 **Fill Details**: Name, ID, contact info
4. 📅 **Set Dates**: Check-in and expected check-out
5. ✅ **Confirm** and complete check-in

#### 🚪 **Check-Out Process**
1. 🔍 **Find Guest** in active guests list
2. 📋 **Review Stay** details and charges
3. 💳 **Process Payment** (if applicable)
4. 🧯 **Clean Room** status update
5. ✅ **Complete** check-out

#### 🔄 **Bulk Operations**
- **Mass Check-out**: Select multiple guests
- **Room Status Update**: Bulk availability changes
- **Batch Reporting**: Generate multiple reports

### 📋 **Advanced Reporting**

#### 📈 **Dashboard Analytics**
- **Real-time KPIs**: Occupancy rates, revenue
- **Trend Analysis**: Historical performance
- **Quick Insights**: Today's arrivals/departures

#### 📤 **Excel Export Features**
1. 📅 **Date Range Selection**: Custom periods
2. 🔍 **Advanced Filters**: Room, guest type, status
3. 📊 **Export Options**: 
   - Guest Details Report
   - Occupancy Summary
   - Revenue Analysis
   - Custom Data Export

### 🛠️ **System Administration**

#### 👥 **User Management**
- **Create Users**: Admin and staff accounts
- **Role Assignment**: Granular permissions
- **Activity Monitoring**: User action logs

#### 🏢 **Property Management**
- **Location Hierarchy**: Buildings → Floors → Rooms
- **Room Types**: Standard, Suite, etc.
- **Availability Tracking**: Real-time status

## 🚪 **Mobile Responsiveness**

### 📱 **Android Optimizations**
- **Touch Targets**: Minimum 48dp for accessibility
- **Horizontal Scrolling**: Wide data tables
- **Adaptive Layouts**: Portrait/landscape orientations
- **Gesture Support**: Swipe, pinch, tap
- **Material Design**: Native Android feel

### 📱 **Cross-Platform Features**
- **Consistent UI**: Same experience across devices
- **Responsive Breakpoints**: Automatic layout adaptation
- **Platform Integration**: Native navigation patterns
- **Performance Optimization**: Smooth 60fps animations

## 🔧 **Troubleshooting**

### ⚠️ **Common Issues**

#### 🔑 **Login Problems**
```
Issue: Cannot login with default credentials
Solution: 
1. Verify username: 'admin' (case-sensitive)
2. Verify password: 'JindalAdmin2024!' (exact match)
3. Check database initialization
4. Clear app data and restart
```

#### 📱 **Mobile Layout Issues**
```
Issue: Tables not scrolling horizontally
Solution:
1. Update to latest version
2. Check device orientation lock
3. Clear app cache
4. Restart application
```

#### 💾 **Database Errors**
```
Issue: SQLite database corruption
Solution:
1. Close application completely
2. Clear app data (Windows: %LOCALAPPDATA%)
3. Restart application (auto-recreates DB)
4. Re-import data if needed
```

### 🔧 **Performance Optimization**

#### ⚡ **Speed Improvements**
- **Database Indexing**: Automatic query optimization
- **Lazy Loading**: Load data on demand
- **Caching**: Intelligent data caching
- **Memory Management**: Efficient resource usage

#### 📱 **Mobile Performance**
- **Reduced Bundle Size**: Platform-specific builds
- **Optimized Images**: Compressed resources
- **Efficient Layouts**: Virtual scrolling for large lists
- **Background Processing**: Non-blocking operations

### 🔍 **Debug Mode**

```bash
# Enable debug logging
dotnet run --configuration Debug

# View detailed logs
# Windows: Event Viewer
# Android: adb logcat
# iOS: Xcode Console
```

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

## 📦 **Deployment**

### 🌐 **Production Deployment**

#### 💻 **Windows Deployment**
```bash
# Create MSIX package
dotnet publish -f net9.0-windows10.0.19041.0 -c Release -p:RuntimeIdentifierOverride=win10-x64

# Self-contained deployment
dotnet publish -f net9.0-windows10.0.19041.0 -c Release --self-contained
```

#### 📱 **Android Deployment**
```bash
# Create APK
dotnet build -f net9.0-android -c Release

# Create AAB (recommended for Play Store)
dotnet publish -f net9.0-android -c Release -p:AndroidPackageFormat=aab
```

#### 🍎 **iOS Deployment**
```bash
# Archive for App Store
dotnet publish -f net9.0-ios -c Release -p:ArchiveOnBuild=true

# Ad hoc distribution
dotnet publish -f net9.0-ios -c Release -p:CodesignProvision="AdHoc"
```

### 🔒 **Security Checklist**

- [ ] Change default admin password
- [ ] Enable HTTPS for all endpoints
- [ ] Configure proper file permissions
- [ ] Set up regular database backups
- [ ] Enable audit logging
- [ ] Test on target devices

### 🔊 **Monitoring & Analytics**

- **Performance Monitoring**: Built-in performance counters
- **Error Tracking**: Automatic crash reporting
- **Usage Analytics**: User interaction tracking
- **Health Checks**: Application availability monitoring

## 🔄 Version History

### v2.0.0 (Current) - Enhanced Mobile & Professional Features
- 🎆 **NEW**: Comprehensive README with detailed documentation
- 📱 **ENHANCED**: Full Android responsiveness and horizontal scrolling
- 🔧 **FIXED**: All Frame to Border migrations for .NET 9
- 🔄 **UPDATED**: Modern Application.MainPage usage patterns
- 🎨 **IMPROVED**: Professional UI/UX across all platforms
- 🛡️ **ENHANCED**: Enterprise-grade security features
- ⚡ **OPTIMIZED**: Performance improvements for mobile devices
- 📊 **NEW**: Advanced analytics and reporting capabilities
- 🔍 **IMPROVED**: Better error handling and user feedback
- 📋 **NEW**: Comprehensive troubleshooting guide

### v1.1.0 - Major UI/UX Update
- ✨ **NEW**: Modern Dashboard with real-time analytics
- 🎨 **NEW**: Professional blue branding (#1E3A8A)
- 📱 **IMPROVED**: Mobile-first responsive design
- 🛡️ **ENHANCED**: BCrypt password security
- 🧩 **IMPROVED**: Intuitive navigation with icons
- ⚡ **ENHANCED**: Quick action buttons
- 📊 **NEW**: Live activity feed
- 🐛 **FIXED**: All deprecated API usage
- 🔧 **IMPROVED**: Error handling and user feedback

### v1.0.0 - Initial Release
- 🎆 Initial release
- 👥 Basic guest management functionality
- 🌐 Multi-platform support
- 🔑 Basic authentication system

## 📞 **Support & Community**

### 📞 **Getting Help**
| Channel | Description | Response Time |
|---------|-------------|---------------|
| 🐛 **Issues** | Bug reports and feature requests | 24-48 hours |
| 💬 **Discussions** | Community Q&A and ideas | Community-driven |
| 📧 **Email** | Enterprise support | 24 hours |
| 📚 **Documentation** | Comprehensive guides | Always available |

### 🏆 **Recognition**
*Thank you to all contributors who have helped make this project better!*

### 🔮 **Roadmap**
- 🌍 **Multi-language Support**: Internationalization
- ☁️ **Cloud Sync**: Real-time data synchronization
- 📊 **Advanced Analytics**: BI dashboard integration
- 🔔 **Push Notifications**: Real-time alerts
- 🔌 **API Integration**: Third-party service connectivity

---

**© 2024 Jindal Guest Management System. All rights reserved.**
