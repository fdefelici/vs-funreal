#pragma once

#include "CoreMinimal.h"
#include "Widgets/SCompoundWidget.h"

class @{TPL_MODULE_API} S@{TPL_CLASS_NAME} : public SCompoundWidget
{
public:
	SLATE_BEGIN_ARGS(S@{TPL_CLASS_NAME})
	{}
	SLATE_END_ARGS()

	/** Constructs this widget with InArgs */
	void Construct(const FArguments& InArgs);
};
