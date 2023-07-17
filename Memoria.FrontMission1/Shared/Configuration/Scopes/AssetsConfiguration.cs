﻿using System;

namespace Memoria.FrontMission1.Configuration;

[ConfigScope("Assets")]
public abstract partial class AssetsConfiguration
{
    // [ConfigEntry($"Export the supported resources to the {nameof(ExportDirectory)}.")]
    // public virtual Boolean ExportEnabled { get; set; } = false;
    //
    // [ConfigEntry($"Directory into which the supported resources will be exported.")]
    // [ConfigConverter(nameof(ExportDirectoryConverter))]
    // public virtual String ExportDirectory => "%StreamingAssets%";
    //
    // [ConfigEntry($"Export text resources: .txt, .csv, .json, etc.")]
    // public virtual Boolean ExportText => true;

    // [ConfigEntry($"Import the supported resources from the {nameof(ImportDirectory)}.")]
    // public virtual Boolean ImportEnabled => true;
    //
    // [ConfigEntry($"Directory from which the supported resources will be imported.")]
    // [ConfigConverter(nameof(ImportDirectoryConverter))]
    // public virtual String ImportDirectory => "%StreamingAssets%";
    //
    // [ConfigEntry($"Import text resources: .txt, .csv, .json, etc.")]
    // public virtual Boolean ImportText => true;

    [ConfigEntry($"Overwrite the supported resources from the {nameof(ModsDirectory)}.")]
    public virtual Boolean ModsEnabled => true;

    [ConfigEntry($"Directory from which the supported resources will be updated.")]
    [ConfigConverter(nameof(ModsDirectoryConverter))]
    [ConfigDependency(nameof(ModsEnabled), "String.Empty")]
    public virtual String ModsDirectory => "%StreamingAssets%/Mods";

    // protected IAcceptableValue<String> ExportDirectoryConverter { get; } = new AcceptableDirectoryPath(nameof(ExportDirectory));
    // protected IAcceptableValue<String> ImportDirectoryConverter { get; } = new AcceptableDirectoryPath(nameof(ImportDirectory));
    protected IAcceptableValue<String> ModsDirectoryConverter { get; } = new AcceptableDirectoryPath(nameof(ModsDirectory), create: true);

    public abstract void CopyFrom(AssetsConfiguration configuration);

    // public String GetExportDirectoryIfEnabled() => ExportEnabled ? ExportDirectory : String.Empty;
    // public String GetImportDirectoryIfEnabled() => ImportEnabled ? ImportDirectory : String.Empty;
}