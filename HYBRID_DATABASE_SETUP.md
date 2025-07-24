# Jindal Guest Management - Hybrid Database Setup

## Overview

The Jindal Guest Management System now supports a **hybrid database approach** that uses both **SQL Server** (primary) and **SQLite** (fallback) to ensure maximum reliability and availability.

### Key Features

- ✅ **Primary Database**: SQL Server for production use
- ✅ **Fallback Database**: SQLite for offline/local operation
- ✅ **Automatic Failover**: Seamlessly switches between databases
- ✅ **Real-time Monitoring**: Built-in connection testing and diagnostics
- ✅ **Easy Configuration**: Simple connection string setup

## Prerequisites

### SQL Server Setup

1. **Install SQL Server**:
   - SQL Server 2019 or later (Express edition is sufficient)
   - SQL Server Management Studio (SSMS) - recommended for management
   
2. **Enable Windows Authentication** (recommended):
   - The default configuration uses Windows Authentication
   - Ensure your Windows user has appropriate SQL Server permissions

3. **Alternative**: Use SQL Server Authentication:
   - Create a SQL Server login with appropriate permissions
   - Update the connection string accordingly

### Connection String Configuration

The default connection string in `appsettings.json`:

```json
{
  "DatabaseSettings": {
    "SqlServerConnectionString": "Server=localhost;Database=JindalGuestManagement;Integrated Security=true;TrustServerCertificate=true;ConnectRetryCount=3;ConnectRetryInterval=10;"
  }
}
```

#### Connection String Options

**For Windows Authentication (Default):**
```
Server=localhost;Database=JindalGuestManagement;Integrated Security=true;TrustServerCertificate=true;
```

**For SQL Server Authentication:**
```
Server=localhost;Database=JindalGuestManagement;User Id=your_username;Password=your_password;TrustServerCertificate=true;
```

**For Named Instance:**
```
Server=localhost\SQLEXPRESS;Database=JindalGuestManagement;Integrated Security=true;TrustServerCertificate=true;
```

**For Remote Server:**
```
Server=192.168.1.100;Database=JindalGuestManagement;Integrated Security=true;TrustServerCertificate=true;
```

## Setup Instructions

### Method 1: Automatic Setup (Recommended)

1. **Build and Run the Application**:
   ```bash
   dotnet build
   dotnet run --framework net9.0-windows10.0.19041.0
   ```

2. **Access Database Management**:
   - Navigate to the Database Management page in the app
   - Click "🚀 Initialize Hybrid Database"
   - The system will automatically create the database and tables

### Method 2: Manual Setup

1. **Run the SQL Script**:
   - Open SQL Server Management Studio
   - Connect to your SQL Server instance
   - Open `SqlScripts/SetupDatabase.sql`
   - Execute the script

2. **Verify Setup**:
   - Check that the `JindalGuestManagement` database was created
   - Verify that all tables (Users, Locations, Rooms, CheckInOut) exist

## Database Management Features

### Built-in Database Management Page

The application includes a comprehensive Database Management page with the following features:

#### 🔍 Connection Testing
- Test SQL Server connectivity
- Test SQLite availability
- Performance metrics and response times
- Detailed error reporting

#### 📊 Real-time Monitoring
- Connection status indicators
- Primary database identification
- Last connection check timestamps
- Database statistics and record counts

#### 🔧 Administrative Tools
- Initialize database system
- Update connection strings
- View detailed diagnostics
- Open SQL Server Management Studio

#### 🛠️ Troubleshooting Tools
- Connection diagnostics
- Server information display
- Permission verification
- Error code analysis

## How the Hybrid System Works

### Database Selection Logic

1. **Primary Check**: System attempts to connect to SQL Server
2. **Fallback**: If SQL Server unavailable, uses SQLite
3. **Automatic Recovery**: Periodically rechecks SQL Server availability
4. **Seamless Operation**: Application works regardless of database backend

