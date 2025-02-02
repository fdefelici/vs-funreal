// Copyright Epic Games, Inc. All Rights Reserved.

#include "@{TPL_MODULE_NAME}SimpleTool.h"
#include "InteractiveToolManager.h"
#include "ToolBuilderUtil.h"
#include "CollisionQueryParams.h"
#include "Engine/World.h"

// localization namespace
#define LOCTEXT_NAMESPACE "@{TPL_MODULE_NAME}SimpleTool"

/*
 * ToolBuilder implementation
 */

UInteractiveTool* U@{TPL_MODULE_NAME}SimpleToolBuilder::BuildTool(const FToolBuilderState& SceneState) const
{
	U@{TPL_MODULE_NAME}SimpleTool* NewTool = NewObject<U@{TPL_MODULE_NAME}SimpleTool>(SceneState.ToolManager);
	NewTool->SetWorld(SceneState.World);
	return NewTool;
}



/*
 * ToolProperties implementation
 */

U@{TPL_MODULE_NAME}SimpleToolProperties::U@{TPL_MODULE_NAME}SimpleToolProperties()
{
	ShowExtendedInfo = true;
}


/*
 * Tool implementation
 */

U@{TPL_MODULE_NAME}SimpleTool::U@{TPL_MODULE_NAME}SimpleTool()
{
}


void U@{TPL_MODULE_NAME}SimpleTool::SetWorld(UWorld* World)
{
	this->TargetWorld = World;
}


void U@{TPL_MODULE_NAME}SimpleTool::Setup()
{
	USingleClickTool::Setup();

	Properties = NewObject<U@{TPL_MODULE_NAME}SimpleToolProperties>(this);
	AddToolPropertySource(Properties);
}


void U@{TPL_MODULE_NAME}SimpleTool::OnClicked(const FInputDeviceRay& ClickPos)
{
	// we will create actor at this position
	FVector NewActorPos = FVector::ZeroVector;

	// cast ray into world to find hit position
	FVector RayStart = ClickPos.WorldRay.Origin;
	FVector RayEnd = ClickPos.WorldRay.PointAt(99999999.f);
	FCollisionObjectQueryParams QueryParams(FCollisionObjectQueryParams::AllObjects);
	FHitResult Result;
	if (TargetWorld->LineTraceSingleByObjectType(Result, RayStart, RayEnd, QueryParams))
	{
		if (AActor* ClickedActor = Result.GetActor())
		{
			FText ActorInfoMsg;

			if (Properties->ShowExtendedInfo)
			{
				ActorInfoMsg = FText::Format(LOCTEXT("ExtendedActorInfo", "Name: {0}\nClass: {1}"), 
					FText::FromString(ClickedActor->GetName()), 
					FText::FromString(ClickedActor->GetClass()->GetName())
				);
			}
			else
			{
				ActorInfoMsg = FText::Format(LOCTEXT("BasicActorInfo", "Name: {0}"), FText::FromString(Result.GetActor()->GetName()));
			}

			FText Title = LOCTEXT("ActorInfoDialogTitle", "Actor Info");
			// JAH TODO: consider if we can highlight the actor prior to opening the dialog box or make it non-modal
			FMessageDialog::Open(EAppMsgType::Ok, ActorInfoMsg, &Title);
		}
	}
}


#undef LOCTEXT_NAMESPACE