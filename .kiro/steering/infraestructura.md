# Convenciones de Infraestructura — Kelas

## Docker

### Dockerfile API (.NET)

Multi-stage build para optimizar tamaño de imagen. El build context es `./kelas-backend`:

```dockerfile
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Kelas.Api/Kelas.Api.csproj", "Kelas.Api/"]
COPY ["Kelas.Domain/Kelas.Domain.csproj", "Kelas.Domain/"]
COPY ["Kelas.Repositories/Kelas.Repositories.csproj", "Kelas.Repositories/"]
COPY ["Kelas.Services/Kelas.Services.csproj", "Kelas.Services/"]
COPY ["Kelas.IoC.Resolver/Kelas.IoC.Resolver.csproj", "Kelas.IoC.Resolver/"]
RUN dotnet restore "Kelas.Api/Kelas.Api.csproj"
COPY . .
WORKDIR "/src/Kelas.Api"
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 5000
ENV ASPNETCORE_URLS=http://+:5000
ENTRYPOINT ["dotnet", "Kelas.Api.dll"]
```

### Dockerfile Frontend (Vue.js)

Build + nginx para servir estáticos:

```dockerfile
# Build stage
FROM node:20-alpine AS build
WORKDIR /app
COPY package*.json ./
RUN npm ci
COPY . .
RUN npm run build

# Production stage
FROM nginx:alpine AS production
COPY --from=build /app/dist /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
```

### nginx.conf (Frontend)

```nginx
server {
    listen 80;
    server_name _;
    root /usr/share/nginx/html;
    index index.html;

    # SPA: redirigir todas las rutas a index.html
    location / {
        try_files $uri $uri/ /index.html;
    }

    # Proxy API requests en producción (si se usa mismo dominio)
    location /api/ {
        proxy_pass http://api:5000/api/;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
    }
}
```

## Docker Compose (Desarrollo Local)

```yaml
version: '3.8'

services:
  api:
    build:
      context: ./kelas-backend
      dockerfile: Kelas.Api/Dockerfile
    ports:
      - "5000:5000"
    environment:
      - MongoDb__ConnectionString=mongodb://mongo:27017/kelas?replicaSet=rs0
      - MongoDb__DatabaseName=kelas
      - Auth__Email=${AUTH_EMAIL:-admin@kelas.com}
      - Auth__Password=${AUTH_PASSWORD:-admin123}
      - Jwt__Secret=${JWT_SECRET:-super-secret-key-for-development-only}
      - Jwt__ExpirationHours=24
      - ASPNETCORE_ENVIRONMENT=Development
    depends_on:
      mongo:
        condition: service_healthy

  frontend:
    build:
      context: ./kelas-frontend
      dockerfile: Dockerfile
    ports:
      - "3000:80"
    depends_on:
      - api

  mongo:
    image: mongo:7
    ports:
      - "27017:27017"
    volumes:
      - mongo-data:/data/db
      - ./kelas-backend/scripts/mongo-init.js:/docker-entrypoint-initdb.d/init.js
    command: ["--replSet", "rs0"]
    healthcheck:
      test: echo "try { rs.status() } catch (err) { rs.initiate({_id:'rs0',members:[{_id:0,host:'mongo:27017'}]}) }" | mongosh --quiet
      interval: 5s
      timeout: 30s
      retries: 5

volumes:
  mongo-data:
```

**Nota sobre Replica Set**: MongoDB requiere replica set para soportar transacciones. Se configura como replica set de un solo nodo para desarrollo local.

## Variables de Ambiente

### `.env.example`

```env
# Auth
AUTH_EMAIL=admin@kelas.com
AUTH_PASSWORD=admin123

# JWT
JWT_SECRET=super-secret-key-change-in-production

# MongoDB (solo si no se usa docker-compose)
MONGODB_CONNECTION_STRING=mongodb://localhost:27017/kelas?replicaSet=rs0
MONGODB_DATABASE_NAME=kelas

# Frontend
VITE_API_URL=http://localhost:5000/api
```

