FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["CaterinGO/CaterinGO/CaterinGO.csproj", "CaterinGO/CaterinGO/"]
COPY ["CaterinGO/CaterinGO.Client/CaterinGO.Client.csproj", "CaterinGO/CaterinGO.Client/"]
RUN dotnet restore "CaterinGO/CaterinGO/CaterinGO.csproj"

COPY . .
RUN dotnet publish "CaterinGO/CaterinGO/CaterinGO.csproj" \
	--configuration Release \
	--output /app/publish \
	--no-restore \
	/p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

COPY --from=build /app/publish .

USER $APP_UID
ENTRYPOINT ["dotnet", "CaterinGO.dll"]
