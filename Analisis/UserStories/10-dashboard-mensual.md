# Historia de Usuario 10: Dashboard Mensual

## Descripción

Vista principal del sistema que muestra las métricas financieras clave del negocio para un mes determinado, permitiendo responder las tres preguntas fundamentales: ¿Estoy ganando plata? ¿Qué productos son más rentables? ¿Vale la pena el tiempo invertido?

## Actores

- Usuario (dueño/operador del negocio)

## Precondiciones

- Deben existir registros de ventas, producción y/o gastos para el período consultado.

## Flujo Principal

```mermaid
flowchart TD
    A[Usuario accede al Dashboard] --> B[Se muestra el período actual por defecto\ndesde inicio hasta fin del mes en curso]
    B --> C[Usuario puede cambiar rango de fechas\nFecha desde / Fecha hasta]
    C --> D[Sistema recopila datos del período]
    D --> E[Calcula métricas financieras]
    E --> F[Calcula métricas por canal]
    F --> G[Calcula ranking de productos]
    G --> H[Renderiza dashboard completo]
```

## Cálculo de Métricas Financieras

```mermaid
flowchart TD
    A[Obtener ventas del mes] --> B[Ingresos Totales =\nSuma de ingresos brutos de cada venta]
    
    C[Obtener COGS de cada venta] --> D[COGS Total =\nSuma de COGS de todas las ventas]
    
    B --> E[Ganancia Bruta =\nIngresos Totales - COGS Total]
    D --> E
    
    E --> E2[Costos de Venta =\nSuma TaxCost + ChannelCost de todas las ventas]

    F[Obtener gastos del mes] --> G[Gastos Totales =\nSuma de todos los gastos]
    
    E --> H[Resultado Neto =\nGanancia Bruta - Costos de Venta - Gastos Totales]
    E2 --> H
    G --> H
    
    H --> I{Resultado Neto > 0?}
    I -- Sí --> J[Se muestra en verde:\nGanancia]
    I -- No --> K[Se muestra en rojo:\nPérdida]
```

## Métricas por Canal

```mermaid
flowchart TD
    A[Agrupar ventas por Canal] --> B[Para cada canal calcular:]
    B --> C[Cantidad de ventas]
    B --> D[Ingreso total]
    B --> E[Porcentaje del total]
    C & D & E --> F[Ordenar por ingreso descendente]
    F --> G[Mostrar gráfico de torta o barras]
```

## Ranking de Productos

```mermaid
flowchart TD
    A[Agrupar ítems de venta por Producto] --> B[Para cada producto calcular:]
    B --> C[Unidades vendidas]
    B --> D[Ingreso total por producto]
    B --> E[COGS total por producto]
    B --> F[Ganancia = Ingreso - COGS]
    B --> G[Rentabilidad % = Ganancia / Ingreso × 100]
    C & D --> H[Top productos por Ventas\ngráfico con Producto + Unidades + Ingresos]
    F & G --> I[Top productos por Rentabilidad\nordenado por rentabilidad %]
```

## Layout del Dashboard

```
┌─────────────────────────────────────────────────────────┐
│  Dashboard                [Desde: 01/04/2026] [Hasta: 30/04/2026]  │
├─────────────┬──────────────┬──────────────┬─────────────┤
│  Ingresos   │    COGS      │  Costos Vta  │  Gastos     │
│  por Ventas │              │              │             │
│  $45.000    │   $22.000    │   $3.130     │  $8.000     │
├─────────────┴──────────────┴──────────────┴─────────────┤
│                                                         │
│  Ganancia Bruta: $23.000 (51.1%)                        │
│  Costos de Venta: $3.130 (TaxCost + ChannelCost)        │
│  Gastos Operativos: $8.000                              │
│  Resultado Neto: +$11.870 (26.4%)  ✅ Verde             │
│                                                         │
├──────────────────────────┬──────────────────────────────┤
│  Ventas por Canal        │  Top Productos por Ventas    │
│                          │  (gráfico)                   │
│  ● Feria: $20.000 (44%) │                              │
│  ● Instagram: $15.000   │  Producto     | Uds | Ingreso│
│  ● Tienda: $10.000      │  Vela Vainilla|  12 | $18.000│
│                          │  Vela Lavanda |   8 | $12.000│
├──────────────────────────┤  Vela Canela  |   5 | $8.000 │
│  Top Productos           │  Set Regalo   |   3 | $7.000 │
│  Rentabilidad            │                              │
│                          │                              │
│  1. Set Regalo    62.0%  │                              │
│  2. Vela Canela   55.0%  │                              │
│  3. Vela Vainilla 48.6%  │                              │
│  4. Vela Lavanda  45.0%  │                              │
└──────────────────────────┴──────────────────────────────┘
```

## Ejemplo Concreto

> **Dashboard - 01/04/2026 al 30/04/2026**
>
> | Métrica | Valor |
> |---|---|
> | Ingresos por Ventas | $45.000 |
> | COGS (costo de ventas realizadas) | $22.000 |
> | Ganancia Bruta | $23.000 |
> | Costos de Venta (Tax + Canal) | $3.130 |
> | Gastos Operativos | $8.000 |
> | **Resultado Neto** | **+$11.870** |
>
> **¿Estoy ganando plata?** → Sí, $11.870 netos en el período.
> **¿Qué producto conviene más?** → Set Regalo tiene mejor rentabilidad (62%).
> **¿Estamos cubriendo gastos?** → Sí, la ganancia bruta ($23.000) cubre costos de venta ($3.130) y gastos ($8.000) con margen.

## Reglas de Negocio

- El dashboard filtra por rango de fechas (fecha desde / fecha hasta), no por mes cerrado.
- Por defecto muestra el mes en curso (desde el 1° hasta hoy o fin de mes).
- "Ingresos" se refiere exclusivamente a ingresos por ventas.
- El COGS es el costo de las ventas realizadas en el período (no el gasto total en insumos/compras de MP).
- Todos los cálculos se basan en datos registrados (ventas, producción, gastos).
- Los gastos incluyen solo los del módulo Gastos (no compras de MP).
- El resultado neto es: Ingresos por Ventas - COGS - Costos de Venta (TaxCost + ChannelCost) - Gastos.
- Si no hay datos para el período, se muestra todo en $0.
- El panel "Gastos por categoría" NO se muestra en el dashboard (se muestra en la sección Gastos).
- "Top productos por ventas" se muestra como gráfico con columnas: Producto, Unidades, Ingresos.

## Entidades Involucradas

| Entidad | Acción |
|---|---|
| Venta / Ítems de Venta | Consultar (ingresos, COGS, canal, productos) |
| Gasto | Consultar (gastos por categoría) |
| Producción | Consultar (para COGS de productos) |
| Producto | Consultar (nombres, datos) |
