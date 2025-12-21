# Cambios Necesarios en Base de Datos para Sistema de Bajas

## 1. Crear Tabla de Historial de Bajas

```sql
CREATE TABLE TCTPB_Reg_Bajas_Materiales (
    TN_Id_Baja INT IDENTITY(1,1) PRIMARY KEY,
    TN_Id_Material INT NOT NULL,
    TN_Cantidad INT NOT NULL,
    TC_Observacion NVARCHAR(500) NOT NULL,
    TF_Fecha_Baja DATETIME NOT NULL DEFAULT GETDATE(),
    TN_Id_Usuario INT NOT NULL,
    CONSTRAINT FK_Bajas_Material FOREIGN KEY (TN_Id_Material)
        REFERENCES TCTPB_Cat_Materiales(TN_Id_Material),
    CONSTRAINT FK_Bajas_Usuario FOREIGN KEY (TN_Id_Usuario)
        REFERENCES TCTPB_Reg_Usuarios(TN_Id_Usuario)
);
```

## 2. Modificar Stored Procedure usp_DarDeBajaMaterial

El stored procedure debe:
- Recibir `@Cantidad` en lugar de cambiar el estado de todo el material
- Decrementar la cantidad total y disponible del material
- Insertar un registro en la tabla de historial de bajas
- Validar que la cantidad a dar de baja no sea mayor a la disponible

```sql
ALTER PROCEDURE usp_DarDeBajaMaterial
    @Id_Material INT,
    @Cantidad INT,
    @Id_Usuario INT,
    @Observacion NVARCHAR(500)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @CantidadActual INT;
    DECLARE @DisponiblesActual INT;

    -- Validar que el material existe
    SELECT @CantidadActual = TN_Cantidad,
           @DisponiblesActual = TN_Disponibles
    FROM TCTPB_Cat_Materiales
    WHERE TN_Id_Material = @Id_Material;

    IF @CantidadActual IS NULL
    BEGIN
        RAISERROR('El material no existe.', 16, 1);
        RETURN;
    END

    -- Validar que hay suficientes ejemplares disponibles
    IF @Cantidad > @DisponiblesActual
    BEGIN
        RAISERROR('No hay suficientes ejemplares disponibles para dar de baja.', 16, 1);
        RETURN;
    END

    BEGIN TRANSACTION;

    BEGIN TRY
        -- Decrementar la cantidad total y disponible
        UPDATE TCTPB_Cat_Materiales
        SET TN_Cantidad = TN_Cantidad - @Cantidad,
            TN_Disponibles = TN_Disponibles - @Cantidad
        WHERE TN_Id_Material = @Id_Material;

        -- Registrar en el historial de bajas
        INSERT INTO TCTPB_Reg_Bajas_Materiales
            (TN_Id_Material, TN_Cantidad, TC_Observacion, TN_Id_Usuario)
        VALUES
            (@Id_Material, @Cantidad, @Observacion, @Id_Usuario);

        -- Si la cantidad llega a 0, cambiar estado a "Dado de baja"
        UPDATE TCTPB_Cat_Materiales
        SET TC_Estado = 'Dado de baja'
        WHERE TN_Id_Material = @Id_Material
          AND TN_Cantidad = 0;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO
```

## 3. Crear Stored Procedure para Obtener Historial de Bajas

```sql
CREATE PROCEDURE usp_GetHistorialBajas
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        b.TN_Id_Baja,
        b.TN_Id_Material,
        m.TC_Titulo,
        m.TC_ISBN,
        b.TN_Cantidad,
        b.TC_Observacion,
        b.TF_Fecha_Baja,
        u.TC_UserName
    FROM TCTPB_Reg_Bajas_Materiales b
    INNER JOIN TCTPB_Cat_Materiales m ON b.TN_Id_Material = m.TN_Id_Material
    INNER JOIN TCTPB_Reg_Usuarios u ON b.TN_Id_Usuario = u.TN_Id_Usuario
    ORDER BY b.TF_Fecha_Baja DESC;
END
GO
```

## Notas Importantes

1. La tabla de bajas mantiene un registro histórico de todas las bajas realizadas
2. Cada baja registra la cantidad de ejemplares dados de baja, no todo el material
3. El material solo cambia a estado "Dado de baja" cuando la cantidad llega a 0
4. Se valida que no se pueda dar de baja más ejemplares de los disponibles
5. El historial muestra quién dio de baja, cuándo y el motivo
