-- =============================================
-- FIX: Stored Procedure usp_GetPrestamoDetalles
-- Corrige el error: TC_Nombre
-- =============================================

USE AppBibliteca2;
GO

IF OBJECT_ID('dbo.usp_GetPrestamoDetalles', 'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_GetPrestamoDetalles;
GO

CREATE PROCEDURE dbo.usp_GetPrestamoDetalles
    @Id_Prestamo INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        pd.TN_Id_Detalle,
        pd.TN_Id_Prestamo,
        pd.TN_Id_Material,
        m.TC_Titulo AS TituloMaterial,
        m.TC_ISBN AS ISBN,
        -- Concatenar autores del material
        STUFF((
            SELECT ', ' + CONCAT(a.TC_Apellidos, ', ', a.TC_Nombre)
            FROM TCTPB_Cat_MaterialesAutores ma
            INNER JOIN TCTPB_Cat_Autores a ON ma.TN_Id_Autor = a.TN_Id_Autor
            WHERE ma.TN_Id_Material = m.TN_Id_Material
            FOR XML PATH(''), TYPE
        ).value('.', 'NVARCHAR(MAX)'), 1, 2, '') AS Autores
    FROM TCTPB_Reg_PrestamoDetalle pd
    INNER JOIN TCTPB_Cat_Materiales m ON pd.TN_Id_Material = m.TN_Id_Material
    WHERE pd.TN_Id_Prestamo = @Id_Prestamo
    ORDER BY m.TC_Titulo;
END;
GO

PRINT 'Stored Procedure usp_GetPrestamoDetalles creado/actualizado correctamente';
