﻿#include "FPropertyExporter.h"

void UFPropertyExporter::ExportFunctions(FRegisterExportedFunction RegisterExportedFunction)
{
	EXPORT_FUNCTION(GetNativePropertyFromName)
	EXPORT_FUNCTION(GetPropertyOffsetFromName)
	EXPORT_FUNCTION(GetPropertyArrayDimFromName)
	EXPORT_FUNCTION(GetPropertyOffset)
	EXPORT_FUNCTION(GetSize)
	EXPORT_FUNCTION(GetArrayDim)
	EXPORT_FUNCTION(DestroyValue)
	EXPORT_FUNCTION(InitializeValue)
	EXPORT_FUNCTION(GetInnerFields)
	EXPORT_FUNCTION(Identical)
	EXPORT_FUNCTION(GetValueTypeHash)
	EXPORT_FUNCTION(HasAnyPropertyFlags)
	EXPORT_FUNCTION(CopySingleValue)
	EXPORT_FUNCTION(DestroyValue_InContainer)
	EXPORT_FUNCTION(HasAllPropertyFlags)
	EXPORT_FUNCTION(GetBoolPropertyFieldMaskFromName)
}

FProperty* UFPropertyExporter::GetNativePropertyFromName(UStruct* Struct, const char* PropertyName)
{
	FProperty* Property = FindFProperty<FProperty>(Struct, PropertyName);
	return Property;
}

int32 UFPropertyExporter::GetPropertyOffset(FProperty* Property)
{
	return Property->GetOffset_ForInternal();
}

int32 UFPropertyExporter::GetSize(FProperty* Property)
{
	return Property->GetSize();
}

int32 UFPropertyExporter::GetArrayDim(FProperty* Property)
{
	return Property->ArrayDim;
}

void UFPropertyExporter::DestroyValue(FProperty* Property, void* Value)
{
	Property->DestroyValue(Value);
}

void UFPropertyExporter::DestroyValue_InContainer(FProperty* Property, void* Value)
{
	Property->DestroyValue_InContainer(Value);
}

void UFPropertyExporter::InitializeValue(FProperty* Property, void* Value)
{
	Property->InitializeValue(Value);
}

bool UFPropertyExporter::Identical(const FProperty* Property, void* ValueA, void* ValueB)
{
	bool bIsIdentical = Property->Identical(ValueA, ValueB);
	return bIsIdentical;
}

void UFPropertyExporter::GetInnerFields(FProperty* SetProperty, TArray<FField*>* OutFields)
{
	SetProperty->GetInnerFields(*OutFields);
}

uint32 UFPropertyExporter::GetValueTypeHash(FProperty* Property, void* Source)
{
	return Property->GetValueTypeHash(Source);
}

bool UFPropertyExporter::HasAnyPropertyFlags(FProperty* Property, EPropertyFlags FlagsToCheck)
{
	return Property->HasAnyPropertyFlags(FlagsToCheck);
}

bool UFPropertyExporter::HasAllPropertyFlags(FProperty* Property, EPropertyFlags FlagsToCheck)
{
	return Property->HasAllPropertyFlags(FlagsToCheck);
}

void UFPropertyExporter::CopySingleValue(FProperty* Property, void* Dest, void* Src)
{
	Property->CopySingleValue(Dest, Src);
}

uint8 UFPropertyExporter::GetBoolPropertyFieldMaskFromName(UStruct* InStruct, const char* InPropertyName)
{
	FBoolProperty* Property = FindFProperty<FBoolProperty>(InStruct, InPropertyName);
	if (!Property)
	{
		return 0;
	}

	return Property->GetFieldMask();
}

int32 UFPropertyExporter::GetPropertyOffsetFromName(UStruct* InStruct, const char* InPropertyName)
{
	FProperty* FoundProperty = GetNativePropertyFromName(InStruct, InPropertyName);
	if (!FoundProperty)
	{
		return -1;
	}
	
	return GetPropertyOffset(FoundProperty);
}

int32 UFPropertyExporter::GetPropertyArrayDimFromName(UStruct* InStruct, const char* PropertyName)
{
	FProperty* Property = GetNativePropertyFromName(InStruct, PropertyName);
	return GetArrayDim(Property);
}
