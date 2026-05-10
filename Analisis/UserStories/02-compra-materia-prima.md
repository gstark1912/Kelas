# Historia de Usuario 2: Compra de Materia Prima

## Descripción

Registra la compra de materia prima, actualizando stock, caja y precio histórico.

## Actores

- Usuario (dueño/operador del negocio)

## Precondiciones

- La materia prima debe existir en el sistema.
- Debe existir al menos una cuenta de caja.

## Flujo Principal

```mermaid
flowchart TD
    A[Usuario accede al módulo de Compras de MP] --> B[Selecciona 'Nueva Compra']
    B --> C[Selecciona Materia Prima]
    C --> D[Completa: Cantidad, Precio total, Fecha, Proveedor opcional, Notas]
    D --> E{¿Datos válidos?}
    E -- No --> F[Muestra errores de validación]
    F --> D
    E -- Sí --> G[Se registra la Compra de MP]
    G --> H[Se calcula precio por unidad = Precio total / Cantidad]
    H --> I[Se crea StockMovement\ntype: PurchaseEntry, qty: +Cantidad\nref: CompraId]
    I --> I2[Se actualiza Stock de MP\nCurrentQuantity += Cantidad]
    I2 --> J[Se crea registro en Precios de MP\nPrecio por unidad, DateFrom = fecha compra]
    J --> K[Se registra Movimiento de Caja\ntipo: egreso, monto: precio total]
    K --> L[Se actualiza saldo de la cuenta]
    L --> M[Confirmación al usuario]
```

## Diagrama de Impacto en Entidades

```mermaid
flowchart LR
    COMPRA[Compra de MP] --> SM[StockMovement\nPurchaseEntry +Cantidad]
    SM --> STOCK[Stock de MP\nCurrentQuantity += Cantidad]
    COMPRA --> PRECIO[Precio Histórico MP\nNuevo registro]
    COMPRA --> CAJA[Movimiento de Caja\nEgreso automático]
    CAJA --> SALDO[Saldo Cuenta\n-Monto]
```

## Ejemplo Concreto

> Se compra 1kg de Cera de Soja a $5.000.
>
> 1. Se registra compra: Cera de Soja, 1000gr, $5.000, 28/04/2026.
> 2. Stock de Cera de Soja: +1000gr.
> 3. Precio histórico: $5 por gr desde 28/04/2026.
> 4. Caja (Efectivo): egreso de $5.000.
> 5. Saldo Efectivo se reduce en $5.000.

## Reglas de Negocio

- La cantidad debe ser > 0.
- El precio total debe ser > 0.
- El precio por unidad se calcula automáticamente (precio total / cantidad).
- El nuevo precio histórico no reemplaza los anteriores, se agrega con fecha.
- El movimiento de caja se genera automáticamente (no manual).
- Se debe seleccionar desde qué cuenta se paga.

## Entidades Involucradas

| Entidad | Acción |
|---|---|
| Compra de MP | Crear |
| Stock de MP | Actualizar (+cantidad) |
| StockMovement | Crear (PurchaseEntry, ref: CompraId) |
| Precio Histórico MP | Crear nuevo registro |
| Movimiento de Caja | Crear (egreso automático) |
| Cuenta de Caja | Actualizar saldo (-monto) |
