# Setup project to use C# utilities.
include(CSharpUtilities)

project(SelfUpdater)

set(SOURCE_FILES
    app.manifest
    AssemblyInfo.cs
    Program.cs
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