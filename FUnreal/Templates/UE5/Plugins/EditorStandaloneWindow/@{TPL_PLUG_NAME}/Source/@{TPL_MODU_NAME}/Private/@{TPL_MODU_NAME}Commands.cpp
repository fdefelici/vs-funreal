// Copyright Epic Games, Inc. All Rights Reserved.

#include "@{TPL_MODU_NAME}Commands.h"

#define LOCTEXT_NAMESPACE "F@{TPL_MODU_CLASS}"

void F@{TPL_MODU_NAME}Commands::RegisterCommands()
{
	UI_COMMAND(OpenPluginWindow, "@{TPL_MODU_NAME}", "Bring up @{TPL_MODU_NAME} window", EUserInterfaceActionType::Button, FInputChord());
}

#undef LOCTEXT_NAMESPACE
