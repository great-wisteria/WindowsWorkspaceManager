# Windows Workspace Manager

Windows Workspace Manager is a Windows utility application that allows you to register and manage your frequently used folder structures and boilerplate files (templates) as ZIP files, enabling you to deploy them as a working folder with a single click whenever needed.

## 🌟 Key Features

1. **Template Registration and Management**
   - Register your frequently used project templates and folder hierarchies as ZIP files into the application.
   - Managed templates are displayed in a list and can be deployed at any time (fast searching and sorting powered by database management).

2. **Fast Deployment of Workspace Folders**
   - Select a registered template and extract (deploy) it into any target directory with a single click.
   - During deployment, the current "Date and Time" (e.g., `20260718_223000_`) is automatically prepended to the folder name, strongly assisting you in organizing and sorting your folders.
   - Automatically opens Windows Explorer after deployment, allowing you to start your work immediately.

3. **Context Menu (Right-Click) Integration *(Planned/In Development)***
   - By right-clicking a folder in Windows Explorer and selecting "Deploy Workspace Here," you will be able to deploy a template directly into that directory.

## 🎯 Use Cases & Problem Solving
* "It's tedious to create the same folder structure or copy the same configuration files every time I start a new project."
* "I want to quickly create a folder with today's date for meeting minutes or daily reports."
* This application automates such daily routine work and dramatically speeds up your initial startup phase.

## 🛠 Technology Stack
* **Framework:** .NET 10 (WPF - Windows Presentation Foundation)
* **Language:** C#
* **Database:** SQLite (Entity Framework Core / Microsoft.Data.Sqlite)
* **Installer:** MSIX / Windows Application Packaging Project (Planned)

## 📦 Installation
*(To be updated once the installer distribution is ready.)*

## 🤝 Development Rules
Please refer to `[#01_Documents/Project/Gitルール/README.md]` for development rules and Git commit message conventions for this project.