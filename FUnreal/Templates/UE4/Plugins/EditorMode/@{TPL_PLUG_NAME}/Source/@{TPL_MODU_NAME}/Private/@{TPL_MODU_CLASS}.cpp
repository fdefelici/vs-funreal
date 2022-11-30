// Copyright Epic Games, Inc. All Rights Reserved.

#include "@{TPL_MODU_CLASS}.h"
#include "@{TPL_MODU_NAME}EdMode.h"

#define LOCTEXT_NAMESPACE "F@{TPL_MODU_CLASS}"

void F@{TPL_MODU_CLASS}::StartupModule()
{
	// This code will execute after your module is loaded into memory; the exact timing is specified in the .uplugin file per-module
	FEditorModeRegistry::Get().RegisterMode<F@{TPL_MODU_NAME}EdMode>(F@{TPL_MODU_NAME}EdMode::EM_@{TPL_MODU_NAME}EdModeId, LOCTEXT("@{TPL_MODU_NAME}EdModeName", "@{TPL_MODU_NAME}EdMode"), FSlateIcon(), true);
}

void F@{TPL_MODU_CLASS}::ShutdownModule()
{
	// This function may be called during shutdown to clean up your module.  For modules that support dynamic reloading,
	// we call this function before unloading the module.
	FEditorModeRegistry::Get().UnregisterMode(F@{TPL_MODU_NAME}EdMode::EM_@{TPL_MODU_NAME}EdModeId);
}

#undef LOCTEXT_NAMESPACE
	
IMPLEMENT_MODULE(F@{TPL_MODU_CLASS}, @{TPL_MODU_NAME})