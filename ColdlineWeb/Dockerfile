# ===== STAGE 1: build Blazor WASM =====
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# copia só o csproj primeiro p/ cache do restore
COPY ColdlineWeb.csproj ./
RUN dotnet restore ColdlineWeb.csproj

# agora o resto do código
COPY . .
RUN dotnet publish ColdlineWeb.csproj -c Release -o /out /p:UseAppHost=false

# ===== STAGE 2: NGINX estático =====
FROM nginx:alpine AS final

# publica os arquivos do Blazor no nginx
COPY --from=build /out/wwwroot /usr/share/nginx/html

# SPA fallback (rota para index.html) + cache de estáticos
RUN rm -f /etc/nginx/conf.d/default.conf && \
    printf "server {\n\
      listen 80;\n\
      server_name _;\n\
      root /usr/share/nginx/html;\n\
      index index.html;\n\
      location / {\n\
        try_files \$uri \$uri/ /index.html;\n\
      }\n\
      location ~* \\.(js|css|png|jpg|jpeg|gif|svg|ico|wasm|json)\$ {\n\
        expires 7d;\n\
        add_header Cache-Control \"public, no-transform\";\n\
      }\n\
    }\n" > /etc/nginx/conf.d/blazor.conf

EXPOSE 80
