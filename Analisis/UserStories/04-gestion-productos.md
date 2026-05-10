# Historia de Usuario 4: Gestión de Productos

## Descripción

Permite crear, editar y consultar productos terminados, incluyendo su receta y precio de lista.

## Actores

- Usuario (dueño/operador del negocio)

## Precondiciones

- Para definir receta: las materias primas referenciadas deben existir.

## Flujos

### 4a. Alta de Producto

```mermaid
flowchart TD
    A[Usuario accede al módulo de Productos] --> B[Selecciona 'Nuevo Producto']
    B --> C[Completa: Nombre, Descripción, Horas estimadas de producción, Precio de lista]
    C --> D{¿Datos válidos?}
    D -- No --> E[Muestra errores de validación]
    E --> C
    D -- Sí --> F[Se crea el Producto]
    F --> G[Se inicializa Stock de Producto Terminado en 0]
    G --> H[Redirige a definir Receta]
    H --> I[Fin]
```

### 4b. Definición de Receta

```mermaid
flowchart TD
    A[Usuario accede a la Receta del Producto] --> B[Selecciona 'Agregar Ingrediente']
    B --> C[Selecciona Materia Prima del listado]
    C --> D[Indica Cantidad necesaria por unidad de producto]
    D --> E{¿Datos válidos?}
    E -- No --> F[Muestra errores de validación]
    F --> D
    E -- Sí --> G[Se agrega ingrediente a la receta]
    G --> H{¿Agregar otro ingrediente?}
    H -- Sí --> B
    H -- No --> I[Se guarda la Receta completa]
    I --> J[Se calcula y muestra Costo estimado del producto]
    J --> K[Fin]
```

### 4c. Edición de Producto

```mermaid
flowchart TD
    A[Usuario selecciona un Producto existente] --> B[Selecciona 'Editar']
    B --> C[Puede modificar: Nombre, Descripción, Horas estimadas, Precio de lista]
    C --> D{¿Datos válidos?}
    D -- No --> E[Muestra errores de validación]
    E --> C
    D -- Sí --> F[Se actualiza el Producto]
    F --> G[Confirmación al usuario]
```

### 4d. Consulta de Costo Estimado

```mermaid
flowchart TD
    A[Usuario consulta un Producto] --> B[Se obtiene la Receta]
    B --> C[Para cada ingrediente:\nse busca precio histórico vigente de la MP]
    C --> D[Costo ingrediente = Cantidad × Precio por unidad vigente]
    D --> E[Costo total estimado = Suma de todos los ingredientes]
    E --> F[Se muestra: Costo estimado, Precio de lista, Margen estimado]
```

## Ejemplo Concreto

> Se crea el producto "Vela Aromática Vainilla 200gr".
>
> 1. Nombre: Vela Aromática Vainilla 200gr
> 2. Descripción: Vela de cera de soja con fragancia vainilla
> 3. Horas estimadas: 1.5h
> 4. Precio de lista: $3.500
> 5. Receta:
>    - Cera de Soja: 180gr
>    - Fragancia Vainilla: 20gr
>    - Mecha: 1 unidad
>    - Frasco: 1 unidad
> 6. Costo estimado (según precios vigentes): $1.800
> 7. Margen estimado: $1.700 (48.6%)

## Reglas de Negocio

- El nombre del producto debe ser único.
- El precio de lista es de referencia (se puede modificar al vender).
- La receta puede tener uno o más ingredientes.
- Una misma MP no puede aparecer dos veces en la misma receta.
- El costo estimado se recalcula dinámicamente según precios vigentes de MP.
- No se puede eliminar un producto que tenga stock > 0 o ventas/producciones asociadas.
- La tabla de productos permite ordenar por cualquier columna (click en encabezado para alternar ascendente/descendente).

## Entidades Involucradas

| Entidad | Acción |
|---|---|
| Producto | Crear / Editar / Consultar |
| Receta (sub-entidad de Producto) | Crear / Editar |
| Stock de Producto Terminado | Inicializar en 0 al crear producto |
| Precio Histórico MP | Consultar (para costo estimado) |
