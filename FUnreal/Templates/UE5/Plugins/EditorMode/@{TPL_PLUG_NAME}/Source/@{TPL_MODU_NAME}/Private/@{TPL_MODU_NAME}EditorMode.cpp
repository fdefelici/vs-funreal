// Copyright Epic Games, Inc. All Rights Reserved.

#include "@{TPL_MODU_NAME}EditorMode.h"
#include "@{TPL_MODU_NAME}EditorModeToolkit.h"
#include "EdModeInteractiveToolsContext.h"
#include "InteractiveToolManager.h"
#include "@{TPL_MODU_NAME}EditorModeCommands.h"


//////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////// 
// AddYourTool Step 1 - include the header file for your Tools here
//////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////// 
#include "Tools/@{TPL_MODU_NAME}SimpleTool.h"
#include "Tools/@{TPL_MODU_NAME}InteractiveTool.h"

// step 2: register a ToolBuilder in F@{TPL_MODU_NAME}EditorMode::Enter() below


#define LOCTEXT_NAMESPACE "@{TPL_MODU_NAME}EditorMode"

const FEditorModeID U@{TPL_MODU_NAME}EditorMode::EM_@{TPL_MODU_NAME}EditorModeId = TEXT("EM_@{TPL_MODU_NAME}EditorMode");

FString U@{TPL_MODU_NAME}EditorMode::SimpleToolName = TEXT("@{TPL_MODU_NAME}_ActorInfoTool");
FString U@{TPL_MODU_NAME}EditorMode::InteractiveToolName = TEXT("@{TPL_MODU_NAME}_MeasureDistanceTool");


U@{TPL_MODU_NAME}EditorMode::U@{TPL_MODU_NAME}EditorMode()
{
	FModuleManager::Get().LoadModule("EditorStyle");

	// appearance and icon in the editing mode ribbon can be customized here
	Info = FEditorModeInfo(U@{TPL_MODU_NAME}EditorMode::EM_@{TPL_MODU_NAME}EditorModeId,
		LOCTEXT("ModeName", "@{TPL_MODU_NAME}"),
		FSlateIcon(),
		true);
}


U@{TPL_MODU_NAME}EditorMode::~U@{TPL_MODU_NAME}EditorMode()
{
}


void U@{TPL_MODU_NAME}EditorMode::ActorSelectionChangeNotify()
{
}

void U@{TPL_MODU_NAME}EditorMode::Enter()
{
	UEdMode::Enter();

	//////////////////////////////////////////////////////////////////////////
	//////////////////////////////////////////////////////////////////////////
	// AddYourTool Step 2 - register the ToolBuilders for your Tools here.
	// The string name you pass to the ToolManager is used to select/activate your ToolBuilder later.
	//////////////////////////////////////////////////////////////////////////
	////////////////////////////////////////////////////////////////////////// 
	const F@{TPL_MODU_NAME}EditorModeCommands& SampleToolCommands = F@{TPL_MODU_NAME}EditorModeCommands::Get();

	RegisterTool(SampleToolCommands.SimpleTool, SimpleToolName, NewObject<U@{TPL_MODU_NAME}SimpleToolBuilder>(this));
	RegisterTool(SampleToolCommands.InteractiveTool, InteractiveToolName, NewObject<U@{TPL_MODU_NAME}InteractiveToolBuilder>(this));

	// active tool type is not relevant here, we just set to default
	GetToolManager()->SelectActiveToolType(EToolSide::Left, SimpleToolName);
}

void U@{TPL_MODU_NAME}EditorMode::CreateToolkit()
{
	Toolkit = MakeShareable(new F@{TPL_MODU_NAME}EditorModeToolkit);
}

TMap<FName, TArray<TSharedPtr<FUICommandInfo>>> U@{TPL_MODU_NAME}EditorMode::GetModeCommands() const
{
	return F@{TPL_MODU_NAME}EditorModeCommands::Get().GetCommands();
}

#undef LOCTEXT_NAMESPACE
