using System;

namespace Memoria.FrontMission1.Configuration;

[AttributeUsage(AttributeTargets.Property)]
public sealed class ConfigEntryAttribute : Attribute
{
    public String Description { get; }
    
    public ConfigEntryAttribute(String description)
    {
        Description = description;
    }
}