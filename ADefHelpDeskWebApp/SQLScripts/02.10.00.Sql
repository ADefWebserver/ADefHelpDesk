IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[dbo].[ADefHelpDesk_RIAUsers]') AND OBJECTPROPERTY(id, N'IsUserTable') = 1)
BEGIN
CREATE TABLE [dbo].[ADefHelpDesk_RIAUsers](
	[UserID] [int] NOT NULL,
	[RIAPassword] [nvarchar](50) NOT NULL,
	[IPAddress] [nvarchar](50) NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
 CONSTRAINT [PK_ADefHelpDesk_RIAUsers] PRIMARY KEY CLUSTERED 
(
	[UserID] ASC
) ON [PRIMARY]
) ON [PRIMARY]
END

	/** Update Version **/
	DELETE FROM ADefHelpDesk_Version
	INSERT INTO ADefHelpDesk_Version(VersionNumber) VALUES (N'02.10.00')
