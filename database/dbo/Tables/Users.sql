CREATE TABLE [dbo].[Users]
(
    [Id] BIGINT IDENTITY (1, 1) NOT NULL,
    [Email] NVARCHAR(320) NOT NULL,
    [DisplayName] NVARCHAR(120) NOT NULL,
    [PasswordHash] NVARCHAR(500) NOT NULL,
    [Role] NVARCHAR(20) NOT NULL CONSTRAINT [DF_Users_Role] DEFAULT ('User'),
    [CreatedAtUtc] DATETIME2 NOT NULL,
    [IsActive] BIT NOT NULL,
    CONSTRAINT [PK_Users] PRIMARY KEY CLUSTERED ([Id] ASC)
);
GO

CREATE UNIQUE NONCLUSTERED INDEX [UX_Users_Email]
    ON [dbo].[Users] ([Email] ASC);
GO
