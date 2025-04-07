# BlogApp

BlogApp is a modern, feature-rich blog application built with ASP.NET Core. This project was developed as part of the Doğuş Teknoloji Bootcamp training program.

## 📋 Features

### Core Features
- User authentication and authorization with role-based access control
- Blog post creation, editing, and management
- Tag-based categorization with customizable colors
- Comment system for posts
- Reaction system (like/dislike) for posts
- Responsive design for all devices

### Admin Features
- Dashboard with statistics overview
- User management (create, edit, delete)
- Post management
- Tag management

## 🛠️ Technology Stack

- **Backend**: ASP.NET Core MVC
- **Database**: SQLite with Entity Framework Core
- **Frontend**: Bootstrap 5, jQuery, JavaScript
- **Authentication**: Cookie-based authentication
- **Styling**: CSS3, Bootstrap Icons

## 🏗️ Project Structure

The project follows a clean architecture approach:

- **Entity**: Domain models (User, Post, Tag, Comment, PostReaction)
- **Data**: Database context and repository implementations
- **Models/ViewModels**: View-specific models for forms and display
- **Controllers**: Request handling and business logic
- **Views**: Razor views for the UI
- **wwwroot**: Static assets (CSS, JS, images)

## 🚀 Getting Started

### Prerequisites

- .NET SDK 7.0 or later
- Visual Studio 2022 or VS Code

### Installation

1. Clone the repository:
```bash
git clone https://github.com/yourusername/BlogApp.git
cd BlogApp
```

2. Restore dependencies:
```bash
dotnet restore
```

3. Create and update the database:
```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

4. Run the application:
```bash
dotnet run
```

5. Navigate to `http://localhost:5000` in your browser.

### Default Credentials

- **Admin**: admin@example.com / admin123
- **User**: user@example.com / user123

## 📝 Implemented Requirements

### User Management
- [x] User registration and login system
- [x] User profile management
- [x] Role-based authorization (Admin, User)
- [x] Password change functionality
- [x] Profile picture upload

### Post Management
- [x] Create, edit, and delete blog posts
- [x] Rich text editor for post content
- [x] Post tagging system
- [x] Featured image upload
- [x] Post reactions (likes/dislikes)

### Comment System
- [x] Add comments to posts
- [x] View comments by timestamp
- [x] User profile integration with comments

### Tag System
- [x] Create and manage tags
- [x] Color-coded tags with Bootstrap colors
- [x] Tag-based post filtering

### Admin Dashboard
- [x] Statistics overview (users, posts, tags)
- [x] Recent posts and users
- [x] User management interface
- [x] Post management interface
- [x] Tag management interface

## 🌟 Extra Features

In addition to the basic requirements, the following extra features have been implemented:

- [x] Responsive design with Bootstrap 5
- [x] Optimized database queries with Entity Framework
- [x] Advanced authentication with cookie-based auth
- [x] SEO-friendly URLs for posts and tags
- [x] Rich error handling and validation
- [x] AJAX-based comments and reactions

## 🔮 Future Enhancements

The following features could be added in future iterations:

- [ ] Social media sharing integration
- [ ] Email notifications for comments and reactions
- [ ] Advanced search functionality with filters
- [ ] Post scheduling system
- [ ] Analytics and post view tracking
- [ ] Multi-language support
- [ ] Dark/light theme toggle
- [ ] API endpoints for mobile applications
- [ ] Unit and integration tests
- [ ] Docker containerization
- [ ] CI/CD pipeline setup

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 🙏 Acknowledgements

- Doğuş Teknoloji for the Bootcamp opportunity
- All the mentors and instructors who provided guidance
- Bootstrap for the responsive design components
- The ASP.NET Core team for the excellent framework

