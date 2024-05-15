// Copyright Epic Games, Inc. All Rights Reserved.

#include "@{TPL_MODULE_NAME}EditorMode.h"
#include "@{TPL_MODULE_NAME}EditorModeToolkit.h"
#include "EdModeInteractiveToolsContext.h"
#include "InteractiveToolManager.h"
#include "@{TPL_MODULE_NAME}EditorModeCommands.h"


//////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////// 
// AddYourTool Step 1 - include the header file for your Tools here
//////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////// 
#include "Tools/@{TPL_MODULE_NAME}SimpleTool.h"
#include "Tools/@{TPL_MODULE_NAME}InteractiveTool.h"

// step 2: register a ToolBuilder in F@{TPL_MODULE_NAME}EditorMode::Enter() below


#define LOCTEXT_NAMESPACE "@{TPL_MODULE_NAME}EditorMode"

const FEditorModeID U@{TPL_MODULE_NAME}EditorMode::EM_@{TPL_MODULE_NAME}EditorModeId = TEXT("EM_@{TPL_MODULE_NAME}EditorMode");

FString U@{TPL_MODULE_NAME}EditorMode::SimpleToolName = TEXT("@{TPL_MODULE_NAME}_ActorInfoTool");
FString U@{TPL_MODULE_NAME}EditorMode::InteractiveToolName = TEXT("@{TPL_MODULE_NAME}_MeasureDistanceTool");


U@{TPL_MODULE_NAME}EditorMode::U@{TPL_MODULE_NAME}EditorMode()
{
	FModuleManager::Get().LoadModule("EditorStyle");

	// appearance and icon in the editing mode ribbon can be customized here
	Info = FEditorModeInfo(U@{TPL_MODULE_NAME}EditorMode::EM_@{TPL_MODULE_NAME}EditorModeId,
		LOCTEXT("ModeName", "@{TPL_MODULE_NAME}"),
		FSlateIcon(),
		true);
}


U@{TPL_MODULE_NAME}EditorMode::~U@{TPL_MODULE_NAME}EditorMode()
{
}


void U@{TPL_MODULE_NAME}EditorMode::ActorSelectionChangeNotify()
{
}

void U@{TPL_MODULE_NAME}EditorMode::Enter()
{
	UEdMode::Enter();

	//////////////////////////////////////////////////////////////////////////
	//////////////////////////////////////////////////////////////////////////
	// AddYourTool Step 2 - register the ToolBuilders for your Tools here.
	// The string name you pass to the ToolManager is used to select/activate your ToolBuilder later.
	//////////////////////////////////////////////////////////////////////////
	////////////////////////////////////////////////////////////////////////// 
	const F@{TPL_MODULE_NAME}EditorModeCommands& SampleToolCommands = F@{TPL_MODULE_NAME}EditorModeCommands::Get();

	RegisterTool(SampleToolCommands.SimpleTool, SimpleToolName, NewObject<U@{TPL_MODULE_NAME}SimpleToolBuilder>(this));
	RegisterTool(SampleToolCommands.InteractiveTool, InteractiveToolName, NewObject<U@{TPL_MODULE_NAME}InteractiveToolBuilder>(this));

	// active tool type is not relevant here, we just set to default
	GetToolManager()->SelectActiveToolType(EToolSide::Left, SimpleToolName);
}

void U@{TPL_MODULE_NAME}EditorMode::CreateToolkit()
{
	Toolkit = MakeShareable(new F@{TPL_MODULE_NAME}EditorModeToolkit);
}

TMap<FName, TArray<TSharedPtr<FUICommandInfo>>> U@{TPL_MODULE_NAME}EditorMode::GetModeCommands() const
{
	return F@{TPL_MODULE_NAME}EditorModeCommands::Get().GetCommands();
}

#undef LOCTEXT_NAMESPACE
