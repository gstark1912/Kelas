# Historia de Usuario 8: Movimiento Manual de Caja

## Descripción

Permite registrar movimientos de caja que no se generan automáticamente desde otros módulos: retiros de efectivo, ajustes, transferencias entre cuentas, aportes de capital, etc.

## Actores

- Usuario (dueño/operador del negocio)

## Precondiciones

- Debe existir al menos una cuenta de caja.

## Flujos

### 8a. Movimiento Simple (Ingreso o Egreso)

```mermaid
flowchart TD
    A[Usuario accede al módulo de Caja] --> B[Selecciona 'Nuevo Movimiento']
    B --> C[Selecciona Tipo: Ingreso o Egreso]
    C --> D[Selecciona Cuenta]
    D --> E[Completa: Monto, Concepto, Fecha, Notas]
    E --> F{¿Datos válidos?}
    F -- No --> G[Muestra errores de validación]
    G --> E
    F -- Sí --> H{¿Egreso deja saldo negativo?}
    H -- Sí --> I[Advertencia: saldo quedaría negativo]
    I --> J{¿Usuario confirma?}
    J -- No --> E
    J -- Sí --> K
    H -- No --> K[Se registra Movimiento de Caja\ntipo: manual]
    K --> L[Se actualiza saldo de la cuenta]
    L --> M[Confirmación al usuario]
```

### 8b. Transferencia entre Cuentas

```mermaid
flowchart TD
    A[Usuario selecciona 'Transferencia entre cuentas'] --> B[Selecciona Cuenta Origen]
    B --> C[Selecciona Cuenta Destino]
    C --> D[Indica Monto y Fecha]
    D --> E{¿Datos válidos?}
    E -- No --> F[Muestra errores]
    F --> D
    E -- Sí --> G{¿Cuenta origen tiene saldo suficiente?}
    G -- No --> H[Advertencia: saldo insuficiente]
    H --> I{¿Confirma?}
    I -- No --> D
    I -- Sí --> J
    G -- Sí --> J[Se registran 2 movimientos:\n1. Egreso en cuenta origen\n2. Ingreso en cuenta destino]
    J --> K[Se actualizan ambos saldos]
    K --> L[Confirmación al usuario]
```

## Conceptos Comunes

```mermaid
flowchart LR
    MOV[Movimientos Manuales] --> RET[Retiro de efectivo]
    MOV --> APO[Aporte de capital]
    MOV --> AJU[Ajuste de saldo]
    MOV --> TRANS[Transferencia entre cuentas]
    MOV --> OTRO[Otro]
```

## Ejemplo Concreto

> Se retiran $5.000 de la cuenta Banco para tener efectivo en la feria.
>
> 1. Transferencia: Banco → Efectivo, $5.000, 28/04/2026.
> 2. Movimiento 1: Banco, egreso, $5.000, concepto: "Retiro para feria".
> 3. Movimiento 2: Efectivo, ingreso, $5.000, concepto: "Retiro para feria".
> 4. Saldo Banco: -$5.000
> 5. Saldo Efectivo: +$5.000

## Reglas de Negocio

- El monto debe ser > 0.
- El concepto es obligatorio.
- Se permite saldo negativo con advertencia.
- Las transferencias generan dos movimientos vinculados.
- Los movimientos manuales se distinguen de los automáticos (tienen origen: "manual").
- No afectan stock ni otros módulos.

## Entidades Involucradas

| Entidad | Acción |
|---|---|
| Movimiento de Caja | Crear (manual) |
| Cuenta de Caja | Actualizar saldo |
