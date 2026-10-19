IF COL_LENGTH('Users', 'AvatarUrl') IS NULL
    ALTER TABLE Users ADD AvatarUrl nvarchar(500) NULL;
IF COL_LENGTH('Users', 'CitizenId') IS NULL
    ALTER TABLE Users ADD CitizenId nvarchar(20) NULL;
IF COL_LENGTH('Users', 'Position') IS NULL
    ALTER TABLE Users ADD Position nvarchar(40) NULL;
GO

UPDATE Users
SET Position = 'TicketSeller',
    CitizenId = '001204012345'
WHERE Id = 1;

UPDATE Users
SET Position = 'Admin',
    CitizenId = '079204009876'
WHERE Id = 2;
GO

DECLARE @hash nvarchar(200) = N'$2a$11$UHyv0cr.MyEbf/g3K766ZuZC/4xptnmMTSYAV1nBd6DMLQc2KV5Ie';

IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = 'quan@cinema.local')
    INSERT INTO Users (Email, PasswordHash, FullName, Phone, Role, IsActive, CreatedAt, CitizenId, Position)
    VALUES ('quan@cinema.local', @hash, N'Tran Minh Quan', '0903123456', 1, 1, SYSUTCDATETIME(), '001099001234', 'ShiftManager');

IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = 'mai@cinema.local')
    INSERT INTO Users (Email, PasswordHash, FullName, Phone, Role, IsActive, CreatedAt, CitizenId, Position)
    VALUES ('mai@cinema.local', @hash, N'Le Thi Mai', '0918234567', 1, 1, SYSUTCDATETIME(), '079301002345', 'TicketChecker');

IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = 'anh@cinema.local')
    INSERT INTO Users (Email, PasswordHash, FullName, Phone, Role, IsActive, CreatedAt, CitizenId, Position)
    VALUES ('anh@cinema.local', @hash, N'Pham Duc Anh', '0988123456', 1, 1, SYSUTCDATETIME(), '001204003456', 'TicketSeller');

IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = 'ha@cinema.local')
    INSERT INTO Users (Email, PasswordHash, FullName, Phone, Role, IsActive, CreatedAt, CitizenId, Position)
    VALUES ('ha@cinema.local', @hash, N'Do Thu Ha', '0977123456', 1, 0, SYSUTCDATETIME(), '038297004567', 'TicketChecker');
GO

DECLARE @quan int = (SELECT Id FROM Users WHERE Email = 'quan@cinema.local');
DECLARE @mai int = (SELECT Id FROM Users WHERE Email = 'mai@cinema.local');
DECLARE @anh int = (SELECT Id FROM Users WHERE Email = 'anh@cinema.local');
DECLARE @ha int = (SELECT Id FROM Users WHERE Email = 'ha@cinema.local');

IF NOT EXISTS (SELECT 1 FROM StaffShifts WHERE StaffId = @quan AND StartedAt = '2026-10-14T01:00:00')
    INSERT INTO StaffShifts (StaffId, StartedAt, EndedAt, ExpectedCash, ExpectedTransfer, Note, AwaitingHandover)
    VALUES
    (@quan, '2026-10-14T01:00:00', '2026-10-14T09:00:00', 1800000, 700000, N'Mau ca toi', 0),
    (@quan, '2026-10-15T01:00:00', '2026-10-15T09:30:00', 2100000, 450000, N'Mau ca toi', 0);

IF NOT EXISTS (SELECT 1 FROM StaffShifts WHERE StaffId = @mai AND StartedAt = '2026-10-15T06:00:00')
    INSERT INTO StaffShifts (StaffId, StartedAt, EndedAt, ExpectedCash, ExpectedTransfer, Note, AwaitingHandover)
    VALUES (@mai, '2026-10-15T06:00:00', '2026-10-15T12:00:00', 0, 960000, N'Mau soat ve', 0);

IF NOT EXISTS (SELECT 1 FROM StaffShifts WHERE StaffId = @anh AND StartedAt = '2026-10-16T01:30:00')
    INSERT INTO StaffShifts (StaffId, StartedAt, EndedAt, ExpectedCash, ExpectedTransfer, Note, AwaitingHandover)
    VALUES (@anh, '2026-10-16T01:30:00', '2026-10-16T08:00:00', 1500000, 320000, N'Mau ban ve', 0);

IF NOT EXISTS (SELECT 1 FROM StaffShifts WHERE StaffId = @ha AND StartedAt = '2026-10-10T02:00:00')
    INSERT INTO StaffShifts (StaffId, StartedAt, EndedAt, ExpectedCash, ExpectedTransfer, Note, AwaitingHandover)
    VALUES (@ha, '2026-10-10T02:00:00', '2026-10-10T09:00:00', 420000, 0, N'Mau ca cu', 0);
GO

SELECT Id, FullName, Phone, CitizenId, Position, Role, IsActive FROM Users WHERE Role IN (1, 2) ORDER BY Id;
