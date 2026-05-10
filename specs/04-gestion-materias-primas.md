# Spec 04: Gestión de Materias Primas

## Descripción

CRUD de materias primas con stock inicial en 0, alerta de stock mínimo y filtros.

## Referencia

- US 01 (Gestión de Materias Primas).
- Correcciones: agregar `minStock`, precio vigente en listado, última compra, filtro por unidad.
- POC: `Analisis/POC/materias-primas.html`
- DER: colecciones `rawMaterials`, `stock`.

## Colecciones del DER

- `rawMaterials`
- `stock`

## Alcance

### Backend

- **Endpoints**:
  - `GET /api/raw-materials` — listar con stock actual, precio vigente, estado (OK/Bajo/Sin stock). Filtros: búsqueda por texto, unidad de medida.
  - `GET /api/raw-materials/:id` — detalle con stock.
  - `POST /api/raw-materials` — crear MP + inicializar stock en 0.
  - `PUT /api/raw-materials/:id` — editar (nombre, unidad, minStock).
- **Servicio**: `RawMaterialService`.
- **Repositorio**: `RawMaterialRepository`, `StockRepository`.
- **Reglas**:
  - Nombre único.
  - Unidades: gr, kg, ml, lt, unidad, cm.
  - Estado: si `currentQuantity` < `minStock` → "Bajo"; si 0 → "Sin stock"; sino "OK".
  - No se puede eliminar MP con stock > 0 o referenciada en recetas.

### Frontend

- **Pantalla**: Listado de materias primas con tabla.
- **Columnas**: Nombre, Unidad, Stock Actual, Alerta Mín., Estado, Precio Vigente, Última Compra.
- **Filtros**: Búsqueda por texto + combo de unidad de medida.
- **Acciones por fila**: Editar, Historial (link a spec futura), Ajustar (link a spec futura), Registrar Compra (link a spec futura).
- **Modal**: Alta/Edición de MP (nombre, unidad, stock mínimo).
- **Componente reutilizable**: `DataTable` con filtros y ordenamiento.

## Criterios de Aceptación

- [ ] Se puede crear una MP con nombre, unidad y stock mínimo.
- [ ] El stock se inicializa en 0 al crear.
- [ ] No se puede crear MP con nombre duplicado.
- [ ] El listado muestra estado correcto según stock vs minStock.
- [ ] Se puede editar nombre, unidad y minStock.
- [ ] Los filtros de búsqueda y unidad funcionan.

## Tests de Integración Esperados

- Test crear MP → stock inicializado en 0.
- Test crear MP con nombre duplicado → error.
- Test editar MP → datos actualizados.
- Test listar con filtro por unidad → resultados correctos.
- Test estado "Bajo" cuando stock < minStock.
- Test estado "Sin stock" cuando stock = 0.
