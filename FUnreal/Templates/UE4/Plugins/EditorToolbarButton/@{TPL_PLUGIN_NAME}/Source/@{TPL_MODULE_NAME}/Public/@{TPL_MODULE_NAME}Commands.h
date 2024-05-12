// Copyright Epic Games, Inc. All Rights Reserved.

#pragma once

#include "CoreMinimal.h"
#include "Framework/Commands/Commands.h"
#include "@{TPL_MODULE_NAME}Style.h"

class F@{TPL_MODULE_NAME}Commands : public TCommands<F@{TPL_MODULE_NAME}Commands>
{
public:

	F@{TPL_MODULE_NAME}Commands()
		: TCommands<F@{TPL_MODULE_NAME}Commands>(TEXT("@{TPL_MODULE_NAME}"), NSLOCTEXT("Contexts", "@{TPL_MODULE_NAME}", "@{TPL_MODULE_NAME} Module"), NAME_None, F@{TPL_MODULE_NAME}Style::GetStyleSetName())
	{
	}

	// TCommands<> interface
	virtual void RegisterCommands() override;

public:
	TSharedPtr< FUICommandInfo > PluginAction;
};
