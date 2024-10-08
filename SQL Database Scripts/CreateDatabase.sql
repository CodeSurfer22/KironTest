USE [master]
GO

/****** Object:  Database [KironTest]    Script Date: 17/09/2024 18:09:34 ******/
CREATE DATABASE [KironTest]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'KironTest', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL14.SQLEXPRESS01\MSSQL\DATA\KironTest.mdf' , SIZE = 8192KB , MAXSIZE = UNLIMITED, FILEGROWTH = 65536KB )
 LOG ON 
( NAME = N'KironTest_log', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL14.SQLEXPRESS01\MSSQL\DATA\KironTest_log.ldf' , SIZE = 8192KB , MAXSIZE = 2048GB , FILEGROWTH = 65536KB )
GO

IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [KironTest].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO

ALTER DATABASE [KironTest] SET ANSI_NULL_DEFAULT OFF 
GO

ALTER DATABASE [KironTest] SET ANSI_NULLS OFF 
GO

ALTER DATABASE [KironTest] SET ANSI_PADDING OFF 
GO

ALTER DATABASE [KironTest] SET ANSI_WARNINGS OFF 
GO

ALTER DATABASE [KironTest] SET ARITHABORT OFF 
GO

ALTER DATABASE [KironTest] SET AUTO_CLOSE OFF 
GO

ALTER DATABASE [KironTest] SET AUTO_SHRINK OFF 
GO

ALTER DATABASE [KironTest] SET AUTO_UPDATE_STATISTICS ON 
GO

ALTER DATABASE [KironTest] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO

ALTER DATABASE [KironTest] SET CURSOR_DEFAULT  GLOBAL 
GO

ALTER DATABASE [KironTest] SET CONCAT_NULL_YIELDS_NULL OFF 
GO

ALTER DATABASE [KironTest] SET NUMERIC_ROUNDABORT OFF 
GO

ALTER DATABASE [KironTest] SET QUOTED_IDENTIFIER OFF 
GO

ALTER DATABASE [KironTest] SET RECURSIVE_TRIGGERS OFF 
GO

ALTER DATABASE [KironTest] SET  DISABLE_BROKER 
GO

ALTER DATABASE [KironTest] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO

ALTER DATABASE [KironTest] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO

ALTER DATABASE [KironTest] SET TRUSTWORTHY OFF 
GO

ALTER DATABASE [KironTest] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO

ALTER DATABASE [KironTest] SET PARAMETERIZATION SIMPLE 
GO

ALTER DATABASE [KironTest] SET READ_COMMITTED_SNAPSHOT OFF 
GO

ALTER DATABASE [KironTest] SET HONOR_BROKER_PRIORITY OFF 
GO

ALTER DATABASE [KironTest] SET RECOVERY FULL 
GO

ALTER DATABASE [KironTest] SET  MULTI_USER 
GO

ALTER DATABASE [KironTest] SET PAGE_VERIFY CHECKSUM  
GO

ALTER DATABASE [KironTest] SET DB_CHAINING OFF 
GO

ALTER DATABASE [KironTest] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO

ALTER DATABASE [KironTest] SET TARGET_RECOVERY_TIME = 60 SECONDS 
GO

ALTER DATABASE [KironTest] SET DELAYED_DURABILITY = DISABLED 
GO

ALTER DATABASE [KironTest] SET QUERY_STORE = OFF
GO

ALTER DATABASE [KironTest] SET  READ_WRITE 
GO


