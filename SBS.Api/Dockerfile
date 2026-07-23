FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS base
RUN apk add --no-cache curl icu-libs tzdata
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["SBS.Api/SBS.Api.csproj", "SBS.Api/"]
COPY ["SBS.Application/SBS.Application.csproj", "SBS.Application/"]
COPY ["SBS.Domain/SBS.Domain.csproj", "SBS.Domain/"]
COPY ["SBS.Infrastructure/SBS.Infrastructure.csproj", "SBS.Infrastructure/"]
RUN dotnet restore "SBS.Api/SBS.Api.csproj"

COPY . .
WORKDIR "/src/SBS.Api"

FROM build AS publish
RUN dotnet publish "SBS.Api.csproj" -c Release -o /app/publish --no-restore /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish --chown=app:app /app/publish .
USER app
ENTRYPOINT ["dotnet", "SBS.Api.dll"]
