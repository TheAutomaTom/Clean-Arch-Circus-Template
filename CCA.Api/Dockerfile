#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 7281
EXPOSE 5028

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["CCA.Api/CCA.Api.csproj", "CCA.Api/"]
COPY ["CCA.Core.Application/CCA.Core.Application.csproj", "CCA.Core.Application/"]
COPY ["CCA.Core.Domain/CCA.Core.Domain.csproj", "CCA.Core.Domain/"]
COPY ["CCA.Core.Infra/CCA.Core.Infra.csproj", "CCA.Core.Infra/"]
COPY ["CCA.Data.Persistence/CCA.Data.Persistence.csproj", "CCA.Data.Persistence/"]
COPY ["CCA.Data.Infra/CCA.Data.Infra.csproj", "CCA.Data.Infra/"]
RUN dotnet restore "./CCA.Api/CCA.Api.csproj"
COPY . .
WORKDIR "/src/CCA.Api"
RUN dotnet build "./CCA.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./CCA.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CCA.Api.dll"]