# Tennis Wave 🎾

A modern social platform connecting tennis enthusiasts for partner matching, court bookings, and real-time communication.

[![Frontend](https://img.shields.io/badge/Frontend-Next.js%2015-black?style=for-the-badge&logo=next.js)](https://nextjs.org/)
[![Backend](https://img.shields.io/badge/Backend-.NET%208-blue?style=for-the-badge&logo=.net)](https://dotnet.microsoft.com/)
[![Database](https://img.shields.io/badge/Database-SQL%20Server-red?style=for-the-badge&logo=microsoft-sql-server)](https://www.microsoft.com/en-us/sql-server/)
[![Deployment](https://img.shields.io/badge/Deployment-Railway-black?style=for-the-badge&logo=railway)](https://railway.app/)

## 📖 Project Introduction

Tennis Wave is a full-stack web application designed to revolutionize how tennis players connect, organize matches, and build communities. Built with modern technologies including Next.js 15, .NET 8, and SignalR, it provides a comprehensive solution for tennis enthusiasts to find partners, book courts, and communicate in real-time.

### Key Features
- **Partner Matching**: Find tennis partners based on skill level and location
- **Court Booking System**: Discover and book tennis courts in your area
- **Real-time Chat**: Communicate with partners using WebSocket technology
- **User Profiles**: Showcase your tennis skills and preferences
- **Responsive Design**: Seamless experience across desktop and mobile devices

## 🎯 Theme Relationship: Networking

Tennis Wave perfectly aligns with the **Networking** theme by creating a social platform that facilitates connections between tennis enthusiasts. The application serves as a digital networking hub where users can:

- **Build Communities**: Connect with local tennis players and form communities
- **Skill Sharing**: Match with players of similar skill levels for learning and improvement
- **Project Collaboration**: Organize tennis events, tournaments, and group activities
- **Social Interaction**: Engage in real-time conversations and build relationships

The platform transforms individual tennis players into a connected network, enabling meaningful social interactions and community building within the tennis ecosystem.

## ✨ Why Tennis Wave is Worth Your Attention

### 🚀 Innovative Social Features
- **Smart Partner Matching**: AI-powered algorithm matches players based on skill level, location, and availability
- **Real-time Communication**: Instant messaging with online status indicators and read receipts
- **Interactive Booking System**: Visual court selection with real-time availability updates

### 🎨 Exceptional User Experience
- **Modern UI/UX**: Beautiful, intuitive interface with dark/light theme support
- **Mobile-First Design**: Optimized for all devices with responsive layouts
- **Accessibility**: WCAG compliant design ensuring inclusivity for all users

### 🔧 Technical Excellence
- **Modern Tech Stack**: Built with the latest technologies (Next.js 15, .NET 8, TypeScript)
- **Real-time Capabilities**: WebSocket integration for instant communication
- **Scalable Architecture**: Microservices-ready design with clean separation of concerns
- **Comprehensive Testing**: Unit tests, integration tests, and component testing

### 🌟 Unique Value Proposition
Unlike generic social platforms, Tennis Wave is specifically designed for tennis communities, offering:
- **Sport-Specific Features**: Skill level matching, court booking, tournament organization
- **Local Community Focus**: Connect with players in your area
- **Activity-Based Networking**: Build relationships through shared tennis activities

## 🛠️ Technology Stack

### Frontend
- **Framework**: Next.js 15.3.4 with React 19.0.0
- **Language**: TypeScript 5
- **Styling**: Tailwind CSS 4 + Radix UI
- **State Management**: Redux Toolkit 2.8.2
- **Real-time**: SignalR Client 8.0.7
- **Testing**: Vitest 3.2.4 + React Testing Library
- **Documentation**: Storybook 9.0.16

### Backend
- **Framework**: ASP.NET Core 8.0
- **Language**: C# 12
- **Database**: SQL Server with Entity Framework Core 8.0.0
- **Authentication**: JWT Bearer Tokens
- **Real-time**: SignalR 1.2.0
- **Documentation**: Swagger/OpenAPI 6.4.0
- **Logging**: Serilog 9.0.0

### DevOps
- **Frontend Deployment**: Railway
- **Backend Deployment**: Railway
- **Database**: Railway SQL Server
- **Containerization**: Docker + Docker Compose
- **Version Control**: Git

## 🚀 Advanced Features Implemented

### 1. **All UI Components Integrated with Storybook**
- Complete component library with 30+ reusable components
- Interactive documentation with live examples
- Accessibility testing and visual regression testing
- Component development environment with hot reload
- Auto-generated documentation with TypeScript support

### 2. **WebSocket Implementation**
- Real-time chat functionality using SignalR
- Live message delivery with typing indicators
- Online/offline status tracking
- Read receipts and message notifications
- Automatic reconnection and error handling
- Group conversations and private messaging

### 3. **State Management with Redux**
- Centralized state management using Redux Toolkit
- User authentication and session management
- Real-time chat state synchronization
- Loading states and error handling
- Optimistic updates for better UX
- Comprehensive Redux testing suite

## 📱 Features Overview

### Authentication & User Management
- Secure user registration and login
- JWT-based authentication
- User profile management with avatar selection
- Password encryption with BCrypt

### Tennis Partner Matching
- Skill-based matching algorithm
- Location-based partner discovery
- Availability scheduling
- Match history tracking

### Court Booking System
- Interactive court discovery
- Real-time availability checking
- Booking management and cancellation
- Payment integration ready

### Real-time Communication
- Instant messaging between users
- Group conversations for team coordination
- File sharing capabilities
- Message search and history

### Responsive Design
- Mobile-first approach
- Cross-browser compatibility
- Progressive Web App features
- Offline capability support

## 🏃‍♂️ Getting Started

### Prerequisites
- Node.js 18+ 
- .NET 8 SDK
- Docker (optional)
- SQL Server 

### Local Development

1. **Clone the repository**
   ```bash
   git clone https://github.com/qwe2820435o2/tennis-wave.git
   cd tennis-wave
   ```

2. **Frontend Setup**
   ```bash
   cd tennis-wave-frontend
   npm install
   npm run dev
   ```

3. **Backend Setup**
   ```bash
   cd tennis-wave-api
   dotnet restore
   dotnet run
   ```

4. **Database Setup**
   ```bash
   dotnet ef database update
   ```

### Docker Deployment

#### Local Development with Docker
```bash
# Prerequisites: Install Docker Desktop first
# https://www.docker.com/products/docker-desktop/

# Build and run frontend container
cd tennis-wave-frontend
docker build -t tennis-wave-frontend .
docker run -p 3000 tennis-wave-frontend

# Build and run backend container
cd tennis-wave-api
docker build -t tennis-wave-api .
docker run -p 516180 tennis-wave-api

# Or use Docker Compose (if you have docker-compose.yml)
docker-compose up -d
```

#### Production Deployment
- **Frontend**: Deployed on Railway 
- **Backend**: Deployed on Railway
- **Database**: Azure SQL Server

> **Note**: Production environment uses Railway's containerized deployment with automatic scaling and SSL certificates.

## 🧪 Testing

### Frontend Testing
```bash
# Unit tests
npm run test

# Component tests with Storybook
npm run storybook

# Test coverage
npm run test:coverage
```

### Backend Testing
```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test tennis-wave-api.Tests
```

## 📊 Project Structure

```
tennis-wave/
├── tennis-wave-frontend/          # Next.js 15 frontend application
│   ├── src/                       # Source code directory
│   │   ├── app/                   # Next.js app router pages
│   │   ├── components/            # Reusable UI components
│   │   │   ├── ui/                # Base UI components (Button, Input, etc.)
│   │   │   ├── common/            # Common components (Avatar, Loading, etc.)
│   │   │   ├── layout/            # Layout components (Header, etc.)
│   │   │   ├── bookings/          # Booking-related components
│   │   │   └── chat/              # Chat-related components
│   │   ├── services/              # API service layer
│   │   ├── store/                 # Redux store configuration
│   │   │   └── slices/            # Redux slices (user, chat, loading)
│   │   ├── types/                 # TypeScript type definitions
│   │   ├── hooks/                 # Custom React hooks
│   │   ├── lib/                   # Utility libraries
│   │   ├── features/              # Feature-based modules
│   │   └── test/                  # Test utilities and setup
│   ├── .storybook/                # Storybook configuration
│   ├── public/                    # Static assets
│   │   └── avatars/               # User avatar images
│   ├── cypress/                   # End-to-end testing
│   ├── coverage/                  # Test coverage reports
│   ├── .next/                     # Next.js build output
│   ├── node_modules/              # Dependencies
│   ├── package.json               # Frontend dependencies and scripts
│   ├── next.config.ts             # Next.js configuration
│   ├── tailwind.config.ts         # Tailwind CSS configuration
│   ├── vitest.config.ts           # Vitest testing configuration
│   ├── Dockerfile                 # Frontend Docker configuration
│   └── vercel.json                # Vercel deployment configuration
│
├── tennis-wave-api/               # .NET 8 backend application
│   ├── Controllers/               # API controllers
│   ├── Services/                  # Business logic services
│   │   └── Interfaces/            # Service interfaces
│   ├── Data/                      # Data access layer
│   │   ├── Interfaces/            # Repository interfaces
│   │   └── ApplicationDbContext.cs # EF Core DbContext
│   ├── Models/                    # Data models and DTOs
│   │   ├── DTOs/                  # Data Transfer Objects
│   │   ├── Entities/              # Database entities
│   │   └── Enums/                 # Enumeration types
│   ├── Extensions/                # Extension methods and middleware
│   ├── Helpers/                   # Helper utilities
│   ├── Migrations/                # Entity Framework migrations
│   ├── Properties/                # Project properties
│   ├── Logs/                      # Application logs
│   ├── tennis-wave-api.Tests/     # Backend unit tests
│   ├── bin/                       # Compiled binaries
│   ├── obj/                       # Build artifacts
│   ├── Program.cs                 # Application entry point
│   ├── appsettings.json           # Application configuration
│   ├── appsettings.*.json         # Environment-specific configs
│   ├── tennis-wave-api.csproj     # C# project file
│   ├── tennis-wave-api.sln        # Solution file
│   ├── Dockerfile                 # Backend Docker configuration
│   ├── railway.json               # Railway deployment config
│   └── railway.toml               # Railway TOML configuration
│
├── LICENSE                        # MIT License
└── README.md                      # Project documentation
```

## 🌐 Live Demo

- **Website**: [https://tennis-wave-front-production.up.railway.app/](https://tennis-wave-front-production.up.railway.app/)

> **Note**: All services are deployed on Railway for consistent performance and reliability.

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 👨‍💻 Author

**JinLin Nong**
- GitHub: [qwe2820435o2](https://github.com/qwe2820435o2)
- LinkedIn: [Travis Nong](linkedin.com/in/travis-nong)

## 🙏 Acknowledgments

- Next.js team for the amazing React framework
- Microsoft for .NET 8 and Entity Framework Core
- Tailwind CSS for the utility-first CSS framework
- Radix UI for accessible component primitives
- Storybook team for component development tools

---

**Built with ❤️ for the tennis community** 