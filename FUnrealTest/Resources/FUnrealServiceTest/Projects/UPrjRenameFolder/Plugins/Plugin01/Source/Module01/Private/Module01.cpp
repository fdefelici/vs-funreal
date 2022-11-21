// Copyright Epic Games, Inc. All Rights Reserved.

#include "Module01.h"
#include "Hello/Actor01.h"

#define LOCTEXT_NAMESPACE "FModule01Module"

void FModule01Module::StartupModule()
{
	// This code will execute after your module is loaded into memory; the exact timing is specified in the .uplugin file per-module
}

void FModule01Module::ShutdownModule()
{
	// This function may be called during shutdown to clean up your module.  For modules that support dynamic reloading,
	// we call this function before unloading the module.
}

#undef LOCTEXT_NAMESPACE

IMPLEMENT_MODULE(FModule01Module, Module01)