#include "@{TPL_INC_PATH}@{TPL_CLS_NAME}.h"

// Sets default values for this component's properties
U@{TPL_CLS_NAME}::U@{TPL_CLS_NAME}()
{
	// Set this component to be initialized when the game starts, and to be ticked every frame.  You can turn these features
	// off to improve performance if you don't need them.
	PrimaryComponentTick.bCanEverTick = true;

	// ...
}

// Called when the game starts
void U@{TPL_CLS_NAME}::BeginPlay()
{
	Super::BeginPlay();

	// ...

}

// Called every frame
void U@{TPL_CLS_NAME}::TickComponent(float DeltaTime, ELevelTick TickType, FActorComponentTickFunction* ThisTickFunction)
{
	Super::TickComponent(DeltaTime, TickType, ThisTickFunction);

	// ...
}
