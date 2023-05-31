FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build-env
WORKDIR /app
# must expose the port fly.io uses to run health checks on docker
EXPOSE 8080

# copy csproj and restore as distinct layers
COPY *.csproj ./
RUN dotnet restore

# copy everything else and build (executes inside our workign directory)
COPY . ./
RUN dotnet publish -c Release -o out

# build runtime image (this will be a smaller image without the full SDK)
FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /app
# everything from the out directory will be coppied to our container
COPY --from=build-env /app/out .
ENTRYPOINT [ "dotnet", "API.dll" ]