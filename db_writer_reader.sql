-- =============================================================
-- SCRIPT PHÂN QUYỀN CQRS — CHẠY 1 LẦN DUY NHẤT
-- =============================================================

-- Bước 1: Tạo 2 Login ở cấp Server
USE [master]
GO
CREATE LOGIN [sbs_writer] WITH PASSWORD = N'Writer@123', DEFAULT_DATABASE = [SwimmingBookingDB];
CREATE LOGIN [sbs_reader] WITH PASSWORD = N'Reader@123', DEFAULT_DATABASE = [SwimmingBookingDB];
GO

-- Bước 2: Tạo User + gán quyền trong Database
USE [SwimmingBookingDB]
GO

-- Writer: đọc + ghi + quản lý schema (cho EF Migrations)
CREATE USER [sbs_writer] FOR LOGIN [sbs_writer];
ALTER ROLE [db_owner] ADD MEMBER [sbs_writer];
GO

-- Reader: chỉ đọc
CREATE USER [sbs_reader] FOR LOGIN [sbs_reader];
ALTER ROLE [db_datareader] ADD MEMBER [sbs_reader];
GO
