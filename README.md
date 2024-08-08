
# GrayLogExample

Bu proje, .NET Core 8 ile Graylog entegrasyonunu için ufak bir örnek yapılmıştır.. Docker Compose kullanarak Graylog, MongoDB ve OpenSearch konteynerlerini çalıştırmaktadır. Ayrıca, Serilog kullanarak logların Graylog'a nasıl gönderileceği gösterilmektedir.

## Gereksinimler

- Docker ve Docker Compose
- .NET 8 SDK

## Kurulum

Projenin ana dizininde bulunan **docker-compose.yml** dosyası ile docker konteynerlerini başlatın:
   ```sh
   docker-compose up -d
   ```

## Kullanım

### program.cs

Serilog yapılandırması `program.cs` dosyasında aşağıdaki gibi yapılmıştır:

```csharp
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext() 
    );
```

### appsettings.json

`appsettings.json` dosyası aşağıdaki gibi yapılandırılmıştır:

```json
{
  "Serilog": {
    "Using": [ "Serilog.Sinks.Console", "Serilog.Sinks.Graylog" ],
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Information",
        "System": "Warning"
      }
    },
    "WriteTo": [
      { "Name": "Console" },
      {
        "Name": "Graylog",
        "Args": {
          "hostnameOrAddress": "localhost",
          "port": "1515",
          "transportType": "Udp"
        }
      }
    ],
    "Properties": {
      "Application": "GraylogExampleDemo"
    }
  },
  "AllowedHosts": "*"
}
```

### docker-compose.yml

`docker-compose.yml` dosyası şu şekildedir:

```yaml
version: '3'
networks:
  graynet:
    driver: bridge
volumes:
  mongo_data:
    driver: local
  log_data:
    driver: local
  graylog_data:
    driver: local
services:
  mongo:
    image: mongo:6.0.5-jammy
    container_name: mongodb
    volumes:
      - "mongo_data:/data/db"
    networks:
      - graynet
    restart: unless-stopped
  opensearch:
    image: opensearchproject/opensearch:2
    container_name: opensearch
    environment:
      - "OPENSEARCH_JAVA_OPTS=-Xms1g -Xmx1g"
      - "bootstrap.memory_lock=true"
      - "discovery.type=single-node"
      - "action.auto_create_index=false"
      - "plugins.security.ssl.http.enabled=false"
      - "plugins.security.disabled=true"
      - "OPENSEARCH_INITIAL_ADMIN_PASSWORD=SetPassw0rdL3ttersAndNumb3r5"
    volumes:
      - "log_data:/usr/share/opensearch/data"
    ulimits:
      memlock:
        soft: -1
        hard: -1
      nofile:
        soft: 65536
        hard: 65536
    ports:
      - 9200:9200/tcp
    networks:
      - graynet
    restart: unless-stopped
  graylog:
    image: graylog/graylog:6.0
    container_name: graylog
    environment:
      GRAYLOG_PASSWORD_SECRET: "somepasswordpepper"
      GRAYLOG_ROOT_PASSWORD_SHA2: "8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918"
      GRAYLOG_HTTP_BIND_ADDRESS: "0.0.0.0:9000"
      GRAYLOG_HTTP_EXTERNAL_URI: "http://localhost:9000/"
      GRAYLOG_ELASTICSEARCH_HOSTS: "http://opensearch:9200"
      GRAYLOG_MONGODB_URI: "mongodb://mongodb:27017/graylog"
      GRAYLOG_TIMEZONE: "Europe/Istanbul"
      TZ: "Europe/Istanbul"
    entrypoint: /usr/bin/tini -- wait-for-it opensearch:9200 -- /docker-entrypoint.sh
    volumes:
      - "${PWD}/config/graylog/graylog.conf:/usr/share/graylog/config/graylog.conf"
      - "graylog_data:/usr/share/graylog/data"
    networks:
      - graynet
    restart: always
    depends_on:
      opensearch:
        condition: "service_started"
      mongo:
        condition: "service_started"
    ports:
      - 9000:9000/tcp
      - 1514:1514/tcp
      - 1514:1514/udp
      - 12201:12201/tcp
      - 12201:12201/udp
      - 1515:1515/tcp
      - 1515:1515/udp
```

## Detaylı Bilgi

Detaylı bilgi ve adım adım kurulum için lütfen [bu blog yazısına](https://medium.com/@onurkarasoy/net-core-ile-graylog-entegrasyonu-kurulum-ve-kullan%C4%B1m-457d6b24a36b) göz atın.


