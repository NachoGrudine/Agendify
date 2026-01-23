# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copiar el archivo csproj y restaurar dependencias
COPY ["Agendify/Agendify.csproj", "Agendify/"]
RUN dotnet restore "Agendify/Agendify.csproj"

# Copiar todo el código fuente y compilar
COPY . .
WORKDIR "/src/Agendify"
RUN dotnet build "Agendify.csproj" -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish "Agendify.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Agendify.dll"]

