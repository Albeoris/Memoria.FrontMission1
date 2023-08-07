using System;

namespace Memoria.FrontMission1.Configuration;

[ConfigScope("Saves")]
public abstract partial class SavesConfiguration
{
    [ConfigEntry("If enabled, each save will be duplicated under a separate name. WARNING: these files will take up space and will not be deleted automatically! If you want to load from them, you need to rename the save file, replacing one of the existing ones: slot_0, slot_1, etc." +
                 "$[Russian]: При включении, каждый файл сохранения будет дублироваться с отдельным названием. ВНИМАНИЕ: эти файлы будут занимать место и не будут удаляться автоматически! Если вы захотите загрузиться из них, вам понадобится переименовать файл сохранения, заменив один из уже существующих: slot_0, slot_1, и т.д.")]
    public virtual Boolean BackupSaveFiles => false;

    public abstract void CopyFrom(SavesConfiguration configuration);
}