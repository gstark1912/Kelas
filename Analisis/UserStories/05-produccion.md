# Historia de Usuario 5: Producción

## Descripción

Registra una tanda de producción de un producto, descontando materia prima del stock, calculando el costo de producción y generando stock de producto terminado.

## Actores

- Usuario (dueño/operador del negocio)

## Precondiciones

- El producto debe existir y tener receta definida.
- Debe haber stock suficiente de todas las materias primas de la receta.

## Flujo Principal

```mermaid
flowchart TD
    A[Usuario accede al módulo de Producción] --> B[Selecciona 'Nueva Producción']
    B --> C[Selecciona Producto]
    C --> D[Indica Cantidad a producir]
    D --> E[Sistema calcula MP necesaria\nReceta × Cantidad]
    E --> F{¿Hay stock suficiente\nde todas las MP?}
    F -- No --> G[Muestra detalle de faltantes\nMP necesaria vs disponible]
    G --> H{¿Usuario desea continuar\nigual?}
    H -- No --> D
    H -- Sí --> I[Advertencia registrada]
    I --> J
    F -- Sí --> J[Completa: Fecha, Notas]
    J --> K[Se registra la Producción]
    K --> L[Se crean StockMovements por cada ingrediente\ntype: ProductionConsumption, qty: -cantidad\nref: ProducciónId]
    L --> L2[Se actualiza Stock de MP por cada ingrediente\nCurrentQuantity -= cantidad]
    L2 --> M[Se calcula Costo de Producción]
    M --> N[Se crea StockMovement\ntype: ProductionOutput, qty: +cantidad producida\nref: ProducciónId]
    N --> N2[Se actualiza Stock de Producto Terminado\nCurrentQuantity += cantidad]
    N2 --> O[Confirmación con resumen:\nCantidad, Costo total, Costo unitario]
```

## Cálculo de Costo de Producción

```mermaid
flowchart TD
    A[Obtener Receta del Producto] --> B[Para cada ingrediente de la receta]
    B --> C[Buscar Precio Histórico vigente\nde la MP a la fecha de producción]
    C --> D[Costo ingrediente =\nCantidad receta × Cantidad producida × Precio por unidad]
    D --> E{¿Más ingredientes?}
    E -- Sí --> B
    E -- No --> F[Costo Total Producción =\nSuma de costos de ingredientes]
    F --> G[Costo Unitario =\nCosto Total / Cantidad Producida]
```

## Ejemplo Concreto

> Se producen 10 unidades de "Vela Aromática Vainilla 200gr".
>
> **Receta por unidad:**
> - Cera de Soja: 180gr
> - Fragancia Vainilla: 20gr
> - Mecha: 1 unidad
> - Frasco: 1 unidad
>
> **Para 10 unidades se necesita:**
> - Cera de Soja: 1800gr → se descuenta del stock
> - Fragancia Vainilla: 200gr → se descuenta del stock
> - Mecha: 10 unidades → se descuenta del stock
> - Frasco: 10 unidades → se descuenta del stock
>
> **Cálculo de costo (precios vigentes):**
> - Cera de Soja: 1800gr × $5/gr = $9.000
> - Fragancia Vainilla: 200gr × $15/gr = $3.000
> - Mecha: 10 × $200 = $2.000
> - Frasco: 10 × $500 = $5.000
> - **Costo total: $19.000**
> - **Costo unitario: $1.900**
>
> **Resultado:**
> - Stock Vela Aromática Vainilla: +10 unidades
> - Costo registrado en la tanda: $19.000

## Reglas de Negocio

- La cantidad a producir debe ser > 0.
- Se permite producir con stock insuficiente de MP (con advertencia), para cubrir casos donde el stock no está actualizado.
- El costo se calcula con el precio histórico vigente a la fecha de producción.
- El costo de producción queda asociado a la tanda (para calcular COGS después).
- No genera movimiento de caja (la plata ya salió en la compra de MP).
- Cada tanda queda registrada individualmente para trazabilidad.

## Entidades Involucradas

| Entidad | Acción |
|---|---|
| Producción | Crear |
| Producto / Receta | Consultar |
| Stock de MP | Actualizar (-cantidad por cada ingrediente) |
| StockMovement (MP) | Crear N registros (ProductionConsumption, ref: ProducciónId) |
| Precio Histórico MP | Consultar (para cálculo de costo) |
| Stock de Producto Terminado | Actualizar (+cantidad producida) |
| StockMovement (PT) | Crear (ProductionOutput, ref: ProducciónId) |
