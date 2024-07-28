
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ADefHelpDesk_LastSearch]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[ADefHelpDesk_LastSearch](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[UserID] [int] NOT NULL,
	[PortalID] [int] NOT NULL,
	[SearchText] [nvarchar](150) NULL,
	[Status] [nvarchar](50) NULL,
	[Priority] [nvarchar](50) NULL,
	[CreatedDate] [datetime] NULL,
	[DueDate] [datetime] NULL,
	[AssignedRoleID] [int] NULL,
	[Categories] [nvarchar](2000) NULL,
	[CurrentPage] [int] NULL,
	[PageSize] [int] NULL,
 CONSTRAINT [PK_ADefHelpDesk_LastSearch] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[ADefHelpDesk_LastSearch]') AND name = N'IX_ADefHelpDesk_LastSearch')
CREATE NONCLUSTERED INDEX [IX_ADefHelpDesk_LastSearch] ON [dbo].[ADefHelpDesk_LastSearch] 
(
	[UserID] ASC,
	[PortalID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ADefHelpDesk_Categories]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[ADefHelpDesk_Categories](
	[CategoryID] [int] IDENTITY(1,1) NOT NULL,
	[PortalID] [int] NOT NULL,
	[ParentCategoryID] [int] NULL,
	[CategoryName] [nvarchar](50) NULL,
	[Level] [int] NOT NULL,
	[RequestorVisible] [bit] NOT NULL,
	[Selectable] [bit] NOT NULL,
 CONSTRAINT [PK_ADefHelpDesk_Categories] PRIMARY KEY CLUSTERED 
(
	[CategoryID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[ADefHelpDesk_Categories]') AND name = N'IX_ADefHelpDesk_Categories')
CREATE NONCLUSTERED INDEX [IX_ADefHelpDesk_Categories] ON [dbo].[ADefHelpDesk_Categories] 
(
	[PortalID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ADefHelpDesk_Settings]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[ADefHelpDesk_Settings](
	[SettingID] [int] IDENTITY(1,1) NOT NULL,
	[PortalID] [int] NOT NULL,
	[SettingName] [nvarchar](150) NOT NULL,
	[SettingValue] [nvarchar](250) NOT NULL,
 CONSTRAINT [PK_ADefHelpDesk_Settings] PRIMARY KEY CLUSTERED 
(
	[SettingID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[ADefHelpDesk_Settings]') AND name = N'IX_ADefHelpDesk_Settings_PortalID')
CREATE NONCLUSTERED INDEX [IX_ADefHelpDesk_Settings_PortalID] ON [dbo].[ADefHelpDesk_Settings] 
(
	[PortalID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ADefHelpDesk_Roles]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[ADefHelpDesk_Roles](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[PortalID] [int] NOT NULL,
	[RoleName] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_ADefHelpDesk_Roles] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[ADefHelpDesk_Roles]') AND name = N'IX_ADefHelpDesk_Roles')
CREATE NONCLUSTERED INDEX [IX_ADefHelpDesk_Roles] ON [dbo].[ADefHelpDesk_Roles] 
(
	[PortalID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ADefHelpDesk_Tasks]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[ADefHelpDesk_Tasks](
	[TaskID] [int] IDENTITY(1,1) NOT NULL,
	[PortalID] [int] NOT NULL,
	[Description] [nvarchar](150) NOT NULL,
	[Status] [nvarchar](50) NOT NULL,
	[Priority] [nvarchar](50) NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
	[EstimatedStart] [datetime] NULL,
	[EstimatedCompletion] [datetime] NULL,
	[DueDate] [datetime] NULL,
	[AssignedRoleID] [int] NOT NULL,
	[TicketPassword] [nvarchar](50) NOT NULL,
	[RequesterUserID] [int] NOT NULL,
	[RequesterName] [nvarchar](350) NULL,
	[RequesterEmail] [nvarchar](350) NULL,
	[RequesterPhone] [nvarchar](50) NULL,
	[EstimatedHours] [int] NULL,
 CONSTRAINT [PK_ADefHelpDeskTasks] PRIMARY KEY CLUSTERED 
(
	[TaskID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[ADefHelpDesk_Tasks]') AND name = N'IX_ADefHelpDesk_Tasks_AssignedRoleID')
CREATE NONCLUSTERED INDEX [IX_ADefHelpDesk_Tasks_AssignedRoleID] ON [dbo].[ADefHelpDesk_Tasks] 
(
	[AssignedRoleID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[ADefHelpDesk_Tasks]') AND name = N'IX_ADefHelpDesk_Tasks_CreatedDate')
CREATE NONCLUSTERED INDEX [IX_ADefHelpDesk_Tasks_CreatedDate] ON [dbo].[ADefHelpDesk_Tasks] 
(
	[CreatedDate] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[ADefHelpDesk_Tasks]') AND name = N'IX_ADefHelpDesk_Tasks_Status')
CREATE NONCLUSTERED INDEX [IX_ADefHelpDesk_Tasks_Status] ON [dbo].[ADefHelpDesk_Tasks] 
(
	[Status] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ADefHelpDesk_Version]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[ADefHelpDesk_Version](
	[VersionNumber] [varchar](10) NOT NULL,
 CONSTRAINT [PK_ADefHelpDesk_Version] PRIMARY KEY CLUSTERED 
(
	[VersionNumber] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
END

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ADefHelpDesk_Users]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[ADefHelpDesk_Users](
	[UserID] [int] IDENTITY(1,1) NOT NULL,
	[Username] [nvarchar](100) NOT NULL,
	[FirstName] [nvarchar](50) NOT NULL,
	[LastName] [nvarchar](50) NOT NULL,
	[IsSuperUser] [bit] NOT NULL,
	[Email] [nvarchar](256) NOT NULL,
	[Password] [nvarchar](50) NOT NULL,
	[RIAPassword] [nvarchar](50) NULL,
	[VerificationCode] [nvarchar](50) NULL,
 CONSTRAINT [PK_ADefHelpDesk_Users] PRIMARY KEY CLUSTERED 
(
	[UserID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[ADefHelpDesk_Users]') AND name = N'IX_ADefHelpDesk_Users')
CREATE NONCLUSTERED INDEX [IX_ADefHelpDesk_Users] ON [dbo].[ADefHelpDesk_Users] 
(
	[Username] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[ADefHelpDesk_Users]') AND name = N'IX_ADefHelpDesk_Users_Email')
CREATE UNIQUE NONCLUSTERED INDEX [IX_ADefHelpDesk_Users_Email] ON [dbo].[ADefHelpDesk_Users] 
(
	[Email] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ADefHelpDesk_UserRoles]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[ADefHelpDesk_UserRoles](
	[UserRoleID] [int] IDENTITY(1,1) NOT NULL,
	[UserID] [int] NOT NULL,
	[RoleID] [int] NOT NULL,
 CONSTRAINT [PK_ADefHelpDesk_UserRoles] PRIMARY KEY CLUSTERED 
(
	[UserRoleID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
END

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ADefHelpDesk_TaskDetails]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[ADefHelpDesk_TaskDetails](
	[DetailID] [int] IDENTITY(1,1) NOT NULL,
	[TaskID] [int] NOT NULL,
	[DetailType] [nvarchar](50) NOT NULL,
	[InsertDate] [datetime] NOT NULL,
	[UserID] [int] NOT NULL,
	[Description] [nvarchar](max) NOT NULL,
	[StartTime] [datetime] NULL,
	[StopTime] [datetime] NULL,
 CONSTRAINT [PK_ADefHelpDesk_TaskDetails] PRIMARY KEY CLUSTERED 
(
	[DetailID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[ADefHelpDesk_TaskDetails]') AND name = N'IX_ADefHelpDesk_TaskDetails')
CREATE NONCLUSTERED INDEX [IX_ADefHelpDesk_TaskDetails] ON [dbo].[ADefHelpDesk_TaskDetails] 
(
	[TaskID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ADefHelpDesk_TaskCategories]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[ADefHelpDesk_TaskCategories](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[TaskID] [int] NOT NULL,
	[CategoryID] [int] NOT NULL,
 CONSTRAINT [PK_ADefHelpDesk_TaskCategories] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[ADefHelpDesk_TaskCategories]') AND name = N'IX_ADefHelpDesk_TaskCategories')
CREATE NONCLUSTERED INDEX [IX_ADefHelpDesk_TaskCategories] ON [dbo].[ADefHelpDesk_TaskCategories] 
(
	[TaskID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ADefHelpDesk_TaskAssociations]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[ADefHelpDesk_TaskAssociations](
	[TaskRelationID] [int] IDENTITY(1,1) NOT NULL,
	[TaskID] [int] NOT NULL,
	[AssociatedID] [int] NOT NULL,
 CONSTRAINT [PK_ADefHelpDesk_TaskAssociations] PRIMARY KEY CLUSTERED 
(
	[TaskRelationID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[ADefHelpDesk_TaskAssociations]') AND name = N'IX_ADefHelpDesk_TaskAssociations')
CREATE NONCLUSTERED INDEX [IX_ADefHelpDesk_TaskAssociations] ON [dbo].[ADefHelpDesk_TaskAssociations] 
(
	[TaskID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ADefHelpDesk_Log]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[ADefHelpDesk_Log](
	[LogID] [int] IDENTITY(1,1) NOT NULL,
	[TaskID] [int] NOT NULL,
	[LogDescription] [nvarchar](500) NOT NULL,
	[DateCreated] [datetime] NOT NULL,
	[UserID] [int] NOT NULL,
 CONSTRAINT [PK_ADefHelpDesk_Log] PRIMARY KEY CLUSTERED 
(
	[LogID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[ADefHelpDesk_Log]') AND name = N'IX_ADefHelpDesk_Log')
CREATE NONCLUSTERED INDEX [IX_ADefHelpDesk_Log] ON [dbo].[ADefHelpDesk_Log] 
(
	[TaskID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ADefHelpDesk_Attachments]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[ADefHelpDesk_Attachments](
	[AttachmentID] [int] IDENTITY(1,1) NOT NULL,
	[DetailID] [int] NOT NULL,
	[AttachmentPath] [nvarchar](1000) NOT NULL,
	[FileName] [nvarchar](150) NOT NULL,
	[OriginalFileName] [nvarchar](150) NOT NULL,
	[UserID] [int] NOT NULL,
 CONSTRAINT [PK_ADefHelpDesk_Attachments] PRIMARY KEY CLUSTERED 
(
	[AttachmentID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[ADefHelpDesk_Attachments]') AND name = N'IX_ADefHelpDesk_Attachments')
CREATE NONCLUSTERED INDEX [IX_ADefHelpDesk_Attachments] ON [dbo].[ADefHelpDesk_Attachments] 
(
	[DetailID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_ADefHelpDesk_Attachments_ADefHelpDesk_TaskDetails]') AND parent_object_id = OBJECT_ID(N'[dbo].[ADefHelpDesk_Attachments]'))
ALTER TABLE [dbo].[ADefHelpDesk_Attachments]  WITH CHECK ADD  CONSTRAINT [FK_ADefHelpDesk_Attachments_ADefHelpDesk_TaskDetails] FOREIGN KEY([DetailID])
REFERENCES [dbo].[ADefHelpDesk_TaskDetails] ([DetailID])

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_ADefHelpDesk_Attachments_ADefHelpDesk_TaskDetails]') AND parent_object_id = OBJECT_ID(N'[dbo].[ADefHelpDesk_Attachments]'))
ALTER TABLE [dbo].[ADefHelpDesk_Attachments] CHECK CONSTRAINT [FK_ADefHelpDesk_Attachments_ADefHelpDesk_TaskDetails]

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_ADefHelpDesk_Log_ADefHelpDesk_Tasks]') AND parent_object_id = OBJECT_ID(N'[dbo].[ADefHelpDesk_Log]'))
ALTER TABLE [dbo].[ADefHelpDesk_Log]  WITH CHECK ADD  CONSTRAINT [FK_ADefHelpDesk_Log_ADefHelpDesk_Tasks] FOREIGN KEY([TaskID])
REFERENCES [dbo].[ADefHelpDesk_Tasks] ([TaskID])

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_ADefHelpDesk_Log_ADefHelpDesk_Tasks]') AND parent_object_id = OBJECT_ID(N'[dbo].[ADefHelpDesk_Log]'))
ALTER TABLE [dbo].[ADefHelpDesk_Log] CHECK CONSTRAINT [FK_ADefHelpDesk_Log_ADefHelpDesk_Tasks]

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_ADefHelpDesk_TaskAssociations_ADefHelpDesk_Tasks]') AND parent_object_id = OBJECT_ID(N'[dbo].[ADefHelpDesk_TaskAssociations]'))
ALTER TABLE [dbo].[ADefHelpDesk_TaskAssociations]  WITH CHECK ADD  CONSTRAINT [FK_ADefHelpDesk_TaskAssociations_ADefHelpDesk_Tasks] FOREIGN KEY([TaskID])
REFERENCES [dbo].[ADefHelpDesk_Tasks] ([TaskID])

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_ADefHelpDesk_TaskAssociations_ADefHelpDesk_Tasks]') AND parent_object_id = OBJECT_ID(N'[dbo].[ADefHelpDesk_TaskAssociations]'))
ALTER TABLE [dbo].[ADefHelpDesk_TaskAssociations] CHECK CONSTRAINT [FK_ADefHelpDesk_TaskAssociations_ADefHelpDesk_Tasks]

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_ADefHelpDesk_TaskCategories_ADefHelpDesk_Categories]') AND parent_object_id = OBJECT_ID(N'[dbo].[ADefHelpDesk_TaskCategories]'))
ALTER TABLE [dbo].[ADefHelpDesk_TaskCategories]  WITH CHECK ADD  CONSTRAINT [FK_ADefHelpDesk_TaskCategories_ADefHelpDesk_Categories] FOREIGN KEY([CategoryID])
REFERENCES [dbo].[ADefHelpDesk_Categories] ([CategoryID])

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_ADefHelpDesk_TaskCategories_ADefHelpDesk_Categories]') AND parent_object_id = OBJECT_ID(N'[dbo].[ADefHelpDesk_TaskCategories]'))
ALTER TABLE [dbo].[ADefHelpDesk_TaskCategories] CHECK CONSTRAINT [FK_ADefHelpDesk_TaskCategories_ADefHelpDesk_Categories]

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_ADefHelpDesk_TaskCategories_ADefHelpDesk_Tasks]') AND parent_object_id = OBJECT_ID(N'[dbo].[ADefHelpDesk_TaskCategories]'))
ALTER TABLE [dbo].[ADefHelpDesk_TaskCategories]  WITH CHECK ADD  CONSTRAINT [FK_ADefHelpDesk_TaskCategories_ADefHelpDesk_Tasks] FOREIGN KEY([TaskID])
REFERENCES [dbo].[ADefHelpDesk_Tasks] ([TaskID])

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_ADefHelpDesk_TaskCategories_ADefHelpDesk_Tasks]') AND parent_object_id = OBJECT_ID(N'[dbo].[ADefHelpDesk_TaskCategories]'))
ALTER TABLE [dbo].[ADefHelpDesk_TaskCategories] CHECK CONSTRAINT [FK_ADefHelpDesk_TaskCategories_ADefHelpDesk_Tasks]

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_ADefHelpDesk_TaskDetails_ADefHelpDesk_Tasks]') AND parent_object_id = OBJECT_ID(N'[dbo].[ADefHelpDesk_TaskDetails]'))
ALTER TABLE [dbo].[ADefHelpDesk_TaskDetails]  WITH CHECK ADD  CONSTRAINT [FK_ADefHelpDesk_TaskDetails_ADefHelpDesk_Tasks] FOREIGN KEY([TaskID])
REFERENCES [dbo].[ADefHelpDesk_Tasks] ([TaskID])

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_ADefHelpDesk_TaskDetails_ADefHelpDesk_Tasks]') AND parent_object_id = OBJECT_ID(N'[dbo].[ADefHelpDesk_TaskDetails]'))
ALTER TABLE [dbo].[ADefHelpDesk_TaskDetails] CHECK CONSTRAINT [FK_ADefHelpDesk_TaskDetails_ADefHelpDesk_Tasks]

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_ADefHelpDesk_UserRoles_ADefHelpDesk_Users]') AND parent_object_id = OBJECT_ID(N'[dbo].[ADefHelpDesk_UserRoles]'))
ALTER TABLE [dbo].[ADefHelpDesk_UserRoles]  WITH CHECK ADD  CONSTRAINT [FK_ADefHelpDesk_UserRoles_ADefHelpDesk_Users] FOREIGN KEY([UserID])
REFERENCES [dbo].[ADefHelpDesk_Users] ([UserID])

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_ADefHelpDesk_UserRoles_ADefHelpDesk_Users]') AND parent_object_id = OBJECT_ID(N'[dbo].[ADefHelpDesk_UserRoles]'))
ALTER TABLE [dbo].[ADefHelpDesk_UserRoles] CHECK CONSTRAINT [FK_ADefHelpDesk_UserRoles_ADefHelpDesk_Users]

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_UserRoles_ADefHelpDesk_Roles]') AND parent_object_id = OBJECT_ID(N'[dbo].[ADefHelpDesk_UserRoles]'))
ALTER TABLE [dbo].[ADefHelpDesk_UserRoles]  WITH CHECK ADD  CONSTRAINT [FK_UserRoles_ADefHelpDesk_Roles] FOREIGN KEY([RoleID])
REFERENCES [dbo].[ADefHelpDesk_Roles] ([ID])

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_UserRoles_ADefHelpDesk_Roles]') AND parent_object_id = OBJECT_ID(N'[dbo].[ADefHelpDesk_UserRoles]'))
ALTER TABLE [dbo].[ADefHelpDesk_UserRoles] CHECK CONSTRAINT [FK_UserRoles_ADefHelpDesk_Roles]

/*** This script will only run if the version number in the database is less than 01.00.00 ***/
if((SELECT count(*) FROM ADefHelpDesk_Version) = 0)
BEGIN

	/** Update Version **/
	DELETE FROM ADefHelpDesk_Version
	INSERT INTO ADefHelpDesk_Version(VersionNumber) VALUES (N'01.00.00')
	
	DELETE FROM ADefHelpDesk_Settings WHERE (SettingName = N'SMTPServer')
	INSERT INTO ADefHelpDesk_Settings(PortalID, SettingName, SettingValue) VALUES (N'0', N'SMTPServer', N'')
	DELETE FROM ADefHelpDesk_Settings WHERE (SettingName = N'SMTPSecure')
	INSERT INTO ADefHelpDesk_Settings(PortalID, SettingName, SettingValue) VALUES (N'0', N'SMTPSecure', N'False')
	DELETE FROM ADefHelpDesk_Settings WHERE (SettingName = N'SMTPUserName')
	INSERT INTO ADefHelpDesk_Settings(PortalID, SettingName, SettingValue) VALUES (N'0', N'SMTPUserName', N'')
	DELETE FROM ADefHelpDesk_Settings WHERE (SettingName = N'SMTPPassword')
	INSERT INTO ADefHelpDesk_Settings(PortalID, SettingName, SettingValue) VALUES (N'0', N'SMTPPassword', N'')
	DELETE FROM ADefHelpDesk_Settings WHERE (SettingName = N'SMTPAuthendication')
	INSERT INTO ADefHelpDesk_Settings(PortalID, SettingName, SettingValue) VALUES (N'0', N'SMTPAuthendication', N'0')
	DELETE FROM ADefHelpDesk_Settings WHERE (SettingName = N'SMTPFromEmail')
	INSERT INTO ADefHelpDesk_Settings(PortalID, SettingName, SettingValue) VALUES (N'0', N'SMTPFromEmail', N'')
	DELETE FROM ADefHelpDesk_Settings WHERE (SettingName = N'FileUploadPath')
	INSERT INTO ADefHelpDesk_Settings(PortalID, SettingName, SettingValue) VALUES (N'0', N'FileUploadPath', N'')
	DELETE FROM ADefHelpDesk_Settings WHERE (SettingName = N'UploadPermission')
	INSERT INTO ADefHelpDesk_Settings(PortalID, SettingName, SettingValue) VALUES (N'0', N'UploadPermission', N'Administrator')
	DELETE FROM ADefHelpDesk_Settings WHERE (SettingName = N'AllowRegistration')
	INSERT INTO ADefHelpDesk_Settings(PortalID, SettingName, SettingValue) VALUES (N'0', N'AllowRegistration', N'False')
	DELETE FROM ADefHelpDesk_Settings WHERE (SettingName = N'VerifiedRegistration')
	INSERT INTO ADefHelpDesk_Settings(PortalID, SettingName, SettingValue) VALUES (N'0', N'VerifiedRegistration', N'False')
	DELETE FROM ADefHelpDesk_Settings WHERE (SettingName = N'ApplicationName')
	INSERT INTO ADefHelpDesk_Settings(PortalID, SettingName, SettingValue) VALUES (N'0', N'ApplicationName', N'ADefHelpDesk')
END
