FROM mcr.microsoft.com/dotnet/sdk:8.0-jammy AS publish
WORKDIR /src

# Copy project files
COPY *.sln .
COPY UI/*.csproj ./UI/
COPY Application/*.csproj ./Application/
COPY MikrotikAPI/*.csproj ./MikrotikAPI/

# Restore packages
RUN dotnet restore --runtime linux-x64

# Copy other files
COPY UI/. ./UI/
COPY Application/. ./Application/
COPY MikrotikAPI/. ./MikrotikAPI/

# Publish project
RUN dotnet publish "./UI/MTWireGuard.csproj" -c Release \
  -o /app/publish \
  --no-restore \
  --runtime linux-x64 \
  --self-contained true \
  /p:WarningLevel=0 \
  /p:PublishTrimmed=true

# Create final image and run project
FROM mcr.microsoft.com/dotnet/runtime-deps:8.0-jammy-chiseled AS final

WORKDIR /app
EXPOSE 8080
COPY --from=publish /app/publish .

ENTRYPOINT ["./MTWireGuard"]
