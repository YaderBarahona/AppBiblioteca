-- =============================================
-- SCRIPT DE DIAGNÓSTICO
-- Verifica la existencia de Stored Procedures
-- =============================================

USE AppBibliteca2;
GO

PRINT '========================================';
PRINT 'DIAGNÓSTICO DE STORED PROCEDURES';
PRINT '========================================';
PRINT '';

-- Lista de Stored Procedures requeridos
DECLARE @SPs TABLE (
    Nombre NVARCHAR(100),
    Descripcion NVARCHAR(255)
);

INSERT INTO @SPs (Nombre, Descripcion) VALUES
('usp_GetDashboardEstadisticas', 'Estadísticas generales del dashboard'),
('usp_GetPrestamosPorEstado', 'Préstamos agrupados por estado'),
('usp_GetPrestamosPorMes', 'Préstamos por mes (últimos 12 meses)'),
('usp_GetPrestamosPorCategoria', 'Préstamos agrupados por categoría'),
('usp_GetTop10MaterialesPrestados', 'Top 10 materiales más prestados'),
('usp_GetTop10UsuariosActivos', 'Top 10 usuarios más activos'),
('usp_GetPrestamosPorPeriodo', 'Préstamos filtrados por rango de fechas'),
('usp_GetReporteAtrasos', 'Reporte de préstamos atrasados'),
('usp_GetEstadisticasDisponibilidad', 'Disponibilidad de materiales por categoría'),
('usp_GetReporteRecursosExternos', 'Reporte de recursos externos por asignatura');

-- Verificar existencia
SELECT
    sp.Nombre,
    sp.Descripcion,
    CASE
        WHEN o.object_id IS NOT NULL THEN '✓ EXISTE'
        ELSE '✗ FALTA'
    END AS Estado,
    CASE
        WHEN o.object_id IS NOT NULL THEN o.create_date
        ELSE NULL
    END AS FechaCreacion
FROM @SPs sp
LEFT JOIN sys.objects o
    ON sp.Nombre = o.name
    AND o.type = 'P'
ORDER BY
    CASE WHEN o.object_id IS NULL THEN 0 ELSE 1 END,
    sp.Nombre;

PRINT '';
PRINT '========================================';
PRINT 'RESUMEN';
PRINT '========================================';

DECLARE @Total INT = (SELECT COUNT(*) FROM @SPs);
DECLARE @Existentes INT = (SELECT COUNT(*) FROM @SPs sp INNER JOIN sys.objects o ON sp.Nombre = o.name AND o.type = 'P');
DECLARE @Faltantes INT = @Total - @Existentes;

PRINT 'Total requeridos: ' + CAST(@Total AS VARCHAR(10));
PRINT 'Existentes: ' + CAST(@Existentes AS VARCHAR(10));
PRINT 'Faltantes: ' + CAST(@Faltantes AS VARCHAR(10));
PRINT '';

IF @Faltantes > 0
BEGIN
    PRINT '⚠ ACCIÓN REQUERIDA:';
    PRINT 'Ejecuta el script StoredProcedures_Reportes.sql';
    PRINT 'para crear los stored procedures faltantes.';
END
ELSE
BEGIN
    PRINT '✓ Todos los stored procedures están instalados.';
END

PRINT '========================================';
