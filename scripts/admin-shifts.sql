IF COL_LENGTH('StaffShifts', 'ExpectedCash') IS NULL
    ALTER TABLE StaffShifts ADD ExpectedCash decimal(18,2) NULL;
IF COL_LENGTH('StaffShifts', 'ExpectedTransfer') IS NULL
    ALTER TABLE StaffShifts ADD ExpectedTransfer decimal(18,2) NULL;
IF COL_LENGTH('StaffShifts', 'CountedCash') IS NULL
    ALTER TABLE StaffShifts ADD CountedCash decimal(18,2) NULL;
IF COL_LENGTH('StaffShifts', 'Note') IS NULL
    ALTER TABLE StaffShifts ADD Note nvarchar(400) NULL;
IF COL_LENGTH('StaffShifts', 'ApprovedAt') IS NULL
    ALTER TABLE StaffShifts ADD ApprovedAt datetime2 NULL;
IF COL_LENGTH('StaffShifts', 'ApprovedById') IS NULL
    ALTER TABLE StaffShifts ADD ApprovedById int NULL;
IF COL_LENGTH('StaffShifts', 'AssignedById') IS NULL
    ALTER TABLE StaffShifts ADD AssignedById int NULL;
GO
