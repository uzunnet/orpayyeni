FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app
COPY . .
RUN dotnet restore VizitLink3D.Api/VizitLink3D.Api.csproj \
    --runtime linux-x64
RUN dotnet publish VizitLink3D.Api/VizitLink3D.Api.csproj -c Release \
    -o /app/publish \
    --runtime linux-x64 \
    --self-contained false \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
COPY --from=build /app/publish .

# Sadece web icin gerekli runtime birak (linux-x64), gerisini sil
RUN rm -rf /app/runtimes/win-* /app/runtimes/osx* /app/runtimes/ios* \
    /app/runtimes/android-* /app/runtimes/iossimulator-* /app/runtimes/maccatalyst-* \
    /app/runtimes/linux-arm* /app/runtimes/linux-musl-* /app/runtimes/linux-ppc64le \
    /app/runtimes/linux-s390x /app/runtimes/linux-riscv64 /app/runtimes/linux-loongarch64 \
    /app/runtimes/linux-mips64 2>/dev/null || true

# Firma medyasi volume'dan gelir - imaja gomme (entrypoint eksikleri tamamlar)
RUN rm -rf /app/wwwroot/firmalar 2>/dev/null || true

RUN mkdir -p /app/Veri
RUN rm -f /app/vizitlink3d.db
COPY VizitLink3D.Api/entrypoint.sh /app/entrypoint.sh
RUN apt-get update && apt-get install -y --no-install-recommends curl && rm -rf /var/lib/apt/lists/*
RUN chmod +x /app/entrypoint.sh
ENTRYPOINT ["/app/entrypoint.sh"]
