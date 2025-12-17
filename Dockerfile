# ==================== Stage 1: Build ====================
FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
WORKDIR /src

# Copy csproj files individually to leverage Docker Cache
COPY ["FAM.sln", "./"]
COPY ["src/FAM.WebApi/FAM.WebApi.csproj", "src/FAM.WebApi/"]
COPY ["src/FAM.Application/FAM.Application.csproj", "src/FAM.Application/"]
COPY ["src/FAM.Infrastructure/FAM.Infrastructure.csproj", "src/FAM.Infrastructure/"]
COPY ["src/FAM.Domain/FAM.Domain.csproj", "src/FAM.Domain/"]
COPY ["src/FAM.Cli/FAM.Cli.csproj", "src/FAM.Cli/"]

# Restore dependencies
RUN dotnet restore "src/FAM.WebApi/FAM.WebApi.csproj" && \
    dotnet restore "src/FAM.Cli/FAM.Cli.csproj"

# Smart cache busting: this variable changes from Makefile forces rebuild
ARG CACHEBUST=1

# Copy all source code
COPY . .

# Build all projects
WORKDIR "/src/src/FAM.WebApi"
RUN dotnet build "FAM.WebApi.csproj" -c Release --no-restore

WORKDIR "/src/src/FAM.Cli"
RUN dotnet build "FAM.Cli.csproj" -c Release --no-restore

# Publish WebApi
WORKDIR "/src/src/FAM.WebApi"
RUN dotnet publish "FAM.WebApi.csproj" -c Release -o /app/publish \
    --no-restore --no-build \
    -p:UseAppHost=false

# Publish FAM.Cli (for seeding)
WORKDIR "/src/src/FAM.Cli"
RUN dotnet publish "FAM.Cli.csproj" -c Release -o /app/publish \
    --no-restore --no-build \
    -p:UseAppHost=false

# ==================== Stage 2: Runtime ====================
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final

# Install curl for Healthcheck & clean apt cache immediately to reduce size
RUN apt-get update && apt-get install -y curl \
    && rm -rf /var/lib/apt/lists/*

# Setup User (Security best practice)
RUN groupadd -g 1000 appuser && useradd -u 1000 -g appuser -m appuser

WORKDIR /app

# Copy build from Stage 1 + Chown ownership
COPY --from=build --chown=appuser:appuser /app/publish .

# Make scripts executable
RUN chmod +x FAM.WebApi.dll FAM.Cli.dll

USER appuser

# Default environment variables
ENV ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_URLS=http://+:8000

EXPOSE 8000

# Health Check
HEALTHCHECK --interval=30s --timeout=5s --start-period=30s --retries=3 \
    CMD curl -f http://localhost:8000/health || exit 1

# Run the API (migrations are applied automatically in Program.cs)
CMD ["dotnet", "FAM.WebApi.dll"]