IF COL_LENGTH('dbo.AspNetUsers', 'DisplayName') IS NULL
BEGIN
ALTER TABLE dbo.AspNetUsers ADD	DisplayName nvarchar(256) NULL
END

IF COL_LENGTH('dbo.AspNetUsers', 'NewsletterSubscriber') IS NULL
BEGIN
ALTER TABLE dbo.AspNetUsers ADD	NewsletterSubscriber nvarchar(256) NULL
END

/** Update Version **/
DELETE FROM ADefHelpDesk_Version
INSERT INTO ADefHelpDesk_Version(VersionNumber) VALUES (N'04.30.00')