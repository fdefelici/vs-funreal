// Copyright Epic Games, Inc. All Rights Reserved.

#pragma once

#include "CoreMinimal.h"
#include "EdMode.h"

class F@{TPL_MODULE_NAME}EdMode : public FEdMode
{
public:
	const static FEditorModeID EM_@{TPL_MODULE_NAME}EdModeId;
public:
	F@{TPL_MODULE_NAME}EdMode();
	virtual ~F@{TPL_MODULE_NAME}EdMode();

	// FEdMode interface
	virtual void Enter() override;
	virtual void Exit() override;
	//virtual void Tick(FEditorViewportClient* ViewportClient, float DeltaTime) override;
	//virtual void Render(const FSceneView* View, FViewport* Viewport, FPrimitiveDrawInterface* PDI) override;
	//virtual void ActorSelectionChangeNotify() override;
	bool UsesToolkits() const override;
	// End of FEdMode interface
};
