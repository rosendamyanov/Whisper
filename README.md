# Whisper

Project Whisper: A Self-Hosted High-Fidelity Streaming & Chat Platform
The Vision
Project Whisper is a personal initiative to create a self-hosted communication platform inspired by Discord, but with a primary focus on high-quality, subscription-free streaming for friends. The goal is to build a system where a close-knit community can stream gameplay and content at high resolutions and framerates (like 1080p at 60fps), chat, and talk together.

The core difference from mainstream platforms is ownership and control. Instead of paying for premium features or relying on a third-party service, the entire backend API is hosted by you, for you and your friends. A custom desktop client, built with Electron, will provide a user-friendly interface to connect to your private server from anywhere.

Core Features (The Goal)
The project aims to implement the following key features:

📺 High-Fidelity Screen & Video Streaming: Seamlessly stream your screen or camera to friends. The core goal is to support multiple quality configurations, including 720p30/60 and 1080p30/60.

🎙️ Voice Calls & Channels: Hop into voice channels to talk with one or more friends in real-time.

💬 Real-Time Chat: A full-featured text chat system with servers, channels, and direct messaging.

👫 Friends List: Add friends, see their online status, and manage your connections.

🔒 Self-Hosted: The entire backend is designed to run on your own hardware, giving you complete control over your data and the service.

Architecture Overview
The project is divided into two distinct applications that work together:

1. The API (The Server)
This is the C# backend of the application. It will be built using .NET 8 and will be responsible for:

Handling user authentication and accounts.

Managing friend relationships, chat messages, and server/channel data.

Implementing caching strategies for a responsive and performant experience.

Facilitating real-time communication using technologies like SignalR.

Orchestrating the WebRTC connections required for voice and video streaming.

This application will be containerized with Docker and run 24/7 on a dedicated machine (e.g., an old laptop), acting as the central hub for all clients.

2. The Client (The User Interface)
This is the application your friends will install. It will be developed using Electron, which allows for building cross-platform desktop apps with web technologies (HTML, CSS, JavaScript). The client's responsibilities include:

Providing the user interface for all features (chat, voice, etc.).

Connecting securely to your self-hosted API.

Handling the local rendering of video streams and playback of audio.

Providing a simple installer so anyone can easily set it up and connect to your server.

Technology Stack
Backend (API):

Framework: C# with .NET 8 (ASP.NET Core Web API)

Real-Time: SignalR for chat and signaling

Streaming: WebRTC for peer-to-peer voice and video

Database: SQL Server

Deployment: Docker

Frontend (Client):

Framework: Electron

Web Tech: HTML5, CSS3, JavaScript

(Optional): A front-end framework like React or Vue.js to manage the UI.

Project Status
This project is currently in the planning and early development phase. The immediate focus is on setting up the foundational structure for the API and the client.

Stay tuned for updates!
