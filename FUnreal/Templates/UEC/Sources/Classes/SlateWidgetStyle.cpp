#include "@{TPL_SOUR_INCL}@{TPL_SOUR_CLASS}.h"

F@{TPL_SOUR_CLASS}::F@{TPL_SOUR_CLASS}()
{
}

F@{TPL_SOUR_CLASS}::~F@{TPL_SOUR_CLASS}()
{
}

const FName F@{TPL_SOUR_CLASS}::TypeName(TEXT("F@{TPL_SOUR_CLASS}"));

const F@{TPL_SOUR_CLASS}& F@{TPL_SOUR_CLASS}::GetDefault()
{
	static F@{TPL_SOUR_CLASS} Default;
	return Default;
}

void F@{TPL_SOUR_CLASS}::GetResources(TArray<const FSlateBrush*>& OutBrushes) const
{
	// Add any brush resources here so that Slate can correctly atlas and reference them
}

