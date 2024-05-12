#include "@{TPL_CLASS_RELPATH}@{TPL_CLASS_NAME}.h"

F@{TPL_CLASS_NAME}::F@{TPL_CLASS_NAME}()
{
}

F@{TPL_CLASS_NAME}::~F@{TPL_CLASS_NAME}()
{
}

const FName F@{TPL_CLASS_NAME}::TypeName(TEXT("F@{TPL_CLASS_NAME}"));

const F@{TPL_CLASS_NAME}& F@{TPL_CLASS_NAME}::GetDefault()
{
	static F@{TPL_CLASS_NAME} Default;
	return Default;
}

void F@{TPL_CLASS_NAME}::GetResources(TArray<const FSlateBrush*>& OutBrushes) const
{
	// Add any brush resources here so that Slate can correctly atlas and reference them
}

