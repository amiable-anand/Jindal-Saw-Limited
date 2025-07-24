using Jindal.Models;
using Microsoft.Maui.Storage;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using LocationModel = Jindal.Models.Location;
using SQLite;
using System.Linq;

namespace Jindal.Services
{
    /// <summary>
    /// Hybrid database service that supports both SQL Server (primary) and SQLite (fallback).
    /// Provides seamless failover and ensures maximum reliability.
    /// </summary>
    public static class DatabaseService
    {
        // SQLite database (fallback)
        private static SQLiteAsyncConnection? _database;
        private const string DatabaseName = "JindalGuesthouse.db3";
        private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        
        // SQL Server connection (primary)
        private static string _sqlServerConnectionString = "Server=localhost;Database=JindalGuestManagement;Integrated Security=true;TrustServerCertificate=true;ConnectRetryCount=3;ConnectRetryInterval=10;";
        private static bool _sqlServerAvailable = false;
        private static DateTime _lastSqlServerCheck = DateTime.MinValue;
        private static readonly TimeSpan _sqlServerCheckInterval = TimeSpan.FromMinutes(2);
        
        // Configuration and logging (will be set by initialization)
        private static IConfiguration? _configuration;
        private static ILogger? _logger;

        /// <summary>
        /// Initialize configuration and logging from dependency injection
        /// </summary>
        public static void InitializeServices(IConfiguration? configuration = null, ILogger? logger = null)
        {
            _configuration = configuration;
            _logger = logger;
            
            if (_configuration != null)
            {
                var connectionString = _configuration.GetSection("DatabaseSettings:SqlServerConnectionString").Value;
                if (!string.IsNullOrEmpty(connectionString))
                {
                    _sqlServerConnectionString = connectionString;
                }
            }
        }

