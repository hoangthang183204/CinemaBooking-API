IF COL_LENGTH('Bookings', 'CustomerName') IS NULL
    ALTER TABLE Bookings ADD CustomerName nvarchar(120) NULL;
IF COL_LENGTH('Bookings', 'CustomerPhone') IS NULL
    ALTER TABLE Bookings ADD CustomerPhone nvarchar(30) NULL;
IF COL_LENGTH('Bookings', 'CustomerEmail') IS NULL
    ALTER TABLE Bookings ADD CustomerEmail nvarchar(160) NULL;
GO
IF COL_LENGTH('ElectronicTickets', 'CheckedInAt') IS NULL
    ALTER TABLE ElectronicTickets ADD CheckedInAt datetime2 NULL;
GO
IF COL_LENGTH('Payments', 'StaffId') IS NULL
    ALTER TABLE Payments ADD StaffId int NULL;
GO
IF OBJECT_ID('StaffShifts', 'U') IS NULL
BEGIN
    CREATE TABLE StaffShifts (
        Id int IDENTITY(1,1) NOT NULL PRIMARY KEY,
        StaffId int NOT NULL,
        StartedAt datetime2 NOT NULL,
        EndedAt datetime2 NULL
    );
END
GO
