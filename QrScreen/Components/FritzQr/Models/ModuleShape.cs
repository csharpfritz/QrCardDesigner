namespace QrScreen.Components.FritzQr.Models;

/// <summary>
/// The visual shape used to render an individual "on" module (or finder pattern piece)
/// of a Fritz QR code. Kept small deliberately; more shapes (pentagon, hexagon, star, etc.)
/// can be added later on top of the generic regular-polygon support in <c>ShapePathFactory</c>.
/// </summary>
public enum ModuleShape
{
	Square,
	RoundedSquare,
	Circle
}
