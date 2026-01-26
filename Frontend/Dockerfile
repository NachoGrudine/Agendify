# Etapa 1: Build de la aplicación Angular
FROM node:20-alpine AS build

WORKDIR /app

# Copiar archivos de dependencias
COPY package*.json ./

# Instalar dependencias
RUN npm ci

# Copiar el resto del código fuente
COPY . .

# Build de producción
RUN npm run build

# Etapa 2: Servir con Nginx
FROM nginx:alpine

# Copiar los archivos compilados desde la etapa de build
COPY --from=build /app/dist/frontend/browser /usr/share/nginx/html

# Copiar configuración personalizada de Nginx y eliminar BOM si existe
COPY nginx.conf /etc/nginx/conf.d/default.conf
RUN sed -i '1s/^\xEF\xBB\xBF//' /etc/nginx/conf.d/default.conf

# Exponer puerto 80
EXPOSE 80

# Nginx se ejecuta en foreground por defecto
CMD ["nginx", "-g", "daemon off;"]
