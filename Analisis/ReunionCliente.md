# Notas de Reunión con Cliente

## 2026-05-07

### Requerimientos discutidos

1. **Login** — Se necesita implementar un sistema de autenticación. Es la primera prioridad.

### Sección Producto

- El concepto de horas de producción sirve.
- Falta el botón de editar y de borrar/esconder.
- La idea de editar receta está bien. Falta **cm** como unidad de medida para las materias primas.
- El margen debe tener en cuenta las horas de producción y el costo de la hora.
- Falta un **margen mínimo esperado** como campo del producto. Sirve para pintar el margen en la tabla:
  - Si el margen está "cerca" (a un 10%) del margen mínimo → se pinta la celda de **amarillo**. Ej: margen 11%, mínimo 10% → ya se pintaría.

### Sección Materias Primas

- Vencimiento, rotura y pérdida son el mismo tipo de ajuste (unificar).
- Considerar compras con súper descuentos que no deberían afectar el costo de la materia prima. ¿Campo para forzar precio / excluir del cálculo de costo promedio?
- Falta botón de **editar** materia prima.
- Agregar campo **alerta mínimo de stock** en materia prima. Sirve para pintar la columna "estado" en la tabla cuando el stock está por debajo del mínimo.
- Hay materias primas que se forman a partir de otras materias primas (composición/receta de materia prima).

### Sección Ventas

- Canales: Feria, Instagram, Tienda, Otro.
- Medios de pago: Efectivo, Transferencia, Mercado Pago.
- Al vender se elige el producto y la cantidad; el total se arma automáticamente con los precios actuales.
- **Descuento**: campo en % para aplicar descuentos libremente.
- **Costo impositivo**: campo en % libre editable.
- **Costo canal**: campo en % libre editable.
- **Envío**: campo de texto libre con valor en $ (monto fijo). El envío es alcanzado por el % de impuesto y el % de canal.

### Sección Dashboard

- Filtro por **rango de fechas**, no por mes cerrado.
- "Ingresos" debe decir "Ingresos por Ventas" (aclarar que es solo ventas).
- COGS: está bien como está (es el costo de las ventas realizadas, no el gasto total en insumos). Aclararlo en la documentación.
- "Gastos por categoría" sobra en el dashboard; moverlo a la sección Gastos.
- "Top productos por ventas" debe ser un gráfico (como los otros paneles), mostrando: Producto + Unidades + Ingresos.

### Sección Gastos

- "Promedio diario" no va; eliminarlo.
- Agregar el gráfico de **Gastos por categoría** (movido desde el dashboard).

### Sección Productos

- Las columnas de la tabla deben ser clickeables para ordenar ascendente/descendente.
- "Mayor categoría" no sirve; eliminarlo.
- Incluir el gráfico de **Gastos por categoría** que se quitó del dashboard (nota: esto se movió a Gastos, no a Productos).

### Sección Caja

- Filtro de "origen" (Automático/Manual) no se quiere; eliminarlo.
- En la columna "Origen" de la tabla, que el valor sea un link que lleve a la venta/compra/gasto correspondiente.
- Falta un filtro y una columna de **Concepto**. Actualmente el concepto es texto libre; debería estar más normalizado (valores predefinidos).

### Sección Producción

- "Tandas del mes" sobra como dato; "costo promedio" también sobra.
- Unidades producidas y costo total sí sirven.
- Los filtros están bien.
