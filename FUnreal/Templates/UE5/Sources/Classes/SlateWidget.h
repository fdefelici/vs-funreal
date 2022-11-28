#pragma once

#include "CoreMinimal.h"
#include "Widgets/SCompoundWidget.h"

class @{TPL_MODU_API}S@{TPL_SOUR_CLASS} : public SCompoundWidget
{
public:
	SLATE_BEGIN_ARGS(S@{TPL_SOUR_CLASS})
	{}
	SLATE_END_ARGS()

	/** Constructs this widget with InArgs */
	void Construct(const FArguments& InArgs);
};
