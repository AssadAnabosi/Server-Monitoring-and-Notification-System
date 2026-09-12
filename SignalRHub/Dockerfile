FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app

EXPOSE 5188

ENV ASPNETCORE_HTTP_PORT=https://+:5188
ENV ASPNETCORE_URLS=http://+:5188

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["SignalRHub.csproj", "./"]
RUN dotnet restore "SignalRHub.csproj"
COPY . .
WORKDIR "/src/"
RUN dotnet build "SignalRHub.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SignalRHub.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SignalRHub.dll"]