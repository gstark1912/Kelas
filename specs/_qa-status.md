# QA Status

## Estado actual
- Ultima tarea revisada: `specs/05-compra-materia-prima.md`
- Estado: aprobada con observaciones
- Fecha: 2026-05-10

## Resumen
- Se levanto el entorno con `docker compose up --build -d` y se valido `localhost:3000/raw-materials`.
- Desde la tabla de materias primas, el boton `Comprar` abre el modal `Registrar Compra` con la materia prima preseleccionada.
- El modal muestra campos de cantidad, precio total, fecha, proveedor, cuenta de pago y notas, y calcula `Precio por unidad` en tiempo real.
- Registrar una compra de 4 kg por $12000 crea la compra, incrementa stock a 4, actualiza `lastPricePerUnit` a 3000 y deja estado `OK`.
- En MongoDB se verifico la creacion de `purchases`, `stockMovements` con `PurchaseEntry`, `rawMaterialPrices`, `cashMovements` con `expense`, y reduccion del saldo de `Efectivo` a -12000.
- `GET /api/purchases?rawMaterialId=...` devuelve el historial de la compra creada.
- Validaciones API: cantidad 0 devuelve 400, precio 0 devuelve 400, materia prima inexistente devuelve 404 y cuenta inexistente devuelve 404.
- `dotnet test Kelas.sln` paso: 11 tests ok.
- No hay tests de integracion especificos de compras en `kelas-backend/Kelas.Tests`.
- Observacion: luego de registrar compra, el listado de materias primas mantiene `lastPurchaseDate: null` y la UI muestra `Ultima Compra` como `-`, aunque existe una compra registrada.

## Bloqueos
- Ninguno.

## Siguiente tarea sugerida
- `specs/06-ajuste-stock-materia-prima.md`
