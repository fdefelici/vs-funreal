#include "@{TPL_SOUR_INCL}@{TPL_SOUR_CLASS}.h"

// Sets default values
A@{TPL_SOUR_CLASS}::A@{TPL_SOUR_CLASS}()
{
 	// Set this pawn to call Tick() every frame.  You can turn this off to improve performance if you don't need it.
	PrimaryActorTick.bCanEverTick = true;

}

// Called when the game starts or when spawned
void A@{TPL_SOUR_CLASS}::BeginPlay()
{
	Super::BeginPlay();
	
}

// Called every frame
void A@{TPL_SOUR_CLASS}::Tick(float DeltaTime)
{
	Super::Tick(DeltaTime);

}

// Called to bind functionality to input
void A@{TPL_SOUR_CLASS}::SetupPlayerInputComponent(UInputComponent* PlayerInputComponent)
{
	Super::SetupPlayerInputComponent(PlayerInputComponent);

}

