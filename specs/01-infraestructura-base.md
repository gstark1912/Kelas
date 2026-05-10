# Spec 01: Infraestructura Base

## Descripción

Configurar la estructura del proyecto, Docker, docker-compose y la configuración base para desarrollo local y deploy en Railway.

## Referencia

- No tiene US asociada directamente.
- Requisito técnico del stack definido en el proyecto.

## Alcance

### Backend (.NET)

- Crear solución .NET con proyecto Web API.
- Estructura de carpetas: Controllers, Services, Repositories, Models, Configuration.
- Configurar conexión a MongoDB (driver oficial).
- Configurar CORS para desarrollo local.
- Configurar variables de ambiente (appsettings + env vars).
- Health check endpoint (`GET /api/health`).

### Frontend (Vue.js)

- Crear proyecto Vue.js con Vite.
- Estructura de carpetas: views, components, services (API calls), router, stores.
- Configurar proxy de desarrollo hacia la API.
- Instalar dependencias base (vue-router, axios o fetch wrapper, chart library).

### Infraestructura

- `Dockerfile` para API .NET (multi-stage build).
- `Dockerfile` para Frontend Vue.js (build + nginx).
- `docker-compose.yml` para desarrollo local (API + Frontend + MongoDB).
- Variables de ambiente documentadas en `.env.example`.
- Configuración para deploy en Railway (Procfile o railway.toml si aplica).

## Criterios de Aceptación

- [ ] `docker-compose up` levanta los 3 servicios sin errores.
- [ ] La API responde en `http://localhost:5000/api/health` con status 200.
- [ ] El frontend se sirve en `http://localhost:3000` y muestra una página base.
- [ ] MongoDB es accesible desde la API.
- [ ] Las variables de ambiente se leen correctamente desde docker-compose.

## Tests de Integración Esperados

- Test que verifica conexión a MongoDB desde la API.
- Test que verifica que el health check responde correctamente.
