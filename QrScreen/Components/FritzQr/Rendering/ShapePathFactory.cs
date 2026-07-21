using System.Globalization;
using System.Net;
using QrScreen.Components.FritzQr.Models;

namespace QrScreen.Components.FritzQr.Rendering;

/// <summary>
/// Produces the SVG markup fragment for a single module shape. Also exposes a generic
/// regular-polygon point generator, reserved for future shapes (pentagon, hexagon, star, etc.)
/// that aren't implemented yet.
/// </summary>
public static class ShapePathFactory
{
	public static string CreateMarkup(ModuleShape shape, double x, double y, double size, string color) => shape switch
	{
		ModuleShape.Circle => CreateCircle(x, y, size, color),
		ModuleShape.RoundedSquare => CreateRoundedSquare(x, y, size, color),
		_ => CreateSquare(x, y, size, color)
	};

	public static string CreateSquare(double x, double y, double size, string color)
		=> $"<rect x=\"{FormatNumber(x)}\" y=\"{FormatNumber(y)}\" width=\"{FormatNumber(size)}\" height=\"{FormatNumber(size)}\" fill=\"{SanitizeColor(color)}\" />";

	public static string CreateRoundedSquare(double x, double y, double size, string color)
	{
		double radius = size * 0.3;
		return $"<rect x=\"{FormatNumber(x)}\" y=\"{FormatNumber(y)}\" width=\"{FormatNumber(size)}\" height=\"{FormatNumber(size)}\" rx=\"{FormatNumber(radius)}\" ry=\"{FormatNumber(radius)}\" fill=\"{SanitizeColor(color)}\" />";
	}

	public static string CreateCircle(double x, double y, double size, string color)
	{
		double r = size / 2;
		double cx = x + r;
		double cy = y + r;
		return $"<circle cx=\"{FormatNumber(cx)}\" cy=\"{FormatNumber(cy)}\" r=\"{FormatNumber(r)}\" fill=\"{SanitizeColor(color)}\" />";
	}

	/// <summary>
	/// Computes the vertex points of a regular N-sided polygon centered at (cx, cy). Not wired
	/// up to a <see cref="ModuleShape"/> yet, but kept generic so pentagon/hexagon/star/etc. can
	/// be added later without new per-shape math.
	/// </summary>
	public static string GetRegularPolygonPoints(double cx, double cy, double radius, int sides, double rotationDegrees = -90)
	{
		if (sides < 3)
			throw new ArgumentOutOfRangeException(nameof(sides), sides, "A polygon requires at least 3 sides.");

		var points = new string[sides];
		double rotationRadians = rotationDegrees * Math.PI / 180.0;
		for (int i = 0; i < sides; i++)
		{
			double angle = rotationRadians + (2 * Math.PI * i / sides);
			double px = cx + (radius * Math.Cos(angle));
			double py = cy + (radius * Math.Sin(angle));
			points[i] = $"{FormatNumber(px)},{FormatNumber(py)}";
		}

		return string.Join(" ", points);
	}

	/// <summary>
	/// HTML-encodes a color value before it is embedded in an SVG attribute. Colors can originate
	/// from user input (color pickers / bound properties); encoding prevents attribute-breakout
	/// markup injection when the generated SVG is rendered via MarkupString.
	/// </summary>
	public static string SanitizeColor(string? color) => WebUtility.HtmlEncode(color ?? string.Empty);

	/// <summary>Formats a coordinate/length value for embedding in SVG markup. Shared with <see cref="QrSvgRenderer"/>.</summary>
	internal static string FormatNumber(double value) => value.ToString("0.####", CultureInfo.InvariantCulture);
}
