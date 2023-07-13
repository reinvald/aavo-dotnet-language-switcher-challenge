USE [master]
GO
CREATE DATABASE [TeddyBears]
GO
USE [TeddyBears]
GO
/****** Object:  Table [dbo].[Picnic] ******/
CREATE TABLE [dbo].[Picnic]
(
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[PicnicName] [nvarchar](50) NOT NULL,
	[LocationId] [int] NULL,
	[StartTime] [datetime] NOT NULL,
	[HasMusic] [bit] NOT NULL,
	[HasFood] [bit] NOT NULL,
	CONSTRAINT [PK_Picnic] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
)
GO
/****** Object:  Table [dbo].[PicnicLocation] ******/
CREATE TABLE [dbo].[PicnicLocation]
(
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[LocationName] [nvarchar](50) NOT NULL,
	[Capacity] [int] NOT NULL,
	[Municipality] [nvarchar](50) NOT NULL,
	CONSTRAINT [PK_PicnicLocation] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
)
GO
/****** Object:  Table [dbo].[PicnicParticipants] ******/
CREATE TABLE [dbo].[PicnicParticipants]
(
	[PicnicId] [int] NOT NULL,
	[TeddyBearId] [int] NOT NULL
)
GO
/****** Object:  Table [dbo].[TeddyBear] ******/
CREATE TABLE [dbo].[TeddyBear]
(
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[PrimaryColor] [nvarchar](20) NOT NULL,
	[AccentColor] [nvarchar](20) NULL,
	[IsDressed] [bit] NOT NULL,
	[OwnerName] [nvarchar](50) NOT NULL,
	[Characteristic] [nvarchar](50) NULL,
	CONSTRAINT [PK_TeddyBear] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
)
GO
SET IDENTITY_INSERT [dbo].[Picnic] ON 
GO
INSERT [dbo].[Picnic]
	([Id], [PicnicName], [LocationId], [StartTime], [HasMusic], [HasFood])
VALUES
	(1, N'Picnic At Oakwood', 1, CAST(N'2023-03-04T14:00:00.000' AS DateTime), 1, 1)
GO
INSERT [dbo].[Picnic]
	([Id], [PicnicName], [LocationId], [StartTime], [HasMusic], [HasFood])
VALUES
	(2, N'2nd Picnic At Oakwood', 1, CAST(N'2023-03-11T15:00:00.000' AS DateTime), 1, 1)
GO
INSERT [dbo].[Picnic]
	([Id], [PicnicName], [LocationId], [StartTime], [HasMusic], [HasFood])
VALUES
	(3, N'100 Acre Festival', 2, CAST(N'2023-06-21T14:30:00.000' AS DateTime), 1, 1)
GO
INSERT [dbo].[Picnic]
	([Id], [PicnicName], [LocationId], [StartTime], [HasMusic], [HasFood])
VALUES
	(4, N'Mid-Summer Picnic', 3, CAST(N'2023-07-29T15:00:00.000' AS DateTime), 1, 1)
GO
SET IDENTITY_INSERT [dbo].[Picnic] OFF
GO
SET IDENTITY_INSERT [dbo].[PicnicLocation] ON 
GO
INSERT [dbo].[PicnicLocation]
	([Id], [LocationName], [Capacity], [Municipality])
VALUES
	(1, N'Big Wood', 25, N'Oakwood')
GO
INSERT [dbo].[PicnicLocation]
	([Id], [LocationName], [Capacity], [Municipality])
VALUES
	(2, N'100 Acre Wood', 30, N'East Sussex')
GO
INSERT [dbo].[PicnicLocation]
	([Id], [LocationName], [Capacity], [Municipality])
VALUES
	(3, N'The Commons', 20, N'Coppell')
GO
SET IDENTITY_INSERT [dbo].[PicnicLocation] OFF
GO
INSERT [dbo].[PicnicParticipants]
	([PicnicId], [TeddyBearId])
VALUES
	(1, 1)
GO
INSERT [dbo].[PicnicParticipants]
	([PicnicId], [TeddyBearId])
VALUES
	(1, 2)
GO
INSERT [dbo].[PicnicParticipants]
	([PicnicId], [TeddyBearId])
VALUES
	(1, 4)
