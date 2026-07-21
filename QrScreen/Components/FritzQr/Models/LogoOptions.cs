namespace QrScreen.Components.FritzQr.Models;

/// <summary>
/// Describes a logo image to embed in the center of a Fritz QR code.
/// </summary>
public sealed class LogoOptions
{
	public required byte[] ImageBytes { get; init; }

	/// <summary>MIME type of <see cref="ImageBytes"/> (e.g. "image/png"). Falls back to a safe default if unrecognized.</summary>
	public string MimeType { get; init; } = "image/png";

	/// <summary>
	/// Requested logo size as a percentage of the QR symbol's width (not including quiet zone).
	/// Actual size is clamped to a safe maximum based on the symbol's error correction level.
	/// </summary>
	public double SizePercent { get; init; } = 20;

	/// <summary>Padding between the logo image and the edge of its background knockout area, as a percentage of the knockout's size.</summary>
	public double PaddingPercent { get; init; } = 10;

	/// <summary>Color of the knockout area behind the logo. Defaults to the code's background color when null.</summary>
	public string? BackgroundColor { get; init; }
}
