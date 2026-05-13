# Implementation Plan: Producción

## Overview

Implementar el módulo de producción end-to-end sin cambiar el modelo funcional validado: registrar tandas, descontar materias primas, crear movimientos de stock, calcular costo snapshot con `lastPricePerUnit`, incrementar stock de producto terminado y exponer listado con filtros, KPIs y detalle.

## Tasks

- [ ] 1. Backend domain models and contracts
  - [ ] 1.1 Crear entidad `ProductionBatch` y subdocumento `ProductionBatchIngredient` en `kelas-backend/Kelas.Domain/Entities/ProductionBatch.cs`.
    - Usar `ObjectId`, `[BsonId]`, `[BsonElement("camelCase")]`, `CreatedAt` y `UpdatedAt?` según steering.
    - Persistir `productId`, `quantity`, `date`, `totalCost`, `unitCost`, `ingredients`, `notes`, `createdAt`.
    - _Requirements: R1, R4_
  - [ ] 1.2 Crear DTOs request en `kelas-backend/Kelas.Domain/Models/Requests/`.
    - `CreateProductionBatchRequest` con `ProductId`, `Quantity`, `Date`, `Notes`, `ConfirmInsufficientStock`.
    - Mantener ids como `string` en DTOs.
    - _Requirements: R1, R2_
  - [ ] 1.3 Crear DTOs response en `kelas-backend/Kelas.Domain/Models/Responses/`.
    - `ProductionBatchListResponse`, `ProductionBatchDetailResponse`, `ProductionBatchIngredientResponse`, `ProductionBatchListResultResponse`, `ProductionKpisResponse`.
    - Agregar contrato estable para faltantes: `InsufficientStockItemResponse` y respuesta/error consumible por frontend.
    - _Requirements: R2, R4, R5_

- [ ] 2. Backend repositories
  - [ ] 2.1 Crear `IProductionBatchRepository` en `kelas-backend/Kelas.Domain/Interfaces/Repositories/`.
    - Incluir `CreateAsync`, `GetByIdAsync`, `GetAsync(productId, dateFrom, dateTo)` y `EnsureIndexesAsync`.
    - _Requirements: R1, R5_
  - [ ] 2.2 Implementar `ProductionBatchRepository` en `kelas-backend/Kelas.Repositories/ProductionBatchRepository.cs`.
    - Colección `productionBatches`.
    - Filtros dinámicos con `Builders<ProductionBatch>.Filter`.
    - Ordenar listados por `date` descendente.
    - Crear índices `{ productId: 1, date: -1 }` y `{ date: -1 }`.
    - _Requirements: R1, R5_
  - [ ] 2.3 Extender `IStockRepository` y `StockRepository` para consultas por lote.
    - Agregar `GetByItemsAsync(string itemType, IEnumerable<ObjectId> itemIds)`.
    - Usar `$in`; no hacer queries por ingrediente.
    - Actualizar `IncrementQuantityAsync` para setear `lastUpdated`.
    - Definir método explícito de upsert si hace falta soportar stock faltante.
    - _Requirements: R2, R3_
  - [ ] 2.4 Evaluar si el listado/detalle requiere `IProductRepository.GetByIdsAsync(...)`.
    - Si se necesita mapear varios `ProductName` en listado, agregar método por lote con `$in`.
    - Si el listado se resuelve sin N queries por otra vía, documentar la decisión en el código del servicio.
    - _Requirements: R5_

- [ ] 3. Backend service and API
  - [ ] 3.1 Crear `IProductionService` en `kelas-backend/Kelas.Domain/Interfaces/Services/IProductionService.cs`.
    - Métodos: `CreateAsync`, `GetAsync`, `GetByIdAsync`.
    - _Requirements: R1, R5_
  - [ ] 3.2 Implementar `ProductionService` en `kelas-backend/Kelas.Services/ProductionService.cs`.
    - Validar cantidad, fecha, producto visible y receta no vacía.
    - Cargar materias primas y stock por lote.
    - Calcular faltantes, `quantityUsed`, `pricePerUnit`, `cost`, `totalCost`, `unitCost`.
    - Mantener acceso a datos mediante repositorios; no usar `IMongoDatabase` ni `GetCollection`.
    - _Requirements: R1, R2, R3, R4_
  - [ ] 3.3 Implementar transacción de creación de producción.
    - Crear `productionBatch`.
    - Crear movimientos `ProductionConsumption` por ingrediente.
    - Decrementar stock de MP.
    - Crear movimiento `ProductionOutput`.
    - Incrementar stock de producto terminado.
    - Abort/commit con el patrón actual de transacciones del proyecto.
    - _Requirements: R1, R2, R3, R4_
  - [ ] 3.4 Implementar manejo de stock insuficiente.
    - Si hay faltantes y `ConfirmInsufficientStock != true`, devolver error 400 con contrato estable para frontend.
    - Si está confirmado, permitir stock negativo.
    - _Requirements: R2_
  - [ ] 3.5 Implementar consulta de listado, detalle y KPIs.
    - `GetAsync` debe aplicar filtros por producto y fechas.
    - Calcular KPIs del período filtrado: unidades producidas y costo total.
    - `GetByIdAsync` debe devolver ingredientes con nombres y snapshot.
    - Evitar N queries usando carga por lote cuando aplique.
    - _Requirements: R4, R5_
  - [ ] 3.6 Registrar dependencias en `kelas-backend/Kelas.IoC.Resolver/DependencyResolver.cs`.
    - Registrar repositorio y servicio como scoped.
    - _Requirements: R1_
  - [ ] 3.7 Crear `ProductionBatchesController` en `kelas-backend/Kelas.Api/Controllers/`.
    - `[Authorize]`, ruta `api/production-batches`.
    - `POST`, `GET`, `GET {id}`.
    - Controller delgado; sin lógica de negocio.
    - _Requirements: R1, R5_

