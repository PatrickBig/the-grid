#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This is done to as a workaround for https://github.com/microsoft/DockerTools/issues/409
# Using the pattern NCarlsonMSFT suggested as a workaround until issue is fixed.
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS debug
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy over all the csproj files to perform a restore
COPY ["TheGrid.Server/TheGrid.Server.csproj", "TheGrid.Server/"]
COPY ["TheGrid.Client/TheGrid.Client.csproj", "TheGrid.Client/"]
COPY ["TheGrid.Shared/TheGrid.Shared.csproj", "TheGrid.Shared/"]
COPY ["TheGrid.Data/TheGrid.Data.csproj", "TheGrid.Data/"]
COPY ["TheGrid.Models/TheGrid.Models.csproj", "TheGrid.Models/"]
COPY ["TheGrid.Services/TheGrid.Services.csproj", "TheGrid.Services/"]
COPY ["TheGrid.Postgres/TheGrid.Postgres.csproj", "TheGrid.Postgres/"]
COPY ["TheGrid.Connectors/TheGrid.Connectors.csproj", "TheGrid.Connectors/"]

RUN dotnet restore "./TheGrid.Server/TheGrid.Server.csproj"

COPY . .
WORKDIR "/src/TheGrid.Server"
RUN dotnet build "./TheGrid.Server.csproj" -c "$BUILD_CONFIGURATION" -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./TheGrid.Server.csproj" -c "$BUILD_CONFIGURATION" -o /app/publish /p:UseAppHost=false

# Use the chiseled image for production use.
FROM mcr.microsoft.com/dotnet/aspnet:8.0-jammy-chiseled AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TheGrid.Server.dll"]