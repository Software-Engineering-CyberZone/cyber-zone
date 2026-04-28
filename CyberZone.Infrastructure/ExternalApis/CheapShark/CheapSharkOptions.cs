namespace CyberZone.Infrastructure.ExternalApis.CheapShark;

/// <summary>
/// Options for the CheapShark REST API integration.
/// See <a href="https://apidocs.cheapshark.com">apidocs.cheapshark.com</a>.
/// </summary>
public class CheapSharkOptions
{
    public const string SectionName = "ExternalApis:CheapShark";

    public string BaseUrl { get; set; } = "https://www.cheapshark.com";
    public int TimeoutSeconds { get; set; } = 5;
}
