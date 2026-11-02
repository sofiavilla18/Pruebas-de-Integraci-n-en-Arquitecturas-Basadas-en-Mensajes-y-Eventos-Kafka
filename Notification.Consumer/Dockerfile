FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Notification.Consumer/Notification.Consumer.csproj Notification.Consumer/
RUN dotnet restore Notification.Consumer/Notification.Consumer.csproj

COPY . .
RUN dotnet publish Notification.Consumer/Notification.Consumer.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/runtime:10.0
WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Notification.Consumer.dll"]