FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
RUN adduser -u 5679 --disabled-password --gecos "" smsgatewayuser && chown -R smsgatewayuser:smsgatewayuser /app
USER smsgatewayuser

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["bbt.gateway.messaging/bbt.gateway.messaging.csproj", "."]
COPY ["bbt.gateway.common/bbt.gateway.common.csproj", "."]
RUN dotnet restore "./bbt.gateway.messaging.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "bbt.gateway.messaging.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "bbt.gateway.messaging.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 5000
ENV ASPNETCORE_URLS=http://*:5000
ENTRYPOINT ["dotnet", "bbt.gateway.messaging.dll"]
