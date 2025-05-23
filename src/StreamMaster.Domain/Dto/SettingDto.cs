using StreamMaster.Domain.Configuration;

using System.Xml.Serialization;

namespace StreamMaster.Domain.Dto;

[TsInterface(AutoI = false, IncludeNamespace = false, FlattenHierarchy = true, AutoExportMethods = false)]
public class SettingDto : StreamSettings, IMapFrom<Setting>
{
    [XmlIgnore]
    public SDSettings SDSettings { get; set; } = new();

    public string Release { get; set; } = BuildInfo.Release;

    public string Version { get; set; } = BuildInfo.Version.ToString();

    public bool IsDebug { get; set; } = BuildInfo.IsDebug;
}
