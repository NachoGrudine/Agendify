# 🚀 Agendify - Frontend Login Implementation

## 📋 Lo que se ha implementado

### ✅ Estructura creada:

1. **Modelos** (`models/auth.model.ts`)
   - LoginDto
   - AuthResponseDto
   - DecodedToken

2. **Servicio de Autenticación** (`services/auth.service.ts`)
   - Login con email y password
   - Manejo de token en localStorage
   - Decodificación del token para obtener businessId y userId
   - Signals para estado reactivo
   - RxJS para llamadas HTTP

3. **HTTP Interceptor** (`interceptors/auth.interceptor.ts`)
   - Agrega automáticamente el token Bearer a todas las peticiones HTTP

4. **Guard de Autenticación** (`guards/auth.guard.ts`)
   - Protege rutas que requieren autenticación

5. **Componente de Login** (`components/login/`)
   - Formulario reactivo con validaciones
   - Manejo de errores con signals
   - Feedback visual de carga
   - HTML, CSS y TS separados

6. **Componente Main** (`components/main/`)
   - Página principal después del login
   - Muestra "HELLO {email}"
   - Botón de logout
   - HTML, CSS y TS separados

7. **Configuración**
   - Rutas con lazy loading
   - Guard aplicado a /main
   - HttpClient configurado
   - Interceptor registrado

## 🔧 Cómo usar

### 1. Configurar el Backend

Asegúrate de que el backend esté corriendo en `http://localhost:5000`

### 2. Instalar dependencias (ya hecho)
```bash
npm install
```

### 3. Ejecutar el frontend
```bash
npm start
```

La aplicación se abrirá en `http://localhost:4200`

## 🔐 Flujo de Autenticación

1. **Login**: Usuario ingresa email y password en `/login`
2. **Backend responde** con un token JWT que contiene:
   - userId
   - businessId
   - email
   - exp (expiración)
3. **Token guardado** en localStorage como 'agendify_token'
4. **Interceptor actúa**: Todas las peticiones HTTP incluyen automáticamente el header `Authorization: Bearer {token}`
5. **Guard protege**: La ruta `/main` solo es accesible si hay token válido
6. **Redirección automática**: Si el usuario ya está logueado y va a `/login`, podría redirigirse a `/main`

## 📦 Tecnologías usadas

- ✅ **Formularios Reactivos** (ReactiveFormsModule)
- ✅ **Signals** para manejo de estado
- ✅ **RxJS** para eventos y llamadas HTTP
- ✅ **Standalone Components** (Angular 21)
- ✅ **Lazy Loading** de componentes
- ✅ **HTTP Interceptors funcionales**
- ✅ **Guards funcionales**

## 🎨 Características UI

- Diseño moderno con gradientes
- Animaciones suaves
- Validaciones en tiempo real
- Feedback visual de carga
- Responsive design
- Mensajes de error claros

## 🔄 Próximos pasos

Una vez que hayas probado el login, podemos:
- Implementar el registro de usuarios
- Crear la vista del calendario
- Desarrollar la gestión de turnos
- Agregar la gestión de proveedores y servicios

