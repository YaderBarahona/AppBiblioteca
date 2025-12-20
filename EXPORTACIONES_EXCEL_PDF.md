# Exportación a Excel y PDF - Implementación Completa

## ✅ Funcionalidad Implementada

Se ha implementado la exportación de reportes a formatos Excel (.xlsx) y PDF para el módulo de **Préstamos por Período**.

## 📋 Características

### Exportación a Excel (EPPlus)
- ✓ Formato profesional con encabezados y estilos
- ✓ Título y metadata (fecha de generación, período)
- ✓ Resaltado de filas con atrasos (fondo rojo claro)
- ✓ Ajuste automático de columnas
- ✓ Totales y resúmenes
- ✓ Formato de fechas dd/MM/yyyy

### Exportación a PDF (QuestPDF)
- ✓ Diseño en formato horizontal (Landscape)
- ✓ Encabezado con título y metadata
- ✓ Tabla con datos formateados
- ✓ Resaltado visual de préstamos atrasados
- ✓ Pie de página con numeración
- ✓ Marca de agua "Biblioteca CTP Jicaral"

## 🔧 Instalación y Configuración

### Paso 1: Restaurar Paquetes NuGet

Los siguientes paquetes se han agregado al proyecto:

```xml
<PackageReference Include="EPPlus" Version="7.5.1" />
<PackageReference Include="QuestPDF" Version="2024.12.3" />
```

**Ejecuta el siguiente comando para restaurar:**

```bash
dotnet restore
```

O desde Visual Studio:
- Click derecho en el proyecto → **Restore NuGet Packages**

### Paso 2: Compilar el Proyecto

```bash
dotnet build
```

### Paso 3: Ejecutar la Aplicación

```bash
dotnet run
```

## 📖 Cómo Usar

### 1. Acceder al Reporte de Préstamos

1. Inicia sesión como **Bibliotecario** o **Administrador**
2. Navega a **Reportes → Préstamos por Período**
3. (Opcional) Selecciona un rango de fechas
4. Click en **Generar Reporte**

### 2. Exportar a Excel

1. Una vez generado el reporte, verás botones en la parte superior derecha
2. Click en el botón **Excel** (verde)
3. Se descargará automáticamente un archivo `.xlsx` con el nombre:
   - `Prestamos_YYYYMMDD_HHmmss.xlsx`
4. Abre el archivo en Excel, Google Sheets o LibreOffice

### 3. Exportar a PDF

1. Click en el botón **PDF** (rojo)
2. Se descargará automáticamente un archivo `.pdf` con el nombre:
   - `Prestamos_YYYYMMDD_HHmmss.pdf`
3. Abre el archivo con cualquier lector de PDF

## 🎨 Formato de los Archivos

### Excel (.xlsx)
```
┌─────────────────────────────────────────────────────┐
│  REPORTE DE PRÉSTAMOS POR PERÍODO  (Fondo azul)    │
├─────────────────────────────────────────────────────┤
│  Período: 01/12/2025 - 31/12/2025                  │
│  Fecha de generación: 20/12/2025 15:30             │
├───┬──────────┬─────────────┬────────┬──────────┬───┤
│ID │ Usuario  │   Email     │ Estado │   ...    │   │
├───┼──────────┼─────────────┼────────┼──────────┼───┤
│10 │ Henry M. │ yader@...   │Devuelto│01/12/2025│ 1 │
└───┴──────────┴─────────────┴────────┴──────────┴───┘
```

