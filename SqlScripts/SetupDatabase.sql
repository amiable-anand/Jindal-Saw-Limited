-- Jindal Guest Management - Database Setup Script
-- This script creates the database and tables manually if needed

-- Create database if it doesn't exist
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'JindalGuestManagement')
BEGIN
    CREATE DATABASE JindalGuestManagement
    COLLATE SQL_Latin1_General_CP1_CI_AS;
    PRINT 'Database JindalGuestManagement created successfully.';
END
ELSE
BEGIN
    PRINT 'Database JindalGuestManagement already exists.';
END

-- Switch to the target database
USE JindalGuestManagement;

-- Create Users table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Users' AND xtype='U')
BEGIN
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
    PRINT 'Users table created.';
END

-- Create Locations table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Locations' AND xtype='U')
BEGIN
    CREATE TABLE Locations (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Name NVARCHAR(200) NOT NULL,
        LocationCode NVARCHAR(50) NOT NULL UNIQUE,
        Address NVARCHAR(500),
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );
    PRINT 'Locations table created.';
END

-- Create Rooms table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Rooms' AND xtype='U')
BEGIN
    CREATE TABLE Rooms (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        RoomNumber INT NOT NULL,
        Availability NVARCHAR(50) NOT NULL DEFAULT 'Available',
        LocationId INT NOT NULL,
        Remark NVARCHAR(500),
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        FOREIGN KEY (LocationId) REFERENCES Locations(Id)
    );
    PRINT 'Rooms table created.';
END

-- Create CheckInOut table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='CheckInOut' AND xtype='U')
BEGIN
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
    PRINT 'CheckInOut table created.';
END

-- Create indexes for better performance
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_Rooms_RoomNumber')
BEGIN
    CREATE INDEX IX_Rooms_RoomNumber ON Rooms(RoomNumber);
    PRINT 'Index IX_Rooms_RoomNumber created.';
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_CheckInOut_RoomNumber')
BEGIN
    CREATE INDEX IX_CheckInOut_RoomNumber ON CheckInOut(RoomNumber);
    PRINT 'Index IX_CheckInOut_RoomNumber created.';
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_CheckInOut_CheckInDate')
BEGIN
    CREATE INDEX IX_CheckInOut_CheckInDate ON CheckInOut(CheckInDate);
    PRINT 'Index IX_CheckInOut_CheckInDate created.';
END

-- Insert default locations if none exist
IF NOT EXISTS (SELECT * FROM Locations)
BEGIN
    INSERT INTO Locations (Name, LocationCode, Address) VALUES
    ('Main Building', 'MB', '123 Main St'),
    ('Guest House', 'GH', '456 Guest Ave'),
    ('Conference Center', 'CC', '789 Conference Blvd');
    PRINT 'Default locations inserted.';
END

-- Insert default rooms if none exist
IF NOT EXISTS (SELECT * FROM Rooms)
BEGIN
    INSERT INTO Rooms (RoomNumber, Availability, LocationId, Remark)
    SELECT 101, 'Available', Id, 'Standard room' FROM Locations WHERE LocationCode = 'MB' UNION ALL
    SELECT 102, 'Available', Id, 'Standard room' FROM Locations WHERE LocationCode = 'MB' UNION ALL
    SELECT 103, 'Available', Id, 'Deluxe room' FROM Locations WHERE LocationCode = 'MB' UNION ALL
    SELECT 201, 'Available', Id, 'Conference room' FROM Locations WHERE LocationCode = 'MB' UNION ALL
    SELECT 202, 'Available', Id, 'Meeting room' FROM Locations WHERE LocationCode = 'MB';
    PRINT 'Default rooms inserted.';
END

-- Insert default admin user if none exists
-- Note: In the application, the password will be hashed using BCrypt
-- For manual testing, you can use a pre-hashed password or let the app create the user
IF NOT EXISTS (SELECT * FROM Users WHERE Role = 1) -- 1 = Admin role
BEGIN
    -- This is a placeholder - the actual app will insert with proper BCrypt hash
    PRINT 'Default admin user will be created by the application on first run.';
    PRINT 'Username: admin';
    PRINT 'Password: JindalAdmin2024!@#';
END

-- Display summary
PRINT '';
PRINT '=== Database Setup Complete ===';
PRINT 'Database: JindalGuestManagement';
SELECT 
    'Users' as TableName, COUNT(*) as RecordCount FROM Users
UNION ALL
SELECT 
    'Locations' as TableName, COUNT(*) as RecordCount FROM Locations
UNION ALL
SELECT 
    'Rooms' as TableName, COUNT(*) as RecordCount FROM Rooms
UNION ALL
SELECT 
    'CheckInOut' as TableName, COUNT(*) as RecordCount FROM CheckInOut;

PRINT '';
PRINT 'You can now run the Jindal Guest Management application.';
PRINT 'The application will use this SQL Server database as the primary database.';
PRINT 'SQLite will be used as fallback if this server becomes unavailable.';