### Variables en la API (.NET)

Usar el patrón de configuración de .NET:

```json
// appsettings.json
{
  "MongoDb": {
    "ConnectionString": "mongodb://localhost:27017/kelas?replicaSet=rs0",
    "DatabaseName": "kelas"
  },
  "Auth": {
    "Email": "admin@kelas.com",
    "Password": "admin123"
  },
  "Jwt": {
    "Secret": "super-secret-key-for-development-only",
    "ExpirationHours": 24
  }
}
```

Las variables de ambiente sobreescriben appsettings usando el formato `Section__Key` (doble underscore):
- `MongoDb__ConnectionString`
- `Auth__Email`
- `Jwt__Secret`

### Variables en el Frontend (Vue.js)

Usar archivos `.env`:

```env
# .env.development
VITE_API_URL=http://localhost:5000/api

# .env.production
VITE_API_URL=/api
```

Acceso en código: `import.meta.env.VITE_API_URL`

## Deploy en Railway

### Estructura de servicios en Railway

- **API**: Servicio desde el directorio `kelas-backend/Kelas.Api/`, usando Dockerfile.
- **Frontend**: Servicio desde el directorio `kelas-frontend/`, usando Dockerfile.
- **MongoDB**: Plugin de Railway o servicio externo (MongoDB Atlas free tier).

### railway.toml (API)

```toml
[build]
dockerfilePath = "kelas-backend/Kelas.Api/Dockerfile"

[deploy]
healthcheckPath = "/api/health"
healthcheckTimeout = 30
restartPolicyType = "on_failure"
```

### railway.toml (Frontend)

```toml
[build]
dockerfilePath = "kelas-frontend/Dockerfile"

[deploy]
healthcheckPath = "/"
```

### Variables de ambiente en Railway

Configurar en el panel de Railway:
- `MongoDb__ConnectionString` → connection string del plugin MongoDB o Atlas.
- `MongoDb__DatabaseName` → `kelas`
- `Auth__Email` → email del admin.
- `Auth__Password` → password seguro.
- `Jwt__Secret` → secret largo y aleatorio.
- `VITE_API_URL` → URL pública de la API (se configura en build del frontend).

## Estructura de Carpetas del Proyecto

```
kelas/
├── kelas-backend/              # Backend .NET 8 (multi-proyecto)
│   ├── Kelas.Api/              # Web API — Controllers, Middleware, Program.cs
│   │   ├── Dockerfile
│   │   ├── Kelas.Api.csproj
│   │   └── railway.toml
│   ├── Kelas.Domain/           # Dominio — Entidades, Interfaces, DTOs, Excepciones
│   ├── Kelas.Services/         # Lógica de negocio
│   ├── Kelas.Repositories/    # Acceso a MongoDB
│   ├── Kelas.IoC.Resolver/    # Composición de dependencias (DI)
│   ├── scripts/
│   │   └── mongo-init.js
│   └── Kelas.sln
├── kelas-frontend/             # Proyecto Vue.js
│   ├── Dockerfile
│   ├── nginx.conf
│   ├── package.json
│   ├── railway.toml
│   └── src/
├── docker-compose.yml
├── .env.example
├── .gitignore
└── README.md
```

## Scripts Útiles

### mongo-init.js

```javascript
// kelas-backend/scripts/mongo-init.js
// Seed de cuentas de caja por defecto
db = db.getSiblingDB('kelas');

// Las cuentas se crean desde la aplicación al iniciar (DatabaseInitializer)
// Este script es solo para configuración inicial de MongoDB si se necesita
```

## Reglas Generales

- No commitear archivos `.env` con secretos reales.
- El `.gitignore` debe incluir: `.env`, `node_modules/`, `bin/`, `obj/`, `dist/`.
- Los Dockerfiles deben usar multi-stage builds para minimizar tamaño.
- Docker-compose es solo para desarrollo local.
- En producción (Railway), cada servicio se despliega independientemente.
- La API debe funcionar tanto con variables de `appsettings.json` como con env vars (para flexibilidad entre local y producción).
