# QA Status

## Estado actual
- Ultima tarea revisada: `specs/06-ajuste-stock-materia-prima.md`
- Estado: aprobada
- Fecha: 2026-05-10

## Resumen
- Se valido `localhost:3000/raw-materials` con Chrome MCP y API local en `localhost:5000`.
- Los botones `Ajustar` e `Historial` estan habilitados y abren sus modales.
- El historial consulta `GET /api/stock-movements?itemType=RawMaterial&itemId=...`, muestra movimientos ordenados por fecha descendente y no presento errores de consola.
- Con el contrato implementado (`newStock`) se pudo crear un ajuste positivo y uno de baja: stock 0 -> 10 genero `AdjustmentIncrease`; stock 10 -> 7 genero `AdjustmentDecrease`; el stock final quedo en 7.
- La validacion sin motivo devuelve 400.
- Se verifico en MongoDB que un ajuste exitoso no creo nuevos `cashMovements` ni modifico balances de `cashAccounts`.
- `dotnet test Kelas.sln` paso: 11 tests ok.

## Hallazgos
- Sin hallazgos vigentes.
- Los hallazgos reportados previamente quedan descartados por confirmacion del usuario: el cambio de contrato/criterio fue realizado durante la prueba y el comportamiento actual se acepta como valido.

## Bloqueos
- Ninguno.

## Siguiente tarea sugerida
- `specs/07-gestion-productos.md`
