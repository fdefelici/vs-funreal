#pragma once

#include "CoreMinimal.h"
#include "Styling/SlateWidgetStyle.h"
#include "Styling/SlateWidgetStyleContainerBase.h"
#include "@{TPL_CLASS_NAME}.generated.h"

USTRUCT()
struct @{TPL_MODULE_API} F@{TPL_CLASS_NAME} : public FSlateWidgetStyle
{
	GENERATED_USTRUCT_BODY()

	F@{TPL_CLASS_NAME}();
	virtual ~F@{TPL_CLASS_NAME}();

	// FSlateWidgetStyle
	virtual void GetResources(TArray<const FSlateBrush*>& OutBrushes) const override;
	static const FName TypeName;
	virtual const FName GetTypeName() const override { return TypeName; };
	static const F@{TPL_CLASS_NAME}& GetDefault();
};

/**
 */
UCLASS(hidecategories=Object, MinimalAPI)
class U@{TPL_CLASS_NAME} : public USlateWidgetStyleContainerBase
{
	GENERATED_BODY()

public:
	/** The actual data describing the widget appearance. */
	UPROPERTY(Category=Appearance, EditAnywhere, meta=(ShowOnlyInnerProperties))
	F@{TPL_CLASS_NAME} WidgetStyle;

	virtual const struct FSlateWidgetStyle* const GetStyle() const override
	{
		return static_cast< const struct FSlateWidgetStyle* >( &WidgetStyle );
	}
};
