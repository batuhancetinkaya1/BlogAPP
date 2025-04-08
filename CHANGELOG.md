# Project Clean-up Changelog

## Security Improvements - April 2024

### 1. XSS Protection and HTML Sanitization
- **Added**: `HtmlSanitizerHelper.cs` with HTML sanitization methods to prevent XSS attacks
- **Added**: NuGet package `HtmlSanitizer` for comprehensive HTML sanitization
- **Fixed**: Potential XSS vulnerability in `Details.cshtml` by sanitizing HTML content
- **Updated**: Post content is now sanitized before storage and display

### 2. CSRF Protection Improvements
- **Added**: Missing `[ValidateAntiForgeryToken]` to all POST methods in controllers
- **Fixed**: Post edit method is now protected with antiforgery token
- **Fixed**: Admin controller actions now properly validate CSRF tokens

### 3. File Upload Security
- **Improved**: File upload error handling with safer error responses
- **Standardized**: Content-type validation for uploaded files
- **Added**: Consistent file size limits across all upload methods

### 4. Error Handling
- **Enhanced**: Error logging in file operations
- **Improved**: Error messages to hide implementation details from users

### 5. Tests
- **Added**: Additional unit tests for the ImageHelper class
- **Expanded**: Test coverage for DeleteImageFile method

## Core Improvements

### 1. Profile Image Path Handling
- **Added**: `ImageHelper.cs` with `GetProfileImageUrl()` method to standardize profile image path handling
- **Updated**: All views now use `ImageHelper.GetProfileImageUrl()` for consistent image path rendering
- **Fixed**: Inconsistent image path handling across views

### 2. Centralized File Validation & Processing
- **Added**: `ValidateAndSaveProfileImageAsync()` and `ValidateAndSavePostImageAsync()` methods in `ImageHelper`
- **Added**: `DeleteImageFile()` method to safely remove old image files
- **Added**: `ImageValidationException` for proper error handling during image validation
- **Improved**: File size validation, extension whitelist, and async file operations

### 3. Unified Error Handling
- **Added**: `INotificationService` interface and `TempDataNotificationService` implementation
- **Added**: `_Notifications.cshtml` partial view for centralized display of notifications
- **Updated**: `_Layout.cshtml` to include notifications partial
- **Updated**: Controllers to use notification service instead of direct TempData manipulation

### 4. Secure & Consistent Delete Workflow
- **Updated**: Profile.cshtml to properly check ownership before showing delete buttons
- **Improved**: All delete operations to use proper AJAX with XMLHttpRequest header
- **Updated**: PostsController.DeleteConfirmed to properly handle AJAX requests with consistent responses
- **Removed**: Redundant form ID parameter in favor of route-based ID

### 5. Performance & Async Operations
- **Improved**: Converted blocking file I/O to async operations
- **Improved**: Added proper async/await patterns in controllers

### 6. Routing & Parameter Consistency
- **Standardized**: Route templates across controllers
- **Removed**: Duplicate parameters from routes

## Files Modified

1. **New Files**
   - `Helpers/ImageHelper.cs`
   - `Helpers/HtmlSanitizerHelper.cs`
   - `Services/NotificationService.cs`
   - `Views/Shared/_Notifications.cshtml`
   - `CHANGELOG.md`
   - `Tests/ImageHelperTests.cs`

2. **Updated Files**
   - `Program.cs` - Added service registrations
   - `Controllers/PostsController.cs` - Added CSRF protection, HTML sanitization, improved error responses
   - `Controllers/UsersController.cs` - Used ImageHelper and NotificationService
   - `Controllers/AdminController.cs` - Added CSRF protection to all POST methods
   - `Views/Posts/Details.cshtml` - Fixed XSS vulnerability by using sanitized HTML
   - `Views/Shared/_Layout.cshtml` - Added notifications partial, improved profile image display
   - `Views/Users/Profile.cshtml` - Improved delete post function with AJAX
   - `BlogApp.csproj` - Added HtmlSanitizer package reference

## Testing Notes
- All profile image paths now consistently handle default images
- File uploads properly validate size and type
- Error messages are consistently displayed through the notification service
- Post deletion works via AJAX with proper security checks
- All file operations use async methods
- HTML content is properly sanitized to prevent XSS attacks
- All POST methods are protected against CSRF attacks

# CHANGELOG - BlogApp

## 2023-04-08: Security and Performance Improvements

### Security Enhancements
- Added async file operations for improved performance in `ImageHelper.cs` with `DeleteImageFileAsync` method
- Made file validation constants public in `ImageHelper` to maintain consistent validation across the application
- Created a dedicated `ValidateAndSaveContentImageAsync` method to centralize upload validation logic
- Improved environment-aware error handling in `ErrorHandlingMiddleware` to prevent exposing error details in production
- Removed hard-coded environment setting from `appsettings.json`

### Performance Improvements
- Converted blocking file I/O to async operations
- Centralized file upload validation to eliminate code duplication

## Projede Yapılan Değişiklikler

