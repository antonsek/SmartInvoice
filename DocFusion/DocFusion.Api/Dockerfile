FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["DocFusion/DocFusion.Api/DocFusion.Api.csproj", "DocFusion/DocFusion.Api/"]
COPY ["DocFusion/DocFusion.Application/DocFusion.Application.csproj", "DocFusion/DocFusion.Application/"]
COPY ["DocFusion/DocFusion.Domain/DocFusion.Domain.csproj", "DocFusion/DocFusion.Domain/"]
COPY ["DocFusion/DocFusion.Infrastructure/DocFusion.Infrastructure.csproj", "DocFusion/DocFusion.Infrastructure/"]
RUN dotnet restore "DocFusion/DocFusion.Api/DocFusion.Api.csproj"
COPY . .
WORKDIR "/src/DocFusion/DocFusion.Api"
RUN dotnet build "./DocFusion.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./DocFusion.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app

RUN apt-get update && apt-get install -y \
    tesseract-ocr \
    tesseract-ocr-rus \
    tesseract-ocr-eng \
    libtesseract-dev \
    libleptonica-dev \
    && rm -rf /var/lib/apt/lists/*

# USER $APP_UID  <-- put back later, after testing

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DocFusion.Api.dll"]
