# Spec 03: Cuentas de Caja

## Descripción

Implementar la gestión de cuentas de caja (Efectivo, Banco, Mercado Pago). Son la base para todos los movimientos monetarios del sistema.

## Referencia

- US 09 (Consulta de Caja) — las cuentas son prerequisito.
- DER: colección `cashAccounts`.

## Colecciones del DER

- `cashAccounts`

## Alcance

### Backend

- **Endpoints**:
  - `GET /api/cash-accounts` — listar cuentas activas con saldo.
  - `GET /api/cash-accounts/:id` — detalle de una cuenta.
  - `POST /api/cash-accounts` — crear cuenta (para seed inicial).
- **Servicio**: `CashAccountService` — lógica de consulta y creación.
- **Repositorio**: `CashAccountRepository` — queries a MongoDB.
- **Seed**: Al iniciar la app, crear las 3 cuentas por defecto si no existen (Efectivo, Banco, Mercado Pago) con saldo 0.

### Frontend

- No tiene pantalla propia en esta spec (se usa desde Caja en specs posteriores).
- Componente reutilizable: `CashAccountSelector` (dropdown de cuentas).

## Criterios de Aceptación

- [ ] Al iniciar la app, existen las 3 cuentas por defecto.
- [ ] `GET /api/cash-accounts` retorna las cuentas con su saldo.
- [ ] El nombre de cuenta es único.
- [ ] El saldo inicial es 0.

## Tests de Integración Esperados

- Test que verifica seed de cuentas por defecto.
- Test crear cuenta con nombre duplicado → error.
- Test listar cuentas → retorna las 3 por defecto.
