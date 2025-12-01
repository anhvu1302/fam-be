# Sử dụng multi-stage build để giảm thiểu dung lượng image cuối cùng

# Stage 1: Build stage - sử dụng SDK image để build ứng dụng
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Install dotnet-ef tool globally for migrations and seeding
RUN dotnet tool install --global dotnet-ef --version 9.0.0
ENV PATH="${PATH}:/root/.dotnet/tools"

# Copy solution và project files trước để tận dụng Docker layer caching
COPY ["FAM.sln", "./"]
COPY ["src/FAM.WebApi/FAM.WebApi.csproj", "src/FAM.WebApi/"]
COPY ["src/FAM.Application/FAM.Application.csproj", "src/FAM.Application/"]
COPY ["src/FAM.Infrastructure/FAM.Infrastructure.csproj", "src/FAM.Infrastructure/"]
COPY ["src/FAM.Contracts/FAM.Contracts.csproj", "src/FAM.Contracts/"]
COPY ["src/FAM.Domain/FAM.Domain.csproj", "src/FAM.Domain/"]
COPY ["src/FAM.Cli/FAM.Cli.csproj", "src/FAM.Cli/"]

# Restore dependencies - layer này sẽ được cache nếu không có thay đổi dependencies
RUN dotnet restore "src/FAM.WebApi/FAM.WebApi.csproj"

# Copy toàn bộ source code
COPY . .

# Build ứng dụng với cấu hình Release và các tối ưu hóa
WORKDIR "/src/src/FAM.WebApi"
RUN dotnet build "FAM.WebApi.csproj" \
    -c Release \
    -o /app/build \
    --no-restore

# Stage 2: Publish stage - tạo output cuối cùng
FROM build AS publish
WORKDIR "/src/src/FAM.WebApi"
RUN dotnet publish "FAM.WebApi.csproj" \
    -c Release \
    -o /app/publish \
    --self-contained false \
    --no-restore

# Stage 3: Runtime stage - sử dụng runtime-only image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

# Cài đặt wget cho healthcheck
RUN apt-get update && apt-get install -y wget && rm -rf /var/lib/apt/lists/*

# Tạo non-root user để chạy application (security best practice)
RUN groupadd -g 1000 appuser && \
    useradd -u 1000 -g appuser -s /bin/bash -m appuser

WORKDIR /app

# Copy published files từ publish stage
COPY --from=publish --chown=appuser:appuser /app/publish .

# Chuyển sang non-root user
USER appuser

# Configure ASP.NET Core
ENV ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_URLS=http://+:8000 \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

# Expose port
EXPOSE 8000

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=30s --retries=3 \
    CMD wget --no-verbose --tries=1 --spider http://localhost:8000/health || exit 1

# Entry point
ENTRYPOINT ["dotnet", "FAM.WebApi.dll"]
