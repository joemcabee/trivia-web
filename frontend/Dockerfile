# Build stage
FROM node:20-alpine AS build
WORKDIR /app

# Accept Vite env vars at build time (e.g. API base URL)
ARG VITE_API_URL
ENV VITE_API_URL=${VITE_API_URL}

# Install dependencies
COPY package.json package-lock.json* ./
RUN npm ci

# Build the app
COPY . .
RUN npm run build

# Production image
FROM nginx:alpine AS runtime
COPY --from=build /app/dist /usr/share/nginx/html

# Use a custom NGINX config for SPA routing (fallback to index.html).
# If you want to use the default nginx config instead, remove this line.
COPY nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
