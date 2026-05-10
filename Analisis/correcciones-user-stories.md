# Correcciones Propuestas a las User Stories

Revisión realizada comparando las User Stories documentadas contra el POC navegable.

---

## US 01 - Gestión de Materias Primas

### Discrepancias encontradas

| # | Tema | User Story dice | POC muestra | Propuesta |
|---|---|---|---|---|
| 1 | **Alerta mínima de stock** | No se menciona ningún campo de "stock mínimo" o "alerta" | La tabla tiene columna "ALERTA MÍN." con valores (1.000, 200, etc.) y columna "ESTADO" (OK, Bajo, Sin stock) | **Agregar a la US** el campo `StockMinimo` en la entidad Materia Prima y la regla de negocio: "Si `CurrentQuantity` < `StockMinimo`, el estado se muestra como 'Bajo'. Si es 0, se muestra 'Sin stock'." |
| 2 | **Precio vigente en listado** | No se menciona mostrar precio vigente en la tabla de MP | La tabla muestra columna "PRECIO VIGENTE" ($5,00/gr, etc.) | **Agregar a la US** en el flujo 1c: "Se muestra el último precio por unidad registrado (`lastPricePerUnit`)." |
| 3 | **Última compra** | No se menciona | La tabla muestra columna "ÚLTIMA COMPRA" con fecha | **Agregar a la US** en el flujo 1c: "Se muestra la fecha de la última compra registrada." |
| 4 | **Botón Registrar Compra** | La compra se documenta en US 02 como módulo separado | El POC tiene el botón "Registrar Compra" dentro de la pantalla de Materias Primas | **Decisión de diseño**: Definir si la compra se accede desde el módulo de MP (como muestra el POC) o es un módulo independiente. Sugerencia: mantener acceso desde ambos lados (botón en MP + sección propia si se necesita historial). Documentar en US 02 que el acceso puede ser desde la pantalla de MP. |
| 5 | **Botón Historial** | Se menciona trazabilidad en US 00 pero no se vincula explícitamente a la pantalla de MP | El POC tiene botón "Historial" por cada MP | **Agregar a la US 01** un flujo 1d: "Consulta de historial de movimientos de una MP" que referencie al modelo de StockMovements de US 00. |
| 6 | **Botón Ajustar** | Se documenta en US 03 como flujo separado | El POC tiene botón "Ajustar" inline por cada MP | **Agregar referencia cruzada** en US 01 indicando que desde el listado se puede acceder al ajuste de stock (US 03). |
| 7 | **Filtro por unidad de medida** | Solo menciona "filtrar o buscar" genéricamente | El POC tiene un combo específico "Todas las unidades" (gr, ml, cm, unidad) | **Especificar en la US** los filtros disponibles: búsqueda por texto + filtro por unidad de medida. |

---

## US 02 - Compra de Materia Prima

### Discrepancias encontradas

| # | Tema | User Story dice | POC muestra | Propuesta |
|---|---|---|---|---|
| 1 | **Acceso al módulo** | "Usuario accede al módulo de Compras de MP" | No hay link "Compras" en la navegación lateral. El acceso es desde el botón "Registrar Compra" en Materias Primas | **Corregir la US**: El acceso es desde la pantalla de Materias Primas, no desde un módulo independiente. O bien agregar una sección de historial de compras accesible desde MP. |
| 2 | **Proveedor** | Campo "Proveedor opcional" | No se ve en el POC (no hay modal de compra visible) | **Mantener en la US** como campo opcional. Verificar que el formulario de compra lo incluya. |
| 3 | **Precio Histórico MP** | La US menciona "Se crea registro en Precios de MP" como paso del flujo | — | **Corregir la US**: Al registrar una compra se actualiza `rawMaterials.lastPricePerUnit` (precio operativo). Adicionalmente se crea un registro en `rawMaterialPrices` para auditoría, pero no es parte del flujo funcional visible al usuario. |
| 4 | **Entidades involucradas** | Lista "Precio Histórico MP → Crear nuevo registro" | — | **Corregir**: Reemplazar por "Materia Prima → Actualizar `lastPricePerUnit`". Opcionalmente mencionar que se registra en auditoría de precios. |

---

## US 03 - Ajuste de Stock de Materia Prima

### Discrepancias encontradas

