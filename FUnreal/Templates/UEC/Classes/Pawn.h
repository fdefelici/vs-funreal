#pragma once

#include "CoreMinimal.h"
#include "GameFramework/Pawn.h"
#include "@{TPL_CLASS_NAME}.generated.h"

UCLASS()
class @{TPL_MODULE_API} A@{TPL_CLASS_NAME} : public APawn
{
	GENERATED_BODY()

public:
	// Sets default values for this pawn's properties
	A@{TPL_CLASS_NAME}();

protected:
	// Called when the game starts or when spawned
	virtual void BeginPlay() override;

public:	
	// Called every frame
	virtual void Tick(float DeltaTime) override;

	// Called to bind functionality to input
	virtual void SetupPlayerInputComponent(class UInputComponent* PlayerInputComponent) override;

};
