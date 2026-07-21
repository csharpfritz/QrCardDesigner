namespace QrScreen.Components.FritzQr.Models;

/// <summary>Style options used to render a <see cref="QrMatrix"/> to SVG.</summary>
public sealed class FritzQrRenderOptions
{
	public string ForegroundColor { get; set; } = "#000000";

	public string BackgroundColor { get; set; } = "#FFFFFF";

	public ModuleShape DataModuleShape { get; set; } = ModuleShape.Square;

	public FinderStyles FinderStyles { get; set; } = new();

	/// <summary>Quiet zone width, in modules, rendered around the symbol.</summary>
	public int QuietZoneModules { get; set; } = 4;

	/// <summary>Optional logo to embed in the center of the code. Null means no logo.</summary>
	public LogoOptions? Logo { get; set; }
}
