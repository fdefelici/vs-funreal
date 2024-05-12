#include "@{TPL_CLASS_RELPATH}@{TPL_CLASS_NAME}.h"

// Sets default values
A@{TPL_CLASS_NAME}::A@{TPL_CLASS_NAME}()
{
	// Set this actor to call Tick() every frame.  You can turn this off to improve performance if you don't need it.
	PrimaryActorTick.bCanEverTick = true;

}

// Called when the game starts or when spawned
void A@{TPL_CLASS_NAME}::BeginPlay()
{
	Super::BeginPlay();

}

// Called every frame
void A@{TPL_CLASS_NAME}::Tick(float DeltaTime)
{
	Super::Tick(DeltaTime);

}
