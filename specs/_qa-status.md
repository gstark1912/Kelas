# QA Status

## Estado actual
- Ultima tarea revisada: `specs/03-cuentas-de-caja.md`
- Estado: aprobada con observaciones
- Fecha: 2026-05-10

## Resumen
- La API queda saludable y conectada a MongoDB en `GET /api/health`.
- Al iniciar la app con datos limpios, `GET /api/cash-accounts` retorna las 3 cuentas default: Efectivo, Banco y Mercado Pago.
- Las 3 cuentas default tienen `currentBalance: 0` e `isActive: true`.
- `GET /api/cash-accounts/{id}` retorna el detalle correcto de una cuenta.
- `POST /api/cash-accounts` crea una cuenta nueva con saldo inicial 0.
- Crear una cuenta duplicada devuelve 400 con mensaje de error, incluyendo duplicado con distinta capitalización.
- Crear una cuenta con nombre vacío devuelve 400 con mensaje de error.
- `GET /api/cash-accounts` sin token devuelve 401.
- Desde el frontend en `localhost:3000`, el login funciona y una llamada autenticada a `/api/cash-accounts` responde 200.
- `dotnet test --no-restore` no pudo verificarse desde el host porque intenta resolver paquetes NuGet y el sandbox bloquea la red.

## Bloqueos
- Ninguno.

## Siguiente tarea sugerida
- `specs/04-gestion-materias-primas.md`
