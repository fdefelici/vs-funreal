// Copyright Epic Games, Inc. All Rights Reserved.

#include "@{TPL_MODULE_NAME}Style.h"
#include "@{TPL_MODULE_CLASS}.h"
#include "Framework/Application/SlateApplication.h"
#include "Styling/SlateStyleRegistry.h"
#include "Slate/SlateGameResources.h"
#include "Interfaces/IPluginManager.h"
#include "Styling/SlateStyleMacros.h"

#define RootToContentDir Style->RootToContentDir

TSharedPtr<FSlateStyleSet> F@{TPL_MODULE_NAME}Style::StyleInstance = nullptr;

void F@{TPL_MODULE_NAME}Style::Initialize()
{
	if (!StyleInstance.IsValid())
	{
		StyleInstance = Create();
		FSlateStyleRegistry::RegisterSlateStyle(*StyleInstance);
	}
}

void F@{TPL_MODULE_NAME}Style::Shutdown()
{
	FSlateStyleRegistry::UnRegisterSlateStyle(*StyleInstance);
	ensure(StyleInstance.IsUnique());
	StyleInstance.Reset();
}

FName F@{TPL_MODULE_NAME}Style::GetStyleSetName()
{
	static FName StyleSetName(TEXT("@{TPL_MODULE_NAME}Style"));
	return StyleSetName;
}


const FVector2D Icon16x16(16.0f, 16.0f);
const FVector2D Icon20x20(20.0f, 20.0f);

TSharedRef< FSlateStyleSet > F@{TPL_MODULE_NAME}Style::Create()
{
	TSharedRef< FSlateStyleSet > Style = MakeShareable(new FSlateStyleSet("@{TPL_MODULE_NAME}Style"));
	Style->SetContentRoot(IPluginManager::Get().FindPlugin("@{TPL_PLUGIN_NAME}")->GetBaseDir() / TEXT("Resources"));

	Style->Set("@{TPL_MODULE_NAME}.PluginAction", new IMAGE_BRUSH_SVG(TEXT("PlaceholderButtonIcon"), Icon20x20));
	return Style;
}

void F@{TPL_MODULE_NAME}Style::ReloadTextures()
{
	if (FSlateApplication::IsInitialized())
	{
		FSlateApplication::Get().GetRenderer()->ReloadTextureResources();
	}
}

const ISlateStyle& F@{TPL_MODULE_NAME}Style::Get()
{
	return *StyleInstance;
}
