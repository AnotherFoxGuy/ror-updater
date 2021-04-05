cmake_minimum_required(VERSION 3.14)
project("Rigs of Rods Updater" 
    LANGUAGES CSharp
    VERSION 1.1.1
    DESCRIPTION "Rigs of Rods Installer/Updater for Windows"
)

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

set(CPACK_PACKAGE_NAME "${CMAKE_PROJECT_NAME}")
set(CPACK_PACKAGE_DESCRIPTION "${CMAKE_PROJECT_DESCRIPTION}")
set(CPACK_PACKAGE_DESCRIPTION_SUMMARY "${CMAKE_PROJECT_DESCRIPTION}")
set(CPACK_PACKAGE_VERSION ${PROJECT_VERSION})
set(CPACK_RESOURCE_FILE_LICENSE "${CMAKE_SOURCE_DIR}/license.rtf")
set(CPACK_PACKAGE_EXECUTABLES "ror-updater" "${CMAKE_PROJECT_NAME}")

set(CPACK_GENERATOR ZIP WIX)
set(CPACK_PACKAGE_FILE_NAME "ror-updater-V${PROJECT_VERSION}")

# Windows Add or Remove Program properties
set(CPACK_WIX_PROGRAM_MENU_FOLDER "${CMAKE_PROJECT_NAME}")
set(CPACK_WIX_PROPERTY_ARPCOMMENTS "${CPACK_PACKAGE_DESCRIPTION_SUMMARY}")
set(CPACK_WIX_PROPERTY_ARPURLINFOABOUT "https://rigsofrods.org/")
set(CPACK_WIX_PROPERTY_URLUPDATEINFO "https://rigsofrods.org/")

set(CPACK_WIX_PRODUCT_ICON "${CMAKE_SOURCE_DIR}/Icons/ror.ico")

set(CPACK_WIX_LICENSE_RTF "${CPACK_RESOURCE_FILE_LICENSE}")
set(CPACK_WIX_UPGRADE_GUID "db6e2dc7-800d-440c-8e0e-73bb072dc739")

set(CPACK_PACKAGE_INSTALL_DIRECTORY "Rigs of Rods")
set(CPACK_MODULE_PATH "")

include(CPack)