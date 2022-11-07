#pragma once

#include "CoreMinimal.h"
#include "GameFramework/Actor.h"
#include "@{TPL_CLS_NAME}.generated.h"

UCLASS()
class @{TPL_MOD_API}A@{TPL_CLS_NAME} : public AActor
{
	GENERATED_BODY()

public:
	// Sets default values for this actor's properties
	A@{TPL_CLS_NAME}();

protected:
	// Called when the game starts or when spawned
	virtual void BeginPlay() override;

public:
	// Called every frame
	virtual void Tick(float DeltaTime) override;
};
