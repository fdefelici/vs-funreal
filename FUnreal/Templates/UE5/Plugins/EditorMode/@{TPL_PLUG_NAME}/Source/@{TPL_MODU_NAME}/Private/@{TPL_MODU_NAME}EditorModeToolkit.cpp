// Copyright Epic Games, Inc. All Rights Reserved.

#include "@{TPL_MODU_NAME}EditorModeToolkit.h"
#include "@{TPL_MODU_NAME}EditorMode.h"
#include "Engine/Selection.h"

#include "Modules/ModuleManager.h"
#include "PropertyEditorModule.h"
#include "IDetailsView.h"
#include "EditorModeManager.h"

#define LOCTEXT_NAMESPACE "@{TPL_MODU_NAME}EditorModeToolkit"

F@{TPL_MODU_NAME}EditorModeToolkit::F@{TPL_MODU_NAME}EditorModeToolkit()
{
}

void F@{TPL_MODU_NAME}EditorModeToolkit::Init(const TSharedPtr<IToolkitHost>& InitToolkitHost, TWeakObjectPtr<UEdMode> InOwningMode)
{
	FModeToolkit::Init(InitToolkitHost, InOwningMode);
}

void F@{TPL_MODU_NAME}EditorModeToolkit::GetToolPaletteNames(TArray<FName>& PaletteNames) const
{
	PaletteNames.Add(NAME_Default);
}


FName F@{TPL_MODU_NAME}EditorModeToolkit::GetToolkitFName() const
{
	return FName("@{TPL_MODU_NAME}EditorMode");
}

FText F@{TPL_MODU_NAME}EditorModeToolkit::GetBaseToolkitName() const
{
	return LOCTEXT("DisplayName", "@{TPL_MODU_NAME}EditorMode Toolkit");
}

#undef LOCTEXT_NAMESPACE