GO
INSERT [dbo].[PicnicParticipants]
	([PicnicId], [TeddyBearId])
VALUES
	(1, 6)
GO
INSERT [dbo].[PicnicParticipants]
	([PicnicId], [TeddyBearId])
VALUES
	(2, 1)
GO
INSERT [dbo].[PicnicParticipants]
	([PicnicId], [TeddyBearId])
VALUES
	(2, 3)
GO
INSERT [dbo].[PicnicParticipants]
	([PicnicId], [TeddyBearId])
VALUES
	(2, 4)
GO
INSERT [dbo].[PicnicParticipants]
	([PicnicId], [TeddyBearId])
VALUES
	(2, 5)
GO
INSERT [dbo].[PicnicParticipants]
	([PicnicId], [TeddyBearId])
VALUES
	(2, 6)
GO
SET IDENTITY_INSERT [dbo].[TeddyBear] ON 
GO
INSERT [dbo].[TeddyBear]
	([Id], [Name], [PrimaryColor], [AccentColor], [IsDressed], [OwnerName], [Characteristic])
VALUES
	(1, N'Teddy', N'Brown', NULL, 1, N'Little Billy', N'The one true Teddy')
GO
INSERT [dbo].[TeddyBear]
	([Id], [Name], [PrimaryColor], [AccentColor], [IsDressed], [OwnerName], [Characteristic])
VALUES
	(2, N'Suzie', N'Light Brown', N'Black', 1, N'Janey', N'Cuddly')
GO
INSERT [dbo].[TeddyBear]
	([Id], [Name], [PrimaryColor], [AccentColor], [IsDressed], [OwnerName], [Characteristic])
VALUES
	(3, N'TouTou', N'Pink', N'White', 1, N'Sarah', N'Nylon skin')
GO
INSERT [dbo].[TeddyBear]
	([Id], [Name], [PrimaryColor], [AccentColor], [IsDressed], [OwnerName], [Characteristic])
VALUES
	(4, N'Nounours', N'Brown', N'Red', 0, N'Clair', N'Fluffy')
GO
INSERT [dbo].[TeddyBear]
	([Id], [Name], [PrimaryColor], [AccentColor], [IsDressed], [OwnerName], [Characteristic])
VALUES
	(5, N'Bear', N'Light Blue', N'White', 0, N'Xavier', NULL)
GO
INSERT [dbo].[TeddyBear]
	([Id], [Name], [PrimaryColor], [AccentColor], [IsDressed], [OwnerName], [Characteristic])
VALUES
	(6, N'Winnie the Pooh', N'Yellow', NULL, 1, N'Christopher Robin', N'Red Shirt')
