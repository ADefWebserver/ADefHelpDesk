/** Fix table values if upgrading from old version of ADefHelpDesk **/
BEGIN
UPDATE dbo.AspNetUsers SET NewsletterSubscriber = 0 WHERE NewsletterSubscriber IS NULL
END

/** Authentication Settings **/
if(SELECT count(*) FROM [ADefHelpDesk_Settings] where [SettingName] = 'Authentication:Google:ClientId') = 0
BEGIN
INSERT INTO ADefHelpDesk_Settings(PortalID, SettingName, SettingValue) VALUES (N'0', N'Authentication:Google:ClientId', N'')
END

if(SELECT count(*) FROM [ADefHelpDesk_Settings] where [SettingName] = 'Authentication:Google:ClientSecret') = 0
BEGIN
INSERT INTO ADefHelpDesk_Settings(PortalID, SettingName, SettingValue) VALUES (N'0', N'Authentication:Google:ClientSecret', N'')
END

if(SELECT count(*) FROM [ADefHelpDesk_Settings] where [SettingName] = 'Authentication:Microsoft:ClientId') = 0
BEGIN
INSERT INTO ADefHelpDesk_Settings(PortalID, SettingName, SettingValue) VALUES (N'0', N'Authentication:Microsoft:ClientId', N'')
END

if(SELECT count(*) FROM [ADefHelpDesk_Settings] where [SettingName] = 'Authentication:Microsoft:ClientSecret') = 0
BEGIN
INSERT INTO ADefHelpDesk_Settings(PortalID, SettingName, SettingValue) VALUES (N'0', N'Authentication:Microsoft:ClientSecret', N'')
END

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ADefHelpDesk_ApiSecurityPermission]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[ADefHelpDesk_ApiSecurityPermission](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ApiSecurityUser] [int] NOT NULL,
	[PermissionName] [nvarchar](250) NOT NULL,
	[IsEnabled] [bit] NOT NULL,
 CONSTRAINT [PK_ADefHelpDesk_ApiSecurityPermission] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[ADefHelpDesk_ApiSecurity]') AND name = N'IX_ADefHelpDesk_ApiSecurity_Username')
CREATE UNIQUE NONCLUSTERED INDEX [IX_ADefHelpDesk_ApiSecurity_Username] ON [dbo].[ADefHelpDesk_ApiSecurity]
(
	[Username] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_ADefHelpDesk_ApiSecurityPermission_ADefHelpDesk_ApiSecurity]') AND parent_object_id = OBJECT_ID(N'[dbo].[ADefHelpDesk_ApiSecurityPermission]'))
ALTER TABLE [dbo].[ADefHelpDesk_ApiSecurityPermission]  WITH CHECK ADD  CONSTRAINT [FK_ADefHelpDesk_ApiSecurityPermission_ADefHelpDesk_ApiSecurity] FOREIGN KEY([ApiSecurityUser])
REFERENCES [dbo].[ADefHelpDesk_ApiSecurity] ([Id])
ON DELETE CASCADE

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_ADefHelpDesk_ApiSecurityPermission_ADefHelpDesk_ApiSecurity]') AND parent_object_id = OBJECT_ID(N'[dbo].[ADefHelpDesk_ApiSecurityPermission]'))
ALTER TABLE [dbo].[ADefHelpDesk_ApiSecurityPermission] CHECK CONSTRAINT [FK_ADefHelpDesk_ApiSecurityPermission_ADefHelpDesk_ApiSecurity]

/** APIEncryptionKeyKey was added to Settings in this version **/
DELETE FROM ADefHelpDesk_Settings WHERE (SettingName = N'APIEncryptionKeyKey')
INSERT INTO ADefHelpDesk_Settings(PortalID, SettingName, SettingValue) VALUES (N'0', N'APIEncryptionKeyKey', REPLACE(NEWID(),'-','Z'))

/** Update Version **/
DELETE FROM ADefHelpDesk_Version
INSERT INTO ADefHelpDesk_Version(VersionNumber) VALUES (N'04.30.00')