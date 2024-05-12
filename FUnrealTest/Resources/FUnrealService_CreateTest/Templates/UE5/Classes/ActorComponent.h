#pragma once

#include "CoreMinimal.h"
#include "Components/ActorComponent.h"
#include "@{TPL_SOUR_CLASS}.generated.h"


UCLASS(ClassGroup = (Custom), meta = (BlueprintSpawnableComponent))
class @{TPL_MODU_API} U@{TPL_SOUR_CLASS} : public UActorComponent
{
	GENERATED_BODY()

public:
	// Sets default values for this component's properties
	U@{TPL_SOUR_CLASS}();

protected:
	// Called when the game starts
	virtual void BeginPlay() override;

public:
	// Called every frame
	virtual void TickComponent(float DeltaTime, ELevelTick TickType, FActorComponentTickFunction* ThisTickFunction) override;


};
