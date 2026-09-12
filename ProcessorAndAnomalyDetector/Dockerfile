FROM mcr.microsoft.com/dotnet/runtime:10.0 AS base
WORKDIR /app

ENV AnomalyDetectionConfig__AnomalyDetectionConfig=0.4
ENV AnomalyDetectionConfig__CpuUsageAnomalyThresholdPercentage=0.5
ENV AnomalyDetectionConfig__MemoryUsageThresholdPercentage=0.8
ENV AnomalyDetectionConfig__CpuUsageThresholdPercentage=0.9

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["ProcessorAndAnomalyDetector.csproj", "./"]
RUN dotnet restore "ProcessorAndAnomalyDetector.csproj"
COPY . .
WORKDIR "/src/"
RUN dotnet build "ProcessorAndAnomalyDetector.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ProcessorAndAnomalyDetector.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ProcessorAndAnomalyDetector.dll"]