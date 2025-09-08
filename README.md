# Prueba Técnica – Gestión de Customers & Appointments

Aplicación fullstack construida con:
- **Backend**: .NET 9 Web API (minimal APIs, Entity Framework Core, SQL Server)
- **Frontend**: Angular 20.2.2 (standalone components)
- **Base de datos**: SQL Server
- **Servidor**: IIS

---

## 📂 Estructura del proyecto
```
/backend        → API en .NET 9
/frontend       → SPA en Angular 20
/db             → Scripts SQL (schema.sql)
/README.md      → Este archivo
```

---

## 🚀 Requisitos previos
- **.NET 9 SDK** + **.NET Hosting Bundle** (para IIS).
- **SQL Server** (Express o LocalDB).
- **Node.js 18+** y **Angular CLI 20.2.2**.
- **IIS** con **URL Rewrite Module** instalado.
- (Opcional) Visual Studio Code o Visual Studio 2022.

---

## 🗄️ Base de Datos
Ejecutar el script `/db/schema.sql` en SQL Server:

```sql
CREATE TABLE Customers (
  Id UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
  Name NVARCHAR(120) NOT NULL,
  Email NVARCHAR(255) NULL
);

CREATE TABLE Appointments (
  Id UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
  CustomerId UNIQUEIDENTIFIER NOT NULL REFERENCES Customers(Id),
  DateTime DATETIME2 NOT NULL,
  Status NVARCHAR(20) NOT NULL CHECK (Status IN ('scheduled','done','cancelled'))
);

-- Evitar doble booking
CREATE UNIQUE INDEX UQ_Appointments_Slot ON Appointments(CustomerId, DateTime);
```

---

## 🖥️ Backend – .NET 8
### Desarrollo local
1. Ajustar la cadena de conexión en `appsettings.json`:
   ```json
   "ConnectionStrings": {
     "Sql": "Server=localhost;Database=AppointmentsDb;Trusted_Connection=True;TrustServerCertificate=True"
   }
   ```
2. Correr el API:
   ```bash
   cd backend/AppointmentsApi
   dotnet run
   ```
   Swagger: `https://localhost:7162/swagger`

### Endpoints principales
- **Customers**
  - `GET /api/customers`
  - `GET /api/customers/{id}`
  - `POST /api/customers`
  - `PUT /api/customers/{id}`
  - `DELETE /api/customers/{id}`

- **Appointments**
  - `GET /api/appointments?from&to&status`
  - `GET /api/appointments/{id}`
  - `POST /api/appointments`
  - `PUT /api/appointments/{id}`
  - `DELETE /api/appointments/{id}`

Ejemplo `POST /api/customers`:
```json
{
  "name": "Alfredo Berumen",
  "email": "alfredo@example.com"
}
```

---

## 🌐 Frontend – Angular 20 (standalone)
### Desarrollo local
1. Ajustar la URL de la API en `src/app/config.ts`:
   ```ts
   export const API_URL = 'https://localhost:7162/api';
   ```
2. Ejecutar:
   ```bash
   cd frontend/appointments-frontend
   ng serve -o
   ```
   App disponible en `http://localhost:4200`

### Pantallas incluidas
- **Customers**: listado, alta rápida, eliminación.
- **Appointments**: listado, filtros por rango de fechas y estado, alta rápida, eliminación.

---

## 📦 Publicación en IIS
### Backend (API .NET)
1. Publicar:
   ```bash
   cd backend/AppointmentsApi
   dotnet publish -c Release -o C:\inetpub\wwwroot\AppointmentsApi
   ```
2. En IIS:
   - Crear AppPool `AppointmentsApiPool` (No Managed Code).
   - Crear sitio `AppointmentsApi` apuntando a `C:\inetpub\wwwroot\AppointmentsApi`.
   - Asignar AppPool.
   - Asegurar permisos NTFS para `IIS AppPool\AppointmentsApiPool`.

3. `web.config` generado por publish debe estar en la carpeta.  
   Si no, usar este ejemplo:
   ```xml
   <?xml version="1.0" encoding="utf-8"?>
   <configuration>
     <system.webServer>
       <handlers>
         <add name="aspNetCore" path="*" verb="*" modules="AspNetCoreModuleV2" resourceType="Unspecified"/>
       </handlers>
       <aspNetCore processPath="dotnet" arguments=".\AppointmentsApi.dll" stdoutLogEnabled="false" stdoutLogFile=".\logs\stdout" hostingModel="OutOfProcess"/>
     </system.webServer>
   </configuration>
   ```

### Frontend (Angular)
1. Construir en modo producción:
   ```bash
   cd frontend/appointments-frontend
   ng build --configuration production
   ```
2. Copiar `dist/appointments-frontend/` a:
   ```
   C:\inetpub\wwwroot\AppointmentsFront
   ```
3. En IIS:
   - Crear AppPool `AppointmentsFrontPool`.
   - Crear sitio `AppointmentsFront` apuntando a la carpeta anterior.
   - Asignar AppPool.

4. Crear `web.config` en la raíz del frontend:
   ```xml
   <?xml version="1.0" encoding="utf-8"?>
   <configuration>
     <system.webServer>
       <rewrite>
         <rules>
           <rule name="AngularRoutes" stopProcessing="true">
             <match url=".*" />
             <conditions logicalGrouping="MatchAll">
               <add input="{REQUEST_FILENAME}" matchType="IsFile" negate="true" />
               <add input="{REQUEST_FILENAME}" matchType="IsDirectory" negate="true" />
               <add input="{REQUEST_URI}" pattern="^/api" negate="true" />
             </conditions>
             <action type="Rewrite" url="/index.html" />
           </rule>
         </rules>
       </rewrite>
       <staticContent>
         <remove fileExtension=".json" />
         <mimeMap fileExtension=".json" mimeType="application/json" />
       </staticContent>
     </system.webServer>
   </configuration>
   ```

---

## 🔧 Depuración
- Revisar logs en `Event Viewer` → Windows Logs → Application.
- Para depurar API, habilitar temporalmente logs en `web.config`:
  ```xml
  stdoutLogEnabled="true"
  stdoutLogFile=".\logs\stdout"
  ```
  y dar permisos de escritura a la carpeta `logs`.
- Verificar CORS en `Program.cs` si frontend y backend están en diferentes dominios:
  ```csharp
  app.UseCors(p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
  ```

