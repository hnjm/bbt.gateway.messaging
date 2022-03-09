FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
WORKDIR /app

RUN adduser -u 5679 --disabled-password --gecos "" smsgatewayuser && chown -R smsgatewayuser:smsgatewayuser /app
USER smsgatewayuser

# copy everything and build the project
COPY . ./
RUN dotnet restore bbt.gateway.messaging/*.csproj
RUN dotnet publish bbt.gateway.messaging/*.csproj -c Release -o out

# build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY --from=build-env /app/out ./
EXPOSE 5000
ENV ASPNETCORE_URLS=http://*:5000
ENTRYPOINT ["dotnet", "bbt.gateway.messaging.dll"]
