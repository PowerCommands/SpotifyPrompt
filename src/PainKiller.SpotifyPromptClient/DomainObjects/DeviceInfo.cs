namespace PainKiller.SpotifyPromptClient.DomainObjects;
public class DeviceInfo
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool IsRestricted { get; set; }
    public int VolumePercent { get; set; }
}