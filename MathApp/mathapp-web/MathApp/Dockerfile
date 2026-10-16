# Compilación
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY MathApp.Api/MathApp.Api.csproj MathApp.Api/
RUN dotnet restore MathApp.Api/MathApp.Api.csproj
COPY MathApp.Api/ MathApp.Api/
RUN dotnet publish MathApp.Api/MathApp.Api.csproj -c Release -o /app --no-restore

# Ejecución (imagen ligera, sin el SDK)
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .
USER app
ENTRYPOINT ["dotnet", "MathApp.Api.dll"]
