# Setup project to use C# utilities.
include(CSharpUtilities)

project("ror-updater")

set(ICON_FILE "${CMAKE_SOURCE_DIR}/Icons/ror.ico")
set(SPLASH_IMG "${CMAKE_SOURCE_DIR}/Icons/ror_updater.jpg")
string(TIMESTAMP YEAR "%Y")

configure_file(${CMAKE_SOURCE_DIR}/AssemblyInfo.cs.in AssemblyInfo.cs @ONLY)
configure_file(Properties/Resources.resx.in Resources.resx @ONLY)

set(SOURCE_FILES
    ISwitchable.cs
    PageManager.cs
    Settings.cs
    Properties/Resources.Designer.cs
    
    Properties/Settings.Designer.cs
    Properties/Settings.settings
    Properties/app.manifest

    ${ICON_FILE}

    ${CMAKE_SOURCE_DIR}/Shared/Utils.cs
    ${CMAKE_SOURCE_DIR}/Shared/SharedClasses.cs

    ${CMAKE_CURRENT_BINARY_DIR}/AssemblyInfo.cs
    ${CMAKE_CURRENT_BINARY_DIR}/Resources.resx
)

set(XAML_FILES
    App.xaml
	App.xaml.cs
    PageSwitcher.xaml
    PageSwitcher.xaml.cs

    Pages/BlankPage.xaml
    Pages/BlankPage.xaml.cs
    Pages/ChoicePage.xaml
    Pages/ChoicePage.xaml.cs
    Pages/MainPage.xaml
    Pages/MainPage.xaml.cs
    Pages/UpdateDonePage.xaml
    Pages/UpdateDonePage.xaml.cs
    Pages/UpdatePage.xaml
    Pages/UpdatePage.xaml.cs
)

set(FORM_FILES
    StartupForm.Designer.cs
    StartupForm.cs
    StartupForm.resx
)

# Add executable project.
add_executable(${PROJECT_NAME} ${SOURCE_FILES} ${XAML_FILES} ${FORM_FILES})

# Set Windows Forms properties
csharp_set_designer_cs_properties(${SOURCE_FILES})
csharp_set_xaml_cs_properties(${XAML_FILES})
csharp_set_windows_forms_properties(${FORM_FILES})

set_property(SOURCE App.xaml PROPERTY VS_XAML_TYPE "ApplicationDefinition")

# Set CLR assembly properties.
set_target_properties(${PROJECT_NAME} PROPERTIES
	WIN32_EXECUTABLE TRUE
    VS_DOTNET_REFERENCES "${DOTNET_REF}"
    VS_PACKAGE_REFERENCES "${NUGET_PKGS}"
    VS_GLOBAL_ROOTNAMESPACE "ror_updater"
    VS_CSHARP_GenerateManifests false
    VS_CSHARP_ApplicationManifests "app.manifest"
)

install(TARGETS ${PROJECT_NAME} RUNTIME DESTINATION .)
#install(FILES ${PROJECT_NAME}.exe.config RUNTIME DESTINATION .)
install(DIRECTORY "$<TARGET_FILE_DIR:${PROJECT_NAME}>/" DESTINATION . FILES_MATCHING PATTERN "*.dll")