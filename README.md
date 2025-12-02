# Whisper - Self-Hosted Communication Platform

A Discord-inspired platform focused on high-fidelity streaming and real-time chat for close-knit communities.

## 🎯 The Vision

Project Whisper is a self-hosted communication platform where you control your data and experience. Stream gameplay at high quality, chat in real-time, and connect with friends - all on infrastructure you own.

## 📁 Project Structure

- **server/** - ASP.NET Core Web API backend with SignalR for real-time communication
- **client/** - Electron desktop application (coming soon)
- **docs/** - Project documentation and guides

## 🚀 Quick Start

### Server
\\\ash
cd server
dotnet restore
dotnet run --project Whisper.API
\\\

The API will be available at \https://localhost:7126\ with Swagger UI for testing endpoints.

### Client
Coming soon - Electron desktop application in development.

## ✨ Features

| Feature | Status |
|---------|--------|
| 🔐 Authentication (JWT) | ✅ Complete |
| 👥 Friendships | ✅ Complete |
| 💬 Real-Time Chat | ✅ Complete |
| 🎙️ Voice Calls | ✅ Complete |
| 📺 Live Streaming | ✅ Complete |
| 🖥️ Desktop Client | 🔄 In Progress |

## 📖 Documentation

- [Server Documentation](server/README.md) - API setup and architecture
- [API Documentation](docs/api/) - Endpoint references
- [Deployment Guide](docs/deployment/) - How to deploy your own instance

## 🛠️ Technology Stack

**Backend:**
- .NET 8 (ASP.NET Core Web API)
- Entity Framework Core
- SQL Server
- SignalR (WebSockets)
- JWT Authentication

**Frontend (Planned):**
- Electron
- WebRTC for voice/video

## 📝 License

MIT License - See [LICENSE.txt](LICENSE.txt)

## 🤝 Contributing

This is a personal project, but suggestions and feedback are welcome! Feel free to open issues.
