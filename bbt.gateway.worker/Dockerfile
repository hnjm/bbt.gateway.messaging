FROM mcr.microsoft.com/dotnet/aspnet:6.0-focal AS base
WORKDIR /app
EXPOSE 5000

ENV ASPNETCORE_URLS=http://+:5000

# Creates a non-root user with an explicit UID and adds permission to access the /app folder
# For more info, please refer to https://aka.ms/vscode-docker-dotnet-configure-containers
RUN adduser -u 5678 --disabled-password --gecos "" smsgateway-consoleuser && chown -R smsgateway-consoleuser /app
USER smsgateway-consoleuser

FROM mcr.microsoft.com/dotnet/sdk:6.0-focal AS build
WORKDIR /src
COPY ["bbt.gateway.worker.csproj", "bbt.gateway.worker/"]
RUN dotnet restore "bbt.gateway.worker/bbt.gateway.worker.csproj"
COPY . .
WORKDIR "/src/bbt.gateway.worker"
RUN dotnet build "bbt.gateway.worker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "bbt.gateway.worker.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "bbt.gateway.worker.dll"]
