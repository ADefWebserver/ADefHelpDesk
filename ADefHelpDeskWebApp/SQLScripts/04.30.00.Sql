IF COL_LENGTH('dbo.AspNetUsers', 'DisplayName') IS NULL
BEGIN
ALTER TABLE dbo.AspNetUsers ADD	DisplayName nvarchar(256) NULL
END

IF COL_LENGTH('dbo.AspNetUsers', 'NewsletterSubscriber') IS NULL
BEGIN
ALTER TABLE dbo.AspNetUsers ADD	NewsletterSubscriber [bit] NULL
END

	/** APIEncryptionKeyKey was added to Settings in this version **/
	DELETE FROM ADefHelpDesk_Settings WHERE (SettingName = N'APIEncryptionKeyKey')
	INSERT INTO ADefHelpDesk_Settings(PortalID, SettingName, SettingValue) VALUES (N'0', N'APIEncryptionKeyKey', REPLACE(NEWID(),'-','Z'))

/** Update Version **/
DELETE FROM ADefHelpDesk_Version
INSERT INTO ADefHelpDesk_Version(VersionNumber) VALUES (N'04.30.00')