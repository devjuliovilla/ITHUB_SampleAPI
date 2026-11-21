CREATE TABLE [dbo].[Statuses]
(
    [Id] INT NOT NULL,
    [Scope] NVARCHAR(20) NOT NULL,
    [Code] NVARCHAR(30) NOT NULL,
    [Name] NVARCHAR(100) NOT NULL,
    [IsActive] BIT NOT NULL CONSTRAINT [DF_Statuses_IsActive] DEFAULT (1),
    CONSTRAINT [PK_Statuses] PRIMARY KEY CLUSTERED ([Id] ASC)
);
GO

CREATE UNIQUE NONCLUSTERED INDEX [UX_Statuses_Scope_Code]
    ON [dbo].[Statuses] ([Scope] ASC, [Code] ASC);
GO
