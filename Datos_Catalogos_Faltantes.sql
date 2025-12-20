-- =============================================
-- DATOS PARA CATÁLOGOS FALTANTES
-- Inserta registros necesarios en tablas de catálogo
-- =============================================

USE AppBibliteca2;
GO

PRINT '========================================';
PRINT 'INSERTANDO DATOS EN CATÁLOGOS';
PRINT '========================================';
PRINT '';

-- =============================================
-- 1. Catálogo de Estados de Préstamo
-- =============================================
PRINT 'Verificando Estados de Préstamo...';

-- Verificar si existen los 4 estados principales
IF NOT EXISTS (SELECT 1 FROM TCTPB_Cat_EstadoPrestamo WHERE TN_Id_Estado = 1)
BEGIN
    INSERT INTO TCTPB_Cat_EstadoPrestamo (TN_Id_Estado, TC_Estado)
    VALUES (1, 'Reservado');
    PRINT '✓ Estado "Reservado" insertado';
END
ELSE
    PRINT '- Estado "Reservado" ya existe';

IF NOT EXISTS (SELECT 1 FROM TCTPB_Cat_EstadoPrestamo WHERE TN_Id_Estado = 2)
BEGIN
    INSERT INTO TCTPB_Cat_EstadoPrestamo (TN_Id_Estado, TC_Estado)
    VALUES (2, 'Prestado');
    PRINT '✓ Estado "Prestado" insertado';
END
ELSE
    PRINT '- Estado "Prestado" ya existe';

IF NOT EXISTS (SELECT 1 FROM TCTPB_Cat_EstadoPrestamo WHERE TN_Id_Estado = 3)
BEGIN
    INSERT INTO TCTPB_Cat_EstadoPrestamo (TN_Id_Estado, TC_Estado)
    VALUES (3, 'Devuelto');
    PRINT '✓ Estado "Devuelto" insertado';
END
ELSE
    PRINT '- Estado "Devuelto" ya existe';

IF NOT EXISTS (SELECT 1 FROM TCTPB_Cat_EstadoPrestamo WHERE TN_Id_Estado = 4)
BEGIN
    INSERT INTO TCTPB_Cat_EstadoPrestamo (TN_Id_Estado, TC_Estado)
    VALUES (4, 'Cancelado');
    PRINT '✓ Estado "Cancelado" insertado';
END
ELSE
    PRINT '- Estado "Cancelado" ya existe';

PRINT '';

-- =============================================
-- 2. Verificar datos en otras tablas catálogo
-- =============================================
PRINT 'Verificando otras tablas de catálogo...';

-- Categorías
DECLARE @TotalCategorias INT = (SELECT COUNT(*) FROM TCTPB_Cat_Categorias);
PRINT '- Categorías: ' + CAST(@TotalCategorias AS VARCHAR(10)) + ' registros';

-- Asignaturas
DECLARE @TotalAsignaturas INT = (SELECT COUNT(*) FROM TCTPB_Cat_Asignaturas);
PRINT '- Asignaturas: ' + CAST(@TotalAsignaturas AS VARCHAR(10)) + ' registros';

-- Tipos de Contenido
DECLARE @TotalTiposContenido INT = (SELECT COUNT(*) FROM TCTPB_Cat_TiposContenido);
PRINT '- Tipos de Contenido: ' + CAST(@TotalTiposContenido AS VARCHAR(10)) + ' registros';

-- Roles
DECLARE @TotalRoles INT = (SELECT COUNT(*) FROM TCTPB_Cat_Roles);
PRINT '- Roles: ' + CAST(@TotalRoles AS VARCHAR(10)) + ' registros';

PRINT '';
PRINT '========================================';
PRINT 'VERIFICACIÓN DE DATOS COMPLETADA';
PRINT '========================================';
PRINT '';

-- Mostrar conteo de registros principales
PRINT 'RESUMEN DE DATOS EN EL SISTEMA:';
PRINT '----------------------------------------';
PRINT 'Materiales: ' + CAST((SELECT COUNT(*) FROM TCTPB_Cat_Materiales WHERE TC_Estado = 'Disponible') AS VARCHAR(10));
PRINT 'Usuarios: ' + CAST((SELECT COUNT(*) FROM TCTPB_Reg_Usuarios WHERE TB_Activo = 1) AS VARCHAR(10));
PRINT 'Préstamos totales: ' + CAST((SELECT COUNT(*) FROM TCTPB_Pro_Prestamos) AS VARCHAR(10));
PRINT 'Préstamos activos: ' + CAST((SELECT COUNT(*) FROM TCTPB_Pro_Prestamos WHERE TN_Id_Estado = 2) AS VARCHAR(10));
PRINT 'Préstamos reservados: ' + CAST((SELECT COUNT(*) FROM TCTPB_Pro_Prestamos WHERE TN_Id_Estado = 1) AS VARCHAR(10));
PRINT 'Préstamos devueltos: ' + CAST((SELECT COUNT(*) FROM TCTPB_Pro_Prestamos WHERE TN_Id_Estado = 3) AS VARCHAR(10));
PRINT 'Recursos externos: ' + CAST((SELECT COUNT(*) FROM TCTPB_Reg_ContenidosExternos WHERE TB_Activo = 1) AS VARCHAR(10));
PRINT '';
