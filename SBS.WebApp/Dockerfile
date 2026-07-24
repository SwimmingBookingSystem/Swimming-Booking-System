FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS base
RUN apk add --no-cache curl icu-libs tzdata
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["SBS.WebApp/SBS.WebApp.csproj", "SBS.WebApp/"]
COPY ["SBS.Application/SBS.Application.csproj", "SBS.Application/"]
COPY ["SBS.Domain/SBS.Domain.csproj", "SBS.Domain/"]
RUN dotnet restore "SBS.WebApp/SBS.WebApp.csproj"

COPY . .
WORKDIR "/src/SBS.WebApp"

FROM build AS publish
RUN dotnet publish "SBS.WebApp.csproj" -c Release -o /app/publish --no-restore /p:UseAppHost=false

FROM base AS final
WORKDIR /app
RUN mkdir -p /home/app/.aspnet/DataProtection-Keys \
    && chown -R app:app /home/app/.aspnet
COPY --from=publish --chown=app:app /app/publish .
USER app
ENTRYPOINT ["dotnet", "SBS.WebApp.dll"]
