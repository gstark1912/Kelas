# Spec 10: Gastos

## Descripción

Registrar gastos operativos no productivos, generando movimiento de caja automático. Incluye gráfico de gastos por categoría.

## Referencia

- US 07 (Registro de Gasto).
- Correcciones: agregar filtros, KPIs, renombrar "Notas" a "Descripción", gráfico de gastos por categoría movido desde dashboard a esta sección.
- Reunión cliente: "Promedio diario no va; eliminarlo." "Agregar gráfico de Gastos por categoría."
- POC: `Analisis/POC/gastos.html`
- DER: colección `expenses`.

## Colecciones del DER

- `expenses`
- `cashMovements` (crear egreso automático)
- `cashAccounts` (actualizar saldo)

## Alcance

### Backend

- **Endpoints**:
  - `POST /api/expenses` — registrar gasto.
  - `GET /api/expenses` — listar con filtros (categoría, cuenta, rango de fechas). Incluir KPIs.
  - `GET /api/expenses/:id` — detalle.
  - `GET /api/expenses/by-category?from=X&to=Y` — agrupado por categoría para gráfico.
- **Servicio**: `ExpenseService`.
- **Repositorio**: `ExpenseRepository`.
- **Flujo al registrar gasto**:
  1. Validar (monto > 0, categoría obligatoria, fecha obligatoria, descripción obligatoria).
  2. Crear documento en `expenses`.
  3. Crear `cashMovement` (egreso automático).
  4. Actualizar `cashAccounts.currentBalance`.
- **Categorías**: Marketing, Packaging, Envíos, Alquiler, Servicios, Herramientas, Otro.
- **KPIs**: Total gastos del período, categoría con mayor gasto.

### Frontend

- **Pantalla**: Listado de gastos con tabla.
- **KPIs**: Cards con Total Gastos (monto + cantidad de registros), Mayor Categoría.
- **Filtros**: Por categoría, cuenta, rango de fechas.
- **Columnas**: Fecha, Monto, Categoría, Descripción, Cuenta.
- **Gráfico**: Gastos por categoría (barras horizontales o torta) del período filtrado.
- **Modal de nuevo gasto**: Fecha, Monto, Categoría (select), Cuenta de pago (select), Descripción, Notas.

## Criterios de Aceptación

- [ ] Se registra el gasto correctamente.
- [ ] Se crea CashMovement de egreso automático.
- [ ] El saldo de la cuenta se reduce.
- [ ] Los gastos no afectan stock.
- [ ] Los filtros funcionan.
- [ ] El gráfico por categoría muestra datos correctos.
- [ ] KPIs se calculan correctamente.

## Tests de Integración Esperados

- Test gasto exitoso → movimiento de caja creado, saldo actualizado.
- Test gasto con monto 0 → error.
- Test gasto sin categoría → error.
- Test filtro por categoría → resultados correctos.
- Test agrupación por categoría para gráfico.
- Test KPIs del período.
