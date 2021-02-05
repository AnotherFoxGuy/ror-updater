cmake_minimum_required(VERSION 3.18)
project(ror-updater LANGUAGES NONE)

file(GLOB XSDCSDC LIST_DIRECTORIES false CONFIGURE_DEPENDS "Client/bin/Release/*")


add_custom_target(
        compile ALL
        COMMAND packages/ILMerge.3.0.41/tools/net452/ILMerge.exe ${XSDCSDC}
        VERBATIM
)