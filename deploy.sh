#!/bin/bash
echo "=== 1. Coolify container'lari durdur ==="
docker stop $(docker ps -q --filter "label=coolify.managed=true") 2>/dev/null
docker update --restart=no $(docker ps -q --filter "label=coolify.managed=true") 2>/dev/null

echo "=== 2. Mevcut orpay container'lari durdur ==="
cd /opt/orpay 2>/dev/null && docker compose down 2>/dev/null
docker stop orpay-api orpay-ui 2>/dev/null
docker rm orpay-api orpay-ui 2>/dev/null

echo "=== 3. Coolify proxy durdur ve serbest birak ==="
docker stop coolify-proxy 2>/dev/null
docker update --restart=no coolify-proxy 2>/dev/null

echo "=== 4. Port 80/443 serbest mi? ==="
sleep 3
ss -tlnp | grep -E ":80 |:443 " && echo "HALA MEŞGUL" || echo "BOS"

echo "=== 5. Basit docker-compose olustur ==="
mkdir -p /opt/orpay

cat > /opt/orpay/docker-compose.yaml << 'COMPOSE'
services:
  vizitlink3d-api:
    image: ghcr.io/uzunnet/orpay-vizitlink3d-api:latest
    container_name: orpay-api
    restart: unless-stopped
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - VeriTabani__Yol=/app/Veri/vizitlink3d.db
      - Jwt__Anahtar=VizitLink3D_Production_Gizli_Anahtari_2026
      - Cors__IzinliDomainler__0=https://orpay.uzunreklam.com
      - Cors__IzinliDomainler__1=http://orpay.uzunreklam.com
      - Saas__VarsayilanFirmaSlug=orpay
      - Saas__LocalTekFirmaZorla=true
      - FORCE_SEED=1
    expose:
      - "5015"
    volumes:
      - orpay-veri:/app/Veri
      - orpay-medya:/app/wwwroot/medya
    networks:
      - orpay-net
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:5015/api/health"]
      interval: 30s
      timeout: 10s
      retries: 3
      start_period: 20s

  vizitlink3d-ui:
    image: ghcr.io/uzunnet/orpay-vizitlink3d-ui:latest
    container_name: orpay-ui
    restart: unless-stopped
    depends_on:
      vizitlink3d-api:
        condition: service_healthy
    networks:
      - orpay-net

volumes:
  orpay-veri:
    driver: local
  orpay-medya:
    driver: local

networks:
  orpay-net:
    driver: bridge
COMPOSE

echo "=== 6. Traefik dynamic config ==="
mkdir -p /data/coolify/proxy/dynamic
cat > /data/coolify/proxy/dynamic/orpay.yml << 'TRAEFIK'
http:
  routers:
    orpay-http:
      entryPoints: ["http"]
      rule: "Host(`orpay.uzunreklam.com`)"
      service: orpay-ui
      middlewares: [redirect-https]
    orpay-https:
      entryPoints: ["https"]
      rule: "Host(`orpay.uzunreklam.com`)"
      service: orpay-ui
      tls:
        certresolver: letsencrypt
    orpay-api-http:
      entryPoints: ["http"]
      rule: "Host(`orpay.uzunreklam.com`) && PathPrefix(`/api/`)"
      service: orpay-api
      middlewares: [redirect-https]
    orpay-api-https:
      entryPoints: ["https"]
      rule: "Host(`orpay.uzunreklam.com`) && PathPrefix(`/api/`)"
      service: orpay-api
      tls:
        certresolver: letsencrypt
  services:
    orpay-ui:
      loadBalancer:
        servers:
          - url: "http://orpay-ui:80"
    orpay-api:
      loadBalancer:
        servers:
          - url: "http://orpay-api:5015"
  middlewares:
    redirect-https:
      redirectScheme:
        scheme: https
TRAEFIK

echo "=== 7. Container'lari baslat ==="
cd /opt/orpay
docker compose up -d
sleep 15

echo "=== 8. Traefik'i bagla ==="
docker network connect orpay_orpay-net coolify-proxy 2>/dev/null || echo "Zaten bagli"

echo "=== 9. Durum ==="
docker ps --format "{{.Names}}: {{.Status}}" | grep -E "orpay|coolify-proxy"

echo "=== 10. Test ==="
curl -s -H "Host: orpay.uzunreklam.com" http://localhost/ -o /dev/null -w "HTTP: %{http_code}\n"
curl -sk -H "Host: orpay.uzunreklam.com" https://localhost/ -o /dev/null -w "HTTPS: %{http_code}\n"