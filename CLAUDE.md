# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is **Cirreum.Runtime.Client**, a Blazor WebAssembly runtime client library that provides comprehensive client-side infrastructure for .NET applications. The project is built on .NET 10.0 and follows the Cirreum foundation framework architecture.

## Architecture

### Core Structure
- **src/Cirreum.Runtime.Client/**: Main library containing runtime client components
- **samples/Cirreum.Demo.Client/**: Demo Blazor WebAssembly application showcasing the library
- **build/**: MSBuild configuration files for packaging and CI/CD

### Key Components
- **Authentication**: Custom authentication services with claims processing and no-auth fallback
- **Components**: Reusable Blazor components including authorization, presence, profile, theme management, and validation
- **State Management**: ViewModels and state components for local, memory, session, and container state
- **Security**: Client user management and authorization infrastructure
- **Startup Tasks**: Application initialization and configuration

### Technology Stack
- Blazor WebAssembly with .NET 10.0
- SCSS compilation via AspNetCore.SassCompiler
- Component architecture based on Cirreum framework packages
- Global usings for simplified namespace management

## Development Commands

### Build
```bash
dotnet build
```

### Run Demo Application
```bash
dotnet run --project samples/Cirreum.Demo.Client/Cirreum.Demo.Client.csproj
```

### SCSS Compilation
SCSS files are automatically compiled via AspNetCore.SassCompiler during build. Configuration is in `sasscompiler.json`.

### Solution Management
The project uses Visual Studio solution format (`.slnx`) with organized folder structure for build scripts, samples, and source code.

## Configuration

### Global Settings
- **.editorconfig**: Comprehensive C# coding standards with tab indentation and specific naming conventions
- **global.json**: .NET 10.0.100 SDK with latest feature rollforward
- **Directory.Build.props**: CI/CD detection, versioning, and package configuration

### Package References
The library depends on:
- Microsoft.AspNetCore.Components.WebAssembly
- Cirreum.Components.WebAssembly
- Cirreum.Services.Client
- AspNetCore.SassCompiler

## Development Notes

### Namespace Convention
- Root namespace is `Cirreum.Runtime` (not matching folder structure by design)
- Extensive global usings reduce boilerplate in component files
- Follow .editorconfig rules for consistent code formatting

### State Management Pattern
The library implements a layered state management system:
- **ViewModels**: Abstract state containers with property change tracking
- **State Components**: Blazor components that manage specific state types
- **Page Base Classes**: Common functionality for different state scopes

### Component Architecture
- Components follow Blazor best practices with code-behind files
- SCSS styling with automatic compilation and source maps
- Presence and profile components for user interaction
- Theme management with dark/light mode support