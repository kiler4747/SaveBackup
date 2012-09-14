
-- --------------------------------------------------
-- Entity Designer DDL Script for SQL Server 2005, 2008, and Azure
-- --------------------------------------------------
-- Date Created: 09/14/2012 19:27:41
-- Generated from EDMX file: D:\Programming\wpf\SaveBackup\SaveBackup\Save.edmx
-- --------------------------------------------------

SET QUOTED_IDENTIFIER OFF;
GO
USE [Save];
GO
IF SCHEMA_ID(N'dbo') IS NULL EXECUTE(N'CREATE SCHEMA [dbo]');
GO

-- --------------------------------------------------
-- Dropping existing FOREIGN KEY constraints
-- --------------------------------------------------


-- --------------------------------------------------
-- Dropping existing tables
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[Saves]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Saves];
GO

-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

-- Creating table 'Saves'
CREATE TABLE [dbo].[Saves] (
    [ID] int IDENTITY(1,1) NOT NULL,
    [Check] bit  NOT NULL,
    [Name] nvarchar(max)  NOT NULL,
    [Path] nvarchar(max)  NOT NULL,
    [Type] nvarchar(max)  NOT NULL
);
GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [ID], [Check], [Name], [Path], [Type] in table 'Saves'
ALTER TABLE [dbo].[Saves]
ADD CONSTRAINT [PK_Saves]
    PRIMARY KEY CLUSTERED ([ID], [Check], [Name], [Path], [Type] ASC);
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------