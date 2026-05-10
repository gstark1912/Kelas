# Spec 02: Autenticación (Login con JWT)

## Descripción

Implementar login sencillo con JWT. Usuario y contraseña fijos configurados por variables de ambiente. Sin roles, sin registro, sin recuperación de contraseña.

## Referencia

- Reunión con cliente: "Login — Se necesita implementar un sistema de autenticación. Es la primera prioridad."
- POC: `Analisis/POC/login.html`

## Colecciones del DER

- Ninguna (credenciales en variables de ambiente).

## Alcance

### Backend

- **Endpoint**: `POST /api/auth/login` — recibe `{ email, password }`, valida contra env vars, retorna JWT.
- **Configuración**: Variables de ambiente `AUTH_EMAIL` y `AUTH_PASSWORD`.
- **JWT**: Token con expiración configurable (ej: 24h). Secret en variable de ambiente `JWT_SECRET`.
- **Middleware**: `[Authorize]` en todos los controllers excepto login y health.
- **Endpoint**: `GET /api/auth/me` — retorna datos básicos del usuario autenticado (para validar token).

### Frontend

- **Pantalla de Login**: Email + contraseña + botón "Ingresar".
- **Manejo de token**: Guardar JWT en localStorage. Incluir en header `Authorization: Bearer {token}` en cada request.
- **Guard de rutas**: Redirigir a login si no hay token válido.
- **Interceptor HTTP**: Si la API retorna 401, redirigir a login y limpiar token.

## Criterios de Aceptación

- [ ] Login exitoso con credenciales correctas retorna JWT.
- [ ] Login fallido retorna 401 con mensaje de error.
- [ ] Endpoints protegidos retornan 401 sin token.
- [ ] Endpoints protegidos responden correctamente con token válido.
- [ ] Frontend redirige a login si no hay sesión.
- [ ] Frontend muestra error si las credenciales son incorrectas.

## Tests de Integración Esperados

- Test login exitoso → retorna token válido.
- Test login fallido → retorna 401.
- Test acceso a endpoint protegido sin token → 401.
- Test acceso a endpoint protegido con token válido → 200.
- Test acceso con token expirado → 401.