**Características:**
- Filas con atraso: Fondo rojo claro (#FFE6E6)
- Encabezados: Fondo gris claro con texto en negrita
- Título: Fondo azul (#0D6EFD) con texto blanco
- Todas las columnas auto-ajustadas

### PDF (.pdf)
```
┌─────────────────────────────────────────────────┐
│         REPORTE DE PRÉSTAMOS POR PERÍODO        │
│         (Encabezado azul con logo)              │
├─────────────────────────────────────────────────┤
│ Período: 01/12/2025 - 31/12/2025                │
│                     Generado: 20/12/2025 15:30  │
├────┬─────────┬──────────┬─────────┬──────────┬──┤
│ ID │ Usuario │  Email   │ Estado  │   ...    │  │
├────┼─────────┼──────────┼─────────┼──────────┼──┤
│ 10 │ Henry M.│yader@... │Devuelto │01/12/2025│1 │
└────┴─────────┴──────────┴─────────┴──────────┴──┘
            Página 1 de 1 - Biblioteca CTP Jicaral
```

**Características:**
- Formato horizontal (A4 Landscape)
- Filas con atraso: Fondo rojo muy claro
- Paginación automática
- Pie de página con numeración

## 🔍 Rutas de las Acciones

### Excel
```
GET /Reportes/ExportarPrestamosExcel?fechaInicio=2025-12-01&fechaFin=2025-12-31
```

### PDF
```
GET /Reportes/ExportarPrestamosPDF?fechaInicio=2025-12-01&fechaFin=2025-12-31
```

**Parámetros:**
- `fechaInicio` (opcional): Fecha de inicio en formato `YYYY-MM-DD`
- `fechaFin` (opcional): Fecha de fin en formato `YYYY-MM-DD`
- Si no se envían parámetros, se exportan **todos** los préstamos

## 📊 Datos Incluidos en la Exportación

Ambos formatos incluyen las siguientes columnas:

| Columna | Descripción |
|---------|-------------|
| ID | ID del préstamo |
| Usuario | Nombre completo del usuario |
| Email | Correo electrónico |
| Estado | Estado del préstamo (Reservado, Prestado, Devuelto, Cancelado) |
| F. Préstamo | Fecha en que se realizó el préstamo |
| F. Devolución | Fecha límite de devolución |
| F. Devuelto | Fecha real de devolución (si aplica) |
| Días Atraso | Cantidad de días de atraso (si aplica) |
| Materiales | Cantidad de materiales en el préstamo |

## ⚙️ Configuración de Licencias

### EPPlus (Excel)
```csharp
ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
```
- Configurada para uso **no comercial**
- Válida para uso educativo e institucional
- Para uso comercial, se requiere licencia de EPPlus

### QuestPDF (PDF)
```csharp
QuestPDF.Settings.License = LicenseType.Community;
```
- Configurada para **uso comunitario**
- Gratuita para proyectos no comerciales
- Para uso comercial, revisar licencias en [QuestPDF.com](https://www.questpdf.com/)

## 🚀 Extender a Otros Reportes

Para agregar exportación a otros reportes:

### 1. Crear métodos en el controlador

```csharp
[HttpGet]
public IActionResult ExportarAtrasosExcel()
{
    ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

    var atrasos = ObtenerAtrasosDesdeDB();

    using (var package = new ExcelPackage())
    {
        // Tu código de generación aquí
        // Seguir el patrón de ExportarPrestamosExcel()
    }
}
```

### 2. Actualizar la vista

```html
<a href="@Url.Action("ExportarAtrasosExcel", "Reportes")"
   class="btn btn-sm btn-outline-success">
    <i class="bi bi-file-earmark-excel"></i> Excel
</a>
```

## 🐛 Solución de Problemas

### Error: "Package EPPlus was not found"

**Solución:**
```bash
dotnet restore
```

### Error: "LicenseContext must be set"

**Solución:** Asegúrate de tener esta línea al inicio del método:
```csharp
ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
```

### El archivo se descarga vacío

**Verificaciones:**
1. Revisa que haya datos en el período seleccionado
2. Verifica la conexión a la base de datos
3. Revisa los logs de la aplicación en busca de errores

### Error de compilación con QuestPDF

**Solución:** Asegúrate de tener .NET 7.0 o superior:
```bash
dotnet --version
```

## 📝 Archivos Modificados

- ✓ `AppBiblioteca.csproj` - Agregados paquetes NuGet
- ✓ `Controllers/ReportesController.cs` - Agregados métodos de exportación
- ✓ `Views/Reportes/Prestamos.cshtml` - Actualizadas funciones JavaScript

## ✨ Próximas Mejoras Sugeridas

- [ ] Agregar exportación a los demás reportes:
  - Reporte de Atrasos
  - Reporte de Disponibilidad
  - Dashboard (gráficos como imágenes)
- [ ] Agregar gráficos embebidos en Excel
- [ ] Agregar filtros avanzados en la exportación
- [ ] Permitir seleccionar columnas a exportar
- [ ] Exportación en segundo plano para reportes grandes
- [ ] Envío de reportes por email

## 📞 Soporte

Si encuentras problemas:
1. Verifica que los stored procedures estén instalados
2. Revisa que los paquetes NuGet estén correctamente instalados
3. Compila el proyecto desde cero (`dotnet clean` → `dotnet build`)
4. Revisa los logs de la aplicación

---

**Fecha de implementación**: 2025-12-20
**Versión**: 1.0
**Sistema**: Biblioteca CTP JICARAL
**Desarrollador**: Claude AI
