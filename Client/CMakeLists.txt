# Setup project to use C# utilities.
include(CSharpUtilities)

project(ror-updater)

set(SOURCE_FILES
    ISwitchable.cs
    PageManager.cs
    Settings.cs

    Properties/AssemblyInfo.cs
    Properties/Resources.Designer.cs
    Properties/Resources.resx
    Properties/Settings.Designer.cs
    Properties/Settings.settings
    Properties/app.manifest

    Resources/ror.ico

    ${CMAKE_SOURCE_DIR}/Shared/Utils.cs
    ${CMAKE_SOURCE_DIR}/Shared/SharedClasses.cs
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
    Pages/InstallPage.xaml
    Pages/InstallPage.xaml.cs
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
set_property(SOURCE app.manifest PROPERTY VS_XAML_TYPE "ApplicationManifest")
set_property(SOURCE Resources/ror.ico PROPERTY VS_XAML_TYPE "ApplicationIcon")



# Set CLR assembly properties.
set_target_properties(${PROJECT_NAME} PROPERTIES
	WIN32_EXECUTABLE TRUE
    VS_DOTNET_REFERENCES "${DOTNET_REF}"
    VS_PACKAGE_REFERENCES "${NUGET_PKGS}"
    VS_GLOBAL_ROOTNAMESPACE ${PROJECT_NAME}
)