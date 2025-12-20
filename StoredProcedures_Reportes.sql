-- =============================================
-- STORED PROCEDURES PARA REPORTES Y ESTADÍSTICAS
-- Sistema de Biblioteca - CTP JICARAL
-- =============================================

USE AppBibliteca2;
GO

-- =============================================
-- 1. usp_GetDashboardEstadisticas
-- Obtiene estadísticas generales para el dashboard
-- =============================================
IF OBJECT_ID('dbo.usp_GetDashboardEstadisticas', 'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_GetDashboardEstadisticas;
GO

CREATE PROCEDURE dbo.usp_GetDashboardEstadisticas
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        -- Materiales y Ejemplares
        (SELECT COUNT(DISTINCT TN_Id_Material)
         FROM TCTPB_Cat_Materiales
         WHERE TC_Estado = 'Disponible') AS TotalMateriales,

        (SELECT ISNULL(SUM(TN_Cantidad), 0)
         FROM TCTPB_Cat_Materiales
         WHERE TC_Estado = 'Disponible') AS TotalEjemplares,

        (SELECT ISNULL(SUM(TN_Disponibles), 0)
         FROM TCTPB_Cat_Materiales
         WHERE TC_Estado = 'Disponible') AS EjemplaresDisponibles,

        -- Usuarios
        (SELECT COUNT(*)
         FROM TCTPB_Reg_Usuarios
         WHERE TB_Activo = 1) AS TotalUsuarios,

        (SELECT COUNT(DISTINCT u.TN_Id_Usuario)
         FROM TCTPB_Reg_Usuarios u
         INNER JOIN TCTPB_Reg_UsuarioRoles ur ON u.TN_Id_Usuario = ur.TN_Id_Usuario
         INNER JOIN TCTPB_Cat_Roles r ON ur.TN_Id_Role = r.TN_Id_Role
         WHERE u.TB_Activo = 1 AND r.TC_Role = 'Estudiante') AS TotalEstudiantes,

        -- Préstamos
        (SELECT COUNT(*)
         FROM TCTPB_Pro_Prestamos
         WHERE TN_Id_Estado = 2) AS PrestamosActivos, -- Estado 2 = Prestado

        (SELECT COUNT(*)
         FROM TCTPB_Pro_Prestamos
         WHERE TN_Id_Estado = 1) AS ReservasPendientes, -- Estado 1 = Reservado

        (SELECT COUNT(*)
         FROM TCTPB_Pro_Prestamos
         WHERE TN_Id_Estado = 2
           AND TF_FechaDevolucion < GETDATE()
           AND TF_FechaDevuelto IS NULL) AS PrestamosAtrasados,

        -- Recursos Externos
        (SELECT COUNT(*)
         FROM TCTPB_Reg_ContenidosExternos
         WHERE TB_Activo = 1) AS TotalRecursosExternos;
END;
GO

-- =============================================
-- 2. usp_GetPrestamosPorEstado
-- Obtiene cantidad de préstamos por estado
-- =============================================
IF OBJECT_ID('dbo.usp_GetPrestamosPorEstado', 'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_GetPrestamosPorEstado;
GO

CREATE PROCEDURE dbo.usp_GetPrestamosPorEstado
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        e.TC_Estado AS Estado,
        COUNT(p.TN_Id_Prestamo) AS Cantidad
    FROM TCTPB_Cat_EstadoPrestamo e
    LEFT JOIN TCTPB_Pro_Prestamos p ON e.TN_Id_Estado = p.TN_Id_Estado
    GROUP BY e.TC_Estado, e.TN_Id_Estado
    ORDER BY e.TN_Id_Estado;
END;
GO

-- =============================================
-- 3. usp_GetPrestamosPorMes
-- Obtiene préstamos por mes (últimos N meses)
-- =============================================
IF OBJECT_ID('dbo.usp_GetPrestamosPorMes', 'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_GetPrestamosPorMes;
GO

CREATE PROCEDURE dbo.usp_GetPrestamosPorMes
    @Meses INT = 12
AS
BEGIN
    SET NOCOUNT ON;

    -- Crear tabla temporal con los últimos N meses
    DECLARE @FechaInicio DATE = DATEADD(MONTH, -@Meses, GETDATE());

    WITH Meses AS (
        SELECT
            YEAR(DATEADD(MONTH, number, @FechaInicio)) AS Anio,
            MONTH(DATEADD(MONTH, number, @FechaInicio)) AS Mes
        FROM master..spt_values
        WHERE type = 'P' AND number <= @Meses
    )
    SELECT
        m.Anio,
        m.Mes,
        CASE m.Mes
            WHEN 1 THEN 'Enero'
            WHEN 2 THEN 'Febrero'
            WHEN 3 THEN 'Marzo'
            WHEN 4 THEN 'Abril'
            WHEN 5 THEN 'Mayo'
            WHEN 6 THEN 'Junio'
            WHEN 7 THEN 'Julio'
            WHEN 8 THEN 'Agosto'
            WHEN 9 THEN 'Septiembre'
            WHEN 10 THEN 'Octubre'
            WHEN 11 THEN 'Noviembre'
            WHEN 12 THEN 'Diciembre'
        END AS NombreMes,
        ISNULL(COUNT(p.TN_Id_Prestamo), 0) AS TotalPrestamos,
        ISNULL(SUM(CASE WHEN p.TN_Id_Estado = 3 THEN 1 ELSE 0 END), 0) AS Devueltos,
        ISNULL(SUM(CASE WHEN p.TN_Id_Estado = 2 AND p.TF_FechaDevolucion < GETDATE() AND p.TF_FechaDevuelto IS NULL THEN 1 ELSE 0 END), 0) AS Atrasados
    FROM Meses m
    LEFT JOIN TCTPB_Pro_Prestamos p
        ON YEAR(p.TF_FechaPrestamo) = m.Anio
        AND MONTH(p.TF_FechaPrestamo) = m.Mes
    GROUP BY m.Anio, m.Mes
    ORDER BY m.Anio, m.Mes;
END;
GO

-- =============================================
-- 4. usp_GetPrestamosPorCategoria
-- Obtiene cantidad de préstamos por categoría
-- =============================================
IF OBJECT_ID('dbo.usp_GetPrestamosPorCategoria', 'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_GetPrestamosPorCategoria;
GO

CREATE PROCEDURE dbo.usp_GetPrestamosPorCategoria
    @FechaInicio DATE = NULL,
    @FechaFin DATE = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        c.TC_Categoria AS Categoria,
        COUNT(DISTINCT pd.TN_Id_Prestamo) AS CantidadPrestamos
    FROM TCTPB_Cat_Categorias c
    LEFT JOIN TCTPB_Cat_Materiales m ON c.TN_Id_Categoria = m.TN_Id_Categoria
    LEFT JOIN TCTPB_Reg_PrestamoDetalle pd ON m.TN_Id_Material = pd.TN_Id_Material
    LEFT JOIN TCTPB_Pro_Prestamos p ON pd.TN_Id_Prestamo = p.TN_Id_Prestamo
    WHERE (@FechaInicio IS NULL OR p.TF_FechaPrestamo >= @FechaInicio)
      AND (@FechaFin IS NULL OR p.TF_FechaPrestamo <= @FechaFin)
    GROUP BY c.TC_Categoria
    HAVING COUNT(DISTINCT pd.TN_Id_Prestamo) > 0
    ORDER BY CantidadPrestamos DESC;
END;
GO

-- =============================================
-- 5. usp_GetTop10MaterialesPrestados
-- Obtiene los 10 materiales más prestados
-- =============================================
IF OBJECT_ID('dbo.usp_GetTop10MaterialesPrestados', 'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_GetTop10MaterialesPrestados;
GO

CREATE PROCEDURE dbo.usp_GetTop10MaterialesPrestados
    @FechaInicio DATE = NULL,
    @FechaFin DATE = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP 10
        m.TN_Id_Material,
        m.TC_Titulo,
        m.TC_ISBN,
        c.TC_Categoria,
        COUNT(DISTINCT pd.TN_Id_Prestamo) AS VecesPrestado,
        m.TN_Disponibles,
        m.TN_Cantidad
    FROM TCTPB_Cat_Materiales m
    INNER JOIN TCTPB_Cat_Categorias c ON m.TN_Id_Categoria = c.TN_Id_Categoria
    LEFT JOIN TCTPB_Reg_PrestamoDetalle pd ON m.TN_Id_Material = pd.TN_Id_Material
    LEFT JOIN TCTPB_Pro_Prestamos p ON pd.TN_Id_Prestamo = p.TN_Id_Prestamo
    WHERE m.TC_Estado = 'Disponible'
      AND (@FechaInicio IS NULL OR p.TF_FechaPrestamo >= @FechaInicio)
      AND (@FechaFin IS NULL OR p.TF_FechaPrestamo <= @FechaFin)
    GROUP BY m.TN_Id_Material, m.TC_Titulo, m.TC_ISBN, c.TC_Categoria, m.TN_Disponibles, m.TN_Cantidad
    ORDER BY VecesPrestado DESC;
END;
GO

-- =============================================
-- 6. usp_GetTop10UsuariosActivos
-- Obtiene los 10 usuarios más activos
-- =============================================
IF OBJECT_ID('dbo.usp_GetTop10UsuariosActivos', 'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_GetTop10UsuariosActivos;
GO

CREATE PROCEDURE dbo.usp_GetTop10UsuariosActivos
    @FechaInicio DATE = NULL,
    @FechaFin DATE = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP 10
        u.TN_Id_Usuario,
        CONCAT(u.TC_UserName, ' ', u.TC_LastName) AS NombreCompleto,
        u.TC_Email,
        r.TC_Role AS Rol,
        COUNT(p.TN_Id_Prestamo) AS TotalPrestamos,
        SUM(CASE WHEN p.TN_Id_Estado = 2 THEN 1 ELSE 0 END) AS PrestamosActivos,
        SUM(CASE WHEN p.TN_Id_Estado = 2 AND p.TF_FechaDevolucion < GETDATE() AND p.TF_FechaDevuelto IS NULL THEN 1 ELSE 0 END) AS PrestamosAtrasados
    FROM TCTPB_Reg_Usuarios u
    LEFT JOIN TCTPB_Reg_UsuarioRoles ur ON u.TN_Id_Usuario = ur.TN_Id_Usuario
    LEFT JOIN TCTPB_Cat_Roles r ON ur.TN_Id_Role = r.TN_Id_Role
    LEFT JOIN TCTPB_Pro_Prestamos p ON u.TN_Id_Usuario = p.TN_Id_Usuario
    WHERE u.TB_Activo = 1
      AND (@FechaInicio IS NULL OR p.TF_FechaPrestamo >= @FechaInicio)
      AND (@FechaFin IS NULL OR p.TF_FechaPrestamo <= @FechaFin)
    GROUP BY u.TN_Id_Usuario, u.TC_UserName, u.TC_LastName, u.TC_Email, r.TC_Role
    HAVING COUNT(p.TN_Id_Prestamo) > 0
    ORDER BY TotalPrestamos DESC;
END;
GO

-- =============================================
-- 7. usp_GetPrestamosPorPeriodo
-- Obtiene préstamos en un rango de fechas
-- =============================================
IF OBJECT_ID('dbo.usp_GetPrestamosPorPeriodo', 'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_GetPrestamosPorPeriodo;
GO

CREATE PROCEDURE dbo.usp_GetPrestamosPorPeriodo
    @FechaInicio DATE = NULL,
    @FechaFin DATE = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        p.TN_Id_Prestamo,
        CONCAT(u.TC_UserName, ' ', u.TC_LastName) AS NombreUsuario,
        u.TC_Email,
        e.TC_Estado AS EstadoPrestamo,
        p.TF_FechaPrestamo,
        p.TF_FechaDevolucion,
        p.TF_FechaDevuelto,
        CASE
            WHEN p.TN_Id_Estado = 2 AND p.TF_FechaDevolucion < GETDATE() AND p.TF_FechaDevuelto IS NULL
            THEN DATEDIFF(DAY, p.TF_FechaDevolucion, GETDATE())
            ELSE 0
        END AS TN_DiasAtraso,
        (SELECT COUNT(*)
         FROM TCTPB_Reg_PrestamoDetalle pd
         WHERE pd.TN_Id_Prestamo = p.TN_Id_Prestamo) AS CantidadMateriales
    FROM TCTPB_Pro_Prestamos p
    INNER JOIN TCTPB_Reg_Usuarios u ON p.TN_Id_Usuario = u.TN_Id_Usuario
    INNER JOIN TCTPB_Cat_EstadoPrestamo e ON p.TN_Id_Estado = e.TN_Id_Estado
    WHERE (@FechaInicio IS NULL OR p.TF_FechaPrestamo >= @FechaInicio)
      AND (@FechaFin IS NULL OR p.TF_FechaPrestamo <= @FechaFin)
    ORDER BY p.TF_FechaPrestamo DESC;
END;
GO

-- =============================================
-- 8. usp_GetReporteAtrasos
-- Obtiene todos los préstamos atrasados
-- =============================================
IF OBJECT_ID('dbo.usp_GetReporteAtrasos', 'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_GetReporteAtrasos;
GO

CREATE PROCEDURE dbo.usp_GetReporteAtrasos
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        p.TN_Id_Prestamo,
        CONCAT(u.TC_UserName, ' ', u.TC_LastName) AS NombreUsuario,
        u.TC_Email,
        u.TC_Telefono,
        p.TF_FechaPrestamo,
        p.TF_FechaDevolucion,
        DATEDIFF(DAY, p.TF_FechaDevolucion, GETDATE()) AS DiasAtraso,
        (SELECT COUNT(*)
         FROM TCTPB_Reg_PrestamoDetalle pd
         WHERE pd.TN_Id_Prestamo = p.TN_Id_Prestamo) AS CantidadMateriales
    FROM TCTPB_Pro_Prestamos p
    INNER JOIN TCTPB_Reg_Usuarios u ON p.TN_Id_Usuario = u.TN_Id_Usuario
    WHERE p.TN_Id_Estado = 2 -- Estado Prestado
      AND p.TF_FechaDevolucion < GETDATE()
      AND p.TF_FechaDevuelto IS NULL
    ORDER BY DiasAtraso DESC;
END;
GO

-- =============================================
-- 9. usp_GetEstadisticasDisponibilidad
-- Obtiene estadísticas de disponibilidad por categoría
-- =============================================
IF OBJECT_ID('dbo.usp_GetEstadisticasDisponibilidad', 'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_GetEstadisticasDisponibilidad;
GO

CREATE PROCEDURE dbo.usp_GetEstadisticasDisponibilidad
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        c.TC_Categoria AS Categoria,
        COUNT(DISTINCT m.TN_Id_Material) AS TotalMateriales,
        ISNULL(SUM(m.TN_Cantidad), 0) AS TotalEjemplares,
        ISNULL(SUM(m.TN_Disponibles), 0) AS EjemplaresDisponibles,
        ISNULL(SUM(m.TN_Prestados), 0) AS EjemplaresPrestados,
        CASE
            WHEN ISNULL(SUM(m.TN_Cantidad), 0) > 0
            THEN CAST((ISNULL(SUM(m.TN_Disponibles), 0) * 100.0 / SUM(m.TN_Cantidad)) AS DECIMAL(5,2))
            ELSE 0
        END AS PorcentajeDisponibilidad
    FROM TCTPB_Cat_Categorias c
    LEFT JOIN TCTPB_Cat_Materiales m
        ON c.TN_Id_Categoria = m.TN_Id_Categoria
        AND m.TC_Estado = 'Disponible'
    GROUP BY c.TN_Id_Categoria, c.TC_Categoria
    HAVING COUNT(DISTINCT m.TN_Id_Material) > 0
    ORDER BY c.TC_Categoria;
END;
GO

-- =============================================
-- 10. usp_GetReporteRecursosExternos
-- Obtiene reporte de recursos externos por asignatura
-- =============================================
IF OBJECT_ID('dbo.usp_GetReporteRecursosExternos', 'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_GetReporteRecursosExternos;
GO

CREATE PROCEDURE dbo.usp_GetReporteRecursosExternos
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        a.TC_Nombre AS Asignatura,
        t.TC_TipoContenido AS TipoContenido,
        COUNT(c.TN_Id_Contenido) AS CantidadRecursos,
        MAX(c.TF_FechaCreacion) AS UltimaPublicacion
    FROM TCTPB_Cat_Asignaturas a
    LEFT JOIN TCTPB_Reg_ContenidosExternos c
        ON a.TN_Id_Asignatura = c.TN_Id_Asignatura
        AND c.TB_Activo = 1
    LEFT JOIN TCTPB_Cat_TiposContenido t
        ON c.TN_Id_TipoContenido = t.TN_Id_TipoContenido
    WHERE a.TB_Activo = 1
    GROUP BY a.TC_Nombre, t.TC_TipoContenido
    HAVING COUNT(c.TN_Id_Contenido) > 0
    ORDER BY a.TC_Nombre, CantidadRecursos DESC;
END;
GO

PRINT '========================================';
PRINT 'Stored Procedures creados exitosamente:';
PRINT '========================================';
PRINT '1. usp_GetDashboardEstadisticas';
PRINT '2. usp_GetPrestamosPorEstado';
PRINT '3. usp_GetPrestamosPorMes';
PRINT '4. usp_GetPrestamosPorCategoria';
PRINT '5. usp_GetTop10MaterialesPrestados';
PRINT '6. usp_GetTop10UsuariosActivos';
PRINT '7. usp_GetPrestamosPorPeriodo';
PRINT '8. usp_GetReporteAtrasos';
PRINT '9. usp_GetEstadisticasDisponibilidad';
PRINT '10. usp_GetReporteRecursosExternos';
PRINT '========================================';
