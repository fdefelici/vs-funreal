// Copyright Epic Games, Inc. All Rights Reserved.

#include "@{TPL_MODULE_NAME}Commands.h"

#define LOCTEXT_NAMESPACE "F@{TPL_MODULE_CLASS}"

void F@{TPL_MODULE_NAME}Commands::RegisterCommands()
{
	UI_COMMAND(OpenPluginWindow, "@{TPL_MODULE_NAME}", "Bring up @{TPL_MODULE_NAME} window", EUserInterfaceActionType::Button, FInputChord());
}

#undef LOCTEXT_NAMESPACE
