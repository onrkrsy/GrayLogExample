# .NET Core Web API Projesinde Graylog Entegrasyonu

Bu projede .NET Core Web API projemize Graylog entegrasyonunu nasıl yapılacağımızı inceledik.

## Proje Yapısı

Projemizin adı `GrayLogExample` ve .NET 8 kullanıyoruz. 

## 1. Docker Compose Dosyası

İlk olarak, `docker-compose.yml` dosyamızı inceleyeceğiz. Bu dosya, Graylog ve gerekli bileşenleri (MongoDB ve OpenSearch) için Docker container'larını tanımlar.

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
  # ... (MongoDB, OpenSearch ve Graylog servisleri)
```

### Önemli Noktalar:
- `networks`: `graynet` adında bir bridge network tanımlanmış. Bu, containerlar arası iletişimi sağlar.
- `volumes`: Verilerin kalıcı olması için üç ayrı volume tanımlanmış.
- `mongo`: Graylog konfigürasyonlarını saklamak için MongoDB kullanılıyor.
- `opensearch`: Logların kendisi OpenSearch'te saklanıyor.
- `graylog`: Ana Graylog servisi.

### Graylog Servis Konfigürasyonu:
- Çeşitli environment variable'lar ile Graylog yapılandırılıyor.
- Önemli portlar dışarıya açılıyor (9000, 1514, 12201, 1515).
- Zaman dilimi "Europe/Istanbul" olarak ayarlanmış.

## 2. Program.cs Dosyası

`Program.cs` dosyasında Serilog konfigürasyonu yapılıyor:

```csharp
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext() 
    );
```

Bu konfigürasyon, Serilog'un appsettings.json dosyasından ayarlarını okumasını ve servislerden loglama bağlamını zenginleştirmesini sağlar.

## 3. appsettings.json Dosyası

`appsettings.json` dosyası, Serilog ve Graylog konfigürasyonlarını içerir:

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

### Önemli Noktalar:
- Serilog, hem Console'a hem de Graylog'a log yazacak şekilde yapılandırılmış.
- Graylog sink'i için localhost:1515 adresi ve UDP protokolü kullanılıyor.
- Minimum log seviyesi "Information" olarak ayarlanmış.

## Neden Bu Yapılandırmalar?

1. **Docker Compose**: Graylog ve bağımlılıklarını (MongoDB, OpenSearch) kolayca ayağa kaldırmak ve yönetmek için kullanılıyor.

2. **Serilog**: .NET uygulamaları için popüler ve esnek bir loglama kütüphanesidir. Farklı "sink"lere (hedeflere) log göndermeyi kolaylaştırır.

3. **Graylog**: Merkezi log yönetimi için güçlü bir platformdur. Logları toplar, analiz eder ve görselleştirir.

## Alternatif Yaklaşımlar

1. **ELK Stack**: Elasticsearch, Logstash ve Kibana kullanarak benzer bir log yönetim sistemi kurulabilir.

2. **Azure Application Insights**: Microsoft Azure kullanan projeler için entegre bir APM ve loglama çözümü olabilir.

3. **Farklı Log Seviyeleri**: Prodüksiyon ortamında "Information" yerine "Warning" veya "Error" seviyesi tercih edilebilir.

4. **HTTPS Kullanımı**: Güvenlik için Graylog web arayüzüne HTTPS üzerinden erişim sağlanabilir.

5. **Email Bildirimleri**: Graylog'un email gönderme özelliği (şu an yorum satırında) aktifleştirilebilir.

 

## Docker Compose'u Çalıştırma

Docker Compose dosyamızı çalıştırmak için aşağıdaki adımları izleyin:

1. Terminal veya komut istemcisini açın.
2. `docker-compose.yml` dosyasının bulunduğu dizine gidin.
3. Aşağıdaki komutu çalıştırın:

```bash
docker-compose up -d
```

Bu komut, Docker Compose dosyasında tanımlanan tüm servisleri (MongoDB, OpenSearch ve Graylog) arka planda başlatacaktır.

4. Servislerin durumunu kontrol etmek için:

```bash
docker-compose ps
```

5. Servisleri durdurmak için:

```bash
docker-compose down
```

## Graylog Arayüzünden Input Oluşturma

Graylog'a log gönderebilmek için öncelikle bir input oluşturmamız gerekiyor. Bunun için:

1. Tarayıcınızda `http://localhost:9000` adresine gidin.
2. Varsayılan kullanıcı adı `admin` ve şifre `admin` ile giriş yapın (ilk girişte şifrenizi değiştirmeniz istenecektir).
3. Sol menüden "System" > "Inputs" seçeneğine tıklayın.
4. "Select input" dropdown'ından "GELF UDP" seçeneğini seçin ve "Launch new input" butonuna tıklayın.
5. Açılan formda:
   - "Title" alanına anlamlı bir isim girin (örn. "GELF UDP Input").
   - "Bind address" alanını `0.0.0.0` olarak bırakın.
   - "Port" alanına `12201` yazın (docker-compose dosyasında tanımladığımız port).
