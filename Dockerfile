# Multi-stage build để giảm thiểu dung lượng image

# Stage 1: Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Install dotnet-ef tool globally
RUN dotnet tool install --global dotnet-ef --version 8.0.11
ENV PATH="${PATH}:/root/.dotnet/tools"

# Copy project files để tận dụng Docker layer caching
COPY ["FAM.sln", "./"]
COPY ["src/FAM.WebApi/FAM.WebApi.csproj", "src/FAM.WebApi/"]
COPY ["src/FAM.Application/FAM.Application.csproj", "src/FAM.Application/"]
COPY ["src/FAM.Infrastructure/FAM.Infrastructure.csproj", "src/FAM.Infrastructure/"]
COPY ["src/FAM.Domain/FAM.Domain.csproj", "src/FAM.Domain/"]
COPY ["src/FAM.Cli/FAM.Cli.csproj", "src/FAM.Cli/"]

# Restore dependencies
RUN dotnet restore "src/FAM.WebApi/FAM.WebApi.csproj" && \
    dotnet restore "src/FAM.Cli/FAM.Cli.csproj"

# Copy source code
COPY . .

# Build
WORKDIR "/src/src/FAM.WebApi"
RUN dotnet build "FAM.WebApi.csproj" -c Release -o /app/build --no-restore

# Stage 2: Publish stage
FROM build AS publish
WORKDIR "/src/src/FAM.WebApi"
RUN dotnet publish "FAM.WebApi.csproj" -c Release -o /app/publish --self-contained false --no-restore

# Stage 3: Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Non-root user
RUN groupadd -g 1000 appuser && useradd -u 1000 -g appuser -m appuser

WORKDIR /app
COPY --from=publish --chown=appuser:appuser /app/publish .
USER appuser

ENV ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_URLS=http://+:8000

EXPOSE 8000

HEALTHCHECK --interval=30s --timeout=10s --start-period=30s --retries=3 \
    CMD curl -f http://localhost:8000/health || exit 1

ENTRYPOINT ["dotnet", "FAM.WebApi.dll"]
