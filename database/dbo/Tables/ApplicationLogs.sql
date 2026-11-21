CREATE TABLE [dbo].[ApplicationLogs]
(
    [Id] BIGINT IDENTITY (1, 1) NOT NULL,
    [CreatedAtUtc] DATETIME2 NOT NULL,
    [Level] NVARCHAR(20) NOT NULL,
    [EventName] NVARCHAR(100) NOT NULL,
    [Message] NVARCHAR(4000) NOT NULL,
    [PropertiesJson] NVARCHAR(MAX) NULL,
    [TraceId] NVARCHAR(100) NULL,
    [UserEmail] NVARCHAR(320) NULL,
    CONSTRAINT [PK_ApplicationLogs] PRIMARY KEY CLUSTERED ([Id] ASC)
);
GO

CREATE NONCLUSTERED INDEX [IX_ApplicationLogs_CreatedAtUtc]
    ON [dbo].[ApplicationLogs] ([CreatedAtUtc] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_ApplicationLogs_EventName]
    ON [dbo].[ApplicationLogs] ([EventName] ASC);
GO