6. "Save" butonuna tıklayarak input'u oluşturun.

## Stream Oluşturma

Streamler, gelen logları belirli kriterlere göre filtrelemek ve yönlendirmek için kullanılır. Örnek bir stream oluşturmak için:

1. Sol menüden "Streams" seçeneğine tıklayın.
2. "Create stream" butonuna tıklayın.
3. Stream için bir başlık ve açıklama girin (örn. "API Logs").
4. "Save" butonuna tıklayın.
5. Oluşturduğunuz stream'e tıklayın ve "Manage Rules" seçeneğini seçin.
6. "Add stream rule" butonuna tıklayın.
7. Kural için:
   - "Field" olarak "source" seçin.
   - "Type" olarak "contains" seçin.
   - "Value" olarak "GraylogExampleDemo" yazın (appsettings.json'da belirttiğimiz uygulama adı).
8. "Save" butonuna tıklayın.
9. Stream sayfasına dönün ve "Start stream" butonuna tıklayarak stream'i aktifleştirin.

## Log Gönderme ve Görüntüleme

Artık .NET Core uygulamanızı çalıştırdığınızda, loglar otomatik olarak Graylog'a gönderilecektir. Logları görüntülemek için:

1. Graylog arayüzünde "Search" sekmesine gidin.
2. Oluşturduğunuz stream'i seçin.
3. Arama kriterleri belirleyebilir veya tüm logları görüntüleyebilirsiniz.

## Önemli Notlar ve İpuçları

1. **Güvenlik**: Prodüksiyon ortamında Graylog arayüzüne erişimi kısıtladığınızdan ve güçlü şifreler kullandığınızdan emin olun.

2. **Performans**: Çok fazla log gönderimi uygulamanızın performansını etkileyebilir. Log seviyelerini ve filtrelerinizi dikkatli ayarlayın.

3. **Disk Alanı**: OpenSearch ve MongoDB'nin kullandığı disk alanını düzenli olarak kontrol edin. Gerekirse eski logları arşivleyin veya silin.

4. **Alerting**: Graylog'un alerting özelliklerini kullanarak, belirli log patternleri için e-posta bildirimleri ayarlayabilirsiniz.

5. **Dashboard'lar**: Sık kullandığınız arama sorgularını ve grafikleri içeren özel dashboard'lar oluşturarak log analizi sürecinizi hızlandırabilirsiniz.

## Sonuç

Bu adımları takip ederek, .NET Core Web API projenizi Graylog ile entegre etmiş ve merkezi bir log yönetim sistemi kurmuş oldunuz. Bu sistem, uygulamanızın davranışını izleme, hata ayıklama ve performans analizi yapma konularında size büyük kolaylık sağlayacaktır.

 
