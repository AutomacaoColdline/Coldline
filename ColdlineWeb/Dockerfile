# ============================
# STAGE 1 - Build Blazor WASM
# ============================
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Restaura
COPY ColdlineWeb.csproj .
RUN dotnet restore ColdlineWeb.csproj --disable-parallel

# Publica
COPY . .
RUN dotnet publish ColdlineWeb.csproj -c Release -o /app/publish /p:UseAppHost=false --no-restore


# ============================
# STAGE 2 - Servidor NGINX
# ============================
FROM nginx:alpine
WORKDIR /usr/share/nginx/html

# Remove default
RUN rm -f /etc/nginx/conf.d/default.conf

# Copia o site gerado
COPY --from=build /app/publish/wwwroot ./

# Config Nginx – mantém porta 4080 e adiciona autoindex do Coldvisio
RUN printf "server {\n\
  listen 4080;\n\
  server_name _;\n\
  root /usr/share/nginx/html;\n\
  index index.html;\n\
\n\
  # SPA fallback (Blazor)\n\
  location / {\n\
    try_files \$uri \$uri/ /index.html;\n\
  }\n\
\n\
  # 🔹 Listagem do diretório Coldvisio (para ler nomes de arquivos pelo HttpClient)\n\
  location ^~ /Coldvisio/files/ {\n\
    autoindex on;\n\
    autoindex_exact_size off;\n\
    autoindex_localtime on;\n\
    try_files \$uri \$uri/ =404;\n\
  }\n\
\n\
  # Arquivos estáticos (sem cache agressivo)\n\
  location ~* \\.(?:js|css|png|jpg|jpeg|gif|svg|ico|wasm|json|html|txt|xml|map)$ {\n\
    add_header Cache-Control 'no-cache, no-store, must-revalidate' always;\n\
    add_header Pragma 'no-cache' always;\n\
    add_header Expires 0 always;\n\
    default_type application/octet-stream;\n\
  }\n\
}\n" > /etc/nginx/conf.d/blazor.conf

EXPOSE 4080
CMD ["nginx", "-g", "daemon off;"]
