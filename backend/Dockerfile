FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY SupportDesk.Domain/SupportDesk.Domain.csproj SupportDesk.Domain/
COPY SupportDesk.Application/SupportDesk.Application.csproj SupportDesk.Application/
COPY SupportDesk.Infrastructure/SupportDesk.Infrastructure.csproj SupportDesk.Infrastructure/
COPY SupportDesk.Api/SupportDesk.Api.csproj SupportDesk.Api/
RUN dotnet restore SupportDesk.Api/SupportDesk.Api.csproj

COPY . .
RUN dotnet publish SupportDesk.Api/SupportDesk.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "SupportDesk.Api.dll"]
