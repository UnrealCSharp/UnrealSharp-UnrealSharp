﻿using System;
using System.Collections.Generic;
using EpicGames.Core;
using EpicGames.UHT.Types;
using UnrealSharpScriptGenerator.PropertyTranslators;

namespace UnrealSharpScriptGenerator.Utilities;

public enum ENameType
{
    Parameter,
    Property,
    Struct,
    Function
}

public static class NameMapper
{
    private static readonly List<string> ReservedKeywords = new()
    {
        "abstract", "as", "base", "bool", "break", "byte", "case", "catch", "char", "checked", "class", "const", "continue",
        "decimal", "default", "delegate", "do", "double", "else", "enum", "event", "explicit", "extern", "false", "finally", 
        "fixed", "float", "for", "foreach", "goto", "if", "implicit", "in", "int", "interface", "internal", "is", "lock",
        "long", "namespace", "new", "null", "object", "operator", "out", "override", "params", "private", "protected", "public", 
        "readonly", "ref", "return", "sbyte", "sealed", "short", "sizeof", "stackalloc", "static", "string", "struct", "switch", 
        "this", "throw", "true", "try", "typeof", "uint", "ulong", "unchecked", "unsafe", "ushort", "using", "virtual",
        "void", "volatile", "while", "System"
    };

    public static string GetParameterName(this UhtProperty property)
    {
        string scriptName = ScriptifyName(property.GetScriptName(), ENameType.Parameter);

        if (property.Outer is not UhtFunction function)
        {
            return scriptName;
        }
        
        foreach (UhtProperty exportedProperty in function.Properties)
        {
            if (exportedProperty != property && scriptName == ScriptifyName(exportedProperty.GetScriptName(), ENameType.Parameter))
            {
                return PascalToCamelCase(exportedProperty.SourceName);
            }
        }
        
        return scriptName;
    }
    
    public static string GetPropertyName(this UhtProperty property)
    {
        string propertyName = ScriptifyName(property.GetScriptName(), ENameType.Property);
        if (property.Outer!.SourceName == propertyName || IsAKeyword(propertyName))
        {
            propertyName = $"K2_{propertyName}";
        }
        return TryResolveConflictingName(property, propertyName);
    }
    
    public static string GetStructName(this UhtType type)
    {
        if (type.EngineType is UhtEngineType.Interface or UhtEngineType.NativeInterface || type == Program.Factory.Session.UInterface)
        {
            return "I" + type.EngineName;
        }
        
        if (type is UhtClass uhtClass && uhtClass.IsChildOf(Program.BlueprintFunctionLibrary))
        {
            return type.GetScriptName();
        }

        return type.SourceName;
    }
    
    public static string GetFullManagedName(this UhtType type)
    {
        return $"{type.GetNamespace()}.{type.GetStructName()}";
    }
    
    static readonly string[] MetadataKeys = { "ScriptName", "ScriptMethod", "DisplayName" };
    public static string GetScriptName(this UhtType type)
    {
        bool OnlyContainsLetters(string str)
        {
            foreach (char c in str)
            {
                if (!char.IsLetter(c) && !char.IsWhiteSpace(c))
                {
                    return false;
                }
            }
            return true;
        }
        
        foreach (var key in MetadataKeys)
        {
            string value = type.GetMetadata(key);
            
            if (string.IsNullOrEmpty(value) || !OnlyContainsLetters(value))
            {
                continue;
            }
            
            // Try remove whitespace from the value
            value = value.Replace(" ", "");
            return value;
        }
        
        return type.SourceName;
    }
    
    public static string GetNamespace(this UhtType typeObj)
    {
        UhtType outer = typeObj;

        string packageShortName = "";
        if (outer is UhtPackage package)
        {
            packageShortName = package.ShortName;
        }
        else
        {
            while (outer.Outer != null)
            {
                outer = outer.Outer;
            
                if (outer is UhtHeaderFile header)
                {
                    packageShortName = header.Package.ShortName;
                }
            }
        }
        
        if (string.IsNullOrEmpty(packageShortName))
        {
            throw new Exception($"Failed to find package name for {typeObj}");
        }
        
        return $"UnrealSharp.{packageShortName}";
    }
    
    public static string GetFunctionName(this UhtFunction function)
    {
        string functionName = function.GetScriptName();

        if (function.HasAnyFlags(EFunctionFlags.Delegate | EFunctionFlags.MulticastDelegate))
        {
            functionName = DelegateBasePropertyTranslator.GetDelegateName(function);
        }
        
        if (functionName.Contains("K2_"))
        {
            functionName = functionName.Replace("K2_", "");
        }

        if (function.Outer is not UhtClass)
        {
            return functionName;
        }
        
        functionName = TryResolveConflictingName(function, functionName);

        return functionName;
    }
    
    public static string TryResolveConflictingName(UhtType type, string scriptName)
    {
        UhtType outer = type.Outer!;
        bool isConflicting = false;
        foreach (UhtType child in outer.Children)
        {
            if (child == type)
            {
                continue;
            }
            
            if (child is UhtProperty property)
            {
                if (scriptName == ScriptifyName(property.GetScriptName(), ENameType.Property))
                {
                    isConflicting = true;
                    break;
                }
            }
            
            if (child is UhtFunction function)
            {
                if (scriptName == ScriptifyName(function.GetScriptName(), ENameType.Function))
                {
                    isConflicting = true;
                    break;
                }
            }
        }
        
        return isConflicting ? type.EngineName : scriptName;
    }

    public static string ScriptifyName(string engineName, ENameType nameType)
    {
        string strippedName = engineName;
        switch (nameType)
        {
            case ENameType.Parameter:
                strippedName = StripPropertyPrefix(strippedName);
                strippedName = PascalToCamelCase(strippedName);
                break;
            case ENameType.Property:
                strippedName = StripPropertyPrefix(strippedName);
                break;
            case ENameType.Struct:
                break;
            case ENameType.Function:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(nameType), nameType, null);
        }
        
        return EscapeKeywords(strippedName);
    }
    
    public static string StripPropertyPrefix(string inName)
    {
        int nameOffset = 0;

        while (true)
        {
            // Strip the "b" prefix from bool names
            if (inName.Length - nameOffset >= 2 && inName[nameOffset] == 'b' && char.IsUpper(inName[nameOffset + 1]))
            {
                nameOffset += 1;
                
                continue;
            }

            // Strip the "In" prefix from names
            if (inName.Length - nameOffset >= 3 && inName[nameOffset] == 'I' && inName[nameOffset + 1] == 'n' && char.IsUpper(inName[nameOffset + 2]))
            {
                nameOffset += 2;
                continue;
            }
            break;
        }

        return nameOffset != 0 ? inName.Substring(nameOffset) : inName;
    }
    
    public static string EscapeKeywords(string name)
    {
        return IsAKeyword(name) ? $"_{name}" : name;
    }
    
    private static bool IsAKeyword(string name)
    {
        return ReservedKeywords.Contains(name);
    }
    
    private static string PascalToCamelCase(string name)
    {
        return char.ToLowerInvariant(name[0]) + name.Substring(1);
    }
}