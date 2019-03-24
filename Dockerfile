FROM microsoft/dotnet:2.1-aspnetcore-runtime-alpine AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src
COPY ["Authentication.API/Authentication.API.csproj", "Authentication.API/"]
RUN dotnet restore "Authentication.API/Authentication.API.csproj"

COPY . .
WORKDIR "/src/Authentication.API"
RUN dotnet build "Authentication.API.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "Authentication.API.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Authentication.API.dll"]