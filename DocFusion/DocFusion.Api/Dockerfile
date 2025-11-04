# ---------- BUILD STAGE ----------
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
RUN dotnet publish "./DocFusion.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false


# ---------- RUNTIME STAGE ----------
FROM mcr.microsoft.com/dotnet/aspnet:8.0

RUN apt-get update && apt-get install -y --no-install-recommends \
    libgdiplus \
    libc6-dev \
    libx11-dev \
    libfontconfig1 \
    libfreetype6 \
    libjpeg-dev \
    libpng-dev \
    libtiff-dev \
    libgif-dev \
    libvips \
    && rm -rf /var/lib/apt/lists/*

# Create tessdata dir
RUN mkdir -p /usr/share/tesseract-ocr/4.00/tessdata
ENV TESSDATA_PREFIX=/usr/share/tesseract-ocr/4.00/

WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "DocFusion.Api.dll"]