        public static async Task Init()
        {
            await _semaphore.WaitAsync();
            try
            {
                _logger?.LogInformation("Initializing hybrid database service...");
                
                // Always initialize SQLite as fallback
                await InitializeSqliteAsync();
                
                // Check SQL Server availability
                await CheckSqlServerAvailabilityAsync();
                
                if (_sqlServerAvailable)
                {
                    await InitializeSqlServerAsync();
                    _logger?.LogInformation("Hybrid database initialized with SQL Server as primary");
                }
                else
                {
                    _logger?.LogWarning("SQL Server not available, using SQLite only");
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }
        
        private static async Task InitializeSqliteAsync()
        {
            if (_database is not null)
                return;

            var databasePath = Path.Combine(FileSystem.AppDataDirectory, DatabaseName);
            _database = new SQLiteAsyncConnection(databasePath);

            await _database.CreateTableAsync<Room>();
            await _database.CreateTableAsync<CheckInOut>();
            await _database.CreateTableAsync<LocationModel>();
            await _database.CreateTableAsync<User>();

            await EnsureDefaultLocationsAsync();
            await EnsureDemoRoomsAsync();
            await EnsureDefaultUserAsync();
        }
        
        private static async Task CheckSqlServerAvailabilityAsync()
        {
            if (DateTime.Now - _lastSqlServerCheck < _sqlServerCheckInterval)
                return;

            _lastSqlServerCheck = DateTime.Now;

            try
            {
                using var connection = new SqlConnection(_sqlServerConnectionString);
                await connection.OpenAsync();
                await connection.CloseAsync();
                _sqlServerAvailable = true;
                _logger?.LogDebug("SQL Server connection test successful");
            }
            catch (Exception ex)
            {
                _sqlServerAvailable = false;
                _logger?.LogWarning(ex, "SQL Server connection test failed");
            }
        }
        
        private static async Task InitializeSqlServerAsync()
        {
            try
            {
                using var connection = new SqlConnection(_sqlServerConnectionString);
                await connection.OpenAsync();

                // Create database if it doesn't exist
                await CreateDatabaseIfNotExistsAsync();
                
                // Create tables
                await CreateSqlServerTablesAsync(connection);
                
                // Ensure default data
                await EnsureDefaultDataSqlServerAsync(connection);
                
                _logger?.LogInformation("SQL Server database initialized successfully");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to initialize SQL Server database");
                _sqlServerAvailable = false;
            }
        }

        
        


        private static async Task EnsureDefaultLocationsAsync()
        {
            if (_database == null)
                throw new InvalidOperationException("Database not initialized");
                
            var existingLocations = await _database.Table<LocationModel>().CountAsync();
            if (existingLocations == 0)
            {
                // Add some demo locations for testing
                await _database.InsertAllAsync(new[]
                {
                    new LocationModel { Name = "Main Building", LocationCode = "MB", Address = "123 Main St" },
                    new LocationModel { Name = "Guest House", LocationCode = "GH", Address = "456 Guest Ave" },
                    new LocationModel { Name = "Conference Center", LocationCode = "CC", Address = "789 Conference Blvd" }
                });
            }
        }

        private static async Task EnsureDemoRoomsAsync()
        {
            if (_database == null)
                throw new InvalidOperationException("Database not initialized");
                
            var existingRooms = await _database.Table<Room>().CountAsync();
            if (existingRooms == 0)
            {
                // Ensure we have locations first
                var locations = await _database.Table<LocationModel>().ToListAsync();
                if (locations.Any())
                {
                    var mainBuildingId = locations.First().Id;
                    
                    // Add some demo rooms for testing
                    await _database.InsertAllAsync(new[]
                    {
                        new Room { RoomNumber = 101, Availability = "Available", LocationId = mainBuildingId, Remark = "Standard room" },
                        new Room { RoomNumber = 102, Availability = "Available", LocationId = mainBuildingId, Remark = "Standard room" },
                        new Room { RoomNumber = 103, Availability = "Available", LocationId = mainBuildingId, Remark = "Deluxe room" },
                        new Room { RoomNumber = 201, Availability = "Available", LocationId = mainBuildingId, Remark = "Conference room" },
                        new Room { RoomNumber = 202, Availability = "Available", LocationId = mainBuildingId, Remark = "Meeting room" }
                    });
                }
            }
        }

        private static async Task EnsureDefaultUserAsync()
        {
            if (_database == null)
                throw new InvalidOperationException("Database not initialized");
                
            var existingUser = await _database.Table<User>()
                .Where(u => u.Role == UserRole.Admin)
                .FirstOrDefaultAsync();

            if (existingUser == null)
            {
                var adminUser = new User
                {
                    Username = "admin",
                    Password = BCrypt.Net.BCrypt.HashPassword("JindalAdmin2024!@#"),
                    Role = UserRole.Admin,
                    FullName = "System Administrator",
                    Email = "admin@jindal.com",
                    Permissions = (int)Permission.All,
                    IsActive = true
                };

                await _database.InsertAsync(adminUser);
            }
        }


        // Room
        public static async Task<List<Room>> GetRooms()
        {
            await Init();
            if (_database == null)
                throw new InvalidOperationException("Database not initialized");
                
            var rooms = await _database.Table<Room>().ToListAsync();
            var locations = await GetLocations();

            foreach (var room in rooms)
            {
                room.LocationName = locations.FirstOrDefault(l => l.Id == room.LocationId)?.Name ?? "";
            }

            return rooms;
        }

        public static async Task AddRoom(Room room) 
        {
            await Init();
            if (_database == null)
                throw new InvalidOperationException("Database not initialized");
                
            await _database.InsertAsync(room);
        }
        
        public static async Task UpdateRoom(Room room) 
        {
            await Init();
            if (_database == null)
                throw new InvalidOperationException("Database not initialized");
                
            await _database.UpdateAsync(room);
        }
        
        public static async Task DeleteRoom(Room room) 
        {
            await Init();
            if (_database == null)
                throw new InvalidOperationException("Database not initialized");
            await _database.DeleteAsync(room);
        }

        public static async Task<List<Room>> GetAvailableRooms()
        {
            return await GetAvailableRoomsWithLogic();
        }

        public static async Task<List<Room>> GetCompletelyAvailableRooms() => await GetAvailableRooms();

        // CheckInOut
        public static async Task<List<CheckInOut>> GetCheckInOuts()
        {
            await Init();
            if (_database == null)
                throw new InvalidOperationException("Database not initialized");
            return await _database.Table<CheckInOut>().ToListAsync();
        }
        
        /// <summary>
        /// Get only active guests (those who haven't checked out yet)
        /// </summary>
        public static async Task<List<CheckInOut>> GetActiveGuests()
        {
            await Init();
            if (_database == null)
                throw new InvalidOperationException("Database not initialized");
            return await _database.Table<CheckInOut>()
                .Where(c => c.CheckOutDate == null && c.CheckOutTime == null)
                .ToListAsync();
        }
        
        /// <summary>
        /// Get only checked out guests for reports
        /// </summary>
        public static async Task<List<CheckInOut>> GetCheckedOutGuests()
        {
            await Init();
            if (_database == null)
                throw new InvalidOperationException("Database not initialized");
            return await _database.Table<CheckInOut>()
                .Where(c => c.CheckOutDate != null && c.CheckOutTime != null)
                .ToListAsync();
        }

        public static async Task<CheckInOut?> GetCheckInOutById(int id)
        {
            await Init();
            return await _database!.Table<CheckInOut>().FirstOrDefaultAsync(c => c.Id == id);
        }

        public static async Task<List<CheckInOut>> GetCheckInOutsByRoomNumber(int roomNumber)
        {
            await Init();
            return await _database!.Table<CheckInOut>()
                .Where(c => c.RoomNumber == roomNumber)
                .ToListAsync();
        }

        public static async Task AddCheckInOut(CheckInOut check)
        {
            await Init();
            if (_database == null)
                throw new InvalidOperationException("Database not initialized");
                
            await _database.InsertAsync(check);
        }
        
        public static async Task UpdateCheckInOut(CheckInOut check)
        {
            await Init();
            if (_database == null)
                throw new InvalidOperationException("Database not initialized");
                
            await _database.UpdateAsync(check);
        }
        
        public static async Task DeleteCheckInOut(CheckInOut check)
        {
            await Init();
            if (_database == null)
                throw new InvalidOperationException("Database not initialized");
                
            await _database.DeleteAsync(check);
        }

        // Location
        public static async Task<List<LocationModel>> GetLocations()
        {
            await Init();
            return await _database!.Table<LocationModel>().ToListAsync();
        }

        public static async Task<List<LocationModel>> SearchLocations(string query)
        {
            await Init();
            return await _database!.Table<LocationModel>()
                .Where(l => l.Name.Contains(query) || l.LocationCode.Contains(query))
                .ToListAsync();
        }

        public static async Task AddLocation(LocationModel loc)
        {
            await Init();
            if (_database == null)
                throw new InvalidOperationException("Database not initialized");
                
            await _database.InsertAsync(loc);
        }
        
        public static async Task UpdateLocation(LocationModel loc)
        {
            await Init();
            if (_database == null)
                throw new InvalidOperationException("Database not initialized");
                
            await _database.UpdateAsync(loc);
        }
        public static async Task DeleteLocation(LocationModel loc)
        {
            await Init();
            if (_database == null)
                throw new InvalidOperationException("Database not initialized");
                
            await _database.DeleteAsync(loc);
        }
        
        // Test database connectivity
        public static async Task<bool> TestDatabaseConnection()
        {
            try
            {
                await Init();
                // Try a simple query to test connectivity
                await _database!.Table<User>().CountAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
        
        // User Management Methods
        public static async Task<User?> AuthenticateUser(string username, string password)
        {
            try
            {
                await Init();
                
                var user = await _database!.Table<User>()
                    .Where(u => u.Username == username && u.IsActive)
                    .FirstOrDefaultAsync();

                if (user != null && BCrypt.Net.BCrypt.Verify(password, user.Password))
                {
                    user.LastLoginAt = DateTime.Now;
                    await _database.UpdateAsync(user);
                    return user;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Authentication error: {ex.Message}");
            }
            
            return null;
        }

        public static async Task<List<User>> GetAllUsers()
        {
            await Init();
            return await _database!.Table<User>().ToListAsync();
        }

        public static async Task<User?> GetUserById(int id)
        {
            await Init();
            return await _database!.Table<User>()
                .Where(u => u.Id == id)
                .FirstOrDefaultAsync();
        }

        public static async Task<bool> CreateUser(User user)
        {
            try
            {
                await Init();
                
                // Hash password before saving
                user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
                await _database!.InsertAsync(user);
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Create user error: {ex.Message}");
                return false;
            }
        }

        public static async Task<bool> UpdateUser(User user)
        {
            try
            {
                await Init();
                await _database!.UpdateAsync(user);
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Update user error: {ex.Message}");
                return false;
            }
        }

        public static async Task<bool> DeleteUser(int userId)
        {
            try
            {
                await Init();
                var user = await GetUserById(userId);
                if (user != null)
                {
                    await _database!.DeleteAsync(user);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Delete user error: {ex.Message}");
                return false;
            }
        }

        public static async Task<bool> ChangePassword(int userId, string oldPassword, string newPassword)
        {
            try
            {
                await Init();
                var user = await GetUserById(userId);
                if (user != null && BCrypt.Net.BCrypt.Verify(oldPassword, user.Password))
                {
                    user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
                    await _database!.UpdateAsync(user);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Change password error: {ex.Message}");
                return false;
            }
        }

        // Room Availability Management Methods
        public static async Task<List<Room>> GetAvailableRoomsWithLogic()
        {
            try
            {
                await Init();
                
                // Get all rooms
                var allRooms = await GetRooms();
                
                // Get active guests (not checked out)
                var activeGuests = await GetActiveGuests();
                
                // Get occupied room numbers
                var occupiedRoomNumbers = activeGuests
                    .Where(g => g.RoomNumber > 0)
                    .Select(g => g.RoomNumber)
                    .ToHashSet();
                
                // Filter available rooms
                var availableRooms = allRooms
                    .Where(r => !occupiedRoomNumbers.Contains(r.RoomNumber))
                    .ToList();
                
                // Update room availability status in database
                await UpdateRoomAvailabilityStatus(allRooms, occupiedRoomNumbers);
                
                return availableRooms;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to get available rooms: {ex.Message}");
                throw;
            }
        }

        public static async Task<List<(Room Room, List<CheckInOut> Guests)>> GetOccupiedRoomsWithGuests()
        {
            try
            {
                await Init();
                
                // Get all rooms
                var allRooms = await GetRooms();
                
                // Get active guests (not checked out)
                var activeGuests = await GetActiveGuests();
                
                // Group guests by room
                var guestsByRoom = activeGuests
                    .Where(g => g.RoomNumber > 0)
                    .GroupBy(g => g.RoomNumber)
                    .ToDictionary(g => g.Key, g => g.ToList());
                
                // Get occupied rooms with guests
                var occupiedRooms = new List<(Room Room, List<CheckInOut> Guests)>();
                
                foreach (var room in allRooms)
                {
                    if (guestsByRoom.TryGetValue(room.RoomNumber, out var guests))
                    {
                        occupiedRooms.Add((room, guests));
                    }
                }
                
                return occupiedRooms;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to get occupied rooms with guests: {ex.Message}");
                throw;
            }
        }

        public static async Task<bool> IsRoomAvailableByNumber(int roomNumber)
        {
            try
            {
                await Init();
                
                // Check if there are any active guests in this room
                var activeGuests = await GetActiveGuests();
                var hasActiveGuests = activeGuests.Any(g => g.RoomNumber == roomNumber);
                
                return !hasActiveGuests;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to check room availability for room {roomNumber}: {ex.Message}");
                throw;
            }
        }


        public static async Task<RoomUtilizationStats> GetRoomUtilizationStats()
        {
            try
            {
                await Init();
                var allRooms = await GetRooms();
                var activeGuests = await GetActiveGuests();
                
                var occupiedRoomNumbers = activeGuests
                    .Where(g => g.RoomNumber > 0)
                    .Select(g => g.RoomNumber)
                    .ToHashSet();
                
                var totalRooms = allRooms.Count;
                var occupiedRooms = allRooms.Count(r => occupiedRoomNumbers.Contains(r.RoomNumber));
                var availableRooms = totalRooms - occupiedRooms;
                var totalActiveGuests = activeGuests.Count;
                
                return new RoomUtilizationStats
                {
                    TotalRooms = totalRooms,
                    AvailableRooms = availableRooms,
                    OccupiedRooms = occupiedRooms,
                    TotalActiveGuests = totalActiveGuests,
                    UtilizationPercentage = totalRooms > 0 ? (double)occupiedRooms / totalRooms * 100 : 0
                };
            }
            catch (Exception ex)
            {
                // Simple logging to debug console instead of complex error handling service
                System.Diagnostics.Debug.WriteLine($"Failed to get room utilization stats: {ex.Message}");
                throw;
            }
        }

        public static async Task UpdateRoomAvailabilityStatus(List<Room>? allRooms = null, HashSet<int>? occupiedRoomNumbers = null)
        {
            try
            {
                await Init();
                
                // Get data if not provided
                if (allRooms == null)
                    allRooms = await GetRooms();
                
                if (occupiedRoomNumbers == null)
                {
                    var activeGuests = await GetActiveGuests();
                    occupiedRoomNumbers = activeGuests
                        .Where(g => g.RoomNumber > 0)
                        .Select(g => g.RoomNumber)
                        .ToHashSet();
                }
                
                // Update room availability status
                var roomsToUpdate = new List<Room>();
                
                foreach (var room in allRooms)
                {
                    var shouldBeOccupied = occupiedRoomNumbers.Contains(room.RoomNumber);
                    var currentlyAvailable = room.Availability == "Available";
                    
                    // Update if status doesn't match reality
                    if (shouldBeOccupied && currentlyAvailable)
                    {
                        room.Availability = "Booked";
                        roomsToUpdate.Add(room);
                    }
                    else if (!shouldBeOccupied && !currentlyAvailable)
                    {
                        room.Availability = "Available";
                        roomsToUpdate.Add(room);
                    }
                }
                
                // Update changed rooms in database
                if (roomsToUpdate.Any())
                {
                    foreach (var room in roomsToUpdate)
                    {
                        await UpdateRoom(room);
                    }
                    
                    System.Diagnostics.Debug.WriteLine($"Updated availability status for {roomsToUpdate.Count} rooms");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to update room availability status: {ex.Message}");
                throw;
            }
        }

        // Get database statistics for debugging
        public static async Task<string> GetDatabaseStats()
        {
            try
            {
                await Init();
                var roomCount = await _database!.Table<Room>().CountAsync();
                var locationCount = await _database!.Table<LocationModel>().CountAsync();
                var guestCount = await _database!.Table<CheckInOut>().CountAsync();
                var userCount = await _database!.Table<User>().CountAsync();
                
                return $"Rooms: {roomCount}, Locations: {locationCount}, Guests: {guestCount}, Users: {userCount}";
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }

        // Additional methods needed for EnterpriseAnalyticsPage compatibility
        public static async Task<List<User>> GetUsers()
        {
            return await GetAllUsers();
        }

        public static async Task PerformPendingSync()
        {
            // For simplified version, this is a no-op since we don't have API sync
            // In the original enterprise version, this would sync with remote API
            await Task.CompletedTask;
        }
        
        // SQL Server Helper Methods
        private static async Task CreateDatabaseIfNotExistsAsync()
        {
            var masterConnectionString = _sqlServerConnectionString.Replace("Database=JindalGuestManagement", "Database=master");
            using var masterConnection = new SqlConnection(masterConnectionString);
            await masterConnection.OpenAsync();

            var checkDbSql = "SELECT COUNT(*) FROM sys.databases WHERE name = 'JindalGuestManagement'";
            using var checkCmd = new SqlCommand(checkDbSql, masterConnection);
            var result = await checkCmd.ExecuteScalarAsync();
            var dbExists = result != null && (int)result > 0;

            if (!dbExists)
            {
                var createDbSql = @"
                    CREATE DATABASE JindalGuestManagement
                    COLLATE SQL_Latin1_General_CP1_CI_AS;";
                using var createCmd = new SqlCommand(createDbSql, masterConnection);
                await createCmd.ExecuteNonQueryAsync();
                _logger?.LogInformation("Created JindalGuestManagement database");
            }
        }
        
        private static async Task CreateSqlServerTablesAsync(SqlConnection connection)
        {
            var createTablesSql = @"
                -- Users Table
                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Users' AND xtype='U')
                CREATE TABLE Users (
                    Id INT IDENTITY(1,1) PRIMARY KEY,
                    Username NVARCHAR(100) NOT NULL UNIQUE,
                    Password NVARCHAR(255) NOT NULL,
                    Role INT NOT NULL,
                    FullName NVARCHAR(200) NOT NULL,
                    Email NVARCHAR(255),
                    Permissions INT NOT NULL DEFAULT 0,
                    IsActive BIT NOT NULL DEFAULT 1,
                    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
                    LastLoginAt DATETIME2 NULL
                );

                -- Locations Table
                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Locations' AND xtype='U')
                CREATE TABLE Locations (
                    Id INT IDENTITY(1,1) PRIMARY KEY,
                    Name NVARCHAR(200) NOT NULL,
                    LocationCode NVARCHAR(50) NOT NULL UNIQUE,
                    Address NVARCHAR(500),
                    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
                );

                -- Rooms Table
                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Rooms' AND xtype='U')
                CREATE TABLE Rooms (
                    Id INT IDENTITY(1,1) PRIMARY KEY,
                    RoomNumber INT NOT NULL,
                    Availability NVARCHAR(50) NOT NULL DEFAULT 'Available',
                    LocationId INT NOT NULL,
                    Remark NVARCHAR(500),
                    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
                    FOREIGN KEY (LocationId) REFERENCES Locations(Id)
                );

                -- CheckInOut Table
                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='CheckInOut' AND xtype='U')
                CREATE TABLE CheckInOut (
                    Id INT IDENTITY(1,1) PRIMARY KEY,
                    GuestName NVARCHAR(200) NOT NULL,
                    ContactNumber NVARCHAR(20),
                    Email NVARCHAR(255),
                    CheckInDate DATE NOT NULL,
                    CheckInTime TIME NOT NULL,
                    CheckOutDate DATE NULL,
                    CheckOutTime TIME NULL,
                    RoomNumber INT NOT NULL,
                    Purpose NVARCHAR(500),
                    Address NVARCHAR(500),
                    CompanyOrganization NVARCHAR(200),
                    IdProofType NVARCHAR(50),
                    IdProofNumber NVARCHAR(100),
                    NumberOfGuests INT NOT NULL DEFAULT 1,
                    AdditionalGuestNames NVARCHAR(MAX),
                    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
                    LastModifiedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
                );

                -- Create indexes for better performance
                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_Rooms_RoomNumber')
                CREATE INDEX IX_Rooms_RoomNumber ON Rooms(RoomNumber);

                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_CheckInOut_RoomNumber')
                CREATE INDEX IX_CheckInOut_RoomNumber ON CheckInOut(RoomNumber);

                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_CheckInOut_CheckInDate')
                CREATE INDEX IX_CheckInOut_CheckInDate ON CheckInOut(CheckInDate);
            ";

            using var command = new SqlCommand(createTablesSql, connection);
            await command.ExecuteNonQueryAsync();
        }
        
        private static async Task EnsureDefaultDataSqlServerAsync(SqlConnection connection)
        {
            // Check and insert default locations
            var locationCheckSql = "SELECT COUNT(*) FROM Locations";
            using var locationCheckCmd = new SqlCommand(locationCheckSql, connection);
            var locationResult = await locationCheckCmd.ExecuteScalarAsync();
            var locationCount = locationResult != null ? (int)locationResult : 0;

            if (locationCount == 0)
            {
                var insertLocationsSql = @"
                    INSERT INTO Locations (Name, LocationCode, Address) VALUES
                    ('Main Building', 'MB', '123 Main St'),
                    ('Guest House', 'GH', '456 Guest Ave'),
                    ('Conference Center', 'CC', '789 Conference Blvd')";
                using var insertLocationsCmd = new SqlCommand(insertLocationsSql, connection);
                await insertLocationsCmd.ExecuteNonQueryAsync();
            }

            // Check and insert default rooms
            var roomCheckSql = "SELECT COUNT(*) FROM Rooms";
            using var roomCheckCmd = new SqlCommand(roomCheckSql, connection);
            var roomResult = await roomCheckCmd.ExecuteScalarAsync();
            var roomCount = roomResult != null ? (int)roomResult : 0;

            if (roomCount == 0)
            {
                var insertRoomsSql = @"
                    INSERT INTO Rooms (RoomNumber, Availability, LocationId, Remark)
                    SELECT 101, 'Available', Id, 'Standard room' FROM Locations WHERE LocationCode = 'MB' UNION ALL
                    SELECT 102, 'Available', Id, 'Standard room' FROM Locations WHERE LocationCode = 'MB' UNION ALL
                    SELECT 103, 'Available', Id, 'Deluxe room' FROM Locations WHERE LocationCode = 'MB' UNION ALL
                    SELECT 201, 'Available', Id, 'Conference room' FROM Locations WHERE LocationCode = 'MB' UNION ALL
                    SELECT 202, 'Available', Id, 'Meeting room' FROM Locations WHERE LocationCode = 'MB'";
                using var insertRoomsCmd = new SqlCommand(insertRoomsSql, connection);
                await insertRoomsCmd.ExecuteNonQueryAsync();
            }

            // Check and insert default admin user
            var userCheckSql = "SELECT COUNT(*) FROM Users WHERE Role = @role";
            using var userCheckCmd = new SqlCommand(userCheckSql, connection);
            userCheckCmd.Parameters.AddWithValue("@role", (int)UserRole.Admin);
            var userResult = await userCheckCmd.ExecuteScalarAsync();
            var userCount = userResult != null ? (int)userResult : 0;

            if (userCount == 0)
            {
                var insertUserSql = @"
                    INSERT INTO Users (Username, Password, Role, FullName, Email, Permissions, IsActive)
                    VALUES (@username, @password, @role, @fullName, @email, @permissions, @isActive)";
                using var insertUserCmd = new SqlCommand(insertUserSql, connection);
                insertUserCmd.Parameters.AddWithValue("@username", "admin");
                insertUserCmd.Parameters.AddWithValue("@password", BCrypt.Net.BCrypt.HashPassword("JindalAdmin2024!@#"));
                insertUserCmd.Parameters.AddWithValue("@role", (int)UserRole.Admin);
                insertUserCmd.Parameters.AddWithValue("@fullName", "System Administrator");
                insertUserCmd.Parameters.AddWithValue("@email", "admin@jindal.com");
                insertUserCmd.Parameters.AddWithValue("@permissions", (int)Permission.All);
                insertUserCmd.Parameters.AddWithValue("@isActive", true);
                await insertUserCmd.ExecuteNonQueryAsync();
            }
        }
        
        // Public methods for database management
        public static async Task<bool> TestSqlServerConnectionAsync()
        {
            await CheckSqlServerAvailabilityAsync();
            return _sqlServerAvailable;
        }
        
        public static DatabaseStatus GetDatabaseStatus()
        {
            return new DatabaseStatus
            {
                SqlServerAvailable = _sqlServerAvailable,
                SqliteAvailable = _database != null,
                PrimaryDatabase = _sqlServerAvailable ? "SQL Server" : "SQLite",
                LastSqlServerCheck = _lastSqlServerCheck,
                ConnectionString = _sqlServerAvailable ? "SQL Server Connected" : "SQLite Only"
            };
        }
    }

    /// <summary>
    /// Represents room utilization statistics
    /// </summary>
    public class RoomUtilizationStats
    {
        public int TotalRooms { get; set; }
        public int AvailableRooms { get; set; }
        public int OccupiedRooms { get; set; }
        public int TotalActiveGuests { get; set; }
        public double UtilizationPercentage { get; set; }
    }
    
    /// <summary>
    /// Represents the current database connection status
    /// </summary>
    public class DatabaseStatus
    {
        public bool SqlServerAvailable { get; set; }
        public bool SqliteAvailable { get; set; }
        public string PrimaryDatabase { get; set; } = "";
        public DateTime LastSqlServerCheck { get; set; }
        public string ConnectionString { get; set; } = "";
    }

}
