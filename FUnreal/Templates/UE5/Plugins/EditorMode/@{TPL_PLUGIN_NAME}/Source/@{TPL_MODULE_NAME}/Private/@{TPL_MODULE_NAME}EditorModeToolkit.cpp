// Copyright Epic Games, Inc. All Rights Reserved.

#include "@{TPL_MODULE_NAME}EditorModeToolkit.h"
#include "@{TPL_MODULE_NAME}EditorMode.h"
#include "Engine/Selection.h"

#include "Modules/ModuleManager.h"
#include "PropertyEditorModule.h"
#include "IDetailsView.h"
#include "EditorModeManager.h"

#define LOCTEXT_NAMESPACE "@{TPL_MODULE_NAME}EditorModeToolkit"

F@{TPL_MODULE_NAME}EditorModeToolkit::F@{TPL_MODULE_NAME}EditorModeToolkit()
{
}

void F@{TPL_MODULE_NAME}EditorModeToolkit::Init(const TSharedPtr<IToolkitHost>& InitToolkitHost, TWeakObjectPtr<UEdMode> InOwningMode)
{
	FModeToolkit::Init(InitToolkitHost, InOwningMode);
}

void F@{TPL_MODULE_NAME}EditorModeToolkit::GetToolPaletteNames(TArray<FName>& PaletteNames) const
{
	PaletteNames.Add(NAME_Default);
}


FName F@{TPL_MODULE_NAME}EditorModeToolkit::GetToolkitFName() const
{
	return FName("@{TPL_MODULE_NAME}EditorMode");
}

FText F@{TPL_MODULE_NAME}EditorModeToolkit::GetBaseToolkitName() const
{
	return LOCTEXT("DisplayName", "@{TPL_MODULE_NAME}EditorMode Toolkit");
}

#undef LOCTEXT_NAMESPACE
