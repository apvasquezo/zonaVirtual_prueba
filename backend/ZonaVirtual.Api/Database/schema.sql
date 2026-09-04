-- =====================================================================
-- ZonaVirtual - Script de creacion de base de datos
-- Alternativa a las migraciones de EF Core (dotnet ef migrations add InitialCreate)
-- =====================================================================

IF DB_ID('ZonaVirtualDB') IS NULL
BEGIN
    CREATE DATABASE ZonaVirtualDB;
END
GO

USE ZonaVirtualDB;
GO

-- Login/usuario de conexion (ajusta la clave antes de usar en un ambiente real)
IF NOT EXISTS (SELECT * FROM sys.server_principals WHERE name = 'zv_user')
BEGIN
    CREATE LOGIN zv_user WITH PASSWORD = 'ClaveIncial+2026';
END
GO

IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = 'zv_user')
BEGIN
    CREATE USER zv_user FOR LOGIN zv_user;
    ALTER ROLE db_owner ADD MEMBER zv_user;
END
GO

IF OBJECT_ID('dbo.Comercios', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Comercios (
        Id              INT IDENTITY(1,1) PRIMARY KEY,
        ComercioCodigo  NVARCHAR(30)  NOT NULL,
        ComercioNombre  NVARCHAR(150) NOT NULL,
        ComercioNit     NVARCHAR(30)  NOT NULL,
        ComercioDireccion NVARCHAR(250) NOT NULL,
        CONSTRAINT UQ_Comercios_Codigo UNIQUE (ComercioCodigo),
        CONSTRAINT UQ_Comercios_Nit UNIQUE (ComercioNit)
    );
END
GO

IF OBJECT_ID('dbo.UsuariosPagadores', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.UsuariosPagadores (
        Id                      INT IDENTITY(1,1) PRIMARY KEY,
        UsuarioIdentificacion   NVARCHAR(30)  NOT NULL,
        UsuarioNombre           NVARCHAR(150) NOT NULL,
        UsuarioEmail            NVARCHAR(150) NOT NULL,
        CONSTRAINT UQ_Usuarios_Identificacion UNIQUE (UsuarioIdentificacion),
        CONSTRAINT UQ_Usuarios_Email UNIQUE (UsuarioEmail)
    );
END
GO

IF OBJECT_ID('dbo.Transacciones', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Transacciones (
        Id                  INT IDENTITY(1,1) PRIMARY KEY,
        TransCodigo         BIGINT NOT NULL,
        TransMedioPago      INT NOT NULL,
        TransEstado         INT NOT NULL,
        TransTotal          DECIMAL(18,2) NOT NULL,
        TransFecha          DATETIME2 NOT NULL,
        TransConcepto       NVARCHAR(300) NULL,
        ComercioId          INT NOT NULL,
        UsuarioPagadorId    INT NOT NULL,
        FechaCreacion       DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        FechaModificacion   DATETIME2 NULL,
        CONSTRAINT UQ_Transacciones_Codigo UNIQUE (TransCodigo),
        CONSTRAINT FK_Transacciones_Comercio FOREIGN KEY (ComercioId) REFERENCES dbo.Comercios(Id),
        CONSTRAINT FK_Transacciones_Usuario FOREIGN KEY (UsuarioPagadorId) REFERENCES dbo.UsuariosPagadores(Id)
    );

    CREATE INDEX IX_Transacciones_Fecha ON dbo.Transacciones(TransFecha);
    CREATE INDEX IX_Transacciones_Estado ON dbo.Transacciones(TransEstado);
    CREATE INDEX IX_Transacciones_Comercio ON dbo.Transacciones(ComercioId);
    CREATE INDEX IX_Transacciones_Usuario ON dbo.Transacciones(UsuarioPagadorId);
END
GO

IF OBJECT_ID('dbo.Cuentas', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Cuentas (
        Id                INT IDENTITY(1,1) PRIMARY KEY,
        Perfil            INT NOT NULL, -- 1 = Pagador, 2 = Comercio
        Username          NVARCHAR(150) NOT NULL,
        PasswordHash      NVARCHAR(300) NOT NULL,
        UsuarioPagadorId  INT NULL,
        ComercioId        INT NULL,
        FechaCreacion     DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        CONSTRAINT UQ_Cuentas_Perfil_Username UNIQUE (Perfil, Username),
        CONSTRAINT UQ_Cuentas_UsuarioPagador UNIQUE (UsuarioPagadorId),
        CONSTRAINT UQ_Cuentas_Comercio UNIQUE (ComercioId),
        CONSTRAINT FK_Cuentas_Usuario FOREIGN KEY (UsuarioPagadorId) REFERENCES dbo.UsuariosPagadores(Id) ON DELETE CASCADE,
        CONSTRAINT FK_Cuentas_Comercio FOREIGN KEY (ComercioId) REFERENCES dbo.Comercios(Id) ON DELETE CASCADE,
        CONSTRAINT CK_Cuentas_UnPerfil CHECK (
            (Perfil = 1 AND UsuarioPagadorId IS NOT NULL AND ComercioId IS NULL) OR
            (Perfil = 2 AND ComercioId IS NOT NULL AND UsuarioPagadorId IS NULL)
        )
    );
END
GO
