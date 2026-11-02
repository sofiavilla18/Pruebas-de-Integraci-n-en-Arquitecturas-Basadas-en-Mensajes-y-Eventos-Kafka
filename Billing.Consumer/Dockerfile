FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Billing.Consumer/Billing.Consumer.csproj Billing.Consumer/
RUN dotnet restore Billing.Consumer/Billing.Consumer.csproj

COPY . .
RUN dotnet publish Billing.Consumer/Billing.Consumer.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/runtime:10.0
WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Billing.Consumer.dll"]