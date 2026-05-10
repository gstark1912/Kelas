# Spec 09: Ventas

## Descripción

Registrar ventas de productos, descontando stock, calculando ingresos/COGS/costos asociados y generando movimiento de caja.

## Referencia

- US 06 (Venta).
- Correcciones: COGS = receta × `lastPricePerUnit` (snapshot), agregar filtros y KPIs en listado.
- Reunión cliente: canales (Feria, Instagram, Tienda, Otro), medios de pago (Efectivo, Transferencia, MP), descuento %, costo impositivo %, costo canal %, envío $.
- POC: `Analisis/POC/ventas.html`
- DER: colección `sales`.

## Colecciones del DER

- `sales`
- `stock` (actualizar Producto Terminado)
- `stockMovements` (crear SaleOutput × N)
- `cashMovements` (crear ingreso automático)
- `cashAccounts` (actualizar saldo)
- `products` (consultar precio de lista y receta para COGS)
- `rawMaterials` (consultar `lastPricePerUnit` para COGS)

## Alcance

### Backend

- **Endpoints**:
  - `POST /api/sales` — registrar venta.
  - `GET /api/sales` — listar con filtros (canal, medio de pago, rango de fechas). Incluir KPIs.
  - `GET /api/sales/:id` — detalle de una venta.
- **Servicio**: `SaleService`.
- **Repositorio**: `SaleRepository`.
- **Flujo al registrar venta**:
  1. Validar (al menos 1 ítem, productos existen, canal y medio de pago obligatorios).
  2. Para cada ítem: `unitPrice` = precio de lista del producto.
  3. Calcular subtotales, base imponible, descuento, grossIncome, taxCost, channelCost.
  4. Para cada ítem: calcular `unitCost` (receta × `lastPricePerUnit` de cada MP) → COGS.
  5. Calcular grossProfit y netProfit.
  6. Verificar stock (advertencia si insuficiente, no bloquear).
  7. Para cada ítem:
     - Crear `stockMovement` (SaleOutput, -cantidad).
     - Actualizar `stock` de Producto Terminado.
  8. Crear `cashMovement` (ingreso, monto = grossIncome).
  9. Actualizar `cashAccounts.currentBalance`.
  10. Guardar `sale` con todos los campos calculados.
- **KPIs del período**: Cantidad de ventas, Ingresos, COGS, Costos de Venta, Ganancia Neta.

### Frontend

- **Pantalla**: Listado de ventas con tabla.
- **KPIs**: Cards con Ventas, Ingresos, COGS, Costos Vta, Ganancia Neta.
- **Filtros**: Por canal, medio de pago, rango de fechas.
- **Columnas**: #, Fecha, Canal, Ítems (resumen), Ingreso Bruto, COGS, Costos Vta, Ganancia, Pago.
- **Modal de nueva venta**:
  - Fecha, Canal (select), Medio de pago (select).
  - Ítems: Producto (select) + Cantidad. Precio se pre-carga. Agregar múltiples ítems.
  - Envío ($), Descuento (%), TaxCost (%), ChannelCost (%).
  - Notas.
  - Resumen calculado en tiempo real.
- **Advertencia**: Si stock insuficiente, mostrar detalle con opción de continuar.

## Criterios de Aceptación

- [ ] Se registra la venta con todos los cálculos correctos.
- [ ] El precio unitario se toma del precio de lista del producto.
- [ ] El COGS se calcula como receta × lastPricePerUnit (snapshot).
- [ ] El stock de productos se reduce.
- [ ] Se crean StockMovements de tipo SaleOutput.
- [ ] Se crea CashMovement de ingreso.
- [ ] El saldo de la cuenta se incrementa.
- [ ] Descuento, TaxCost y ChannelCost se calculan sobre base imponible.
- [ ] Se permite vender con stock insuficiente (con advertencia).
- [ ] Los filtros y KPIs funcionan.

## Tests de Integración Esperados

- Test venta exitosa → todos los cálculos correctos, stocks y caja actualizados.
- Test venta con descuento → grossIncome correcto.
- Test venta con envío + impuesto + canal → todos los montos correctos.
- Test venta con stock insuficiente → se permite.
- Test venta sin ítems → error.
- Test venta sin canal → error.
- Test KPIs del período filtrado.
- Test COGS snapshot: cambiar precio de MP después → COGS de la venta no cambia.
