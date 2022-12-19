#include "@{TPL_SOUR_INCL}@{TPL_SOUR_CLASS}.h"

// Sets default values for this component's properties
U@{TPL_SOUR_CLASS}::U@{TPL_SOUR_CLASS}()
{
	// Set this component to be initialized when the game starts, and to be ticked every frame.  You can turn these features
	// off to improve performance if you don't need them.
	PrimaryComponentTick.bCanEverTick = true;

	// ...
}


// Called when the game starts
void U@{TPL_SOUR_CLASS}()::BeginPlay()
{
	Super::BeginPlay();

	// ...
	
}


// Called every frame
void U@{TPL_SOUR_CLASS}()::TickComponent(float DeltaTime, ELevelTick TickType, FActorComponentTickFunction* ThisTickFunction)
{
	Super::TickComponent(DeltaTime, TickType, ThisTickFunction);

	// ...
}

