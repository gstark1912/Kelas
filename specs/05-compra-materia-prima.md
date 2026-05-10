# Spec 05: Compra de Materia Prima

## Descripción

Registrar compras de MP, actualizando stock, precio vigente, historial de precios y generando movimiento de caja automático.

## Referencia

- US 02 (Compra de Materia Prima).
- Correcciones: acceso desde pantalla de MP, actualizar `lastPricePerUnit`, registro en `rawMaterialPrices` para auditoría.
- DER: colecciones `purchases`, `rawMaterials`, `stock`, `stockMovements`, `rawMaterialPrices`, `cashMovements`.

## Colecciones del DER

- `purchases`
- `rawMaterials` (actualizar `lastPricePerUnit`)
- `stock` (actualizar `currentQuantity`)
- `stockMovements` (crear PurchaseEntry)
- `rawMaterialPrices` (crear registro de auditoría)
- `cashMovements` (crear egreso automático)
- `cashAccounts` (actualizar saldo)

## Alcance

### Backend

- **Endpoints**:
  - `POST /api/purchases` — registrar compra.
  - `GET /api/purchases?rawMaterialId=X` — historial de compras de una MP.
- **Servicio**: `PurchaseService`.
- **Repositorio**: `PurchaseRepository`.
- **Flujo al registrar compra**:
  1. Validar datos (cantidad > 0, precio > 0, MP existe, cuenta existe).
  2. Calcular `pricePerUnit = totalPrice / quantity`.
  3. Crear documento en `purchases`.
  4. Actualizar `rawMaterials.lastPricePerUnit`.
  5. Crear `stockMovement` (PurchaseEntry, +quantity).
  6. Actualizar `stock.currentQuantity`.
  7. Crear registro en `rawMaterialPrices` (auditoría).
  8. Crear `cashMovement` (egreso automático).
  9. Actualizar `cashAccounts.currentBalance`.

### Frontend

- **Modal de compra** accesible desde la pantalla de Materias Primas (botón "Registrar Compra").
- **Campos**: Materia Prima (pre-seleccionada si se accede desde fila), Cantidad, Precio Total, Fecha, Proveedor (opcional), Cuenta de pago, Notas.
- **Cálculo automático**: Precio por unidad mostrado en tiempo real.

## Criterios de Aceptación

- [ ] Se registra la compra correctamente.
- [ ] El stock de la MP se incrementa en la cantidad comprada.
- [ ] `lastPricePerUnit` se actualiza en la MP.
- [ ] Se crea un StockMovement de tipo PurchaseEntry.
- [ ] Se crea un CashMovement de tipo egreso.
- [ ] El saldo de la cuenta se reduce.
- [ ] Se crea registro en rawMaterialPrices.
- [ ] Validación: cantidad y precio > 0.

## Tests de Integración Esperados

- Test compra exitosa → stock actualizado, precio actualizado, movimientos creados.
- Test compra con cantidad 0 → error de validación.
- Test compra con MP inexistente → error.
- Test compra con cuenta inexistente → error.
- Test que el saldo de la cuenta se reduce correctamente.
- Test historial de compras de una MP.
