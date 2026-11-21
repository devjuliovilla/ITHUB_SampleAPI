CREATE TABLE [dbo].[Tasks]
(
    [Id] BIGINT IDENTITY (1, 1) NOT NULL,
    [Title] NVARCHAR(200) NOT NULL,
    [Description] NVARCHAR(MAX) NOT NULL,
    [AssignedUserId] BIGINT NOT NULL,
    [CreatedByUserId] BIGINT NOT NULL,
    [StatusId] INT NOT NULL CONSTRAINT [DF_Tasks_StatusId] DEFAULT (1),
    [CreatedAtUtc] DATETIME2 NOT NULL,
    [UpdatedAtUtc] DATETIME2 NOT NULL,
    CONSTRAINT [PK_Tasks] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Tasks_Statuses_StatusId] FOREIGN KEY ([StatusId]) REFERENCES [dbo].[Statuses] ([Id]),
    CONSTRAINT [FK_Tasks_Users_AssignedUserId] FOREIGN KEY ([AssignedUserId]) REFERENCES [dbo].[Users] ([Id]),
    CONSTRAINT [FK_Tasks_Users_CreatedByUserId] FOREIGN KEY ([CreatedByUserId]) REFERENCES [dbo].[Users] ([Id])
);
GO

CREATE NONCLUSTERED INDEX [IX_Tasks_AssignedUserId]
    ON [dbo].[Tasks] ([AssignedUserId] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_Tasks_StatusId]
    ON [dbo].[Tasks] ([StatusId] ASC);
GO
