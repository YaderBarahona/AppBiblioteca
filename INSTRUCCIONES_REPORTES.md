# Solución para Reportes y Estadísticas

## Problema Identificado

Los reportes del sistema (Dashboard, Préstamos por Período, Disponibilidad) no están mostrando datos debido a que los **stored procedures necesarios no existen o no están devolviendo datos correctamente**.

## Síntomas

- ✗ Dashboard muestra todo en 0 y no pinta gráficos
- ✗ Préstamos por Período no renderiza resultados
- ✗ Disponibilidad no pinta nada
- ✓ Recursos Externos funciona correctamente

## Solución - Pasos a Seguir

### Paso 1: Diagnóstico (Opcional)

Ejecuta el script de diagnóstico para verificar qué stored procedures están faltando:

```sql
-- Ejecutar en SQL Server Management Studio o Azure Data Studio
-- Ubicación: Diagnostico_StoredProcedures.sql
```

Este script te mostrará una lista de todos los stored procedures requeridos y cuáles están faltando.

### Paso 2: Verificar y Completar Catálogos

Ejecuta el script para asegurar que los catálogos tengan los datos necesarios:

```sql
-- Ejecutar en SQL Server Management Studio o Azure Data Studio
-- Ubicación: Datos_Catalogos_Faltantes.sql
```

Este script:
- Inserta los 4 estados de préstamo si no existen:
  - ID 1: Reservado
  - ID 2: Prestado
  - ID 3: Devuelto
  - ID 4: Cancelado
- Muestra un resumen de los datos en el sistema

### Paso 3: Crear Stored Procedures

Ejecuta el script principal que crea todos los stored procedures necesarios:

```sql
-- Ejecutar en SQL Server Management Studio o Azure Data Studio
-- Ubicación: StoredProcedures_Reportes.sql
```

Este script creará 10 stored procedures:

1. **usp_GetDashboardEstadisticas** - Estadísticas generales del dashboard
2. **usp_GetPrestamosPorEstado** - Préstamos agrupados por estado
3. **usp_GetPrestamosPorMes** - Préstamos por mes (últimos 12 meses)
4. **usp_GetPrestamosPorCategoria** - Préstamos agrupados por categoría
5. **usp_GetTop10MaterialesPrestados** - Top 10 materiales más prestados
6. **usp_GetTop10UsuariosActivos** - Top 10 usuarios más activos
7. **usp_GetPrestamosPorPeriodo** - Préstamos filtrados por rango de fechas
8. **usp_GetReporteAtrasos** - Reporte de préstamos atrasados
9. **usp_GetEstadisticasDisponibilidad** - Disponibilidad de materiales por categoría
10. **usp_GetReporteRecursosExternos** - Reporte de recursos externos por asignatura

### Paso 4: Verificar la Solución

1. Reinicia tu aplicación ASP.NET Core si estaba en ejecución
2. Navega a **Reportes > Dashboard**
3. Verifica que:
   - Las tarjetas de estadísticas muestren números reales
   - Los gráficos se rendericen correctamente
4. Prueba **Reportes > Préstamos por Período**
   - Selecciona un rango de fechas
   - Verifica que se muestren resultados
5. Prueba **Reportes > Disponibilidad**
   - Verifica que se muestren las categorías con sus estadísticas

## Orden de Ejecución Recomendado

```
1. Diagnostico_StoredProcedures.sql      (Opcional - para verificar el estado actual)
2. Datos_Catalogos_Faltantes.sql         (Importante - asegura datos básicos)
3. StoredProcedures_Reportes.sql         (Crítico - crea los SPs necesarios)
```

## Notas Importantes

### Sobre la Base de Datos

- **Nombre de BD**: `AppBibliteca2`
- **Server**: `DESKTOP-0S2CDAI` (según appsettings.json)
- Los scripts están diseñados para **crear o reemplazar** los stored procedures existentes

### Datos de Prueba Actuales

Según los datos proporcionados:
- 1 Material: "El moto" (ISBN: 978-9968-48-004-7)
- 1 Usuario: Henry Morales (Admin)
- 1 Préstamo (ID=10, Estado=3 "Devuelto")
- 1 Recurso Externo

**Con tan pocos datos, los reportes pueden mostrar información limitada, pero al menos funcionarán correctamente.**

### Recomendaciones

1. **Agregar más datos de prueba** para visualizar mejor los reportes:
   - Más materiales en diferentes categorías
   - Más usuarios estudiantes
   - Préstamos en diferentes estados (Reservado, Prestado, Atrasado)
   - Más recursos externos en diferentes asignaturas

2. **Verificar la conexión** a la base de datos en `appsettings.json`:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=DESKTOP-0S2CDAI;Database=AppBibliteca2;Trusted_Connection=True;"
   }
   ```

## Archivos Creados

- `StoredProcedures_Reportes.sql` - Script principal con todos los SPs
- `Diagnostico_StoredProcedures.sql` - Script de verificación
- `Datos_Catalogos_Faltantes.sql` - Script para completar catálogos
- `INSTRUCCIONES_REPORTES.md` - Este archivo (instrucciones)

## Soporte

Si después de ejecutar estos scripts los reportes siguen sin funcionar:

1. Verifica que la conexión a la base de datos sea correcta
2. Revisa los logs de la aplicación en busca de errores
3. Ejecuta manualmente los stored procedures en SQL Server para verificar que devuelven datos
4. Verifica que tienes datos suficientes en las tablas para generar reportes

---

**Fecha**: 2025-12-20
**Versión**: 1.0
**Sistema**: Biblioteca CTP JICARAL
