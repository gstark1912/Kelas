# Kelas - MVP Sistema de Seguimiento Financiero

## 🎯 Objetivo

Construir un sistema simple que permita responder:

1. ¿Estoy ganando plata?
2. ¿Qué productos son más rentables?
3. ¿Vale la pena el tiempo invertido?

---

## 🧩 Alcance del MVP

El MVP estará enfocado en registrar:

- Producción
- Ventas
- Gastos
- Costos de productos

Y generar un dashboard mensual básico.

---

## 🏗️ Módulos del MVP

### 1. Productos

Define qué se vende y cómo se produce.

**Campos:**
- `Id`
- `Name`
- `Description`
- `EstimatedProductionHours`
- `Recipe`:
  - `RawMaterialId`
  - `Quantity`

---

### 2. Materias Primas

Registro de insumos utilizados.

**Campos:**
- `Id`
- `Name`
- `Unit` (gr, ml, unidad, etc.)

---

### 3. Precios de Materia Prima (Histórico)

Permite calcular costos correctamente en el tiempo.

**Campos:**
- `Id`
- `RawMaterialId`
- `PricePerUnit`
- `DateFrom`

---

### 4. Producción (CRÍTICO)

Registro de cada tanda producida.

**Campos:**
- `Id`
- `ProductId`
- `QuantityProduced`
- `Date`
- `Notes`

**Impacto:**
- Descuenta materia prima (según receta)
- Genera stock de producto terminado
- Calcula costo de producción

---

### 5. Ventas

Registro de ventas realizadas.

**Campos:**
- `Id`
- `Date`
- `Items`:
  - `ProductId`
  - `Quantity`
  - `UnitPrice`
- `Channel` (Instagram, WhatsApp, Feria, etc.)
- `PaymentMethod` (Efectivo, Transferencia, MP)
- `Discount`
- `Notes`

**Impacto:**
- Genera ingreso de dinero
- Descuenta stock
- Calcula ingreso bruto

---

### 6. Gastos

Incluye todos los gastos no productivos.

**Campos:**
- `Id`
- `Date`
- `Amount`
- `Category` (Marketing, Packaging, Envíos, etc.)
- `Notes`

---

### 7. Caja (Cuentas simples)

Registro de dinero disponible.

**Campos:**
- `Id`
- `AccountName` (Efectivo, Banco, MP)
- `Balance`

---

## 📊 Dashboard (Mensual)

Vista principal del sistema.

### Métricas:

- **Ingresos totales**
- **Costo de productos vendidos (COGS)**
- **Ganancia bruta**
- **Gastos**
- **Resultado neto**
- **Ventas por canal**
- **Top productos por:
  - Ventas
  - Rentabilidad**

---

## 🧮 Lógica Clave

### Costo de Producto

Se calcula usando:
- Receta del producto
- Precio histórico de materias primas

---

### Costo de Venta (COGS)

Se calcula al momento de la venta:
- Basado en producciones anteriores

---

### Resultado
Resultado Neto = Ingresos - COGS - Gastos

## 🚫 Fuera de alcance (por ahora)

- Impuestos complejos
- Multi-moneda
- Automatización bancaria
- Control de stock en tiempo real avanzado
- App mobile

---

## ⚙️ Stack sugerido

- Backend: .NET (Web API)
- DB: MongoDB
- Frontend: Vue.js
- Infra: Docker Compose

---

## 🧠 Iteraciones futuras

- Mano de obra valorizada
- Costos por canal (comisiones)
- Alertas de stock
- Punto de equilibrio
- Proyección financiera

---

## ✅ Criterio de éxito

El MVP es exitoso si permite responder mensualmente:

- ¿Ganamos o perdimos plata?
- ¿Qué producto conviene vender más?
- ¿Estamos cubriendo los gastos?