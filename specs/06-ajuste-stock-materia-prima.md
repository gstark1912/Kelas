# Spec 06: Ajuste de Stock de Materia Prima

## Descripción

Registrar ajustes manuales de stock (vencimiento, rotura, pérdida, corrección) directamente como StockMovements.

## Referencia

- US 03 (Ajuste de Stock de Materia Prima).
- Correcciones: acceso inline desde tabla de MP, no existe entidad "Ajuste" separada — es un StockMovement con tipo y motivo.
- Reunión cliente: "Vencimiento, rotura y pérdida son el mismo tipo de ajuste (unificar)."
- DER: colecciones `stockMovements`, `stock`.

## Colecciones del DER

- `stockMovements` (crear AdjustmentIncrease o AdjustmentDecrease)
- `stock` (actualizar `currentQuantity`)

## Alcance

### Backend

- **Endpoints**:
  - `POST /api/stock-adjustments` — registrar ajuste.
  - `GET /api/stock-movements?itemType=RawMaterial&itemId=X` — historial de movimientos de una MP.
- **Servicio**: `StockService` (o `StockMovementService`).
- **Repositorio**: `StockMovementRepository`, `StockRepository`.
- **Flujo**:
  1. Validar datos (MP existe, motivo obligatorio).
  2. Crear `stockMovement` (AdjustmentIncrease o AdjustmentDecrease según signo).
  3. Actualizar `stock.currentQuantity`.
  4. Si stock queda negativo → permitir con advertencia (no bloquear).
- **Motivos válidos**: Vencimiento, Rotura, Pérdida, Corrección de inventario, Otro.

### Frontend

- **Modal de ajuste** accesible desde botón "Ajustar" en cada fila de la tabla de MP.
- **Campos**: Cantidad (+/-), Motivo (select), Fecha, Notas.
- **Advertencia**: Si el ajuste deja stock negativo, mostrar confirmación.
- **Historial**: Vista de movimientos de stock de una MP (tabla con fecha, tipo, cantidad, saldo, origen).

## Criterios de Aceptación

- [ ] Se puede registrar un ajuste negativo (baja).
- [ ] Se puede registrar un ajuste positivo (corrección a favor).
- [ ] El stock se actualiza correctamente.
- [ ] Se crea StockMovement con tipo y motivo correcto.
- [ ] No genera movimiento de caja.
- [ ] Se permite stock negativo con advertencia.
- [ ] El historial muestra todos los movimientos de la MP.

## Tests de Integración Esperados

- Test ajuste negativo → stock se reduce, movimiento creado.
- Test ajuste positivo → stock aumenta, movimiento creado.
- Test ajuste que deja stock negativo → se permite.
- Test sin motivo → error de validación.
- Test historial de movimientos ordenado por fecha descendente.
