# Etapa de build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore ./ColdlineAPI.sln
RUN dotnet publish ./Presentation/Presentation.csproj -c Release -o /out /p:UseAppHost=false

# Etapa de runtime (dev)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /out .
ENV ASPNETCORE_URLS=http://0.0.0.0:4000
EXPOSE 4000
ENTRYPOINT ["dotnet", "Presentation.dll"]
