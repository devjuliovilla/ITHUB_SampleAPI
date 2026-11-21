IF NOT EXISTS (SELECT 1 FROM [dbo].[Statuses] WHERE [Id] = 1)
    INSERT INTO [dbo].[Statuses] ([Id], [Scope], [Code], [Name], [IsActive])
    VALUES (1, N'Task', N'Assigned', N'Assigned', 1);

IF NOT EXISTS (SELECT 1 FROM [dbo].[Statuses] WHERE [Id] = 2)
    INSERT INTO [dbo].[Statuses] ([Id], [Scope], [Code], [Name], [IsActive])
    VALUES (2, N'Task', N'InProgress', N'In progress', 1);

IF NOT EXISTS (SELECT 1 FROM [dbo].[Statuses] WHERE [Id] = 3)
    INSERT INTO [dbo].[Statuses] ([Id], [Scope], [Code], [Name], [IsActive])
    VALUES (3, N'Task', N'Completed', N'Completed', 1);

IF NOT EXISTS (SELECT 1 FROM [dbo].[Statuses] WHERE [Id] = 4)
    INSERT INTO [dbo].[Statuses] ([Id], [Scope], [Code], [Name], [IsActive])
    VALUES (4, N'Activity', N'Pending', N'Pending', 1);

IF NOT EXISTS (SELECT 1 FROM [dbo].[Statuses] WHERE [Id] = 5)
    INSERT INTO [dbo].[Statuses] ([Id], [Scope], [Code], [Name], [IsActive])
    VALUES (5, N'Activity', N'InProgress', N'In progress', 1);

IF NOT EXISTS (SELECT 1 FROM [dbo].[Statuses] WHERE [Id] = 6)
    INSERT INTO [dbo].[Statuses] ([Id], [Scope], [Code], [Name], [IsActive])
    VALUES (6, N'Activity', N'Completed', N'Completed', 1);
