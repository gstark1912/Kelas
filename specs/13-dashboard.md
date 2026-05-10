# Spec 13: Dashboard

## Descripción

Vista principal del sistema con métricas financieras, ventas por canal y ranking de productos para un rango de fechas.

## Referencia

- US 10 (Dashboard Mensual).
- Correcciones: filtro por rango de fechas (no mes cerrado), "Ingresos por Ventas", panel de gastos por categoría NO va en dashboard, top productos como gráfico.
- Reunión cliente: "Gastos por categoría sobra en el dashboard; moverlo a la sección Gastos."
- POC: `Analisis/POC/dashboard.html`
- DER: consulta a `sales`, `expenses`, `products`.

## Colecciones del DER

- `sales` (ingresos, COGS, costos de venta, canal, productos)
- `expenses` (gastos del período)
- `products` (nombres para ranking)

## Alcance

### Backend

- **Endpoints**:
  - `GET /api/dashboard?from=X&to=Y` — retorna todas las métricas del período.
- **Servicio**: `DashboardService`.
- **Métricas a calcular**:
  - **Ingresos por Ventas**: Suma de `grossIncome` de ventas del período.
  - **COGS**: Suma de `totalCogs` de ventas del período.
  - **Ganancia Bruta**: Ingresos - COGS.
  - **Costos de Venta**: Suma de `taxCostAmount + channelCostAmount` de ventas.
  - **Gastos Operativos**: Suma de `amount` de expenses del período.
  - **Resultado Neto**: Ganancia Bruta - Costos de Venta - Gastos.
  - **Ventas por Canal**: Agrupado por canal (cantidad, ingreso, porcentaje).
  - **Top Productos por Ventas**: Producto, Unidades, Ingresos (gráfico).
  - **Top Productos por Rentabilidad**: Producto, Margen $, Rentabilidad %.
- **Período por defecto**: Mes en curso (1° del mes hasta hoy).

### Frontend

- **Pantalla de Dashboard** (página principal post-login).
- **Filtro**: Rango de fechas (Desde / Hasta).
- **Cards superiores**: Ingresos por Ventas, COGS, Costos Vta, Gastos.
- **Cards de resumen**: Ganancia Bruta, Costos Venta + Gastos, Resultado Neto (verde si positivo, rojo si negativo).
- **Gráfico Ventas por Canal**: Barras horizontales.
- **Gráfico Top Productos por Ventas**: Barras con Producto + Unidades + Ingresos.
- **Tabla Top Productos por Rentabilidad**: #, Producto, Margen $, Rentabilidad %.

## Criterios de Aceptación

- [ ] El dashboard muestra métricas correctas para el período.
- [ ] El filtro de rango de fechas funciona.
- [ ] Por defecto muestra el mes en curso.
- [ ] Resultado Neto se muestra en verde (ganancia) o rojo (pérdida).
- [ ] Ventas por canal muestra datos agrupados correctamente.
- [ ] Top productos por ventas muestra gráfico con unidades e ingresos.
- [ ] Top productos por rentabilidad muestra tabla ordenada por %.
- [ ] NO se muestra "Gastos por categoría" en el dashboard.
- [ ] Si no hay datos, muestra todo en $0.

## Tests de Integración Esperados

- Test dashboard con ventas y gastos → métricas correctas.
- Test dashboard sin datos → todo en 0.
- Test filtro por rango de fechas → solo incluye datos del período.
- Test ventas por canal → agrupación correcta.
- Test top productos → ordenamiento correcto.
- Test resultado neto positivo y negativo.
