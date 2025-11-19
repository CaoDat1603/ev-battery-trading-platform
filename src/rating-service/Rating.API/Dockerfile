# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081
########## ? FIX QUY?N GHI CHO UPLOADS ##########
# T?o th? m?c uploads và c?p quy?n ghi
RUN mkdir -p /app/wwwroot/uploads && \
    chmod -R 777 /app/wwwroot/uploads
##################################################

# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/rating-service/Rating.API/Rating.API.csproj", "src/rating-service/Rating.API/"]
COPY ["src/rating-service/Rating.Application/Rating.Application.csproj", "src/rating-service/Rating.Application/"]
COPY ["src/rating-service/Rating.Domain/Rating.Domain.csproj", "src/rating-service/Rating.Domain/"]
COPY ["src/rating-service/Rating.Infrastructure/Rating.Infrastructure.csproj", "src/rating-service/Rating.Infrastructure/"]
RUN dotnet restore "./src/rating-service/Rating.API/Rating.API.csproj"
COPY . .
WORKDIR "/src/src/rating-service/Rating.API"
RUN dotnet build "./Rating.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Rating.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Rating.API.dll"]