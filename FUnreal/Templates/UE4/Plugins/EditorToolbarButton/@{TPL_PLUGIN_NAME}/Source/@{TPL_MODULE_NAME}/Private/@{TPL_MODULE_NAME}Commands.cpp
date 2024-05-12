// Copyright Epic Games, Inc. All Rights Reserved.

#include "@{TPL_MODULE_NAME}Commands.h"

#define LOCTEXT_NAMESPACE "F@{TPL_MODULE_CLASS}"

void F@{TPL_MODULE_NAME}Commands::RegisterCommands()
{
	UI_COMMAND(PluginAction, "@{TPL_MODULE_NAME}", "Execute @{TPL_MODULE_NAME} action", EUserInterfaceActionType::Button, FInputGesture());
}

#undef LOCTEXT_NAMESPACE
