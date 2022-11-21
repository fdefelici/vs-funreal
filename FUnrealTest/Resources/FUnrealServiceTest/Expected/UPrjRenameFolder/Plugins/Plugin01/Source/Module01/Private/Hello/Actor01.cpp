// Fill out your copyright notice in the Description page of Project Settings.


#include "Actor01.h"

// Sets default values
AActor01::AActor01()
{
	// Set this actor to call Tick() every frame.  You can turn this off to improve performance if you don't need it.
	PrimaryActorTick.bCanEverTick = true;

}

// Called when the game starts or when spawned
void AActor01::BeginPlay()
{
	Super::BeginPlay();

}

// Called every frame
void AActor01::Tick(float DeltaTime)
{
	Super::Tick(DeltaTime);

}

