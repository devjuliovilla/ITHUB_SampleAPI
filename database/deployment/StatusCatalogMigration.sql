SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF OBJECT_ID(N'[dbo].[Tasks]', N'U') IS NULL
   AND OBJECT_ID(N'[dbo].[TaskActivities]', N'U') IS NULL
BEGIN
    COMMIT TRANSACTION;
    SELECT N'No existing task/activity tables; migration skipped.' AS [MigrationStatus];
    RETURN;
END;

IF OBJECT_ID(N'[dbo].[Statuses]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Statuses]
    (
        [Id] INT NOT NULL,
        [Scope] NVARCHAR(20) NOT NULL,
        [Code] NVARCHAR(30) NOT NULL,
        [Name] NVARCHAR(100) NOT NULL,
        [IsActive] BIT NOT NULL CONSTRAINT [DF_Statuses_IsActive] DEFAULT (1),
        CONSTRAINT [PK_Statuses] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
    CREATE UNIQUE NONCLUSTERED INDEX [UX_Statuses_Scope_Code]
        ON [dbo].[Statuses] ([Scope] ASC, [Code] ASC);
END;

DECLARE @SeedStatuses TABLE
(
    [Id] INT NOT NULL,
    [Scope] NVARCHAR(20) NOT NULL,
    [Code] NVARCHAR(30) NOT NULL,
    [Name] NVARCHAR(100) NOT NULL
);

INSERT INTO @SeedStatuses ([Id], [Scope], [Code], [Name])
VALUES
    (1, N'Task', N'Assigned', N'Assigned'),
    (2, N'Task', N'InProgress', N'In progress'),
    (3, N'Task', N'Completed', N'Completed'),
    (4, N'Activity', N'Pending', N'Pending'),
    (5, N'Activity', N'InProgress', N'In progress'),
    (6, N'Activity', N'Completed', N'Completed');

IF EXISTS
(
    SELECT 1
    FROM @SeedStatuses seed
    INNER JOIN [dbo].[Statuses] existingStatus
        ON existingStatus.[Id] = seed.[Id]
        OR (existingStatus.[Scope] = seed.[Scope] AND existingStatus.[Code] = seed.[Code])
    WHERE existingStatus.[Id] <> seed.[Id]
       OR existingStatus.[Scope] <> seed.[Scope]
       OR existingStatus.[Code] <> seed.[Code]
)
    THROW 51000, 'Status catalog contains conflicting IDs or codes; resolve those rows before publishing.', 1;

INSERT INTO [dbo].[Statuses] ([Id], [Scope], [Code], [Name], [IsActive])
SELECT seed.[Id], seed.[Scope], seed.[Code], seed.[Name], 1
FROM @SeedStatuses seed
WHERE NOT EXISTS (SELECT 1 FROM [dbo].[Statuses] existingStatus WHERE existingStatus.[Id] = seed.[Id]);

IF OBJECT_ID(N'[dbo].[Tasks]', N'U') IS NOT NULL
BEGIN
    IF COL_LENGTH(N'dbo.Tasks', N'StatusId') IS NULL
        ALTER TABLE [dbo].[Tasks] ADD [StatusId] INT NULL;

    IF COL_LENGTH(N'dbo.Tasks', N'Status') IS NOT NULL
    BEGIN
        EXEC sys.sp_executesql N'
            IF EXISTS
            (
                SELECT 1
                FROM [dbo].[Tasks] task
                LEFT JOIN [dbo].[Statuses] status
                    ON status.[Scope] = N''Task'' AND status.[Code] = task.[Status]
                WHERE status.[Id] IS NULL
            )
                THROW 51001, ''A task has a status that is not present in the Task status catalog.'', 1;

            UPDATE task
            SET task.[StatusId] = status.[Id]
            FROM [dbo].[Tasks] task
            INNER JOIN [dbo].[Statuses] status
                ON status.[Scope] = N''Task'' AND status.[Code] = task.[Status];';
    END;

    EXEC sys.sp_executesql N'UPDATE [dbo].[Tasks] SET [StatusId] = 1 WHERE [StatusId] IS NULL;';

    EXEC sys.sp_executesql N'ALTER TABLE [dbo].[Tasks] ALTER COLUMN [StatusId] INT NOT NULL;';

    IF NOT EXISTS
    (
        SELECT 1 FROM sys.default_constraints
        WHERE [parent_object_id] = OBJECT_ID(N'[dbo].[Tasks]')
          AND [name] = N'DF_Tasks_StatusId'
    )
        EXEC sys.sp_executesql N'ALTER TABLE [dbo].[Tasks] ADD CONSTRAINT [DF_Tasks_StatusId] DEFAULT (1) FOR [StatusId];';

    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE [object_id] = OBJECT_ID(N'[dbo].[Tasks]') AND [name] = N'IX_Tasks_StatusId')
        EXEC sys.sp_executesql N'CREATE NONCLUSTERED INDEX [IX_Tasks_StatusId] ON [dbo].[Tasks] ([StatusId] ASC);';

    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE [parent_object_id] = OBJECT_ID(N'[dbo].[Tasks]') AND [name] = N'FK_Tasks_Statuses_StatusId')
        EXEC sys.sp_executesql N'ALTER TABLE [dbo].[Tasks] WITH CHECK ADD CONSTRAINT [FK_Tasks_Statuses_StatusId]
            FOREIGN KEY ([StatusId]) REFERENCES [dbo].[Statuses] ([Id]);';

    IF COL_LENGTH(N'dbo.Tasks', N'Status') IS NOT NULL
        EXEC sys.sp_executesql N'ALTER TABLE [dbo].[Tasks] DROP COLUMN [Status];';
