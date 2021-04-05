# Setup project to use C# utilities.
include(CSharpUtilities)

set(BIN_NAME "ListGenerator")

set(SOURCE_FILES
    AssemblyInfo.cs
    Program.cs
    
    ${CMAKE_SOURCE_DIR}/Shared/Utils.cs
    ${CMAKE_SOURCE_DIR}/Shared/SharedClasses.cs
)

# Add executable project.
add_executable(${BIN_NAME} ${SOURCE_FILES})

# Set CLR assembly properties.
set_target_properties(${BIN_NAME} PROPERTIES
	WIN32_EXECUTABLE TRUE
    VS_DOTNET_REFERENCES "${DOTNET_REF}"
    VS_PACKAGE_REFERENCES "${NUGET_PKGS}"
    VS_GLOBAL_ROOTNAMESPACE ${BIN_NAME}
)