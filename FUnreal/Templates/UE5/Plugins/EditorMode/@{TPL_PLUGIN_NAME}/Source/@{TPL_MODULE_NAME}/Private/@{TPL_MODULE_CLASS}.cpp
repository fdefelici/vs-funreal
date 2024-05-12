// Copyright Epic Games, Inc. All Rights Reserved.

#include "@{TPL_MODULE_CLASS}.h"
#include "@{TPL_MODULE_NAME}EditorModeCommands.h"

#define LOCTEXT_NAMESPACE "@{TPL_MODULE_CLASS}"

void F@{TPL_MODULE_CLASS}::StartupModule()
{
	// This code will execute after your module is loaded into memory; the exact timing is specified in the .uplugin file per-module

	F@{TPL_MODULE_NAME}EditorModeCommands::Register();
}

void F@{TPL_MODULE_CLASS}::ShutdownModule()
{
	// This function may be called during shutdown to clean up your module.  For modules that support dynamic reloading,
	// we call this function before unloading the module.

	F@{TPL_MODULE_NAME}EditorModeCommands::Unregister();
}

#undef LOCTEXT_NAMESPACE

IMPLEMENT_MODULE(F@{TPL_MODULE_CLASS}, @{TPL_MODULE_NAME})  //@{TPL_MODULE_NAME}EditorMode