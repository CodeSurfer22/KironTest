USE [KironTest]
GO
/****** Object:  Table [dbo].[Holidays]    Script Date: 17/09/2024 18:13:28 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Holidays](
	[HolidayId] [int] IDENTITY(1,1) NOT NULL,
	[Title] [nvarchar](255) NOT NULL,
	[HolidayDate] [date] NOT NULL,
	[Notes] [nvarchar](max) NULL,
	[Bunting] [bit] NULL,
PRIMARY KEY CLUSTERED 
(
	[HolidayId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Navigation]    Script Date: 17/09/2024 18:13:28 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Navigation](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Text] [varchar](50) NOT NULL,
	[ParentID] [int] NOT NULL,
 CONSTRAINT [PK_Navigation] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RegionHolidays]    Script Date: 17/09/2024 18:13:28 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RegionHolidays](
	[RegionHolidayId] [int] IDENTITY(1,1) NOT NULL,
	[HolidayId] [int] NOT NULL,
	[RegionId] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[RegionHolidayId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Regions]    Script Date: 17/09/2024 18:13:28 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Regions](
	[RegionId] [int] IDENTITY(1,1) NOT NULL,
	[RegionName] [nvarchar](100) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[RegionId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Users]    Script Date: 17/09/2024 18:13:28 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Users](
	[UserId] [int] IDENTITY(1,1) NOT NULL,
	[Username] [nvarchar](100) NOT NULL,
	[PasswordHash] [varbinary](64) NOT NULL,
	[Salt] [varbinary](64) NOT NULL,
	[CreatedDate] [datetime] NULL,
 CONSTRAINT [PK_Users] PRIMARY KEY CLUSTERED 
(
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
 CONSTRAINT [UQ_Users_Username] UNIQUE NONCLUSTERED 
(
	[Username] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[Users] ADD  DEFAULT (getdate()) FOR [CreatedDate]
GO
ALTER TABLE [dbo].[RegionHolidays]  WITH CHECK ADD FOREIGN KEY([HolidayId])
REFERENCES [dbo].[Holidays] ([HolidayId])
GO
ALTER TABLE [dbo].[RegionHolidays]  WITH CHECK ADD FOREIGN KEY([RegionId])
REFERENCES [dbo].[Regions] ([RegionId])
GO
/****** Object:  StoredProcedure [dbo].[GetAllRegions]    Script Date: 17/09/2024 18:13:28 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetAllRegions]
AS
BEGIN
    BEGIN TRANSACTION;
    
    BEGIN TRY
        SELECT 
            RegionId,
            RegionName
        FROM 
            Regions;
        
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        -- You can add error handling here
        THROW;
    END CATCH
END;
GO
/****** Object:  StoredProcedure [dbo].[GetHolidayById]    Script Date: 17/09/2024 18:13:28 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetHolidayById]
    @HolidayId INT
AS
BEGIN
    BEGIN TRANSACTION;
    
    BEGIN TRY
        SELECT 
            HolidayId,
            Title,
            HolidayDate,
            Notes,
            Bunting
        FROM 
            Holidays
        WHERE 
            HolidayId = @HolidayId;
        
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        -- You can add error handling here
        THROW;
    END CATCH
END;
GO
/****** Object:  StoredProcedure [dbo].[GetHolidaysByRegion]    Script Date: 17/09/2024 18:13:28 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetHolidaysByRegion]
    @RegionId INT
AS
BEGIN
    BEGIN TRANSACTION;
    
    BEGIN TRY
        SELECT 
            h.HolidayId,
            h.Title,
            h.HolidayDate,
            h.Notes,
            h.Bunting
        FROM 
            Holidays h
        INNER JOIN 
            RegionHolidays rh ON h.HolidayId = rh.HolidayId
        WHERE 
            rh.RegionId = @RegionId;
        
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        -- You can add error handling here
        THROW;
    END CATCH
END;
GO
/****** Object:  StoredProcedure [dbo].[GetRegionById]    Script Date: 17/09/2024 18:13:28 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetRegionById]
    @RegionId INT
AS
BEGIN
    BEGIN TRANSACTION;
    
    BEGIN TRY
        SELECT 
            RegionId,
            RegionName
        FROM 
            Regions
        WHERE 
            RegionId = @RegionId;
        
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        -- You can add error handling here
        THROW;
    END CATCH
END;
GO
/****** Object:  StoredProcedure [dbo].[GetUserByUsername]    Script Date: 17/09/2024 18:13:28 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetUserByUsername]
    @Username NVARCHAR(100)
AS
BEGIN
    SELECT TOP 1 UserId, Username, PasswordHash, Salt, CreatedDate
    FROM Users
    WHERE Username = @Username;
END;
GO
/****** Object:  StoredProcedure [dbo].[InsertHoliday]    Script Date: 17/09/2024 18:13:28 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[InsertHoliday]
    @Title NVARCHAR(255),
    @HolidayDate DATE,
    @Notes NVARCHAR(MAX) = NULL,
    @Bunting BIT = NULL
AS
BEGIN
    BEGIN TRANSACTION;
    
    BEGIN TRY
        INSERT INTO Holidays (Title, HolidayDate, Notes, Bunting)
        VALUES (@Title, @HolidayDate, @Notes, @Bunting);
        
        -- Return the new HolidayId
        SELECT SCOPE_IDENTITY();
        
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO
/****** Object:  StoredProcedure [dbo].[InsertRegion]    Script Date: 17/09/2024 18:13:28 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[InsertRegion]
    @RegionName NVARCHAR(100)
AS
BEGIN
    BEGIN TRANSACTION;
    
    BEGIN TRY
        INSERT INTO Regions (RegionName)
        VALUES (@RegionName);
        
        -- Return the new RegionId
        SELECT SCOPE_IDENTITY();
        
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        -- You can add error handling here
        THROW;
    END CATCH
END;
GO
/****** Object:  StoredProcedure [dbo].[InsertRegionHoliday]    Script Date: 17/09/2024 18:13:28 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[InsertRegionHoliday]
    @HolidayId INT,
    @RegionId INT
AS
BEGIN
    BEGIN TRANSACTION;

    BEGIN TRY
        INSERT INTO RegionHolidays (HolidayId, RegionId)
        VALUES (@HolidayId, @RegionId);

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO
/****** Object:  StoredProcedure [dbo].[InsertUser]    Script Date: 17/09/2024 18:13:28 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[InsertUser]
    @Username NVARCHAR(100),
    @PasswordHash VARBINARY(64),
    @Salt VARBINARY(64)
AS
BEGIN
    BEGIN TRANSACTION;
    BEGIN TRY
        INSERT INTO Users (Username, PasswordHash, Salt)
        VALUES (@Username, @PasswordHash, @Salt);
        
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH;
END;
GO
/****** Object:  StoredProcedure [dbo].[UpdateHoliday]    Script Date: 17/09/2024 18:13:28 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[UpdateHoliday]
    @HolidayId INT,
    @Title NVARCHAR(255),
    @HolidayDate DATE,
    @Notes NVARCHAR(MAX) = NULL,
    @Bunting BIT = NULL
AS
BEGIN
    BEGIN TRANSACTION;
    
    BEGIN TRY
        UPDATE Holidays
        SET 
            Title = @Title,
            HolidayDate = @HolidayDate,
            Notes = @Notes,
            Bunting = @Bunting
        WHERE 
            HolidayId = @HolidayId;
        
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        -- You can add error handling here
        THROW;
    END CATCH
END;
GO
/****** Object:  StoredProcedure [dbo].[UpdateRegion]    Script Date: 17/09/2024 18:13:28 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[UpdateRegion]
    @RegionId INT,
    @RegionName NVARCHAR(100)
AS
BEGIN
    BEGIN TRANSACTION;
    
    BEGIN TRY
        UPDATE Regions
        SET 
            RegionName = @RegionName
        WHERE 
            RegionId = @RegionId;
        
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        -- You can add error handling here
        THROW;
    END CATCH
END;
GO
/****** Object:  StoredProcedure [dbo].[ValidateUser]    Script Date: 17/09/2024 18:13:28 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- Stored procedure to validate a user during login
CREATE PROCEDURE [dbo].[ValidateUser]
    @Username NVARCHAR(100),
    @PasswordHash VARBINARY(64)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @StoredHash VARBINARY(64), @Salt VARBINARY(64);

    -- Retrieve stored hash and salt for the given username
    SELECT @StoredHash = PasswordHash, @Salt = Salt
    FROM [dbo].[Users]
    WHERE Username = @Username;

    -- Validate password by comparing the stored hash with the provided hash
    IF @StoredHash = @PasswordHash
    BEGIN
        SELECT 1 AS Success; -- Login successful
    END
    ELSE
    BEGIN
        SELECT 0 AS Success; -- Login failed
    END
END;
GO
