﻿using System.Diagnostics;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace Memoria.CodeGenerator;

internal static class ConfigurationScopeEmitter
{
    // private static readonly String GeneratedCodeAttribute = $"[global::System.CodeDom.Compiler.GeneratedCodeAttribute(\"{typeof(ConfigurationScopeEmitter).Assembly.GetName().Name}\", \"{typeof(ConfigurationScopeEmitter).Assembly.GetName().Version}\")]";
    
    public static void Emit(SourceProductionContext context, ConfigurationScopeDescriptor descriptor)
    {
        Int32 indent = 0;
        StringBuilder sb = new StringBuilder(4096);

        void AppendLine(String text)
        {
            if (text == "}") indent--;
            sb.Append('\t', indent);
            sb.AppendLine(text);
            if (text == "{") indent++;
        }

        // Comments
        AppendLine(@"// <auto-generated/>");
        AppendLine(String.Empty);

        // Usings // TODO: Resolve dependices
        AppendLine("using System;");
        AppendLine("using UnityEngine;");
        AppendLine("using BepInEx.Configuration;");
        AppendLine("using Memoria.FrontMission1.Configuration;");
        AppendLine("using Memoria.FrontMission1.Configuration.Hotkey;");
        AppendLine(String.Empty);

        // Namespace
        if (!String.IsNullOrWhiteSpace(descriptor.Namespace))
        {
            AppendLine($"namespace {descriptor.Namespace};");
            AppendLine(String.Empty);
        }
        
        // Partial Create implementantion
        if (descriptor.Modifiers.Has(SyntaxKind.PartialKeyword))
        {
            AppendLine($"{descriptor.Modifiers.ToFullString()}class {descriptor.TypeName}");
            AppendLine("{");
            AppendLine($"public static {descriptor.TypeName} Create(ConfigFileProvider provider) => new {descriptor.ImplementationTypeName}(provider);");
            AppendLine($"public static {descriptor.TypeName} Create(ConfigFile file) => new {descriptor.ImplementationTypeName}(file);");
            AppendLine("}");
            AppendLine(String.Empty);
        }

        // // Attributes
        // AppendLine($"{GeneratedCodeAttribute}");

        // Class definition
        AppendLine($"internal sealed class {descriptor.ImplementationTypeName} : {descriptor.TypeName}");
        AppendLine("{");

        // Consts
        AppendLine($"private const String SectionName = \"{descriptor.SectionName}\";");
        AppendLine(String.Empty);

        // Fields
        foreach (ConfigurationEntryDescriptor property in descriptor.Properties!)
            AppendLine($"private readonly ConfigEntry<{property.Type}> {property.BackingFieldName};");
        AppendLine(String.Empty);

        // Constructor 1
        AppendLine($"public {descriptor.ImplementationTypeName}(ConfigFileProvider provider)");
        AppendLine($"    : this(provider.GetAndCache(SectionName))");
        AppendLine("{");
        AppendLine("}");
        AppendLine(String.Empty);

        // Constructor 2
        AppendLine($"public {descriptor.ImplementationTypeName}(ConfigFile file)");
        AppendLine("{");
        foreach (ConfigurationEntryDescriptor property in descriptor.Properties)
        {
            if (property.ConverterInstance is null)
            {
                AppendLine($"{property.BackingFieldName} = file.Bind<{property.Type}>(SectionName, nameof({property.Name}), {property.DefaultValue}, \"{property.Description}\");");
            }
            else
            {
                AppendLine($"{property.BackingFieldName} = file.Bind<{property.Type}>(SectionName, nameof({property.Name}), {property.DefaultValue}, \"{property.Description}\", {property.ConverterInstance});");
            }
        }

        AppendLine("}");

        // Properties
        foreach (ConfigurationEntryDescriptor property in descriptor.Properties.OrderBy(p => p.HasSetter))
        {
            AppendLine(String.Empty);
            
            if (property.HasSetter)
            {
                AppendLine($"public override {property.Type} {property.Name}");
                AppendLine("{");
                
                if (property.ConverterInstance is null)
                {
                    if (property.Dependency is null)
                    {
                        AppendLine($"get => {property.BackingFieldName}.Value;");
                    }
                    else
                    {
                        AppendLine($"get => {property.Dependency.PropertyName} ? {property.BackingFieldName}.Value : {property.Dependency.DefaultValue};");
                    }
                    AppendLine($"set => {property.BackingFieldName}.Value = value;");
                }
                else
                {
                    if (property.Dependency is null)
                    {
                        AppendLine($"get => {property.ConverterInstance}.FromConfig({property.BackingFieldName}.Value);");
                    }
                    else
                    {
                        AppendLine($"get => {property.Dependency.PropertyName} ? {property.ConverterInstance}.FromConfig({property.BackingFieldName}.Value) : {property.Dependency.DefaultValue};");
                    }
                    AppendLine($"set => {property.BackingFieldName}.Value = {property.ConverterInstance}.ToConfig(value);");
                }

                AppendLine("}");
            }
            else
            {
                if (property.Dependency is null)
                {
                    if (property.ConverterInstance is null)
                        AppendLine($"public override {property.Type} {property.Name} => {property.BackingFieldName}.Value;");
                    else
                        AppendLine($"public override {property.Type} {property.Name} => {property.ConverterInstance}.FromConfig({property.BackingFieldName}.Value);");
                }
                else
                {
                    if (property.ConverterInstance is null)
                        AppendLine($"public override {property.Type} {property.Name} => {property.Dependency.PropertyName} ? {property.BackingFieldName}.Value : {property.Dependency.DefaultValue};");
                    else
                        AppendLine($"public override {property.Type} {property.Name} => {property.Dependency.PropertyName} ? {property.ConverterInstance}.FromConfig({property.BackingFieldName}.Value) : {property.Dependency.DefaultValue};");
                }
            }
        }
        
        // CopyFrom
        AppendLine(String.Empty);
        AppendLine($"public override void CopyFrom({descriptor.TypeName} other)");
        AppendLine("{");
        foreach (ConfigurationEntryDescriptor property in descriptor.Properties)
        {
            if (property.ConverterInstance is null)
                AppendLine($"{property.BackingFieldName}.Value = other.{property.Name};");
            else
                AppendLine($"{property.BackingFieldName}.Value = {property.ConverterInstance}.ToConfig(other.{property.Name});");
        }
        AppendLine("}");

        // End of class definition
        AppendLine("}");


        context.AddSource($"{descriptor.ImplementationTypeName}.g.cs", SourceText.From(sb.ToString(), Encoding.UTF8));
    }
}