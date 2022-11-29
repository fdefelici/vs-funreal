// Copyright Epic Games, Inc. All Rights Reserved.

#include "@{TPL_MODU_CLASS}.h"
#include "@{TPL_MODU_NAME}EditorModeCommands.h"

#define LOCTEXT_NAMESPACE "@{TPL_MODU_CLASS}"

void F@{TPL_MODU_CLASS}::StartupModule()
{
	// This code will execute after your module is loaded into memory; the exact timing is specified in the .uplugin file per-module

	F@{TPL_MODU_NAME}EditorModeCommands::Register();
}

void F@{TPL_MODU_CLASS}::ShutdownModule()
{
	// This function may be called during shutdown to clean up your module.  For modules that support dynamic reloading,
	// we call this function before unloading the module.

	F@{TPL_MODU_NAME}EditorModeCommands::Unregister();
}

#undef LOCTEXT_NAMESPACE

IMPLEMENT_MODULE(F@{TPL_MODU_CLASS}, @{TPL_MODU_NAME})  //@{TPL_MODU_NAME}EditorMode