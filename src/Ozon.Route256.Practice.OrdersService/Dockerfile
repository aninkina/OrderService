FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Ozon.Route256.Practice.OrdersService.csproj", "src/Ozon.Route256.Practice.OrdersService/"]
RUN dotnet restore "src/Ozon.Route256.Practice.OrdersService/Ozon.Route256.Practice.OrdersService.csproj"

WORKDIR "src/Ozon.Route256.Practice.OrdersService"
COPY . .

RUN dotnet build "Ozon.Route256.Practice.OrdersService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Ozon.Route256.Practice.OrdersService.csproj" -c Release -o /app/publish 

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Ozon.Route256.Practice.OrdersService.dll"]