### 1. Routes & Controller Actions
- `PostsController.cs`
  - DeleteConfirmed metodu artık `/Posts/Delete/{id:int}` route'unu kabul edecek şekilde güncellendi
  - Delete ve DeleteConfirmed metotlarına `[Route("Posts/Delete/{id:int}")]` eklendi
  - Archive metodu `[HttpPost]` olarak değiştirildi ve `[ValidateAntiForgeryToken]` eklendi
  - Archive metodu artık post durumunu PostStatus.Archived olarak ayarlıyor
  - BulkDelete, BulkPublish, BulkArchive metotlarına `[ValidateAntiForgeryToken]` eklendi

### 2. Security & Validation
- Tüm POST metotlarına `[ValidateAntiForgeryToken]` eklendi
  - TagsController.Edit metodu
  - UsersController.ChangePassword metodu
  - PostsController.BulkXXX metotları
  - AdminController.EditUser ve DeleteUser metotları
- Her form POST işlemine `@Html.AntiForgeryToken()` eklendi
  - commentForm formları
  - reply-form formları
- `ErrorHandlingMiddleware.cs` eklenerek global hata yakalama sağlandı
- Dosya yükleme doğrulaması iyileştirildi
  - ValidateProfileImage helper metodu eklendi
  - Dosya boyutu limiti 2MB olarak ayarlandı
  - İzin verilen dosya uzantıları array olarak tanımlandı
  - Daha açıklayıcı hata mesajları eklendi
- XSS koruması eklendi
  - HtmlSanitizerHelper sınıfı eklendi
  - Tüm post içeriği HTML sanitizasyonundan geçirildi
  - Ganss.Xss paketi eklendi güvenli HTML işleme için

### 3. Tags
- Etiketler için `_Tag.cshtml` partial view oluşturuldu
- Tüm sayfalardaki tag gösterimi bu partial view ile standardize edildi
  - Profile.cshtml
  - Details.cshtml
  - Search.cshtml 
- Info rengi tag'ler için özel CSS düzenlemesi yapıldı

### 4. Video Embeds
- YouTube videoları için daha kapsamlı bir regex ekledik:
  ```csharp
  @"(?:youtube\.com\/(?:[^\/\n\s]+\/\S+\/|(?:v|e(?:mbed)?)\/|\S*?[?&]v=)|youtu\.be\/)([a-zA-Z0-9_-]{11})"`
  ```
- Video URL boş olmadığında embed gösterilecek şekilde düzenlendi
- Hatalı video URL'leri için düzgün hata mesajı gösterimi eklendi

### 5. Tags/Create Navigation
- TagsController.Create metodu için Admin role kısıtlaması kaldırıldı
- _Layout.cshtml'e giriş yapan kullanıcılar için "Etiket Oluştur" linki eklendi

### 6. Pagination
- `PaginatedList<T>` helper sınıfı eklendi
- Tüm liste sayfaları PaginatedList kullanacak şekilde güncellendi:
  - MyPosts.cshtml
  - Search.cshtml
- Sayfalama için standart ve tutarlı bir UI uygulandı (önceki/sonraki butonları eklendi)

### 7. Responsive Design
- Tüm sayfalardaki tablo görünümleri responsive olarak düzenlendi 
- Bootstrap grid, flex ve img-fluid sınıfları doğru şekilde uygulandı
- Search.cshtml sidebar ekledik mobil görünüm için

### 8. Error & Feedback UX
- Tüm hata mesajları daha kullanıcı dostu ve açıklayıcı hale getirildi
- Yükleme limitleri ve dosya uzantı hataları için özel mesajlar eklendi
- Video entegrasyonunda hata durumları için kullanıcı bilgilendirme eklendi

### 9. Code & Asset Optimisation
- asset-version eklenerek CSS ve JS dosyalarının önbelleğe alınması optimize edildi
- Tekrar eden kodlar temizlendi ve partial view'lar ile tekrar kullanılabilir hale getirildi
- Search.cshtml sayfasında gereksiz kodlar temizlendi

### 10. JavaScript Cleanup
- AJAX çağrıları yerine doğrudan form gönderimi kullanıldı
- deletePost fonksiyonu daha temiz ve güvenli hale getirildi
- JavaScript'teki CSRF token yönetimi iyileştirildi

### 11. Shared Partials & Helpers
- _Tag.cshtml eklenerek UI tutarlılığı sağlandı
- PaginatedList<T> eklenerek tüm liste sayfalarında tutarlı bir sayfalama sağlandı
- ValidateProfileImage helper metodu eklenerek dosya doğrulama kodu merkezi hale getirildi
- HtmlSanitizerHelper eklenerek HTML içeriğinin güvenli şekilde işlenmesi sağlandı

### 12. Testing & Verification
- Tüm değişiklikler yapıldı ve dotnet build ile derleme hatası olmadığı doğrulandı
- Route'lar test edildi ve tüm sayfalarda HTTP 404/405 hatalarının olmadığı doğrulandı
- Birim testleri ImageHelper için genişletildi 
- XSS ve CSRF koruma testleri yapıldı 