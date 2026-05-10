# Historia de Usuario 3: Ajuste de Stock de Materia Prima

## Descripción

Permite registrar ajustes manuales de stock de materia prima por motivos no comerciales: vencimiento, rotura, pérdida, corrección de inventario, etc.

## Actores

- Usuario (dueño/operador del negocio)

## Precondiciones

- La materia prima debe existir en el sistema.
- Debe tener stock registrado (puede ser 0).

## Flujo Principal

```mermaid
flowchart TD
    A[Usuario accede al módulo de Stock de MP] --> B[Selecciona una Materia Prima]
    B --> C[Selecciona 'Ajuste de Stock']
    C --> D[Completa: Cantidad +/-, Motivo, Fecha, Notas]
    D --> E{¿Datos válidos?}
    E -- No --> F[Muestra errores de validación]
    F --> D
    E -- Sí --> G{¿El ajuste deja stock negativo?}
    G -- Sí --> H[Advertencia: stock quedaría negativo]
    H --> I{¿Usuario confirma?}
    I -- No --> D
    I -- Sí --> J[Se registra el ajuste]
    G -- No --> J
    J --> K[Se crea StockMovement\ntype: Adjustment +/-, qty: cantidad\nref: AjusteId, motivo en Notes]
    K --> K2[Se actualiza Stock de MP\nCurrentQuantity += cantidad]
    K2 --> L[Confirmación al usuario]
```

## Tipos de Ajuste

```mermaid
flowchart LR
    AJUSTE[Ajuste de Stock] --> VENC[Vencimiento\n-Cantidad]
    AJUSTE --> ROT[Rotura\n-Cantidad]
    AJUSTE --> PERD[Pérdida\n-Cantidad]
    AJUSTE --> CORR[Corrección de inventario\n+/- Cantidad]
    AJUSTE --> OTRO[Otro\n+/- Cantidad]
```

## Ejemplo Concreto

> Se detecta que 200gr de Fragancia Vainilla vencieron.
>
> 1. Se selecciona Fragancia Vainilla.
> 2. Ajuste: -200gr, motivo: Vencimiento, fecha: 28/04/2026.
> 3. Stock de Fragancia Vainilla se reduce en 200gr.
> 4. No se genera movimiento de caja.

## Reglas de Negocio

- La cantidad puede ser positiva (corrección a favor) o negativa (baja).
- El motivo es obligatorio.
- No genera movimiento de caja (es ajuste físico, no monetario).
- Se permite stock negativo con advertencia (para corregir después).
- Queda registro histórico de todos los ajustes.

## Entidades Involucradas

| Entidad | Acción |
|---|---|
| Stock de MP | Actualizar (+/- cantidad) |
| StockMovement | Crear (AdjustmentIncrease o AdjustmentDecrease, ref: AjusteId) |
