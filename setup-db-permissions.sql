-- =============================================================
-- BƯỚC 0: Tạo Database SwimmingBookingDB (nếu chưa có)
-- =============================================================
USE [master]
GO

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'SwimmingBookingDB')
BEGIN
    CREATE DATABASE [SwimmingBookingDB];
    PRINT 'Database SwimmingBookingDB created.'
END
ELSE
BEGIN
    PRINT 'Database SwimmingBookingDB already exists. Skip.'
END
GO

-- =============================================================
-- BƯỚC 1: Tạo 2 Login ở cấp Server (nếu chưa có)
-- =============================================================
USE [master]
GO

IF NOT EXISTS (SELECT name FROM sys.server_principals WHERE name = N'sbs_writer')
BEGIN
    CREATE LOGIN [sbs_writer] WITH PASSWORD = N'Writer@123', DEFAULT_DATABASE = [SwimmingBookingDB];
    PRINT 'Login sbs_writer created.'
END
ELSE
BEGIN
    PRINT 'Login sbs_writer already exists. Skip.'
END

IF NOT EXISTS (SELECT name FROM sys.server_principals WHERE name = N'sbs_reader')
BEGIN
    CREATE LOGIN [sbs_reader] WITH PASSWORD = N'Reader@123', DEFAULT_DATABASE = [SwimmingBookingDB];
    PRINT 'Login sbs_reader created.'
END
ELSE
BEGIN
    PRINT 'Login sbs_reader already exists. Skip.'
END
GO

-- =============================================================
-- BƯỚC 2: Tạo User + gán quyền trong Database
-- =============================================================
USE [SwimmingBookingDB]
GO

-- Writer: đọc + ghi + quản lý schema (cần thiết cho EF Migrations)
IF NOT EXISTS (SELECT name FROM sys.database_principals WHERE name = N'sbs_writer')
BEGIN
    CREATE USER [sbs_writer] FOR LOGIN [sbs_writer];
    ALTER ROLE [db_owner] ADD MEMBER [sbs_writer];
    PRINT 'User sbs_writer created and added to db_owner.'
END
ELSE
BEGIN
    PRINT 'User sbs_writer already exists. Skip.'
END

-- Reader: chỉ đọc
IF NOT EXISTS (SELECT name FROM sys.database_principals WHERE name = N'sbs_reader')
BEGIN
    CREATE USER [sbs_reader] FOR LOGIN [sbs_reader];
    ALTER ROLE [db_datareader] ADD MEMBER [sbs_reader];
    PRINT 'User sbs_reader created and added to db_datareader.'
END
ELSE
BEGIN
    PRINT 'User sbs_reader already exists. Skip.'
END
GO

PRINT '======================================='
PRINT 'Phân quyền CQRS hoàn thành!'
PRINT 'sbs_writer → db_owner  (read + write + migrations)'
PRINT 'sbs_reader → db_datareader (read-only)'
PRINT '======================================='
