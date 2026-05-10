# Spec 11: Movimiento Manual de Caja

## Descripción

Registrar movimientos de caja manuales (retiros, aportes, ajustes) y transferencias entre cuentas.

## Referencia

- US 08 (Movimiento Manual de Caja).
- DER: colecciones `cashMovements`, `cashAccounts`.

## Colecciones del DER

- `cashMovements`
- `cashAccounts`

## Alcance

### Backend

- **Endpoints**:
  - `POST /api/cash-movements/manual` — registrar movimiento manual (ingreso o egreso).
  - `POST /api/cash-movements/transfer` — transferencia entre cuentas.
- **Servicio**: `CashMovementService`.
- **Repositorio**: `CashMovementRepository`.
- **Flujo movimiento simple**:
  1. Validar (monto > 0, concepto obligatorio, cuenta existe).
  2. Crear `cashMovement` (origin: "manual", concepto según tipo).
  3. Actualizar `cashAccounts.currentBalance`.
  4. Si egreso deja saldo negativo → permitir con advertencia.
- **Flujo transferencia**:
  1. Validar (monto > 0, cuenta origen ≠ destino).
  2. Crear 2 `cashMovements` vinculados (egreso en origen, ingreso en destino).
  3. Actualizar ambos saldos.
  4. Vincular con `linkedMovementId`.
- **Conceptos manuales**: Retiro de efectivo, Aporte de capital, Ajuste de saldo, Transferencia, Otro.

### Frontend

- **Acceso**: Desde la pantalla de Caja.
- **Modal de movimiento**: Tipo (Ingreso/Egreso), Cuenta, Monto, Concepto (select), Fecha, Notas.
- **Modal de transferencia**: Cuenta Origen, Cuenta Destino, Monto, Fecha.
- **Advertencia**: Si egreso deja saldo negativo.

## Criterios de Aceptación

- [ ] Se registra movimiento manual de ingreso.
- [ ] Se registra movimiento manual de egreso.
- [ ] El saldo se actualiza correctamente.
- [ ] Se permite saldo negativo con advertencia.
- [ ] La transferencia crea 2 movimientos vinculados.
- [ ] La transferencia actualiza ambos saldos.
- [ ] No se puede transferir a la misma cuenta.

## Tests de Integración Esperados

- Test ingreso manual → saldo aumenta.
- Test egreso manual → saldo disminuye.
- Test egreso que deja saldo negativo → se permite.
- Test transferencia → saldo origen baja, saldo destino sube.
- Test transferencia misma cuenta → error.
- Test sin concepto → error.
