# Prompt para Nuevo Contexto — Generación de Specs y Steering Files

## Contexto del Proyecto

Estoy desarrollando **Kelas**, un sistema de gestión para un emprendimiento de velas artesanales. Ya tengo documentado:

- **User Stories**: `Analisis/UserStories/` (00 a 10) — describen los flujos funcionales del sistema.
- **Correcciones a las US**: `Analisis/correcciones-user-stories.md` — discrepancias detectadas entre las US y el POC, con propuestas de corrección. **Las correcciones propuestas deben considerarse como aprobadas y aplicarse.**
- **DER para MongoDB**: `Analisis/DER-mongodb.md` — modelo de datos con 11 colecciones, índices, decisiones de embedding vs referencing, y queries frecuentes.
- **POC navegable**: `Analisis/POC/` — prototipos HTML estáticos de cada pantalla del sistema.

## Stack Tecnológico

- **Backend**: .NET (C#), API REST
- **Base de datos**: MongoDB
- **Frontend**: Vue.js
- **Infraestructura**: Docker (Dockerfiles para API, frontend y MongoDB), deploy en Railway

## Arquitectura y Buenas Prácticas

### Backend (.NET)

- **Repository Pattern** obligatorio — los repositorios son los únicos que hacen queries a MongoDB.
- **Dependency Injection** obligatorio.
- **Servicios** con responsabilidades bien separadas por entidad/dominio.
- **Controllers** livianos — solo reciben request, llaman al servicio y devuelven response. Sin lógica de negocio.
- **Tests de integración** por cada módulo que se desarrolle (no unit tests). Verificar flujos completos contra MongoDB real (puede ser containerizado para tests).
- Código simple, sin sobre-ingeniería.

### Autenticación

- Login sencillo con JWT.
- User y password **fijos**, configurados por variables de ambiente.
- Controllers securizados con `[Authorize]`.
- Sin roles, sin registro de usuarios, sin recuperación de contraseña.

### Frontend (Vue.js)

- Componentes reutilizables (muchos elementos de UX se repiten: tablas con filtros, cards de KPIs, formularios modales, etc.).
- Arquitectura consistente — todos los módulos/pantallas se tratan de forma similar.
- Buenas prácticas de desarrollo Vue.

### Infraestructura

- Dockerfile para la API .NET.
- Dockerfile para el frontend Vue.js (build + nginx).
- docker-compose para desarrollo local (API + Frontend + MongoDB).
- Configuración lista para deploy en Railway.

## Lo que necesito que generes

### 1. Carpeta `specs/` con especificaciones ordenadas

Crear una carpeta `specs/` con archivos numerados que representen cada spec/tarea a implementar, **en el orden correcto de dependencias** para que el sistema evolucione incrementalmente.

Criterio de orden:
1. Primero lo que no depende de nada (infraestructura, auth).
2. Luego las entidades base (las que no referencian a otras).
3. Después las que dependen de las anteriores.
4. Al final las pantallas de consulta/dashboard que agregan datos de todo.

Cada spec debe incluir:
- Título y descripción breve.
- Referencia a la US correspondiente (si aplica).
- Referencia a la página del POC correspondiente (si aplica).
- Referencia a la colección del DER involucrada.
- Alcance del backend (endpoints, servicio, repositorio).
- Alcance del frontend (pantalla, componentes).
- Criterios de aceptación.
- Tests de integración esperados.

### 2. Steering files en `.kiro/steering/`

Crear steering files para guiar la implementación:

- **`dotnet.md`** — Convenciones de .NET: estructura de proyecto, naming, patrones (repository, DI, controllers livianos), manejo de errores, configuración de MongoDB, estructura de tests de integración.
- **`vue.md`** — Convenciones de Vue.js: estructura de proyecto, componentes reutilizables, manejo de estado, llamadas a API, autenticación (JWT en headers), naming.
- **`mongodb.md`** — Convenciones de MongoDB: referencia al DER, cómo crear repositorios, manejo de ObjectId, convenciones de naming de colecciones, índices.
- **`infraestructura.md`** — Docker, docker-compose, variables de ambiente, configuración para Railway.

---

**Importante**: No implementes código todavía. Solo generá las specs y los steering files. La implementación la vamos a hacer spec por spec después.
