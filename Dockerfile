## Base Configuration
FROM microsoft/dotnet:2.2-aspnetcore-runtime-alpine AS base
WORKDIR /app
EXPOSE 5000

## Restore and build the project
FROM microsoft/dotnet:2.2-sdk AS builder
WORKDIR /src
COPY *.sln ./
COPY ["Authentication.API/Authentication.API.csproj", "Authentication.API/"]
RUN dotnet restore

COPY . .
WORKDIR /src/Authentication.API
RUN dotnet build -c Configuration=Release -o /app


## Publish the project
FROM builder AS publish
RUN dotnet publish -c Release -o /app

## Build image
FROM base AS final
WORKDIR /app
COPY --from=publish /app .

ENTRYPOINT ["dotnet", "Authentication.API.dll"]