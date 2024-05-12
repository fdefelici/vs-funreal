#pragma once

#include "CoreMinimal.h"
#include "Styling/SlateWidgetStyle.h"
#include "Styling/SlateWidgetStyleContainerBase.h"
#include "@{TPL_SOUR_CLASS}.generated.h"

USTRUCT()
struct @{TPL_MODU_API} F@{TPL_SOUR_CLASS} : public FSlateWidgetStyle
{
	GENERATED_USTRUCT_BODY()

	F@{TPL_SOUR_CLASS}();
	virtual ~F@{TPL_SOUR_CLASS}();

	// FSlateWidgetStyle
	virtual void GetResources(TArray<const FSlateBrush*>& OutBrushes) const override;
	static const FName TypeName;
	virtual const FName GetTypeName() const override { return TypeName; };
	static const F@{TPL_SOUR_CLASS}& GetDefault();
};

/**
 */
UCLASS(hidecategories=Object, MinimalAPI)
class UMySlateWidgetStyle : public USlateWidgetStyleContainerBase
{
	GENERATED_BODY()

public:
	/** The actual data describing the widget appearance. */
	UPROPERTY(Category=Appearance, EditAnywhere, meta=(ShowOnlyInnerProperties))
	F@{TPL_SOUR_CLASS} WidgetStyle;

	virtual const struct FSlateWidgetStyle* const GetStyle() const override
	{
		return static_cast< const struct FSlateWidgetStyle* >( &WidgetStyle );
	}
};
