FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

# Copy everything
COPY . ./
# Restore as distinct layers
RUN dotnet restore TheGrid.Server/TheGrid.Server.csproj
# Build and publish a release
RUN dotnet publish TheGrid.Server/TheGrid.Server.csproj -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0-jammy-chiseled

WORKDIR /app

ENV ASPNETCORE_URLS http://+:8080

COPY --from=build-env /app/out .

EXPOSE 8080

ENTRYPOINT ["dotnet", "TheGrid.Server.dll"]