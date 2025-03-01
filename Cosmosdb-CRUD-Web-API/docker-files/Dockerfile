# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.
FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
WORKDIR /src
COPY src/ ./
RUN dotnet restore "AzureCosmos.CRUD.WebAPI/AzureCosmos.CRUD.WebAPI.csproj"

COPY . .
WORKDIR "/src/AzureCosmos.CRUD.WebAPI"
RUN dotnet build "AzureCosmos.CRUD.WebAPI.csproj" -c Release --no-restore -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "AzureCosmos.CRUD.WebAPI.csproj" -c Release -o /app/publish

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS runtime-base
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 8080
USER $APP_UID
# Set the entry point to start the application
ENTRYPOINT ["dotnet", "AzureCosmos.CRUD.WebAPI.dll"]
