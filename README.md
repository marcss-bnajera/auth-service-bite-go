# 🔐 Bite&Go Auth Service (`auth-service-bite-go`)

Microservicio de **autenticación, gestión de usuarios y emisión de JWT** para la plataforma Bite&Go.
Está construido en **.NET 8** con arquitectura limpia (API / Application / Domain / Persistence) y persiste en **PostgreSQL**.
Los tokens que emite son los mismos que consumen los microservicios **Bite-go-admin** (puerto 3002) y **Bite-go-user** (puerto 3001).

---

## 🏗️ Arquitectura

```
auth-service-bite-go/
├── Dockerfile
├── AuthService.sln
└── src/
    ├── AuthService.Api            (Controllers, Program.cs, Swagger)
    ├── AuthService.Application    (AuthService, JwtTokenService, Email, Cloudinary...)
    ├── AuthService.Domain         (Entities, Enums, Constants, Interfaces)
    └── AuthService.Persistence    (DbContext, Migrations, Repositories, DataSeeder)
```

| Capa | Responsabilidad |
|------|-----------------|
| **Api** | Exponer los endpoints REST, configurar Swagger, rate-limiting, CORS, seguridad y middlewares. |
| **Application** | Casos de uso (`AuthService`), hashing Argon2, emisión de JWT, envío de emails (MailKit), subida de avatares (Cloudinary). |
| **Domain** | Entidades puras (`User`, `Role`, `UserProfile`, `UserEmail`, `UserPasswordReset`, `UserRole`) y constantes de roles. |
| **Persistence** | `ApplicationDbContext` (EF Core + Npgsql), repositorios y `DataSeeder`. |

---

## 👥 Roles soportados (Bite&Go)

Definidos en `src/AuthService.Domain/Constants/RoleConstans.cs` y sembrados automáticamente por `DataSeeder` al arrancar:

| Rol | Uso típico |
|-----|-----------|
| `SuperAdmin` | Acceso total multi-restaurante. |
| `Admin_Restaurante` | Administra un restaurante (menú, inventario, reservas, reportes). |
| `Mesero` | Gestiona pedidos y mesas en sala. |
| `Cocinero` | Visualiza la cola de la cocina y marca pedidos como listos. |
| `Cliente` | Usuario final. Es el rol asignado por defecto al registrarse. |

> El claim JWT `role` viaja con el **nombre en texto** (p.ej. `"Admin_Restaurante"`), que es el mismo que los middlewares `hasRole(...)` de Bite-go-admin / Bite-go-user esperan.

---

## 🔑 Endpoints disponibles

Ruta base: `/api/v1/auth`

| Método | Endpoint | Descripción | Auth |
|--------|----------|-------------|------|
| `POST` | `/register` | Registra un usuario (multipart/form-data, admite avatar). Envía email de verificación. | Pública |
| `POST` | `/login` | Autentica con email o username y devuelve JWT. | Pública |
| `GET`  | `/profile` | Perfil del usuario autenticado. | JWT |
| `POST` | `/profile/by-id` | Perfil por ID (consulta puntual). | Pública (rate-limited) |
| `POST` | `/verify-email` | Verifica el email con el token recibido por correo. | Pública |
| `POST` | `/resend-verification` | Reenvía el email de verificación. | Pública |
| `POST` | `/forgot-password` | Genera token de reset y envía correo. | Pública |
| `POST` | `/reset-password` | Cambia la contraseña con el token de reset. | Pública |

Endpoints auxiliares:

- `GET /health` – health check.
- `GET /swagger` – interfaz Swagger UI (habilitada en Development o con `ENABLE_SWAGGER=true`).

---

## 🧾 Formato del JWT

`JwtTokenService` (ver `src/AuthService.Application/Services/JwtTokenService.cs`) emite HS256 con estos claims:

```json
{
  "sub": "<userId>",
  "role": "Admin_Restaurante",
  "jti": "<guid>",
  "iat": 1700000000,
  "exp": 1700003600,
  "iss": "BiteGoAuthService",
  "aud": "BiteGoServices"
}
```

Estos tres valores — `SecretKey`, `Issuer` y `Audience` — **deben ser idénticos**
en los tres servicios. Para evitar desincronizaciones se definen **una sola vez**
en el `.env` de la raíz (`Bite-GO/.env`), y `docker-compose.yml` los inyecta
a todos los contenedores. No hardcodees estos valores en `appsettings.json`
ni en los `.env` de los microservicios Node.

---

## 🗄️ Migraciones de base de datos

El servicio usa **EF Core + Npgsql** y ya trae una migración `InitialCreate`
en `src/AuthService.Persistence/Migrations/`. Al arrancar, `Program.cs` ejecuta:

1. `context.Database.EnsureCreatedAsync()` – crea el esquema si la BD está vacía.
2. `DataSeeder.SeedAsync(context)` – inserta los roles Bite&Go y un `SuperAdmin`
   por defecto (`superadmin@bitego.local` / `BiteGo1234!`).

### Regenerar / aplicar migraciones manualmente

```bash
cd src/AuthService.Api

# (opcional) regenerar desde cero si modificas entidades
dotnet ef migrations remove -p ../AuthService.Persistence -s .
dotnet ef migrations add InitialCreate -p ../AuthService.Persistence -s .

# aplicar a la BD que apunta appsettings.json
dotnet ef database update -p ../AuthService.Persistence -s .
```

