# BlogApp

BlogApp, ASP.NET Core MVC kullanılarak geliştirilmiş bir blog uygulamasıdır. Kullanıcılar blog yazıları oluşturabilir, yorum yapabilir ve etiketlerle içerikleri kategorize edebilir.

## Proje Durumu

### Tamamlanan Özellikler

- **Kullanıcı Yönetimi**
  - Kayıt ve giriş sistemi
  - Kullanıcı rolleri (Admin ve Normal Kullanıcı)
  - Oturum yönetimi (Cookie Authentication)

- **Blog Yazıları**
  - Blog yazısı oluşturma, düzenleme ve silme
  - Blog yazılarını listeleme ve detay görüntüleme
  - Blog yazılarını etiketlerle kategorize etme
  - Blog yazılarını arama

- **Etiketler**
  - Etiket oluşturma, düzenleme ve silme (sadece admin)
  - Etiketlere göre blog yazılarını filtreleme

- **Yorumlar**
  - Blog yazılarına yorum yapma
  - AJAX ile yorum ekleme

- **Arayüz**
  - Responsive tasarım (Bootstrap)
  - Arama formu
  - Kullanıcı dostu navigasyon

### Eksik Özellikler

- **Kullanıcı Profili**
  - Profil düzenleme sayfası
  - Profil resmi yükleme
  - Şifre değiştirme

- **İçerik Yönetimi**
  - Zengin metin editörü (WYSIWYG)
  - Resim yükleme ve yönetimi
  - İçerik önizleme

- **Sosyal Medya Entegrasyonu**
  - Sosyal medya ile giriş
  - Paylaşım butonları

- **Bildirimler**
  - Yorum bildirimleri
  - E-posta bildirimleri

## Teknolojiler

- **Backend**
  - ASP.NET Core 9.0
  - Entity Framework Core 9.0
  - SQLite veritabanı

- **Frontend**
  - Bootstrap 5
  - jQuery
  - AJAX

- **Kimlik Doğrulama**
  - Cookie Authentication
  - SHA256 şifreleme

## Proje Yapısı

```
BlogApp/
├── Controllers/           # MVC Controller sınıfları
├── Data/                  # Veri erişim katmanı
│   ├── Abstract/          # Repository arayüzleri
│   ├── Concrete/          # Repository implementasyonları
│   └── Concrete/EfCore/   # Entity Framework Core yapılandırması
├── Models/                # Model sınıfları
│   ├── Entity/            # Veritabanı entity modelleri
│   └── ViewModels/        # View modelleri
├── Views/                 # Razor view dosyaları
│   ├── Home/              # Ana sayfa view'ları
│   ├── Posts/             # Blog yazıları view'ları
│   ├── Tags/              # Etiketler view'ları
│   ├── Users/             # Kullanıcı işlemleri view'ları
│   └── Shared/            # Paylaşılan view'lar
└── wwwroot/               # Statik dosyalar
    ├── css/               # CSS dosyaları
    ├── js/                # JavaScript dosyaları
    └── lib/               # Kütüphane dosyaları
```

## Veritabanı

Uygulama SQLite veritabanı kullanmaktadır. Veritabanı şeması aşağıdaki tablolardan oluşur:

- **Users**: Kullanıcı bilgileri
- **Posts**: Blog yazıları
- **Tags**: Etiketler
- **Comments**: Yorumlar

## Varsayılan Kullanıcılar

Uygulama ilk çalıştırıldığında aşağıdaki kullanıcılar otomatik olarak oluşturulur:

### Admin Kullanıcısı
- **Kullanıcı Adı**: admin
- **E-posta**: admin@blogapp.com
- **Şifre**: Admin123!

### Normal Kullanıcı
- **Kullanıcı Adı**: user
- **E-posta**: user@blogapp.com
- **Şifre**: User123!

## Kurulum ve Çalıştırma

1. .NET 9.0 SDK'yı yükleyin
2. Projeyi klonlayın
3. Proje dizinine gidin
4. Aşağıdaki komutları çalıştırın:

```bash
dotnet restore
dotnet ef migrations add InitialCreate
dotnet ef database update
dotnet run
```

5. Tarayıcınızda `https://localhost:5001` adresine gidin

## Geliştirme Notları

- Entity Framework Core kullanılarak Code-First yaklaşımı ile veritabanı oluşturulmuştur
- Repository pattern kullanılarak veri erişim katmanı soyutlanmıştır
- Cookie Authentication kullanılarak kimlik doğrulama sağlanmıştır
- AJAX kullanılarak yorum ekleme işlemi asenkron hale getirilmiştir

## Yapılacaklar

- [ ] Kullanıcı profil sayfası oluşturma
- [ ] Zengin metin editörü entegrasyonu
- [ ] Resim yükleme ve yönetimi
- [ ] Sosyal medya ile giriş
- [ ] E-posta bildirimleri
- [ ] Yorum bildirimleri
- [ ] İçerik önizleme
- [ ] Şifre sıfırlama
- [ ] Kullanıcı yönetimi (admin paneli)
- [ ] İstatistikler ve raporlama

## Katkıda Bulunma

1. Bu depoyu fork edin
2. Yeni bir özellik dalı oluşturun (`git checkout -b feature/amazing-feature`)
3. Değişikliklerinizi commit edin (`git commit -m 'Add some amazing feature'`)
4. Dalınıza push edin (`git push origin feature/amazing-feature`)
5. Bir Pull Request oluşturun

## Lisans

Bu proje MIT lisansı altında lisanslanmıştır. Daha fazla bilgi için `LICENSE` dosyasına bakın. 

