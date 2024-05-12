#pragma once

#include "CoreMinimal.h"
#include "Components/ActorComponent.h"
#include "@{TPL_CLASS_NAME}.generated.h"


UCLASS(ClassGroup = (Custom), meta = (BlueprintSpawnableComponent))
class @{TPL_MODULE_API} U@{TPL_CLASS_NAME} : public UActorComponent
{
	GENERATED_BODY()

public:
	// Sets default values for this component's properties
	U@{TPL_CLASS_NAME}();

protected:
	// Called when the game starts
	virtual void BeginPlay() override;

public:
	// Called every frame
	virtual void TickComponent(float DeltaTime, ELevelTick TickType, FActorComponentTickFunction* ThisTickFunction) override;


};
