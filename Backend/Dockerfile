# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copiar el archivo csproj y restaurar dependencias
COPY ["Application/Application.csproj", "Application/"]
RUN dotnet restore "Application/Application.csproj"

# Copiar todo el código fuente y compilar
COPY . .
WORKDIR "/src/Application"
RUN dotnet build "Application.csproj" -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish "Application.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Application.dll"]