- [ ] 4. Backend validation checkpoint
  - [ ] 4.1 Agregar tests de integración para producción.
    - Producción exitosa con stocks, movimientos y costo.
    - Stock insuficiente sin confirmación no modifica datos.
    - Stock insuficiente confirmado permite stock negativo.
    - Producto sin receta devuelve error.
    - Cantidad 0 devuelve error.
    - Snapshot no cambia al modificar `lastPricePerUnit`.
    - Filtros y KPIs.
    - _Requirements: R1, R2, R3, R4, R5_
  - [ ] 4.2 Ejecutar backend validation.
    - `dotnet test` desde `kelas-backend`.
    - Corregir fallos atribuibles a esta spec antes de avanzar al frontend.

- [ ] 5. Frontend production module
  - [ ] 5.1 Crear `productionService.js` en `kelas-frontend/src/services/`.
    - Métodos `getAll(filters)`, `getById(id)`, `create(data)`.
    - Usar instancia base `api.js`.
    - _Requirements: R1, R5_
  - [ ] 5.2 Crear `ProductionView.vue` en `kelas-frontend/src/views/`.
    - Header con "Producción" y acción "Nueva Producción".
    - KPIs de unidades producidas y costo total.
    - Filtros por producto y rango de fechas.
    - Tabla con fecha, producto, cantidad, costo total, costo unitario y notas.
    - Loading, empty state y toast de error/éxito.
    - _Requirements: R5_
  - [ ] 5.3 Crear `ProductionFormModal.vue` en `kelas-frontend/src/components/production/`.
    - Producto select, cantidad, fecha, notas.
    - Validación local mínima antes de enviar.
    - Submit contra `POST /api/production-batches`.
    - _Requirements: R1_
  - [ ] 5.4 Implementar confirmación de stock insuficiente.
    - Renderizar detalle de faltantes: MP, necesario, disponible, faltante.
    - Permitir cancelar o continuar.
    - Al continuar, reintentar con `confirmInsufficientStock: true`.
    - _Requirements: R2_
  - [ ] 5.5 Crear detalle de tanda.
    - Modal o vista secundaria con ingredientes usados, cantidades, precio snapshot y costo.
    - Usar `GET /api/production-batches/{id}`.
    - _Requirements: R4, R5_
  - [ ] 5.6 Registrar navegación.
    - Agregar ruta `/production` en `kelas-frontend/src/router/index.js`.
    - Habilitar item "Producción" en `kelas-frontend/src/components/layout/AppSidebar.vue`.
    - _Requirements: R5_

- [ ] 6. Frontend validation checkpoint
  - [ ] 6.1 Ejecutar build frontend.
    - `npm run build` desde `kelas-frontend`.
    - Corregir errores de compilación o lint/build.
  - [ ] 6.2 Validar manualmente el flujo principal.
    - Abrir pantalla Producción.
    - Crear producción con stock suficiente.
    - Crear producción con stock insuficiente y confirmar.
    - Verificar tabla, KPIs, filtros y detalle.
    - _Requirements: R1, R2, R3, R4, R5_

- [ ] 7. Final integration checkpoint
  - [ ] 7.1 Ejecutar validación completa disponible.
    - `dotnet test` en backend.
    - `npm run build` en frontend.
  - [ ] 7.2 Revisar consistencia de datos generados.
    - `productionBatches` contiene snapshot de ingredientes.
    - `stockMovements` tiene `ProductionConsumption` x N y `ProductionOutput` x 1.
    - `stock` refleja decrementos de MP e incremento de producto terminado.
  - [ ] 7.3 Actualizar checkboxes de este archivo sólo para tareas realmente implementadas y verificadas.

## Notes

- No implementar hasta que este plan sea validado explícitamente.
- Mantener el alcance en producción; no modificar ventas, compras ni productos salvo extensiones necesarias de repositorios compartidos.
- Evitar queries por ingrediente; usar `$in` y diccionarios en memoria.
- No exponer endpoints de update/delete para `productionBatches` ni `stockMovements`.
