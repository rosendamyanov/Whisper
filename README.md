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

### Core Features (The Goal)  

- 📺 **High-Fidelity Screen & Video Streaming**  
  Seamlessly stream your screen or camera to friends. Supports multiple quality configurations:  
  - 720p30/60  
  - 1080p30/60  

- 🎙️ **Voice Calls & Channels**  
  Hop into voice channels to talk with one or more friends in real-time.  

- 💬 **Real-Time Chat**  
  Full-featured text chat system with servers, channels, and direct messaging.  

- 👫 **Friends List**  
  Add friends, see their online status, and manage your connections.  

- 🔒 **Self-Hosted**  
  The backend runs on your own hardware, giving you full ownership of your data and service.  

---

### Architecture Overview  

The project is divided into two distinct applications:  

#### 1. The API (The Server)  
Built in **C# with .NET 8**, the backend will handle:  
- User authentication and accounts  
- Friend relationships, chat, and server/channel data  
- Caching strategies for responsiveness  
- Real-time communication with **SignalR**  
- Orchestrating **WebRTC** for voice/video streaming  

It will be **containerized with Docker** and run 24/7 on a dedicated machine (e.g., an old laptop).  

#### 2. The Client (The User Interface)  
Built with **Electron**, the client will:  
- Provide the UI for chat, voice, and streaming  
- Connect securely to the self-hosted API  
- Render video streams and handle audio playback  
- Offer a simple installer for easy setup  

---

### Technology Stack  

**Backend (API):**  
- Framework: C# with .NET 8 (ASP.NET Core Web API)  
- Real-Time: SignalR  
- Streaming: WebRTC  
- Database: SQL Server  
- Deployment: Docker  

**Frontend (Client):**  
- Framework: Electron  
- Web Tech: HTML5, CSS3, JavaScript  
- (Optional): React or Vue.js for UI management  

---

### Project Status  
🚧 **Currently in planning and early development phase.**  

The immediate focus is on:  
- Setting up the foundational structure for the API  
- Laying out the basic client framework  

Stay tuned for updates!  
