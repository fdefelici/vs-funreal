// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "GameFramework/Actor.h"
#include "Actor01Ren.h"
#include "From/ElseWhere/Actor01.h"   //Not exists on filesystem. Just for testing purpose. This include should stay untouched because is not the same "Actor01.h"
#include "Actor02.generated.h"

UCLASS()
class MODULE02_API AActor02 : public AActor
{
	GENERATED_BODY()

public:
	// Sets default values for this actor's properties
	AActor02();

protected:
	// Called when the game starts or when spawned
	virtual void BeginPlay() override;

public:
	// Called every frame
	virtual void Tick(float DeltaTime) override;
};
