FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY MultiRacingApi/MultiRacingApi.csproj .
RUN dotnet restore
COPY . .
RUN dotnet publish "MultiRacingApi/MultiRacingApi.csproj" -c release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "MultiRacingApi.dll"]