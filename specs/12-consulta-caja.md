# Spec 12: Consulta de Caja

## Descripción

Pantalla de consulta de caja con resumen de cuentas, historial de movimientos con filtros y links a entidades origen.

## Referencia

- US 09 (Consulta de Caja).
- Correcciones: columna "Origen" como link navegable, concepto normalizado, eliminar filtro "Automático/Manual", agregar columna "Descripción".
- Reunión cliente: "Falta un filtro y una columna de Concepto normalizado."
- POC: `Analisis/POC/caja.html`
- DER: colecciones `cashAccounts`, `cashMovements`.

## Colecciones del DER

- `cashAccounts` (consultar saldos)
- `cashMovements` (consultar/filtrar)

## Alcance

### Backend

- **Endpoints**:
  - `GET /api/cash-accounts/summary` — resumen de todas las cuentas con saldo + total.
  - `GET /api/cash-movements` — listar movimientos con filtros (rango de fechas, tipo ingreso/egreso, concepto, cuenta). Incluir subtotales del período.
- **Servicio**: `CashMovementService` (extender).
- **Repositorio**: `CashMovementRepository` (extender).
- **Subtotales**: Total ingresos, Total egresos, Neto del período filtrado.
- **Conceptos normalizados**: Venta, Compra MP, Gasto, Aporte de capital, Retiro de efectivo, Transferencia, Ajuste de saldo.

### Frontend

- **Pantalla de Caja**:
  - **Resumen**: Cards con saldo de cada cuenta + saldo total.
  - **Tabla de movimientos**: Fecha, Tipo (Ingreso/Egreso), Monto, Concepto, Descripción, Cuenta, Origen (link).
  - **Filtros**: Rango de fechas, Tipo, Concepto (select), Cuenta (select).
  - **Subtotales**: Mostrar total ingresos / egresos / neto del período filtrado.
  - **Origen como link**: Para movimientos automáticos, link a la venta/compra/gasto. Para manuales, texto "Manual" sin link.
- **Botones de acción**: "Nuevo Movimiento" y "Transferencia entre cuentas" (abren modals de spec 11).

## Criterios de Aceptación

- [ ] Se muestra resumen de cuentas con saldos correctos.
- [ ] Se muestra saldo total (suma de todas las cuentas).
- [ ] El historial muestra todos los movimientos.
- [ ] Los filtros funcionan (fechas, tipo, concepto, cuenta).
- [ ] Los subtotales del período son correctos.
- [ ] La columna Origen es un link navegable para movimientos automáticos.
- [ ] Para movimientos manuales, Origen muestra "Manual" sin link.
- [ ] El concepto es un valor normalizado.

## Tests de Integración Esperados

- Test resumen de cuentas → saldos correctos.
- Test listado de movimientos con filtro por fecha.
- Test filtro por concepto.
- Test filtro por tipo (ingreso/egreso).
- Test subtotales del período.
- Test que movimientos automáticos tienen referenceType y referenceId.