| # | Tema | User Story dice | POC muestra | Propuesta |
|---|---|---|---|---|
| 1 | **Acceso** | "Usuario accede al módulo de Stock de MP → Selecciona una MP → Selecciona 'Ajuste de Stock'" | El POC tiene botón "Ajustar" directamente en cada fila de la tabla de MP | **Corregir el flujo**: El acceso es inline desde la tabla de Materias Primas (botón "Ajustar" por fila), no desde un módulo separado de Stock. |

---

## US 04 - Gestión de Productos

### Discrepancias encontradas

| # | Tema | User Story dice | POC muestra | Propuesta |
|---|---|---|---|---|
| 1 | **Margen mínimo** | No se menciona | La tabla tiene columna "MARGEN MÍN." con valores (40%, 50%, 35%) | **Agregar a la US** el campo `MargenMinimo` en la entidad Producto. Regla de negocio: "Si el margen calculado es menor al margen mínimo configurado, se muestra una alerta visual." |
| 2 | **Botón Ocultar** | "No se puede eliminar un producto que tenga stock > 0..." | El POC tiene botón "Ocultar" en vez de "Eliminar" | **Corregir la US**: Cambiar concepto de "eliminar" por "ocultar/archivar". Agregar campo `Visible` (boolean) a la entidad Producto. Regla: "Los productos ocultos no aparecen en listados de producción ni venta, pero se mantienen para historial." |
| 3 | **Ordenamiento de tabla** | Se menciona "ordenar por cualquier columna (click en encabezado)" | El POC muestra "⇅" en los encabezados: PRODUCTO, PRECIO LISTA, COSTO EST., MARGEN, MARGEN MÍN., STOCK, HS. PROD. | **OK** - Está alineado. Solo confirmar que DESCRIPCIÓN no es ordenable (no tiene ⇅ en el POC). |
| 4 | **Búsqueda** | No se menciona explícitamente | El POC tiene campo "Buscar producto..." | **Agregar a la US** la funcionalidad de búsqueda por texto en el listado de productos. |

---

## US 05 - Producción

### Discrepancias encontradas

| # | Tema | User Story dice | POC muestra | Propuesta |
|---|---|---|---|---|
| 1 | **Filtros** | No se mencionan filtros en el listado de producciones | El POC tiene: filtro por producto, rango de fechas | **Agregar a la US** un flujo de "Consulta de producciones" con filtros: por producto y por rango de fechas. |
| 2 | **KPIs resumen** | No se mencionan | El POC muestra cards: "UNIDADES PRODUCIDAS: 126" y "COSTO TOTAL: $198.400" | **Agregar a la US** que la vista de producción muestra KPIs del período: total unidades producidas y costo total. |
| 3 | **Notas en tabla** | Se menciona "Notas" como campo del registro | El POC muestra columna NOTAS en la tabla (algunas vacías) | **OK** - Está alineado. Las notas son opcionales. |
| 4 | **Cálculo de costo** | La US dice "se busca Precio Histórico vigente de la MP a la fecha de producción" | — | **Corregir la US**: El costo se calcula con `rawMaterials.lastPricePerUnit` (precio actual al momento de producir). No se busca un precio histórico por fecha. El costo queda grabado como snapshot inmutable en el registro de producción. |
| 5 | **Entidades involucradas** | Lista "Precio Histórico MP → Consultar (para cálculo de costo)" | — | **Corregir**: Reemplazar por "Materia Prima → Consultar `lastPricePerUnit`". |

---

## US 06 - Venta

### Discrepancias encontradas