### Data Synchronization

- **Write Operations**: Attempted on primary database first, fallback to SQLite
- **Read Operations**: Uses available database (prioritizes SQL Server)
- **Consistency**: Both databases maintain the same schema and data structure

### Connection Management

- **Connection Pooling**: Efficient connection reuse
- **Timeout Handling**: Configurable connection and command timeouts
- **Retry Logic**: Automatic retry on transient failures
- **Health Checks**: Regular availability monitoring

## Troubleshooting

### Common Issues

#### 1. "SQL Server not available"
**Possible Causes:**
- SQL Server service not running
- Incorrect server name/instance
- Firewall blocking connection
- Authentication failure

**Solutions:**
- Check SQL Server service: `services.msc` → SQL Server (instance)
- Verify server name: Use `localhost` or `.\SQLEXPRESS`
- Test with SSMS first
- Check Windows Authentication permissions

#### 2. "Database creation failed"
**Possible Causes:**
- Insufficient permissions
- Database already exists with conflicts
- Disk space issues

**Solutions:**
- Run as administrator
- Grant `sysadmin` role to user
- Check available disk space
- Drop existing database if needed

#### 3. "Connection timeout"
**Possible Causes:**
- Network latency
- Server overloaded
- Firewall restrictions

**Solutions:**
- Increase connection timeout in connection string
- Check network connectivity
- Verify SQL Server is accepting connections

### Getting Help

Use the built-in diagnostics:

1. Open Database Management page
2. Click "📋 View Connection Diagnostics"
3. Review detailed connection information
4. Check error codes and messages

### Connection String Testing

Test your connection string manually:
```bash
sqlcmd -S "your_server" -d "JindalGuestManagement" -E
```

## Performance Considerations

### SQL Server Optimization

- **Indexes**: Automatically created on frequently queried columns
- **Connection Pooling**: Reuses connections for better performance
- **Query Optimization**: Efficient queries with proper joins

### SQLite Optimization

- **WAL Mode**: Enabled for better concurrent access
- **Synchronous**: Optimized for mobile/desktop use
- **Cache Size**: Configured for optimal memory usage

## Security

### SQL Server Security

- **Windows Authentication**: Recommended for domain environments
- **SQL Authentication**: Use strong passwords
- **SSL/TLS**: `TrustServerCertificate=true` for development
- **Permissions**: Principle of least privilege

### SQLite Security

- **File Permissions**: Protected by OS file system
- **Encryption**: Consider SQLCipher for sensitive data
- **Access Control**: App-level security model

## Monitoring and Maintenance

### Health Checks

The system performs regular health checks:
- Connection availability every 2 minutes
- Automatic failover detection
- Performance metrics collection

### Logging

Comprehensive logging includes:
- Connection attempts and results
- Error conditions and recovery
- Performance metrics
- Database operation traces

### Backup Strategy

**SQL Server:**
- Use SQL Server backup features
- Schedule regular automated backups
- Consider point-in-time recovery

**SQLite:**
- File-based backup (copy database file)
- Application-level export features
- Regular backup to cloud storage

## Best Practices

1. **Test Connection Strings** before deployment
2. **Monitor Database Health** regularly
3. **Keep Backups Current** for both databases
4. **Update Connection Timeouts** based on network conditions
5. **Use Windows Authentication** when possible
6. **Monitor Disk Space** on database servers
7. **Test Failover Scenarios** in development

## Support

For technical support or issues:

1. Check the built-in diagnostics first
2. Review application logs
3. Test connection manually with SSMS
4. Verify SQL Server service status
5. Check network connectivity

## Version Information

- **Database Schema Version**: 1.0
- **Supported SQL Server**: 2016+
- **SQLite Version**: 3.x
- **Framework**: .NET 9 MAUI

---

**Note**: This hybrid database system ensures your guest management application continues to operate even when the primary SQL Server is temporarily unavailable, providing maximum reliability for your business operations.
