# 📝 BlogApp

<div align="center">
  
![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-7.0-blueviolet)
![EF Core](https://img.shields.io/badge/Entity%20Framework%20Core-7.0-blue)
![Bootstrap](https://img.shields.io/badge/Bootstrap-5.3-purple)
![SQLite](https://img.shields.io/badge/SQLite-3-blue)
![License](https://img.shields.io/badge/license-MIT-green)

</div>

<p align="center">
  <img src="./wwwroot/img/readme/blog-app-logo.png" alt="BlogApp Logo" width="200" height="auto">
</p>

> A modern, high-performance blog platform built with ASP.NET Core MVC, featuring a rich text editor, real-time reactions, and a responsive design optimized for all devices.

<p align="center">
  <a href="#-key-features">Key Features</a> •
  <a href="#%EF%B8%8F-technology-stack">Technology Stack</a> •
  <a href="#-architecture">Architecture</a> •
  <a href="#-getting-started">Getting Started</a> •
  <a href="#-screenshots">Screenshots</a> •
  <a href="#-security">Security</a> •
  <a href="#-performance">Performance</a> •
  <a href="#-contributing">Contributing</a> •
  <a href="#-license">License</a>
</p>

*This project was developed as part of the Doğuş Teknoloji Bootcamp training program, demonstrating practical implementation of modern web technologies and best practices in software development.*

## ✨ Key Features

### Core Features
- **Robust Authentication System** - User registration, login, profile management with secure password handling via BCrypt
- **Complete Blog Ecosystem** - Create, edit, publish, archive, and delete blog posts with rich media support
- **Interactive Comments & Reactions** - AJAX-powered comment threads and like/dislike system
- **Tag-based Organization** - Customizable color-coded tags for content categorization
- **Responsive Design** - Seamless experience across desktops, tablets, and mobile devices

### Admin Features
- **Comprehensive Dashboard** - Real-time statistics and activity monitoring
- **User Management** - Full CRUD operations with role assignment (Admin, User)
- **Content Moderation** - Post and comment approval, editing, and removal capabilities
- **Tag Management** - Create, edit, and delete tags with custom color assignments

## 🛠️ Technology Stack

### Backend
- **ASP.NET Core MVC 7.0** - Modern, high-performance web framework
- **Entity Framework Core** - ORM for database operations with code-first approach
- **SQLite** - Lightweight, file-based database for easy deployment
- **Identity System** - Custom authentication implementation with cookie-based auth

### Frontend
- **Bootstrap 5.3** - Responsive UI components and grid system
- **jQuery** - DOM manipulation and AJAX requests
- **JavaScript** - Enhanced interactivity and client-side validation
- **HTML5 & CSS3** - Modern web standards

### Security
- **BCrypt.NET** - Industry-standard password hashing
- **HtmlSanitizer** - XSS protection for user-generated content
- **CSRF Protection** - Built-in anti-forgery tokens
- **Input Validation** - Server-side and client-side validation

### Development Tools
- **Visual Studio / VS Code** - Development environments
- **Git** - Version control
- **NuGet** - Package management

## 🏗 Architecture

The application follows a clean, layered architecture pattern:

```
BlogApp/
├── Controllers/         # Request handling and business logic
├── Models/              # View models and data transfer objects
├── Views/               # Razor views for UI rendering
├── Entity/              # Domain models and entities
├── Data/                # Data access layer
│   ├── Abstract/        # Repository interfaces
│   └── Concrete/        # Repository implementations
├── Services/            # Business services and utilities
├── Helpers/             # Helper classes and extensions
├── Middleware/          # Custom middleware components
├── wwwroot/             # Static files (CSS, JS, images)
└── Tests/               # Unit and integration tests
```

### Design Patterns
- **Repository Pattern** - Data access abstraction
- **Dependency Injection** - Loose coupling between components
- **MVC Architecture** - Separation of concerns

## 🚀 Getting Started

### Prerequisites

- [.NET SDK 7.0](https://dotnet.microsoft.com/download/dotnet/7.0) or later
- [Visual Studio 2022](https://visualstudio.microsoft.com/) / [VS Code](https://code.visualstudio.com/) with C# extension
- [Git](https://git-scm.com/downloads) for version control

### Installation

1. **Clone the repository:**
   ```bash
   git clone https://github.com/yourusername/BlogApp.git
   cd BlogApp
   ```

2. **Restore dependencies:**
   ```bash
   dotnet restore
   ```

3. **Create and update the database:**
   ```bash
   dotnet ef database update
   ```

4. **Run the application:**
   ```bash
   dotnet run
   # or
   dotnet watch run  # For hot reload during development
   ```

5. **Access the application:**
   Navigate to `https://localhost:5001` or `http://localhost:5000` in your browser.

### Docker Deployment (Alternative)

```bash
# Build the Docker image
docker build -t blogapp:latest .

# Run the container
docker run -d -p 8080:80 --name blogapp-container blogapp:latest

# Access at http://localhost:8080
```

### Default Credentials

After initialization, the following accounts will be available:

| Role  | Email              | Password  | Capabilities                         |
|-------|--------------------|-----------|------------------------------------|
| Admin | admin@example.com  | admin123  | Full access to all features         |
| User  | user@example.com   | user123   | Create/edit posts, comment, react   |

## 📸 Screenshots

<div align="center">
  <img src="./wwwroot/img/readme/homepage.png" alt="Home Page" width="45%">
  <img src="./wwwroot/img/readme/post-detail.png" alt="Post Detail" width="45%">
</div>

<div align="center">
  <img src="./wwwroot/img/readme/admin-dashboard.png" alt="Admin Dashboard" width="45%">
  <img src="./wwwroot/img/readme/responsive-view.png" alt="Responsive View" width="45%">
</div>

## 🔒 Security

BlogApp implements multiple layers of security:

- **Sanitized User Content** - All user inputs are sanitized to prevent XSS attacks
- **CSRF Protection** - All forms include anti-forgery tokens
- **Secure Password Storage** - BCrypt hashing with salt for passwords
- **Authorization Policies** - Role-based and resource-based authorization
- **Secure File Uploads** - Content-type validation and size restrictions
- **SQL Injection Prevention** - Parameterized queries through Entity Framework
- **Environment-Aware Error Handling** - Detailed errors in development, generic errors in production

## ⚡ Performance

The application is optimized for performance:

- **Async/Await Pattern** - Non-blocking I/O operations
- **Entity Framework Optimizations** - AsSplitQuery for large joins, eager/lazy loading as appropriate
- **Image Optimization** - Image resizing and compression
- **Response Compression** - Gzip/Brotli compression for HTTP responses
- **Caching** - In-memory caching for frequently accessed data
- **Pagination** - Efficient data retrieval for large datasets
- **Asynchronous File Operations** - Responsive handling of file uploads/downloads

## 🌐 API Documentation

The application provides JSON endpoints for third-party integration:

- `GET /api/posts` - Retrieve all published posts
- `GET /api/posts/{id}` - Retrieve specific post by ID
- `GET /api/tags` - Retrieve all tags
- `POST /api/posts/reaction` - Add reaction to a post (authenticated)

## 📦 Implemented Requirements

### User Management
- [x] User registration and login system
- [x] User profile management
- [x] Role-based authorization (Admin, User)
- [x] Password change functionality
- [x] Profile picture upload with validation

### Post Management
- [x] Create, edit, archive, and delete blog posts
- [x] Rich text editor with HTML sanitization
- [x] Post tagging system with color coding
- [x] Featured image upload with size/type validation
- [x] Post reactions (likes/dislikes) with AJAX

### Comment System
- [x] Nested comments with replies
- [x] Live comment reactions (likes/dislikes)
- [x] Comment moderation for admins
- [x] AJAX-based comment submission

### Tag System
- [x] Create and manage tags with custom colors
- [x] Tag-based post filtering
- [x] Tag cloud visualization

### Admin Dashboard
- [x] Statistics overview (users, posts, tags)
- [x] Recent posts and users monitoring
- [x] User management interface
- [x] Post management interface
- [x] Tag management interface

## 🔮 Future Enhancements

Planned features for future iterations:

- [ ] **Social Media Integration** - Login with Google/Facebook and sharing capabilities
- [ ] **Email Notifications** - Comment and reaction notifications for post authors
- [ ] **Advanced Search** - Full-text search with filters and sorting options
- [ ] **Post Scheduling** - Schedule posts for future publication
- [ ] **Analytics** - User activity tracking and content performance metrics
- [ ] **Multi-language Support** - Internationalization for UI elements
- [ ] **Dark/Light Theme** - Theme toggle and user preference storage
- [ ] **Mobile Applications** - Native apps consuming the API
- [ ] **CI/CD Pipeline** - Automated testing and deployment

## 🤝 Contributing

Contributions are welcome! Please check out our [contribution guidelines](CONTRIBUTING.md) first.

1. **Fork the repository**
2. **Create a feature branch**:
   ```bash
   git checkout -b feature/amazing-feature
   ```
3. **Commit your changes**:
   ```bash
   git commit -m 'Add some amazing feature'
   ```
4. **Push to the branch**:
   ```bash
   git push origin feature/amazing-feature
   ```
5. **Open a Pull Request**

### Code Style Guidelines

- Follow the [.NET Core Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/inside-a-program/coding-conventions)
- Use asynchronous programming when dealing with I/O operations
- Write comprehensive comments for public APIs
- Include unit tests for new features

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgements

- **Doğuş Teknoloji** for the Bootcamp opportunity and mentorship
- **Bootstrap Team** for the excellent UI framework
- **Microsoft** for ASP.NET Core and related technologies
- **NuGet Package Authors** for the incredible tools that made this project possible

---

<div align="center">
  <p>Developed with ❤️ by Bootcamp Participants</p>
  <p>&copy; 2023 BlogApp Team</p>
</div>

