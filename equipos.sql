IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [Categorias] (
    [Id] int NOT NULL IDENTITY,
    [Nombre] nvarchar(100) NOT NULL,
    CONSTRAINT [PK_Categorias] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Usuarios] (
    [Id] int NOT NULL IDENTITY,
    [Username] nvarchar(50) NOT NULL,
    [PasswordHash] nvarchar(max) NOT NULL,
    [Email] nvarchar(150) NOT NULL,
    [Rol] nvarchar(20) NOT NULL,
    CONSTRAINT [PK_Usuarios] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Equipos] (
    [Id] int NOT NULL IDENTITY,
    [CodigoInventario] nvarchar(50) NOT NULL,
    [Nombre] nvarchar(150) NOT NULL,
    [Descripcion] nvarchar(max) NULL,
    [CategoriaId] int NOT NULL,
    [Estado] nvarchar(20) NOT NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_Equipos] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Equipos_Categorias_CategoriaId] FOREIGN KEY ([CategoriaId]) REFERENCES [Categorias] ([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [Prestamos] (
    [Id] int NOT NULL IDENTITY,
    [EquipoId] int NOT NULL,
    [UsuarioId] int NOT NULL,
    [FechaPrestamo] datetime2 NOT NULL,
    [FechaVencimiento] datetime2 NOT NULL,
    [FechaDevolucion] datetime2 NULL,
    [Observaciones] nvarchar(max) NULL,
    CONSTRAINT [PK_Prestamos] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Prestamos_Equipos_EquipoId] FOREIGN KEY ([EquipoId]) REFERENCES [Equipos] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Prestamos_Usuarios_UsuarioId] FOREIGN KEY ([UsuarioId]) REFERENCES [Usuarios] ([Id]) ON DELETE NO ACTION
);
GO

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Nombre') AND [object_id] = OBJECT_ID(N'[Categorias]'))
    SET IDENTITY_INSERT [Categorias] ON;
INSERT INTO [Categorias] ([Id], [Nombre])
VALUES (1, N'Laptops'),
(2, N'Proyectores'),
(3, N'Kits IoT'),
(4, N'Tarjetas Arduino');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Nombre') AND [object_id] = OBJECT_ID(N'[Categorias]'))
    SET IDENTITY_INSERT [Categorias] OFF;
GO

CREATE UNIQUE INDEX [IX_Categorias_Nombre] ON [Categorias] ([Nombre]);
GO

CREATE INDEX [IX_Equipos_CategoriaId] ON [Equipos] ([CategoriaId]);
GO

CREATE UNIQUE INDEX [IX_Equipos_CodigoInventario] ON [Equipos] ([CodigoInventario]);
GO

CREATE INDEX [IX_Prestamos_EquipoId] ON [Prestamos] ([EquipoId]);
GO

CREATE INDEX [IX_Prestamos_UsuarioId] ON [Prestamos] ([UsuarioId]);
GO

CREATE UNIQUE INDEX [IX_Usuarios_Username] ON [Usuarios] ([Username]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20261028031212_InitialCreate', N'8.0.11');
GO

COMMIT;
GO

