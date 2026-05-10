# QA Status

## Estado actual
- Ultima tarea revisada: `specs/02-autenticacion.md`
- Estado: rechazada
- Fecha: 2026-05-10

## Resumen
- `POST /api/auth/login` con credenciales correctas retorna JWT.
- `POST /api/auth/login` con credenciales incorrectas retorna 401 con mensaje de error.
- `GET /api/auth/me` retorna 401 sin token y 200 con token válido.
- `GET /api/auth/me` retorna 401 con token expirado.
- El frontend redirige de `/` a `/login` cuando no hay token.
- El formulario de login muestra error visible ante credenciales incorrectas.
- El login exitoso guarda token en `localStorage` y navega a `/`.
- Hallazgo: el guard del frontend permite acceder a `/` con un token inválido si existe cualquier valor en `localStorage.token`.
- No se encontraron archivos/proyectos de tests de integración para los casos esperados por la spec.

## Bloqueos
- Ninguno.

## Siguiente tarea sugerida
- Reprobar `specs/02-autenticacion.md` luego de corregir la validación del token en el guard de rutas.
