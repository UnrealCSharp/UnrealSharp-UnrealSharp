﻿#include "GEngineExporter.h"
#include "Engine/Engine.h"
#include "UnrealSharpCore/CSManager.h"

void UGEngineExporter::ExportFunctions(FRegisterExportedFunction RegisterExportedFunction)
{
	EXPORT_FUNCTION(GetEngineSubsystem)
}

void* UGEngineExporter::GetEngineSubsystem(UClass* SubsystemClass)
{
	UEngineSubsystem* GameInstanceSubsystem = GEngine->GetEngineSubsystemBase(SubsystemClass);
	return UCSManager::Get().FindManagedObject(GameInstanceSubsystem).GetIntPtr();
}