cmake_minimum_required(VERSION 3.14)
project("Rigs of Rods Updater" LANGUAGES CSharp)

# Set global .NET Framework version.
set(CMAKE_DOTNET_TARGET_FRAMEWORK_VERSION "v4.8")

set(DOTNET_REF
	System
    System.Core
    System.Data
    System.Data.DataSetExtensions
    System.Drawing
    System.Net.Http
    System.Xaml
    System.Xml.Linq
    Microsoft.CSharp
    PresentationCore
    PresentationFramework
    WindowsBase
)

set(NUGET_PKGS
	Sentry_3.2.0
	MahApps.Metro_2.4.4
    Semver_2.0.6
    Newtonsoft.Json_13.0.1
)

# Set the C# language version for all projects.
set(CMAKE_CSharp_FLAGS "/langversion:9")

# setup paths
set(RUNTIME_OUTPUT_DIRECTORY "${CMAKE_BINARY_DIR}/bin/")
set(LIBRARY_OUTPUT_DIRECTORY "${CMAKE_BINARY_DIR}/lib/")
set(ARCHIVE_OUTPUT_DIRECTORY "${CMAKE_BINARY_DIR}/lib/")
set(EXECUTABLE_OUTPUT_PATH ${RUNTIME_OUTPUT_DIRECTORY})
set(LIBRARY_OUTPUT_PATH ${RUNTIME_OUTPUT_DIRECTORY})

add_subdirectory(Client)
#add_subdirectory(ListGenerator)
#add_subdirectory(SelfUpdater)