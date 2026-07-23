FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app
COPY . .
RUN dotnet restore VizitLink3D.Api/VizitLink3D.Api.csproj && dotnet restore VizitLink3D.UI/VizitLink3D.UI.csproj && dotnet restore VizitLink3D.Ortak/VizitLink3D.Ortak.csproj
RUN dotnet publish VizitLink3D.UI/VizitLink3D.UI.csproj -c Release -o /app/ui-out
RUN dotnet publish VizitLink3D.Api/VizitLink3D.Api.csproj -c Release -o /app/api-out
RUN cp -r /app/ui-out/wwwroot/* /app/api-out/wwwroot/ 2>/dev/null || true

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/api-out .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
ENTRYPOINT ["dotnet", "VizitLink3D.Api.dll"]