| # | Tema | User Story dice | POC muestra | Propuesta |
|---|---|---|---|---|
| 1 | **Filtros en listado** | No se mencionan filtros para el listado de ventas | El POC tiene: filtro por canal, filtro por medio de pago, rango de fechas | **Agregar a la US** un flujo de "Consulta de ventas" con filtros: por canal, por medio de pago y por rango de fechas. |
| 2 | **KPIs resumen** | No se mencionan | El POC muestra cards: VENTAS (32), INGRESOS ($185.300), COGS ($82.400), COSTOS VENTA ($12.630), GANANCIA NETA (+$90.270) | **Agregar a la US** que la vista de ventas muestra KPIs del período filtrado. |
| 3 | **Columnas de la tabla** | No se especifica qué columnas tiene el listado | El POC muestra: #, FECHA, CANAL, ÍTEMS, INGRESO BRUTO, COGS, COSTOS VTA, GANANCIA, PAGO | **Agregar a la US** la especificación de columnas del listado de ventas. |
| 4 | **Envío en la tabla** | Se menciona como campo de la venta | No se ve columna de envío en la tabla del listado | **OK** - El envío es un campo del detalle de la venta, no necesariamente visible en la tabla resumen. |
| 5 | **Descuento en la tabla** | Se menciona como campo de la venta | No se ve columna de descuento en la tabla del listado | **OK** - Mismo caso que envío, es dato del detalle. |
| 6 | **Cálculo de COGS** | La US dice "buscar costo de producción promedio del producto" | — | **Corregir la US**: El COGS se calcula con el costo actual del producto al momento de la venta (receta × `lastPricePerUnit` de cada MP). Se graba como snapshot en `sales.items.unitCost` y queda fijo. No se usa promedio de producciones. |

---

## US 07 - Registro de Gasto

### Discrepancias encontradas

| # | Tema | User Story dice | POC muestra | Propuesta |
|---|---|---|---|---|
| 1 | **Filtros** | No se mencionan filtros | El POC tiene: filtro por categoría, filtro por cuenta, rango de fechas | **Agregar a la US** los filtros disponibles en la consulta de gastos. |
| 2 | **KPIs resumen** | No se mencionan | El POC muestra: TOTAL GASTOS ($34.500, 18 registros), MAYOR CATEGORÍA (Marketing), VS. MES ANTERIOR (+12%) | **Agregar a la US** los KPIs: total gastos del período, categoría con mayor gasto, comparación vs. mes anterior. |
| 3 | **Gráfico de gastos por categoría** | Se menciona en la US como "Vista de Gastos - Gráfico por Categoría" | El POC lo muestra correctamente con barras horizontales | **OK** - Está alineado. |
| 4 | **Campo Notas vs Descripción** | La US dice "Notas" | El POC muestra columna "DESCRIPCIÓN" con texto descriptivo | **Corregir la US**: Renombrar "Notas" a "Descripción" para alinearse con el POC, o bien mantener ambos campos (Descripción obligatoria + Notas opcionales). Sugerencia: usar "Descripción" como campo principal. |

---

## US 08 - Movimiento Manual de Caja

### Discrepancias encontradas

| # | Tema | User Story dice | POC muestra | Propuesta |
|---|---|---|---|---|
| 1 | **Conceptos** | Menciona: Retiro de efectivo, Aporte de capital, Ajuste de saldo, Transferencia, Otro | El POC en el filtro de Caja muestra conceptos: Venta, Compra MP, Gasto, Aporte de capital, Retiro de efectivo, Transferencia, Ajuste de saldo | **Alinear**: Los conceptos del POC incluyen los automáticos (Venta, Compra MP, Gasto) y los manuales. La US 08 solo cubre los manuales, lo cual es correcto. Pero la US 09 debería listar todos los conceptos posibles (automáticos + manuales). |
| 2 | **Botón Transferencia** | Se documenta como flujo 8b | El POC tiene botón separado "Transferencia entre cuentas" | **OK** - Está alineado. |

---

## US 09 - Consulta de Caja

### Discrepancias encontradas

| # | Tema | User Story dice | POC muestra | Propuesta |
|---|---|---|---|---|
| 1 | **Columna Descripción** | No se menciona | El POC tiene columna "DESCRIPCIÓN" con texto libre (ej: "Venta #42 — Feria", "Cera de Soja 1kg") | **Agregar a la US** la columna "Descripción" que muestra un texto descriptivo del movimiento. |
| 2 | **Filtro por Concepto** | La US menciona filtro por "Concepto" | El POC tiene combo "Todos los conceptos" con valores normalizados | **OK** - Está alineado. |
| 3 | **Origen como link** | La US dice "La columna Origen es un link" | El POC muestra columna ORIGEN con links navegables (ej: "Venta #42" → ventas.html) | **OK** - Está alineado. |
| 4 | **Movimientos manuales - Origen** | No se especifica qué muestra en Origen para manuales | El POC muestra "Manual" como texto (sin link) para movimientos manuales | **Agregar a la US**: Para movimientos manuales, la columna Origen muestra "Manual" (sin link). |
| 5 | **Subtotales del período** | La US menciona "Total ingresos, Total egresos, Neto" | El POC muestra exactamente eso: "$198.500 / $87.200 / +$111.300" | **OK** - Está alineado. |

