-- =============================================
-- FIX: Stored Procedure usp_GetAutoresByMaterial
-- Corrige el error: TC_Nombre en MaterialesController.Details
-- =============================================

USE AppBibliteca2;
GO

IF OBJECT_ID('dbo.usp_GetAutoresByMaterial', 'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_GetAutoresByMaterial;
GO

CREATE PROCEDURE dbo.usp_GetAutoresByMaterial
    @Id_Material INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        a.TN_Id_Autor,
        a.TC_Nombre,
        a.TC_Apellidos,
        p.TC_Pais
    FROM TCTPB_Cat_MaterialesAutores ma
    INNER JOIN TCTPB_Cat_Autores a ON ma.TN_Id_Autor = a.TN_Id_Autor
    LEFT JOIN TCTPB_Cat_Paises p ON a.TN_Id_Pais = p.TN_Id_Pais
    WHERE ma.TN_Id_Material = @Id_Material
    ORDER BY a.TC_Apellidos, a.TC_Nombre;
END;
GO

PRINT 'Stored Procedure usp_GetAutoresByMaterial creado/actualizado correctamente';
PRINT 'Ahora la vista de Detalles de Materiales debe funcionar sin errores';
