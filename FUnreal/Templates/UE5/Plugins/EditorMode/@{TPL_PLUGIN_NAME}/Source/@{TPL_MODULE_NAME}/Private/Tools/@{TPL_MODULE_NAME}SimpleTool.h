// Copyright Epic Games, Inc. All Rights Reserved.

#pragma once

#include "CoreMinimal.h"
#include "UObject/NoExportTypes.h"
#include "InteractiveToolBuilder.h"
#include "BaseTools/SingleClickTool.h"
#include "@{TPL_MODULE_NAME}SimpleTool.generated.h"

/**
 * Builder for U@{TPL_MODULE_NAME}SimpleTool
 */
UCLASS()
class U@{TPL_MODULE_NAME}SimpleToolBuilder : public UInteractiveToolBuilder
{
	GENERATED_BODY()

public:
	virtual bool CanBuildTool(const FToolBuilderState& SceneState) const override { return true; }
	virtual UInteractiveTool* BuildTool(const FToolBuilderState& SceneState) const override;
};



/**
 * Settings UObject for U@{TPL_MODULE_NAME}SimpleTool. This UClass inherits from UInteractiveToolPropertySet,
 * which provides an OnModified delegate that the Tool will listen to for changes in property values.
 */
UCLASS(Transient)
class U@{TPL_MODULE_NAME}SimpleToolProperties : public UInteractiveToolPropertySet
{
	GENERATED_BODY()
public:
	U@{TPL_MODULE_NAME}SimpleToolProperties();

	/** If enabled, dialog should display extended information about the actor clicked on. Otherwise, only basic info will be shown. */
	UPROPERTY(EditAnywhere, Category = Options, meta = (DisplayName = "Show Extended Info"))
	bool ShowExtendedInfo;
};




/**
 * U@{TPL_MODULE_NAME}SimpleTool is an example Tool that opens a message box displaying info about an actor that the user
 * clicks left mouse button. All the action is in the ::OnClicked handler.
 */
UCLASS()
class U@{TPL_MODULE_NAME}SimpleTool : public USingleClickTool
{
	GENERATED_BODY()

public:
	U@{TPL_MODULE_NAME}SimpleTool();

	virtual void SetWorld(UWorld* World);

	virtual void Setup() override;

	virtual void OnClicked(const FInputDeviceRay& ClickPos);


protected:
	UPROPERTY()
	TObjectPtr<U@{TPL_MODULE_NAME}SimpleToolProperties> Properties;


protected:
	/** target World we will raycast into to find actors */
	UWorld* TargetWorld;
};