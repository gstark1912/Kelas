# Kelas

Sistema de gestión para emprendimientos productivos. Monorepo con backend .NET 8 Web API, frontend Vue.js 3, y MongoDB.

## Requisitos Previos

- [Docker](https://docs.docker.com/get-docker/) y Docker Compose
- (Opcional para desarrollo sin Docker) .NET 8 SDK, Node.js 20+, MongoDB 7

## Inicio Rápido (Docker Compose)

```bash
# 1. Clonar el repositorio
git clone <url-del-repo>
cd kelas

# 2. Copiar variables de ambiente
cp .env.example .env

# 3. Levantar todos los servicios
docker-compose up --build
```

Una vez que los contenedores estén corriendo:

| Servicio | URL |
|----------|-----|
| Frontend | http://localhost:3000 |
| API | http://localhost:5000 |
| Health Check | http://localhost:5000/api/health |
| MongoDB | localhost:27017 |

## Desarrollo Local (sin Docker)

### Backend

```bash
cd kelas-backend/Kelas.Api
dotnet restore
dotnet run
```

La API se levanta en `http://localhost:5000`. Requiere MongoDB corriendo localmente como replica set.

### Frontend

```bash
cd kelas-frontend
npm install
npm run dev
```

El frontend se levanta en `http://localhost:5173` (Vite dev server) y apunta a la API en `http://localhost:5000/api` (configurado en `.env.development`).

### MongoDB (replica set local)

Si corrés MongoDB sin Docker, necesitás iniciarlo como replica set:

```bash
mongod --replSet rs0
```

Y luego inicializar el replica set:

```bash
mongosh --eval "rs.initiate({_id:'rs0', members:[{_id:0, host:'localhost:27017'}]})"
```

## Variables de Ambiente

| Variable | Descripción | Default |
|----------|-------------|---------|
| `AUTH_EMAIL` | Email del usuario admin | `admin@kelas.com` |
| `AUTH_PASSWORD` | Password del usuario admin | `admin123` |
| `JWT_SECRET` | Secret para firmar tokens JWT | `super-secret-key-for-development-only` |
| `MONGODB_CONNECTION_STRING` | Connection string de MongoDB | `mongodb://localhost:27017/kelas?replicaSet=rs0` |
| `MONGODB_DATABASE_NAME` | Nombre de la base de datos | `kelas` |
| `VITE_API_URL` | URL base de la API para el frontend | `http://localhost:5000/api` |

## Estructura del Proyecto

```
kelas/
├── kelas-backend/              # Backend .NET 8 (multi-proyecto)
│   ├── Kelas.Api/              # Web API — Controllers, Middleware, Program.cs
│   ├── Kelas.Domain/           # Dominio — Entidades, Interfaces, DTOs, Excepciones
│   ├── Kelas.Services/         # Lógica de negocio
│   ├── Kelas.Repositories/    # Acceso a MongoDB
│   ├── Kelas.IoC.Resolver/    # Composición de dependencias (DI)
│   ├── scripts/
│   │   └── mongo-init.js
│   └── Kelas.sln
├── kelas-frontend/             # Frontend Vue.js 3 + Vite
│   ├── src/
│   │   ├── views/              # Páginas
│   │   ├── components/         # Componentes reutilizables
│   │   ├── services/           # Llamadas a la API (axios)
│   │   ├── stores/             # Estado global (Pinia)
│   │   ├── router/             # Rutas
│   │   ├── composables/        # Composables
│   │   └── utils/              # Utilidades
│   ├── Dockerfile
│   ├── nginx.conf
│   └── railway.toml
├── docker-compose.yml          # Orquestación para desarrollo local
├── .env.example                # Variables de ambiente documentadas
├── .gitignore
└── README.md
```

## Comandos Útiles

```bash
# Levantar servicios en background
docker-compose up -d --build

# Ver logs
docker-compose logs -f api
docker-compose logs -f frontend
docker-compose logs -f mongo

# Detener servicios
docker-compose down

# Detener y borrar volúmenes (reset de datos)
docker-compose down -v

# Rebuild de un servicio específico
docker-compose up --build api
```

## Deploy (Railway)

Cada servicio se despliega como un servicio independiente en Railway. Los archivos `railway.toml` en cada directorio configuran el build y healthcheck.

Variables a configurar en Railway:
- `MongoDb__ConnectionString` → connection string de MongoDB Atlas o plugin
- `MongoDb__DatabaseName` → `kelas`
- `Auth__Email` → email del admin
- `Auth__Password` → password seguro
- `Jwt__Secret` → secret largo y aleatorio
