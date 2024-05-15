// Copyright Epic Games, Inc. All Rights Reserved.

#include "@{TPL_MODULE_CLASS}.h"
#include "@{TPL_MODULE_NAME}EdMode.h"

#define LOCTEXT_NAMESPACE "F@{TPL_MODULE_CLASS}"

void F@{TPL_MODULE_CLASS}::StartupModule()
{
	// This code will execute after your module is loaded into memory; the exact timing is specified in the .uplugin file per-module
	FEditorModeRegistry::Get().RegisterMode<F@{TPL_MODULE_NAME}EdMode>(F@{TPL_MODULE_NAME}EdMode::EM_@{TPL_MODULE_NAME}EdModeId, LOCTEXT("@{TPL_MODULE_NAME}EdModeName", "@{TPL_MODULE_NAME}EdMode"), FSlateIcon(), true);
}

void F@{TPL_MODULE_CLASS}::ShutdownModule()
{
	// This function may be called during shutdown to clean up your module.  For modules that support dynamic reloading,
	// we call this function before unloading the module.
	FEditorModeRegistry::Get().UnregisterMode(F@{TPL_MODULE_NAME}EdMode::EM_@{TPL_MODULE_NAME}EdModeId);
}

#undef LOCTEXT_NAMESPACE
	
IMPLEMENT_MODULE(F@{TPL_MODULE_CLASS}, @{TPL_MODULE_NAME})