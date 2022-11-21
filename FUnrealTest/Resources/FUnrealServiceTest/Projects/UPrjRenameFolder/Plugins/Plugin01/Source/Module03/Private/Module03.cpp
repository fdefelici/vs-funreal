// Copyright Epic Games, Inc. All Rights Reserved.

#include "Module03.h"
#include "Hello/Actor01.h" //not valid, because no dependency from module03 to module01. Just for testing purpose, should stay untouched.

#define LOCTEXT_NAMESPACE "FModule03Module"

void FModule03Module::StartupModule()
{
	// This code will execute after your module is loaded into memory; the exact timing is specified in the .uplugin file per-module
}

void FModule03Module::ShutdownModule()
{
	// This function may be called during shutdown to clean up your module.  For modules that support dynamic reloading,
	// we call this function before unloading the module.
}

#undef LOCTEXT_NAMESPACE

IMPLEMENT_MODULE(FModule03Module, Module03)