FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY Vexcash.Sms.slnx ./
COPY src/Vexcash.Sms.Api/Vexcash.Sms.Api.csproj src/Vexcash.Sms.Api/
RUN dotnet restore src/Vexcash.Sms.Api/Vexcash.Sms.Api.csproj
COPY src/Vexcash.Sms.Api/ src/Vexcash.Sms.Api/
RUN dotnet publish src/Vexcash.Sms.Api/Vexcash.Sms.Api.csproj -c Release -o /app --no-restore /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_URLS=http://0.0.0.0:8080 \
    PORT=8080
EXPOSE 8080
USER $APP_UID
ENTRYPOINT ["dotnet", "Vexcash.Sms.Api.dll"]