> Si cambias la cadena de conexión, basta con reiniciar el servicio:
> `EnsureCreatedAsync` + `DataSeeder` volverán a crear esquema y semillas.

---

## ⚙️ Configuración mediante variables de entorno

Para no exponer secretos en el repo, **ningún secreto vive en `appsettings.json`**.
Toda la configuración sensible se carga desde archivos `.env` (ya en `.gitignore`):

| Archivo | Qué contiene | Lo lee |
|---------|--------------|--------|
| `Bite-GO/.env` | JWT compartido (SecretKey, Issuer, Audience) y credenciales de Postgres. | `docker-compose.yml` para inyectarlo a los 3 servicios. |
| `auth-service-bite-go/.env` | Credenciales SMTP (Gmail App Password) y Cloudinary. | `docker-compose` (vía `env_file`) y `dotnet run` local (vía `DotNetEnv`). |
| `Bite-go-admin/.env` y `Bite-go-user/.env` | Configuración propia de cada servicio Node (puerto, MongoDB). | Cada microservicio Node. |

> **Cómo funciona en .NET**: ASP.NET Core lee variables de entorno automáticamente.
> Una variable `SmtpSettings__Username` se mapea a `Configuration["SmtpSettings:Username"]`
> y sobrescribe el valor (vacío) que está en `appsettings.json`. Sin impacto de
> rendimiento porque `IConfiguration` cachea todo al iniciar.

### Setup para compañeros que clonan el repo (primera vez)

```bash
git clone --recurse-submodules <repo-url>
cd Bite-GO

# 1) Variables compartidas (JWT, Postgres del auth-service)
cp .env.example .env
#    Edita .env y opcionalmente cambia el JWT_SECRET. Si el equipo ya tiene uno
#    fijo en pruebas, pídeselo y pégalo tal cual; si lo cambias, los tokens
#    emitidos con el secret anterior dejarán de validar.

# 2) Secretos del auth-service (SMTP + Cloudinary)
cp auth-service-bite-go/.env.example auth-service-bite-go/.env
#    Pide al líder del proyecto las credenciales del equipo o usa las tuyas
#    propias (ver Cloudinary y Gmail App Password más abajo).

# 3) Levanta todo
docker compose up --build
```

### Cómo obtener las credenciales propias

- **Gmail App Password (SMTP)**: requiere 2FA activo en tu cuenta Google. Genera
  una en `https://myaccount.google.com/apppasswords`. Te dará 16 caracteres en
  4 grupos de 4 (ej. `cmgh kmzd wfrd bhet`). Pégalos **sin espacios** en
  `SmtpSettings__Password`.
- **Cloudinary**: crea cuenta gratis en `https://cloudinary.com`. En el Dashboard
  copia `Cloud name`, `API Key` y `API Secret`. La URL base se forma como
  `https://res.cloudinary.com/<cloud_name>/image/upload/v1/bitego_auth/`.

### Errores comunes al clonar

| Síntoma | Causa | Solución |
|---------|-------|----------|
| `JWT SecretKey not configured` al arrancar el auth-service | No copiaste `Bite-GO/.env` desde el `.env.example`. | `cp .env.example .env` en la raíz. |
| `401 Unauthorized` en endpoints protegidos de admin/user | El `JWT_SECRET` del auth-service no coincide con el de los Node. | Asegúrate de que la variable está **solo** en el `.env` raíz; no la dupliques. |
| `Authentication failed` al enviar email | App Password incorrecta o tiene espacios. | Regenera y pégala sin espacios. |
| `Invalid api_key` al subir avatar | Pegaste el ApiSecret en el ApiKey o viceversa. | Verifica en el Dashboard de Cloudinary. |
| `connection refused 5432` en logs | El healthcheck de Postgres aún no termina. | Ya está cubierto con `depends_on.condition: service_healthy`. Espera ~10s en el primer arranque. |

---

## 🐳 Ejecutar todo con Docker Compose

Desde la raíz de Bite-GO (después del setup de variables):

```bash
docker compose up --build
```

Esto levanta:

| Servicio | Puerto host | Descripción |
|----------|------------|-------------|
| `mongodb` | 27017 | Base de datos de los microservicios Node. |
| `auth-postgres` | 5432 | PostgreSQL del auth-service. |
| `auth-service` | **3000** | Auth service .NET con Swagger en `http://localhost:3000/swagger`. |
| `user-service` | 3001 | Bite-go-user. |
| `admin-service` | 3002 | Bite-go-admin. |

---

## 🧪 Probar el flujo end-to-end

1. **Registrar un usuario**
   ```bash
   curl -X POST http://localhost:3000/api/v1/auth/register \
        -F "name=Juan" -F "surname=Perez" \
        -F "username=juanp" -F "email=juan@demo.com" \
        -F "password=SuperSeguro1!" -F "phone=55512345"
   ```
2. **Verificar el email** con el token que llega por SMTP (o marcar `status=true` en la BD para pruebas locales).
3. **Login** y copiar el `token` de la respuesta:
   ```bash
   curl -X POST http://localhost:3000/api/v1/auth/login \
        -H "Content-Type: application/json" \
        -d '{"emailOrUsername":"juanp","password":"SuperSeguro1!"}'