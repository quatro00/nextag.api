# NexTag API

Backend de **NexTag**, plataforma SaaS para inventario, activos, etiquetas NFC/QR, asignaciones, contratos, garantías, mantenimientos y soporte.

## Estructura
- Solución: `Nextag.sln`
- Proyecto: `Nextag.Api/Nextag.Api.csproj`
- Namespace: `Nextag.Api`
- EF Core: `NextagContext` y `AuthDbContext`

## Preparación local
Se requiere .NET 8, SQL Server y una base de datos independiente para NexTag.

Las credenciales de base de datos y claves JWT **no deben guardarse en Git**. Usa secretos de usuario para desarrollo:

```bash
cd Nextag.Api
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:NextagConnection" "<CADENA_DE_CONEXION>"
dotnet user-secrets set "Jwt:Key" "<CLAVE_ALEATORIA_SEGURA_DE_32_BYTES_O_MAS>"
dotnet user-secrets set "Jwt:Issuer" "Nextag.Api"
dotnet user-secrets set "Jwt:Audience" "Nextag.Web"
dotnet run
```

La conexión `NextagConnection` se usa en ambos DbContext. En producción utiliza variables de entorno o un gestor de secretos.

```bash
dotnet restore Nextag.sln
dotnet build Nextag.sln
dotnet run --project Nextag.Api/Nextag.Api.csproj
```

Documentación Swagger: `/swagger`.

## Dependencias de base de datos
El código conserva las entidades y tablas del origen. Renombrar archivos, clases y namespaces **no crea ni migra una base de datos**. El catálogo `AspNetSystem` de la nueva base de datos deberá incluir un registro con `Clave = 'NEXTAG'`; los roles de administrador que necesite esta API deben estar vinculados a ese registro mediante `SistemaId`.

## Seguridad del origen
El commit importado contenía una cadena SQL, una clave JWT y un comando de scaffold con credenciales. Se han excluido del árbol actual, pero **siguen en el historial Git**. Rota los secretos comprometidos, restringe el repositorio y limpia el historial antes de usar credenciales reales.