GO
SET IDENTITY_INSERT [dbo].[TeddyBear] OFF
GO
/****** Object:  Index [IX_PicnicName] ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_PicnicName] ON [dbo].[Picnic]
(
	[PicnicName] ASC
)
GO
/****** Object:  Index [IX_PicnicLocationName] ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_PicnicLocationName] ON [dbo].[PicnicLocation]
(
	[LocationName] ASC
)
GO
/****** Object:  Index [PK_persionId] ******/
ALTER TABLE [dbo].[PicnicParticipants] ADD  CONSTRAINT [PK_persionId] PRIMARY KEY NONCLUSTERED 
(
	[PicnicId] ASC,
	[TeddyBearId] ASC
)
GO
/****** Object:  Index [IX_TeddyBearName] ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_TeddyBearName] ON [dbo].[TeddyBear]
(
	[Name] ASC
)
GO
ALTER TABLE [dbo].[Picnic] ADD  CONSTRAINT [DF_Picnic_HasMusic]  DEFAULT ((1)) FOR [HasMusic]
GO
ALTER TABLE [dbo].[Picnic] ADD  CONSTRAINT [DF_Picnic_HasFood]  DEFAULT ((1)) FOR [HasFood]
GO
ALTER TABLE [dbo].[PicnicLocation] ADD  CONSTRAINT [DF_PicnicLocation_Capacity]  DEFAULT ((25)) FOR [Capacity]
GO
ALTER TABLE [dbo].[TeddyBear] ADD  CONSTRAINT [DF_TeddyBear_IsDressed]  DEFAULT ((1)) FOR [IsDressed]
GO
ALTER TABLE [dbo].[Picnic]  WITH CHECK ADD  CONSTRAINT [FK_Picnic_PicnicLocation] FOREIGN KEY([LocationId])
REFERENCES [dbo].[PicnicLocation] ([Id])
GO
ALTER TABLE [dbo].[Picnic] CHECK CONSTRAINT [FK_Picnic_PicnicLocation]
GO
ALTER TABLE [dbo].[PicnicParticipants]  WITH CHECK ADD  CONSTRAINT [FK_PicnicParticipants_Picnic] FOREIGN KEY([PicnicId])
REFERENCES [dbo].[Picnic] ([Id])
GO
ALTER TABLE [dbo].[PicnicParticipants] CHECK CONSTRAINT [FK_PicnicParticipants_Picnic]
GO
ALTER TABLE [dbo].[PicnicParticipants]  WITH CHECK ADD  CONSTRAINT [FK_PicnicParticipants_TeddyBear] FOREIGN KEY([TeddyBearId])
REFERENCES [dbo].[TeddyBear] ([Id])
GO
ALTER TABLE [dbo].[PicnicParticipants] CHECK CONSTRAINT [FK_PicnicParticipants_TeddyBear]
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'The primary key' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Picnic', @level2type=N'COLUMN',@level2name=N'Id'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'The name of the picnic.  All picnics have unique names' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Picnic', @level2type=N'COLUMN',@level2name=N'PicnicName'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'A reference to where the picnic will be held (a foreign key into the PicnicLocation table)' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Picnic', @level2type=N'COLUMN',@level2name=N'LocationId'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Picnics have a start time.  They always end at 6:00pm, when the mommies and daddies take them home to bed because they are tired little teddy bears' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Picnic', @level2type=N'COLUMN',@level2name=N'StartTime'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Will the picnic have music (default = true)' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Picnic', @level2type=N'COLUMN',@level2name=N'HasMusic'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Will the picnic have food (default = true)' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Picnic', @level2type=N'COLUMN',@level2name=N'HasFood'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Teddy bears have picnics.  Picnics have locations and participants (teddy bears)' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Picnic'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'The Primary Key' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PicnicLocation', @level2type=N'COLUMN',@level2name=N'Id'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'The name of the location (must be unique)' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PicnicLocation', @level2type=N'COLUMN',@level2name=N'LocationName'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'How many teddy bears can be accommodated at this location' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PicnicLocation', @level2type=N'COLUMN',@level2name=N'Capacity'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'In what village, town or city is this location' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PicnicLocation', @level2type=N'COLUMN',@level2name=N'Municipality'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'The location of one or more picnics.  Every picnic must have a location' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PicnicLocation'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'A reference to the picnic (via foreign key)' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PicnicParticipants', @level2type=N'COLUMN',@level2name=N'PicnicId'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'A reference to the teddy bear (picnic participant) (via foreign key)' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PicnicParticipants', @level2type=N'COLUMN',@level2name=N'TeddyBearId'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'The "join table" that defines the many-to-many relationship between Teddy Bears and Picnics' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PicnicParticipants'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'The Primary Key' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'TeddyBear', @level2type=N'COLUMN',@level2name=N'Id'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'The Teddy Bear''s name.  Each is unique' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'TeddyBear', @level2type=N'COLUMN',@level2name=N'Name'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'All teddy bears have a primary color.  The color is a string (but should be picked from a list)' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'TeddyBear', @level2type=N'COLUMN',@level2name=N'PrimaryColor'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Teddy Bears may have a secondary color.  The color is a string (but should be picked from a list)' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'TeddyBear', @level2type=N'COLUMN',@level2name=N'AccentColor'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Is the Teddy Bear dressed (true or false)' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'TeddyBear', @level2type=N'COLUMN',@level2name=N'IsDressed'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Who is the teddy bear''s owner' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'TeddyBear', @level2type=N'COLUMN',@level2name=N'OwnerName'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Teddy bears may have a defining characteristic - fluffy, polite, whatever' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'TeddyBear', @level2type=N'COLUMN',@level2name=N'Characteristic'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Teddy Bears are soft and cuddly.  Each has a name and a unique personality' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'TeddyBear'
GO
USE [master]
GO