# QA Status

## Estado actual
- Ultima tarea revisada: `specs/04-gestion-materias-primas.md`
- Estado: aprobada con observaciones
- Fecha: 2026-05-10

## Resumen
- `POST /api/raw-materials` crea una materia prima con stock inicial 0, precio vigente 0 y estado `Sin stock`.
- Crear una materia prima duplicada devuelve 400 con mensaje de error visible en API y frontend.
- `GET /api/raw-materials` lista columnas requeridas: nombre, unidad, stock actual, alerta minima, estado, precio vigente y ultima compra.
- `GET /api/raw-materials?search=...` y `GET /api/raw-materials?unit=...` filtran correctamente.
- `PUT /api/raw-materials/{id}` actualiza nombre, unidad y stock minimo.
- En frontend `localhost:3000/raw-materials`, el alta, edicion, busqueda, filtro por unidad y acciones futuras deshabilitadas funcionan.
- Se valido estado `Bajo` ajustando un fixture de stock en MongoDB: currentQuantity 3 con minStock 10 retorna `Bajo`.
- No se encontraron errores de consola funcionales persistentes; hubo un 400 esperado por duplicado y un 502 transitorio no reproducible al repetir el mismo request.
- No hay tests de integracion de materias primas en `kelas-backend/Kelas.Tests`; solo existen tests para cuentas de caja.

## Bloqueos
- Ninguno.

## Siguiente tarea sugerida
- `specs/05-compra-materia-prima.md`
