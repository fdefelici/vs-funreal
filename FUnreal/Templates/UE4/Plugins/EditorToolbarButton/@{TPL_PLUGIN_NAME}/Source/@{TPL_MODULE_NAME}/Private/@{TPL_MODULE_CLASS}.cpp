// Copyright Epic Games, Inc. All Rights Reserved.

#include "@{TPL_MODULE_CLASS}.h"
#include "@{TPL_MODULE_NAME}Style.h"
#include "@{TPL_MODULE_NAME}Commands.h"
#include "Misc/MessageDialog.h"
#include "ToolMenus.h"

static const FName @{TPL_MODULE_NAME}TabName("@{TPL_MODULE_NAME}");

#define LOCTEXT_NAMESPACE "F@{TPL_MODULE_CLASS}"

void F@{TPL_MODULE_CLASS}::StartupModule()
{
	// This code will execute after your module is loaded into memory; the exact timing is specified in the .uplugin file per-module
	
	F@{TPL_MODULE_NAME}Style::Initialize();
	F@{TPL_MODULE_NAME}Style::ReloadTextures();

	F@{TPL_MODULE_NAME}Commands::Register();
	
	PluginCommands = MakeShareable(new FUICommandList);

	PluginCommands->MapAction(
		F@{TPL_MODULE_NAME}Commands::Get().PluginAction,
		FExecuteAction::CreateRaw(this, &F@{TPL_MODULE_CLASS}::PluginButtonClicked),
		FCanExecuteAction());

	UToolMenus::RegisterStartupCallback(FSimpleMulticastDelegate::FDelegate::CreateRaw(this, &F@{TPL_MODULE_CLASS}::RegisterMenus));
}

void F@{TPL_MODULE_CLASS}::ShutdownModule()
{
	// This function may be called during shutdown to clean up your module.  For modules that support dynamic reloading,
	// we call this function before unloading the module.

	UToolMenus::UnRegisterStartupCallback(this);

	UToolMenus::UnregisterOwner(this);

	F@{TPL_MODULE_NAME}Style::Shutdown();

	F@{TPL_MODULE_NAME}Commands::Unregister();
}

void F@{TPL_MODULE_CLASS}::PluginButtonClicked()
{
	// Put your "OnButtonClicked" stuff here
	FText DialogText = FText::Format(
							LOCTEXT("PluginButtonDialogText", "Add code to {0} in {1} to override this button's actions"),
							FText::FromString(TEXT("F@{TPL_MODULE_CLASS}::PluginButtonClicked()")),
							FText::FromString(TEXT("@{TPL_MODULE_NAME}.cpp"))
					   );
	FMessageDialog::Open(EAppMsgType::Ok, DialogText);
}

void F@{TPL_MODULE_CLASS}::RegisterMenus()
{
	// Owner will be used for cleanup in call to UToolMenus::UnregisterOwner
	FToolMenuOwnerScoped OwnerScoped(this);

	{
		UToolMenu* Menu = UToolMenus::Get()->ExtendMenu("LevelEditor.MainMenu.Window");
		{
			FToolMenuSection& Section = Menu->FindOrAddSection("WindowLayout");
			Section.AddMenuEntryWithCommandList(F@{TPL_MODULE_NAME}Commands::Get().PluginAction, PluginCommands);
		}
	}

	{
		UToolMenu* ToolbarMenu = UToolMenus::Get()->ExtendMenu("LevelEditor.LevelEditorToolBar");
		{
			FToolMenuSection& Section = ToolbarMenu->FindOrAddSection("Settings");
			{
				FToolMenuEntry& Entry = Section.AddEntry(FToolMenuEntry::InitToolBarButton(F@{TPL_MODULE_NAME}Commands::Get().PluginAction));
				Entry.SetCommandList(PluginCommands);
			}
		}
	}
}

#undef LOCTEXT_NAMESPACE
	
IMPLEMENT_MODULE(F@{TPL_MODULE_CLASS}, @{TPL_MODULE_NAME})