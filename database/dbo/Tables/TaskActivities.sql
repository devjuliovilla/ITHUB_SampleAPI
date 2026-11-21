CREATE TABLE [dbo].[TaskActivities]
(
    [Id] BIGINT IDENTITY (1, 1) NOT NULL,
    [TaskId] BIGINT NOT NULL,
    [CreatedByUserId] BIGINT NOT NULL,
    [Title] NVARCHAR(200) NOT NULL,
    [Description] NVARCHAR(MAX) NOT NULL,
    [StatusId] INT NOT NULL CONSTRAINT [DF_TaskActivities_StatusId] DEFAULT (4),
    [CreatedAtUtc] DATETIME2 NOT NULL,
    [UpdatedAtUtc] DATETIME2 NOT NULL,
    CONSTRAINT [PK_TaskActivities] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_TaskActivities_Statuses_StatusId] FOREIGN KEY ([StatusId]) REFERENCES [dbo].[Statuses] ([Id]),
    CONSTRAINT [FK_TaskActivities_Tasks_TaskId] FOREIGN KEY ([TaskId]) REFERENCES [dbo].[Tasks] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_TaskActivities_Users_CreatedByUserId] FOREIGN KEY ([CreatedByUserId]) REFERENCES [dbo].[Users] ([Id])
);
GO

CREATE NONCLUSTERED INDEX [IX_TaskActivities_TaskId]
    ON [dbo].[TaskActivities] ([TaskId] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_TaskActivities_StatusId]
    ON [dbo].[TaskActivities] ([StatusId] ASC);
GO