---

## US 10 - Dashboard Mensual

### Discrepancias encontradas

| # | Tema | User Story dice | POC muestra | Propuesta |
|---|---|---|---|---|
| 1 | **Layout de métricas** | Muestra 4 cards: Ingresos, COGS, Costos Vta, Gastos | El POC muestra 4 cards arriba + 3 cards de resumen abajo (Ganancia Bruta, Costos Venta + Gastos, Resultado Neto) | **OK** - El POC es más detallado pero consistente con la fórmula. Actualizar el layout en la US para reflejar las 7 cards. |
| 2 | **Ventas por Canal** | Menciona gráfico de torta o barras | El POC muestra barras horizontales con: Feria, Instagram, Tienda, Otro | **OK** - Está alineado. |
| 3 | **Top Productos por Ventas** | "gráfico con columnas: Producto, Unidades, Ingresos" | El POC muestra barras horizontales con "48u — $72.000" | **OK** - Está alineado (formato visual diferente pero misma info). |
| 4 | **Top Productos por Rentabilidad** | Menciona "ordenado por rentabilidad %" | El POC muestra tabla con #, PRODUCTO, MARGEN ($), RENTABILIDAD (%) | **Agregar a la US** que la tabla de rentabilidad muestra también el margen en $ además del %. |

---

## Observaciones Generales

### 1. Login (no documentado)
El POC tiene una pantalla de login (email + contraseña) que **no tiene User Story asociada**. 
- **Propuesta**: Crear US 00-bis o US previa para autenticación, o documentar que el login es un requisito técnico fuera del alcance funcional del MVP.

### 2. Navegación lateral
El POC tiene una barra lateral con secciones: PRINCIPAL (Dashboard) y OPERACIONES (Productos, Materias Primas, Producción, Ventas, Gastos, Caja).
- **Propuesta**: Documentar la estructura de navegación en un documento de UX/UI o en el MVP.md.

### 3. Entidad "Compra de MP" sin pantalla propia
La US 02 habla de un "módulo de Compras de MP" pero el POC no tiene una sección dedicada en la navegación. Las compras se registran desde Materias Primas.
- **Propuesta**: Alinear la US 02 con el POC — el registro de compra se hace desde la pantalla de Materias Primas. El historial de compras podría verse desde el "Historial" de cada MP.

### 4. Ajuste de Stock sin pantalla propia
Similar al punto anterior, el ajuste (US 03) se accede desde el botón "Ajustar" en la tabla de MP.
- **Propuesta**: Alinear la US 03 indicando que el acceso es inline desde la tabla de MP.

### 5. Campo "Margen Mínimo" en Productos
El POC muestra este campo que no está documentado. Es una funcionalidad de alerta útil.
- **Propuesta**: Documentar en US 04 y definir qué pasa cuando el margen real cae por debajo del mínimo (¿alerta visual? ¿notificación?).

### 6. Modelo de costos simplificado
Las US originales mencionan "Precio Histórico MP" como entidad separada para cálculos. Según la decisión de diseño:
- El precio operativo es `rawMaterials.lastPricePerUnit` (se actualiza con cada compra).
- Al producir: costo = receta × precio actual → snapshot inmutable en el registro de producción.
- Al vender: COGS = costo actual del producto (receta × precios actuales de MP) → snapshot inmutable en la venta.
- `rawMaterialPrices` existe solo para auditoría (ver evolución de precios), no para cálculos operativos.
- **Propuesta**: Actualizar US 02, 04, 05 y 06 eliminando referencias a "Precio Histórico vigente" y reemplazando por el modelo simplificado.

### 7. Eliminación de `stockAdjustments` como entidad separada
La US 03 implica una entidad "Ajuste" independiente, pero los ajustes se registran directamente como `stockMovements` con `movementType: AdjustmentIncrease/Decrease` y el motivo en el campo `adjustmentReason`.
- **Propuesta**: Actualizar US 03 para reflejar que no existe una entidad "Ajuste" separada; el ajuste ES un StockMovement con tipo y motivo.
