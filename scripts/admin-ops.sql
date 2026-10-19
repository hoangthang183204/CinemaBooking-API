IF COL_LENGTH('Seats', 'IsBlocked') IS NULL
    ALTER TABLE Seats ADD IsBlocked bit NOT NULL CONSTRAINT DF_Seats_IsBlocked DEFAULT 0;
IF COL_LENGTH('Bookings', 'DiscountAmount') IS NULL
    ALTER TABLE Bookings ADD DiscountAmount decimal(18,2) NOT NULL CONSTRAINT DF_Bookings_Discount DEFAULT 0;
IF COL_LENGTH('Bookings', 'SurchargeAmount') IS NULL
    ALTER TABLE Bookings ADD SurchargeAmount decimal(18,2) NOT NULL CONSTRAINT DF_Bookings_Surcharge DEFAULT 0;
IF COL_LENGTH('Bookings', 'PromoCode') IS NULL
    ALTER TABLE Bookings ADD PromoCode nvarchar(40) NULL;
GO

IF OBJECT_ID('Promotions', 'U') IS NULL
BEGIN
    CREATE TABLE Promotions (
        Id int IDENTITY(1,1) NOT NULL PRIMARY KEY,
        Name nvarchar(120) NOT NULL,
        Code nvarchar(40) NULL,
        Kind nvarchar(20) NOT NULL,
        Value decimal(18,2) NOT NULL,
        Weekday int NULL,
        MinSubtotal decimal(18,2) NOT NULL,
        MaxDiscount decimal(18,2) NULL,
        UsageLimit int NULL,
        UsedCount int NOT NULL CONSTRAINT DF_Promotions_Used DEFAULT 0,
        StartsAt datetime2 NULL,
        EndsAt datetime2 NULL,
        AppliesTo nvarchar(20) NOT NULL,
        Note nvarchar(400) NULL,
        IsActive bit NOT NULL CONSTRAINT DF_Promotions_Active DEFAULT 1
    );
    CREATE INDEX IX_Promotions_Code ON Promotions(Code);
END
GO

IF OBJECT_ID('SystemSettings', 'U') IS NULL
BEGIN
    CREATE TABLE SystemSettings (
        Id int IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [Key] nvarchar(80) NOT NULL,
        Value nvarchar(200) NOT NULL,
        Label nvarchar(160) NULL
    );
    CREATE UNIQUE INDEX IX_SystemSettings_Key ON SystemSettings([Key]);
END
GO

IF NOT EXISTS (SELECT 1 FROM SystemSettings WHERE [Key] = N'SeatPrice')
    INSERT INTO SystemSettings ([Key], Value, Label) VALUES (N'SeatPrice', N'75000', N'Giá vé mỗi ghế (đ)');
IF NOT EXISTS (SELECT 1 FROM SystemSettings WHERE [Key] = N'OpenTime')
    INSERT INTO SystemSettings ([Key], Value, Label) VALUES (N'OpenTime', N'08:00', N'Giờ mở cửa');
IF NOT EXISTS (SELECT 1 FROM SystemSettings WHERE [Key] = N'CloseTime')
    INSERT INTO SystemSettings ([Key], Value, Label) VALUES (N'CloseTime', N'23:30', N'Giờ đóng cửa');
IF NOT EXISTS (SELECT 1 FROM SystemSettings WHERE [Key] = N'WeekendSurcharge')
    INSERT INTO SystemSettings ([Key], Value, Label) VALUES (N'WeekendSurcharge', N'0', N'Phụ thu cuối tuần mỗi ghế (đ)');
IF NOT EXISTS (SELECT 1 FROM SystemSettings WHERE [Key] = N'LateShowSurcharge')
    INSERT INTO SystemSettings ([Key], Value, Label) VALUES (N'LateShowSurcharge', N'0', N'Phụ thu suất tối mỗi ghế (đ)');
IF NOT EXISTS (SELECT 1 FROM SystemSettings WHERE [Key] = N'LateShowAfter')
    INSERT INTO SystemSettings ([Key], Value, Label) VALUES (N'LateShowAfter', N'21:00', N'Suất tối tính từ giờ');
IF NOT EXISTS (SELECT 1 FROM SystemSettings WHERE [Key] = N'LowStockAlert')
    INSERT INTO SystemSettings ([Key], Value, Label) VALUES (N'LowStockAlert', N'10', N'Cảnh báo bắp nước khi tồn dưới');
GO

IF NOT EXISTS (SELECT 1 FROM Promotions)
BEGIN
    INSERT INTO Promotions (Name, Code, Kind, Value, Weekday, MinSubtotal, MaxDiscount, UsageLimit, UsedCount, StartsAt, EndsAt, AppliesTo, Note, IsActive)
    VALUES
    (N'Thứ 3 vui vẻ', N'T3VUI', N'Weekday', 20, 2, 0, NULL, 1, 0, NULL, NULL, N'Ticket', N'Giảm 20% tiền ghế vào thứ Ba. Mỗi mã 1 lần.', 1),
    (N'Combo bắp nước', N'COMBO10', N'Percent', 10, NULL, 0, 20000, 1, 0, NULL, NULL, N'Concession', N'Giảm 10% bắp nước, tối đa 20.000đ. Mỗi mã 1 lần.', 1),
    (N'Mã sinh nhật', N'SINHNHAT', N'Fixed', 30000, NULL, 75000, 30000, 1, 0, NULL, NULL, N'All', N'Giảm 30.000đ cho đơn từ 75.000đ. Mỗi mã 1 lần.', 1);
END
GO
