FROM mcr.microsoft.com/dotnet/aspnet:6.0-focal AS base
WORKDIR /app



RUN adduser -u 5679 --disabled-password --gecos "" smsgateway-consoleuser && chown -R smsgateway-consoleuser:smsgateway-consoleuser /app
USER smsgateway-consoleuser

FROM mcr.microsoft.com/dotnet/sdk:6.0-focal AS build
WORKDIR /src
COPY ["bbt.gateway.worker.csproj", "./"]
RUN dotnet restore "bbt.gateway.worker.csproj"
COPY . .
WORKDIR "/src."
RUN dotnet build "bbt.gateway.worker/bbt.gateway.worker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "bbt.gateway.worker.csproj" -c Release -o /app/publish /p:UseAppHost=false



FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 5000
ENV ASPNETCORE_URLS=http://+:5000
ENTRYPOINT ["dotnet", "bbt.gateway.worker.dll"]
