FROM mcr.microsoft.com/dotnet/aspnet:6.0-alpine AS runtime
WORKDIR /app
EXPOSE 80
EXPOSE 443
COPY published/ ./
ENTRYPOINT ["dotnet", "MTWireGuard.dll"]
