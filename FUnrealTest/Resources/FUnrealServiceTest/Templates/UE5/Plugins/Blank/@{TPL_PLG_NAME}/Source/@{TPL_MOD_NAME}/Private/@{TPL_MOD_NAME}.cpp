// Copyright Epic Games, Inc. All Rights Reserved.

#include "@{TPL_MOD_NAME}.h"

#define LOCTEXT_NAMESPACE "F@{TPL_MOD_NAME}Module"

void F@{TPL_MOD_NAME}Module::StartupModule()
{
	// This code will execute after your module is loaded into memory; the exact timing is specified in the .uplugin file per-module
}

void F@{TPL_MOD_NAME}Module::ShutdownModule()
{
	// This function may be called during shutdown to clean up your module.  For modules that support dynamic reloading,
	// we call this function before unloading the module.
}

#undef LOCTEXT_NAMESPACE
	
IMPLEMENT_MODULE(F@{TPL_MOD_NAME}Module, @{TPL_MOD_NAME})