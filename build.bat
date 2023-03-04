@echo off
echo Publish project...
dotnet publish -c Release -o published
echo Publish done!
echo.
echo Build docker image...
docker buildx build --no-cache --platform linux/amd64 -t mtwireguard .
echo Docker image built!
echo Save output file!
docker save mtwireguard > mtwg.tar
echo Finish
