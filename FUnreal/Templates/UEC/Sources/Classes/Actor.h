#pragma once

#include "CoreMinimal.h"
#include "GameFramework/Actor.h"
#include "@{TPL_SOUR_CLASS}.generated.h"

UCLASS()
class @{TPL_MODU_API}A@{TPL_SOUR_CLASS} : public AActor
{
	GENERATED_BODY()

public:
	// Sets default values for this actor's properties
	A@{TPL_SOUR_CLASS}();

protected:
	// Called when the game starts or when spawned
	virtual void BeginPlay() override;

public:
	// Called every frame
	virtual void Tick(float DeltaTime) override;
};
