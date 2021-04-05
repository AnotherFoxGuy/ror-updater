cmake_minimum_required(VERSION 3.14)
project("Rigs of Rods Updater" LANGUAGES CSharp)

# Set global .NET Framework version.
set(CMAKE_DOTNET_TARGET_FRAMEWORK_VERSION "v4.8")

set(DOTNET_REF
    Microsoft.CSharp
    PresentationCore
    PresentationFramework
    System
    System.Core
    System.CommandLine
    System.Data
    System.Data.DataSetExtensions
    System.Drawing
    System.IO.Compression
    System.Net.Http
    System.Threading
    System.Windows.Forms
    System.Xaml
    System.Xml.Linq
    WindowsBase
)

set(NUGET_PKGS
	Sentry_3.2.0
	MahApps.Metro_2.4.4
    Semver_2.0.6
    Newtonsoft.Json_13.0.1
    System.CommandLine_2.0.0-beta1.20574.7
    System.IO.Compression.ZipFile_4.3.0
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
add_subdirectory(ListGenerator)
add_subdirectory(SelfUpdater)