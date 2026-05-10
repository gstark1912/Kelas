# Spec 08: Producción

## Descripción

Registrar tandas de producción, descontando MP del stock, calculando costo de producción (snapshot) y generando stock de producto terminado.

## Referencia

- US 05 (Producción).
- Correcciones: costo se calcula con `lastPricePerUnit` (no precio histórico por fecha), agregar filtros y KPIs.
- POC: `Analisis/POC/produccion.html`
- DER: colecciones `productionBatches`, `stock`, `stockMovements`.

## Colecciones del DER

- `productionBatches`
- `stock` (actualizar MP y Producto Terminado)
- `stockMovements` (crear ProductionConsumption × N + ProductionOutput × 1)
- `rawMaterials` (consultar `lastPricePerUnit`)
- `products` (consultar receta)

## Alcance

### Backend

- **Endpoints**:
  - `POST /api/production-batches` — registrar producción.
  - `GET /api/production-batches` — listar con filtros (producto, rango de fechas). Incluir KPIs del período.
  - `GET /api/production-batches/:id` — detalle de una tanda.
- **Servicio**: `ProductionService`.
- **Repositorio**: `ProductionBatchRepository`.
- **Flujo al registrar producción**:
  1. Validar (producto existe, tiene receta, cantidad > 0).
  2. Calcular MP necesaria: `receta[i].quantity × cantidadProducida`.
  3. Verificar stock de cada MP (advertencia si insuficiente, no bloquear).
  4. Para cada ingrediente:
     - Crear `stockMovement` (ProductionConsumption, -cantidad).
     - Actualizar `stock` de la MP.
  5. Calcular costo: `ingrediente.quantity × rawMaterial.lastPricePerUnit` → snapshot en `ingredients`.
  6. Calcular `totalCost` y `unitCost`.
  7. Crear `stockMovement` (ProductionOutput, +cantidadProducida).
  8. Actualizar `stock` de Producto Terminado.
  9. Guardar `productionBatch` con snapshot de ingredientes.
- **KPIs del período**: Total unidades producidas, Costo total.

### Frontend

- **Pantalla**: Listado de producciones con tabla.
- **KPIs**: Cards con "Unidades Producidas" y "Costo Total" del período filtrado.
- **Filtros**: Por producto, rango de fechas.
- **Columnas**: Fecha, Producto, Cantidad, Costo Total, Costo Unitario, Notas.
- **Modal de nueva producción**: Producto (select), Cantidad, Fecha, Notas.
- **Advertencia**: Si stock de MP insuficiente, mostrar detalle de faltantes con opción de continuar.

## Criterios de Aceptación

- [ ] Se registra la producción correctamente.
- [ ] El stock de cada MP se reduce según receta × cantidad.
- [ ] El stock de producto terminado se incrementa.
- [ ] Se crean StockMovements correctos (N consumos + 1 output).
- [ ] El costo se calcula con `lastPricePerUnit` actual.
- [ ] El snapshot de ingredientes queda inmutable.
- [ ] Se permite producir con stock insuficiente (con advertencia).
- [ ] Los filtros y KPIs funcionan.

## Tests de Integración Esperados

- Test producción exitosa → stocks actualizados, movimientos creados, costo calculado.
- Test producción con stock insuficiente → se permite, stocks pueden quedar negativos.
- Test producto sin receta → error.
- Test cantidad 0 → error.
- Test costo snapshot: cambiar precio de MP después de producir → costo de la tanda no cambia.
- Test KPIs del período.
- Test filtro por producto y por fechas.
