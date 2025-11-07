# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/notification-service/NotificationService.API/NotificationService.API.csproj", "src/notification-service/NotificationService.API/"]
COPY ["src/notification-service/NotificationService.Infrastructure/NotificationService.Infrastructure.csproj", "src/notification-service/NotificationService.Infrastructure/"]
COPY ["src/notification-service/NotificationService.Application/NotificationService.Application.csproj", "src/notification-service/NotificationService.Application/"]
COPY ["src/notification-service/NotificationService.Domain/NotificationService.Domain.csproj", "src/notification-service/NotificationService.Domain/"]
RUN dotnet restore "./src/notification-service/NotificationService.API/NotificationService.API.csproj"
COPY . .
WORKDIR "/src/src/notification-service/NotificationService.API"
RUN dotnet build "./NotificationService.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./NotificationService.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NotificationService.API.dll"]