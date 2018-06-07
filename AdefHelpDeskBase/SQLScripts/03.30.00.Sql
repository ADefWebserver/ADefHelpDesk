	/** Add ContentType **/
	IF NOT EXISTS (Select 'ContentType' From INFORMATION_SCHEMA.COLUMNS Where Table_Name = N'ADefHelpDesk_TaskDetails' And Column_Name = 'ContentType')
	BEGIN
		ALTER TABLE dbo.ADefHelpDesk_TaskDetails ADD ContentType nvarchar(50) NULL
	END

	/** ADefHelpDesk_ApiSecurity **/
	IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ADefHelpDesk_ApiSecurity]') AND type = 'U')
	BEGIN
	CREATE TABLE [dbo].[ADefHelpDesk_ApiSecurity](
	[Id] int IDENTITY(1,1) NOT NULL,
	[Username] nvarchar(256) NOT NULL,
	[Password] nvarchar(256) NOT NULL,
	[ContactName] nvarchar(256) NOT NULL,
	[ContactCompany] nvarchar(500) NULL,
	[ContactWebsite] nvarchar(2000) NULL,
	[ContactEmail] nvarchar(500) NULL,
	[ContactPhone] nvarchar(256) NULL,
	[IsActive] bit NOT NULL
	CONSTRAINT [PK_ADefHelpDesk_ApiSecurity] PRIMARY KEY CLUSTERED
	(
	[Id] ASC
	) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]

	CREATE UNIQUE NONCLUSTERED INDEX [IX_ADefHelpDesk_ApiSecurity_Username] ON [dbo].[ADefHelpDesk_ApiSecurity]
	(
	[UserName] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

	END

	/** Update Version **/
	DELETE FROM ADefHelpDesk_Version
	INSERT INTO ADefHelpDesk_Version(VersionNumber) VALUES (N'03.30.00')

	/** GUID was added to Settings in this version **/
	DELETE FROM ADefHelpDesk_Settings WHERE (SettingName = N'ApplicationGUID')
	INSERT INTO ADefHelpDesk_Settings(PortalID, SettingName, SettingValue) VALUES (N'0', N'ApplicationGUID', NEWID())

	/** StorageFileType was added to Settings in this version **/
	DELETE FROM ADefHelpDesk_Settings WHERE (SettingName = N'StorageFileType')
	INSERT INTO ADefHelpDesk_Settings(PortalID, SettingName, SettingValue) VALUES (N'0', N'StorageFileType', 'FileSystem')

	/** AzureStorageConnection was added to Settings in this version **/
	DELETE FROM ADefHelpDesk_Settings WHERE (SettingName = N'AzureStorageConnection')
	INSERT INTO ADefHelpDesk_Settings(PortalID, SettingName, SettingValue) VALUES (N'0', N'AzureStorageConnection', '')

	/** ApplicationName may be missing **/
	if(SELECT count(*) FROM [ADefHelpDesk_Settings] where [SettingName] = 'ApplicationName') = 0
	BEGIN
	INSERT INTO ADefHelpDesk_Settings(PortalID, SettingName, SettingValue) VALUES (N'0', N'ApplicationName', N'ADefHelpDesk')
	END