# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and project files
COPY *.sln ./
COPY Hotel.Api/*.csproj ./Hotel.Api/
COPY Hotel.Application/*.csproj ./Hotel.Application/
COPY Hotel.Infrastructure/*.csproj ./Hotel.Infrastructure/
COPY Hotel.Tests/*.csproj ./Hotel.Tests/

# Restore dependencies
RUN dotnet restore

# Copy source code
COPY . .

# Build application
RUN dotnet build -c Release --no-restore

# Publish application
RUN dotnet publish Hotel.Api/Hotel.Api.csproj -c Release -o /app/publish --no-build

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Create non-root user for security
RUN adduser --disabled-password --gecos "" appuser

# Copy published application
COPY --from=build /app/publish .

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Expose port
EXPOSE 8080

# Switch to non-root user
USER appuser

# Start application
ENTRYPOINT ["dotnet", "Hotel.Api.dll"]
