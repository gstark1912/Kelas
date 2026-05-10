# Spec 07: Gestión de Productos

## Descripción

CRUD de productos terminados con receta, cálculo de costo estimado, margen y margen mínimo.

## Referencia

- US 04 (Gestión de Productos).
- Correcciones: agregar `minMargin`, concepto "ocultar" en vez de "eliminar", búsqueda por texto, columnas ordenables.
- Reunión cliente: margen mínimo con alerta visual (amarillo si está cerca del mínimo).
- POC: `Analisis/POC/productos.html`
- DER: colección `products`, `stock`.

## Colecciones del DER

- `products`
- `stock`
- `rawMaterials` (consultar para receta y cálculo de costo)

## Alcance

### Backend

- **Endpoints**:
  - `GET /api/products` — listar productos visibles con costo estimado, margen, stock. Filtro: búsqueda por texto.
  - `GET /api/products/:id` — detalle con receta completa.
  - `POST /api/products` — crear producto + inicializar stock en 0.
  - `PUT /api/products/:id` — editar (nombre, descripción, precio, horas, margen mínimo).
  - `PUT /api/products/:id/recipe` — actualizar receta.
  - `PATCH /api/products/:id/visibility` — ocultar/mostrar producto.
- **Servicio**: `ProductService`.
- **Repositorio**: `ProductRepository`.
- **Cálculo de costo estimado**: Sumar `recipe[i].quantity × rawMaterials[recipe[i].rawMaterialId].lastPricePerUnit`.
- **Cálculo de margen**: `(listPrice - costoEstimado) / listPrice × 100`.
- **Reglas**:
  - Nombre único.
  - No se puede ocultar producto con stock > 0 (o sí, con advertencia).
  - Una MP no puede aparecer dos veces en la misma receta.

### Frontend

- **Pantalla**: Listado de productos con tabla ordenable.
- **Columnas**: Producto, Precio Lista, Costo Est., Margen %, Margen Mín., Stock, Hs. Prod.
- **Ordenamiento**: Click en encabezado para alternar asc/desc.
- **Alerta visual de margen**: Si margen < margen mínimo → celda roja. Si margen está a un 10% del mínimo → celda amarilla.
- **Búsqueda**: Campo de texto para filtrar.
- **Acciones**: Nuevo Producto, Editar, Ocultar, Editar Receta.
- **Modal de producto**: Nombre, Descripción, Precio de lista, Horas estimadas, Margen mínimo.
- **Modal/Vista de receta**: Lista de ingredientes (MP + cantidad), agregar/quitar ingredientes.

## Criterios de Aceptación

- [ ] Se puede crear un producto con todos sus campos.
- [ ] El stock se inicializa en 0.
- [ ] Se puede definir y editar la receta.
- [ ] El costo estimado se calcula correctamente desde la receta y precios de MP.
- [ ] El margen se calcula y muestra correctamente.
- [ ] La alerta visual de margen funciona (rojo/amarillo).
- [ ] Se puede ocultar un producto (no aparece en listados operativos).
- [ ] Las columnas son ordenables.
- [ ] No se permite MP duplicada en receta.

## Tests de Integración Esperados

- Test crear producto → stock en 0.
- Test crear producto con nombre duplicado → error.
- Test definir receta → costo estimado calculado.
- Test editar receta → costo se recalcula.
- Test MP duplicada en receta → error.
- Test ocultar producto → no aparece en listado de visibles.
- Test cálculo de margen correcto.
