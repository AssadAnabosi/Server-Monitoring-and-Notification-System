FROM mcr.microsoft.com/dotnet/runtime:10.0 AS base
WORKDIR /app

ENV SignalRHub__Url=http://signalr-hub:5188/hubs/alerts

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["SignalREventConsumer.csproj", "./"]
RUN dotnet restore "SignalREventConsumer.csproj"
COPY . .
WORKDIR "/src/"
RUN dotnet build "SignalREventConsumer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SignalREventConsumer.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SignalREventConsumer.dll"]