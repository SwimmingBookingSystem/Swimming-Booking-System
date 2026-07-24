USE [master];
GO

IF DB_ID(N'SwimmingBookingDB') IS NULL
BEGIN
    CREATE DATABASE [SwimmingBookingDB];
    PRINT 'Database SwimmingBookingDB created.';
END
GO

-- Writer Login (Dynamic variable binding)
IF NOT EXISTS (SELECT name FROM sys.server_principals WHERE name = N'$(DB_WRITER_USER)')
BEGIN
    CREATE LOGIN [$(DB_WRITER_USER)] WITH PASSWORD = N'$(DB_WRITER_PASSWORD)', DEFAULT_DATABASE = [SwimmingBookingDB];
    PRINT 'Login writer created.';
END
ELSE
BEGIN
    ALTER LOGIN [$(DB_WRITER_USER)] WITH PASSWORD = N'$(DB_WRITER_PASSWORD)';
    PRINT 'Login writer password updated.';
END

-- Reader Login (Dynamic variable binding)
IF NOT EXISTS (SELECT name FROM sys.server_principals WHERE name = N'$(DB_READER_USER)')
BEGIN
    CREATE LOGIN [$(DB_READER_USER)] WITH PASSWORD = N'$(DB_READER_PASSWORD)', DEFAULT_DATABASE = [SwimmingBookingDB];
    PRINT 'Login reader created.';
END
ELSE
BEGIN
    ALTER LOGIN [$(DB_READER_USER)] WITH PASSWORD = N'$(DB_READER_PASSWORD)';
    PRINT 'Login reader password updated.';
END
GO

USE [SwimmingBookingDB];
GO

-- Writer User & Roles Idempotent (Least Privilege DML only)
IF NOT EXISTS (SELECT name FROM sys.database_principals WHERE name = N'$(DB_WRITER_USER)')
BEGIN
    CREATE USER [$(DB_WRITER_USER)] FOR LOGIN [$(DB_WRITER_USER)];
    PRINT 'User writer created.';
END

IF ISNULL(IS_ROLEMEMBER('db_datareader', N'$(DB_WRITER_USER)'), 0) = 0
    ALTER ROLE [db_datareader] ADD MEMBER [$(DB_WRITER_USER)];

IF ISNULL(IS_ROLEMEMBER('db_datawriter', N'$(DB_WRITER_USER)'), 0) = 0
    ALTER ROLE [db_datawriter] ADD MEMBER [$(DB_WRITER_USER)];

GRANT EXECUTE TO [$(DB_WRITER_USER)];

-- Reader User & Roles Idempotent (Read-Only)
IF NOT EXISTS (SELECT name FROM sys.database_principals WHERE name = N'$(DB_READER_USER)')
BEGIN
    CREATE USER [$(DB_READER_USER)] FOR LOGIN [$(DB_READER_USER)];
    PRINT 'User reader created.';
END

IF ISNULL(IS_ROLEMEMBER('db_datareader', N'$(DB_READER_USER)'), 0) = 0
    ALTER ROLE [db_datareader] ADD MEMBER [$(DB_READER_USER)];
GO

PRINT '======================================='
PRINT 'Phân quyền CQRS Idempotent hoàn thành!'
PRINT 'Writer → db_datareader + db_datawriter + EXECUTE'
PRINT 'Reader → db_datareader (read-only)'
PRINT '======================================='
