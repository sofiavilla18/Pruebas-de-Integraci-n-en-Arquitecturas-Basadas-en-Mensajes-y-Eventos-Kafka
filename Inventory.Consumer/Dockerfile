FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Inventory.Consumer/Inventory.Consumer.csproj Inventory.Consumer/
RUN dotnet restore Inventory.Consumer/Inventory.Consumer.csproj

COPY . .
RUN dotnet publish Inventory.Consumer/Inventory.Consumer.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/runtime:10.0
WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Inventory.Consumer.dll"]