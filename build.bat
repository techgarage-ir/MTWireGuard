@echo off
echo Publish project...
dotnet publish -c Release -o published
echo Publish done!
echo.
echo Build docker image...
docker buildx build --no-cache --platform linux/amd64 -t mtwg .
echo Docker image built!
echo Save output file!
docker save mtwg > mtwg.tar
echo Finish