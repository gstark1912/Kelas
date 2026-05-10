# Historia de Usuario 1: Gestión de Materias Primas

## Descripción

Permite dar de alta, editar y consultar materias primas y su stock actual.

## Actores

- Usuario (dueño/operador del negocio)

## Precondiciones

- Ninguna (es el punto de partida del sistema)

## Flujos

### 1a. Alta de Materia Prima

```mermaid
flowchart TD
    A[Usuario accede al módulo de Materias Primas] --> B[Selecciona 'Nueva Materia Prima']
    B --> C[Completa: Nombre, Unidad de medida]
    C --> D{¿Datos válidos?}
    D -- No --> E[Muestra errores de validación]
    E --> C
    D -- Sí --> F[Se crea la Materia Prima]
    F --> G[Se inicializa Stock de MP en 0]
    G --> H[Confirmación al usuario]
```

### 1b. Edición de Materia Prima

```mermaid
flowchart TD
    A[Usuario selecciona una Materia Prima existente] --> B[Selecciona 'Editar']
    B --> C[Modifica: Nombre y/o Unidad de medida]
    C --> D{¿Datos válidos?}
    D -- No --> E[Muestra errores de validación]
    E --> C
    D -- Sí --> F[Se actualiza la Materia Prima]
    F --> G[Confirmación al usuario]
```

### 1c. Consulta de Stock de Materia Prima

```mermaid
flowchart TD
    A[Usuario accede al listado de Materias Primas] --> B[Se muestra lista con: Nombre, Unidad, Stock actual]
    B --> C[Usuario puede filtrar o buscar]
    C --> D[Se consulta entidad Stock de MP]
    D --> E[Se muestra el saldo actual de cada MP]
```

## Reglas de Negocio

- El nombre de la materia prima debe ser único.
- La unidad de medida es obligatoria (gr, kg, ml, lt, unidad, etc.).
- Al crear una MP, su stock inicial es 0.
- No se puede eliminar una MP que tenga stock > 0 o esté referenciada en recetas.

## Entidades Involucradas

| Entidad | Acción |
|---|---|
| Materia Prima | Crear / Editar / Consultar |
| Stock de MP | Inicializar en 0 al crear MP |
