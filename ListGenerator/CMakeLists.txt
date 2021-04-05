# Setup project to use C# utilities.
include(CSharpUtilities)

project("ListGenerator")

configure_file(${CMAKE_SOURCE_DIR}/AssemblyInfo.cs.in AssemblyInfo.cs @ONLY)

set(SOURCE_FILES
    Program.cs
    
    ${CMAKE_SOURCE_DIR}/Shared/Utils.cs
    ${CMAKE_SOURCE_DIR}/Shared/SharedClasses.cs
    ${CMAKE_CURRENT_BINARY_DIR}/AssemblyInfo.cs
)

# Add executable project.
add_executable(${PROJECT_NAME} ${SOURCE_FILES})

# Set CLR assembly properties.
set_target_properties(${PROJECT_NAME} PROPERTIES
	WIN32_EXECUTABLE TRUE
    VS_DOTNET_REFERENCES "${DOTNET_REF}"
    VS_PACKAGE_REFERENCES "${NUGET_PKGS}"
    VS_GLOBAL_ROOTNAMESPACE ${PROJECT_NAME}
)