END;

IF OBJECT_ID(N'[dbo].[TaskActivities]', N'U') IS NOT NULL
BEGIN
    IF COL_LENGTH(N'dbo.TaskActivities', N'StatusId') IS NULL
        ALTER TABLE [dbo].[TaskActivities] ADD [StatusId] INT NULL;

    IF COL_LENGTH(N'dbo.TaskActivities', N'Status') IS NOT NULL
    BEGIN
        EXEC sys.sp_executesql N'
            IF EXISTS
            (
                SELECT 1
                FROM [dbo].[TaskActivities] activity
                LEFT JOIN [dbo].[Statuses] status
                    ON status.[Scope] = N''Activity'' AND status.[Code] = activity.[Status]
                WHERE status.[Id] IS NULL
            )
                THROW 51002, ''An activity has a status that is not present in the Activity status catalog.'', 1;

            UPDATE activity
            SET activity.[StatusId] = status.[Id]
            FROM [dbo].[TaskActivities] activity
            INNER JOIN [dbo].[Statuses] status
                ON status.[Scope] = N''Activity'' AND status.[Code] = activity.[Status];';
    END;

    EXEC sys.sp_executesql N'UPDATE [dbo].[TaskActivities] SET [StatusId] = 4 WHERE [StatusId] IS NULL;';

    EXEC sys.sp_executesql N'ALTER TABLE [dbo].[TaskActivities] ALTER COLUMN [StatusId] INT NOT NULL;';

    IF NOT EXISTS
    (
        SELECT 1 FROM sys.default_constraints
        WHERE [parent_object_id] = OBJECT_ID(N'[dbo].[TaskActivities]')
          AND [name] = N'DF_TaskActivities_StatusId'
    )
        EXEC sys.sp_executesql N'ALTER TABLE [dbo].[TaskActivities] ADD CONSTRAINT [DF_TaskActivities_StatusId] DEFAULT (4) FOR [StatusId];';

    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE [object_id] = OBJECT_ID(N'[dbo].[TaskActivities]') AND [name] = N'IX_TaskActivities_StatusId')
        EXEC sys.sp_executesql N'CREATE NONCLUSTERED INDEX [IX_TaskActivities_StatusId] ON [dbo].[TaskActivities] ([StatusId] ASC);';

    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE [parent_object_id] = OBJECT_ID(N'[dbo].[TaskActivities]') AND [name] = N'FK_TaskActivities_Statuses_StatusId')
        EXEC sys.sp_executesql N'ALTER TABLE [dbo].[TaskActivities] WITH CHECK ADD CONSTRAINT [FK_TaskActivities_Statuses_StatusId]
            FOREIGN KEY ([StatusId]) REFERENCES [dbo].[Statuses] ([Id]);';

    IF COL_LENGTH(N'dbo.TaskActivities', N'Status') IS NOT NULL
        EXEC sys.sp_executesql N'ALTER TABLE [dbo].[TaskActivities] DROP COLUMN [Status];';
END;

COMMIT TRANSACTION;
SELECT N'Existing task/activity statuses migrated to the shared catalog.' AS [MigrationStatus];
