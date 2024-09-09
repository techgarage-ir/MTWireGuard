FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS publish
WORKDIR /src

# Define platform
ARG TARGETPLATFORM
RUN if [ "$TARGETPLATFORM" = "linux/amd64" ]; then \
    RID=linux-x64 ; \
    elif [ "$TARGETPLATFORM" = "linux/arm64" ]; then \
    RID=linux-arm64 ; \
    fi

# Copy project files
COPY *.sln .
COPY UI/*.csproj ./UI/
COPY Application/*.csproj ./Application/
COPY MikrotikAPI/*.csproj ./MikrotikAPI/
COPY Serilog.Ui.SqliteProvider/*.csproj ./Serilog.Ui.SqliteProvider/

# Restore packages
RUN dotnet restore --runtime $RID

# Copy other files
COPY UI/. ./UI/
COPY Application/. ./Application/
COPY MikrotikAPI/. ./MikrotikAPI/
COPY Serilog.Ui.SqliteProvider/. ./Serilog.Ui.SqliteProvider/

# Publish project
RUN dotnet publish "./UI/MTWireGuard.csproj" -c Release \
  -o /app/publish \
  --no-restore \
  --runtime $RID \
  --self-contained true \
  /p:WarningLevel=0 \
  /p:PublishTrimmed=true

# Create final image and run project
FROM mcr.microsoft.com/dotnet/runtime-deps:8.0-noble-chiseled-extra AS final

ENV TZ=Asia/Tehran
WORKDIR /app
EXPOSE 8080
COPY --from=publish /app/publish .

ENTRYPOINT ["./MTWireGuard"]
