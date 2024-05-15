// Copyright Epic Games, Inc. All Rights Reserved.

#include "@{TPL_MODULE_NAME}EditorModeCommands.h"
#include "@{TPL_MODULE_NAME}EditorMode.h"
#include "EditorStyleSet.h"

#define LOCTEXT_NAMESPACE "@{TPL_MODULE_NAME}EditorModeCommands"

F@{TPL_MODULE_NAME}EditorModeCommands::F@{TPL_MODULE_NAME}EditorModeCommands()
	: TCommands<F@{TPL_MODULE_NAME}EditorModeCommands>("@{TPL_MODULE_NAME}EditorMode",
		NSLOCTEXT("@{TPL_MODULE_NAME}EditorMode", "@{TPL_MODULE_NAME}EditorModeCommands", "@{TPL_MODULE_NAME} Editor Mode"),
		NAME_None,
		FEditorStyle::GetStyleSetName())
{
}

void F@{TPL_MODULE_NAME}EditorModeCommands::RegisterCommands()
{
	TArray <TSharedPtr<FUICommandInfo>>& ToolCommands = Commands.FindOrAdd(NAME_Default);

	UI_COMMAND(SimpleTool, "Show Actor Info", "Opens message box with info about a clicked actor", EUserInterfaceActionType::Button, FInputChord());
	ToolCommands.Add(SimpleTool);

	UI_COMMAND(InteractiveTool, "Measure Distance", "Measures distance between 2 points (click to set origin, shift-click to set end point)", EUserInterfaceActionType::ToggleButton, FInputChord());
	ToolCommands.Add(InteractiveTool);
}

TMap<FName, TArray<TSharedPtr<FUICommandInfo>>> F@{TPL_MODULE_NAME}EditorModeCommands::GetCommands()
{
	return F@{TPL_MODULE_NAME}EditorModeCommands::Get().Commands;
}

#undef LOCTEXT_NAMESPACE
