# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

########## ✅ FIX QUYỀN GHI CHO UPLOADS ##########
# Tạo thư mục uploads và cấp quyền ghi
RUN mkdir -p /app/wwwroot/uploads && \
    chmod -R 777 /app/wwwroot/uploads
##################################################

# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/identity-service/Identity.API/Identity.API.csproj", "src/identity-service/Identity.API/"]
COPY ["src/identity-service/Identity.Application/Identity.Application.csproj", "src/identity-service/Identity.Application/"]
COPY ["src/identity-service/Identity.Domain/Identity.Domain.csproj", "src/identity-service/Identity.Domain/"]
COPY ["src/identity-service/Identity.Infrastructure/Identity.Infrastructure.csproj", "src/identity-service/Identity.Infrastructure/"]
RUN dotnet restore "./src/identity-service/Identity.API/Identity.API.csproj"
COPY . .
WORKDIR "/src/src/identity-service/Identity.API"
RUN dotnet build "./Identity.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Identity.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Identity.API.dll"]