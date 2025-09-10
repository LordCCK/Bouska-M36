<!-- Use this file to provide workspace-specific custom instructions to Copilot. For more details, visit https://code.visualstudio.com/docs/copilot/copilot-customization#_use-a-githubcopilotinstructionsmd-file -->
# M36 Leak Detector Project Instructions

## Project Overview
This is an Electron desktop application with C# backend for M36 card reader and leak detector system.

## Completed Tasks
- [x] Project structure created with Electron frontend and C# ASP.NET Core backend
- [x] Implemented all M36 requirements including card reader interface, leak detector communication, database integration, and Zebra printer support
- [x] Complete documentation provided in README.md
- [x] Build tasks configured

## Prerequisites for Development
1. Install Node.js 16+ from https://nodejs.org/
2. Install .NET 6.0 SDK from https://dotnet.microsoft.com/download
3. Configure database connections in backend services
4. Set up Zebra printer IP address

## Quick Start (after prerequisites)
```bash
npm install
npm run install-backend
npm run build-backend
npm run dev
```

## Key Features Implemented
- Card reader login screen
- Order management with IBM SQL integration  
- Real-time leak detection with serial port communication
- Test result storage in MS SQL database
- Zebra printer label printing
- Responsive UI with fullscreen support
