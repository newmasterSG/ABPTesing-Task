CREATE TABLE [dbo].[Devices]
(
	[DeviceToken] NVARCHAR(250) NOT NULL PRIMARY KEY, 
    [ExperimentId] NVARCHAR(250) NULL, 
    [ReceivedValue] NVARCHAR(250) NULL, 
    [FirstRequest] DATETIME NULL,
    CONSTRAINT FK_Devices_Experiments FOREIGN KEY ([ExperimentId]) REFERENCES [Experiments] ([Id])
);