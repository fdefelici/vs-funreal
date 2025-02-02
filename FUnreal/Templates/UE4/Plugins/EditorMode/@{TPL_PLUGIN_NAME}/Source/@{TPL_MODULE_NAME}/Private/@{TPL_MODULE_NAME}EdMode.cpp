// Copyright Epic Games, Inc. All Rights Reserved.

#include "@{TPL_MODULE_NAME}EdMode.h"
#include "@{TPL_MODULE_NAME}EdModeToolkit.h"
#include "Toolkits/ToolkitManager.h"
#include "EditorModeManager.h"

const FEditorModeID F@{TPL_MODULE_NAME}EdMode::EM_@{TPL_MODULE_NAME}EdModeId = TEXT("EM_@{TPL_MODULE_NAME}EdMode");

F@{TPL_MODULE_NAME}EdMode::F@{TPL_MODULE_NAME}EdMode()
{

}

F@{TPL_MODULE_NAME}EdMode::~F@{TPL_MODULE_NAME}EdMode()
{

}

void F@{TPL_MODULE_NAME}EdMode::Enter()
{
	FEdMode::Enter();

	if (!Toolkit.IsValid() && UsesToolkits())
	{
		Toolkit = MakeShareable(new F@{TPL_MODULE_NAME}EdModeToolkit);
		Toolkit->Init(Owner->GetToolkitHost());
	}
}

void F@{TPL_MODULE_NAME}EdMode::Exit()
{
	if (Toolkit.IsValid())
	{
		FToolkitManager::Get().CloseToolkit(Toolkit.ToSharedRef());
		Toolkit.Reset();
	}

	// Call base Exit method to ensure proper cleanup
	FEdMode::Exit();
}

bool F@{TPL_MODULE_NAME}EdMode::UsesToolkits() const
{
	return true;
}




