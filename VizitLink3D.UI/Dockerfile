FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
RUN apt-get update && apt-get install -y python3
WORKDIR /app
COPY . .
RUN dotnet restore VizitLink3D.UI/VizitLink3D.UI.csproj
RUN dotnet workload restore VizitLink3D.UI/VizitLink3D.UI.csproj
RUN dotnet publish VizitLink3D.UI/VizitLink3D.UI.csproj -c Release -o /app/publish

FROM nginx:alpine AS runtime
WORKDIR /usr/share/nginx/html
COPY --from=build /app/publish/wwwroot .
COPY VizitLink3D.UI/nginx.conf /etc/nginx/nginx.conf
EXPOSE 80
