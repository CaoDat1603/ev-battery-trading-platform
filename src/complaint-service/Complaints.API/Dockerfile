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
COPY ["src/complaint-service/Complaints.API/Complaints.API.csproj", "src/complaint-service/Complaints.API/"]
COPY ["src/complaint-service/Complaints.Application/Complaints.Application.csproj", "src/complaint-service/Complaints.Application/"]
COPY ["src/complaint-service/Complaints.Domain/Complaints.Domain.csproj", "src/complaint-service/Complaints.Domain/"]
COPY ["src/complaint-service/Complaints.Infrastructure/Complaints.Infrastructure.csproj", "src/complaint-service/Complaints.Infrastructure/"]
RUN dotnet restore "./src/complaint-service/Complaints.API/Complaints.API.csproj"
COPY . .
WORKDIR "/src/src/complaint-service/Complaints.API"
RUN dotnet build "./Complaints.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Complaints.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Complaints.API.dll"]