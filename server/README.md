# Project Whisper  
## A Self-Hosted High-Fidelity Streaming & Chat Platform  

### The Vision  
Project Whisper is a personal initiative to create a self-hosted communication platform inspired by Discord, but with a primary focus on **high-quality, subscription-free streaming for friends**.  

The goal is to build a system where a close-knit community can:  
- Stream gameplay and content at high resolutions and framerates (e.g., 1080p at 60fps)  
- Chat in real-time  
- Talk together over voice channels  

The **core difference** from mainstream platforms is ownership and control. Instead of paying for premium features or relying on a third-party service, the **entire backend API is hosted by you, for you and your friends**.  

A custom **desktop client**, built with Electron, will provide a user-friendly interface to connect to your private server from anywhere.  

---

### Core Features

| Feature | Status | Notes |
|---------|--------|-------|
| 🔐 **Authentication** | ✅ Done | Register, login, JWT tokens, refresh tokens, logout |
| 👫 **Friendships** | ✅ Done | Send/accept/decline requests, remove friends |
| 💬 **Real-Time Chat** | ✅ Done | Direct messages, group chats, message history |
| 🎙️ **Voice Calls** | 🔲 Planned | Real-time voice channels |
| 📺 **Live Streaming** | 🔲 Planned | High-fidelity screen/video streaming (720p-1080p @ 60fps) |
| 🔔 **Notifications** | 🔲 Planned | Unread messages, friend requests |
| 🚫 **User Blocking** | 🔲 Planned | Block non-friends |
| 📝 **Audit Logging** | 🔲 Planned | Track account activity, exports |

---

### Architecture Overview  

The project is divided into two distinct applications:  

#### 1. The API (The Server)  
Built in **C# with .NET 8**, the backend handles:  
- User authentication and accounts  
- Friend relationships, chat, and server/channel data  
- Real-time communication with **SignalR**  
- Orchestrating **WebRTC** for voice/video streaming  

It will be **containerized with Docker** and run 24/7 on a dedicated machine.  

#### 2. The Client (The User Interface)  
Built with **Electron**, the client will:  
- Provide the UI for chat, voice, and streaming  
- Connect securely to the self-hosted API  
- Render video streams and handle audio playback  

---

### Technology Stack  

**Backend (API):**  
- Framework: C# with .NET 8 (ASP.NET Core Web API)  
- Real-Time: SignalR  
- Streaming: WebRTC  
- Database: SQL Server  
- ORM: Entity Framework Core
- Deployment: Docker  

**Frontend (Client):**  
- Framework: Electron  
- Web Tech: HTML5, CSS3, JavaScript  
- (Optional): React or Vue.js for UI management  

---

### API Structure
```
Whisper/
├── Whisper.Api/           # Controllers, middleware, Program.cs
├── Whisper.Services/      # Business logic, factories
├── Whisper.Data/          # Repositories, DbContext, models
├── Whisper.Authentication/ # Auth services, JWT, validation
├── Whisper.Common/        # Shared DTOs, responses, constants
└── Whisper.DTOs/          # Request/Response DTOs
```

---

### Project Status  

🚧 **In active development**  

**Completed:**
- ✅ Authentication system (JWT + refresh tokens + cookies)
- ✅ Friendship system (requests, accept/decline, remove)
- ✅ Chat system (DMs, group chats, messages)
- ✅ Soft delete support for relevant entities
- ✅ Global query filters

**Next up:**
- 🔄 Voice sessions
- 🔄 Live streaming
- 🔄 Electron client

---

### Future Enhancements

- Rate limiting on auth endpoints
- Email verification
- Two-factor authentication
- Password reset flow
- Session management
- Notification system
- User blocking (non-friends)
- Audit logging system
- Memory caching for performance
