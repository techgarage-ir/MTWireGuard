FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/aspnet:8.0-noble-chiseled-extra AS final

ENV TZ=Asia/Tehran
WORKDIR /app
EXPOSE 8080

# Copy the published output from the build stage (set by the GitHub Actions workflow)
COPY publish/ .

ENTRYPOINT ["./MTWireGuard"]
