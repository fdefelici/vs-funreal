// Copyright Epic Games, Inc. All Rights Reserved.

#pragma once

#include "CoreMinimal.h"
#include "Framework/Commands/Commands.h"
#include "@{TPL_MODU_NAME}Style.h"

class F@{TPL_MODU_NAME}Commands : public TCommands<F@{TPL_MODU_NAME}Commands>
{
public:

	F@{TPL_MODU_NAME}Commands()
		: TCommands<F@{TPL_MODU_NAME}Commands>(TEXT("@{TPL_MODU_NAME}"), NSLOCTEXT("Contexts", "@{TPL_MODU_NAME}", "@{TPL_MODU_NAME} Module"), NAME_None, F@{TPL_MODU_NAME}Style::GetStyleSetName())
	{
	}

	// TCommands<> interface
	virtual void RegisterCommands() override;

public:
	TSharedPtr< FUICommandInfo > PluginAction;
};
