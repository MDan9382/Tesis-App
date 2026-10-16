# ---------- 1) Compilar la app ----------
FROM node:22-alpine AS build
WORKDIR /app

# Las variables VITE_ se incrustan en el JavaScript al compilar, por eso se
# declaran como ARG: Railway pasa aquí las variables del servicio con el mismo nombre.
ARG VITE_FIREBASE_API_KEY
ARG VITE_FIREBASE_AUTH_DOMAIN
ARG VITE_FIREBASE_PROJECT_ID
ARG VITE_FIREBASE_APP_ID
ARG VITE_API_URL

COPY package.json package-lock.json ./
RUN npm ci --no-audit --no-fund
COPY . .
RUN npm run build

# ---------- 2) Servir los archivos con Caddy ----------
FROM caddy:2-alpine
COPY Caddyfile /etc/caddy/Caddyfile
COPY --from=build /app/dist /srv
# Caddy lee el puerto de la variable PORT que asigna Railway.
CMD ["caddy", "run", "--config", "/etc/caddy/Caddyfile", "--adapter", "caddyfile"]
