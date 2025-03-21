FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/runtime-deps:8.0-noble-chiseled-extra AS final

ENV TZ=Asia/Tehran
WORKDIR /app
EXPOSE 8080
USER $APP_UID

# Copy the published output from the build stage (set by the GitHub Actions workflow)
COPY publish/ .

ENTRYPOINT ["./MTWireGuard"]
