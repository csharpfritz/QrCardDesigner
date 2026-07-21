namespace QrScreen.Components.FritzQr.Models;

/// <summary>
/// The shape and color used for the outer ring and inner eye of a single finder pattern marker.
/// Each of the 3 corners can have its own independent <see cref="FinderCornerStyle"/>.
/// </summary>
public sealed class FinderCornerStyle
{
	public ModuleShape OuterShape { get; set; } = ModuleShape.Square;

	public string OuterColor { get; set; } = "#000000";

	public ModuleShape InnerShape { get; set; } = ModuleShape.Square;

	public string InnerColor { get; set; } = "#000000";
}
