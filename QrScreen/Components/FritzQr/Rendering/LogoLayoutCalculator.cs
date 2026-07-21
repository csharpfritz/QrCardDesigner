using QRCoder;
using QrScreen.Components.FritzQr.Models;

namespace QrScreen.Components.FritzQr.Rendering;

/// <summary>
/// Computes where a logo and its background knockout should sit in module-space, clamping the
/// requested size to a safe maximum based on error correction level so the code stays scannable.
/// This is a heuristic, not a guarantee - very "busy" payloads/shapes can still reduce
/// scannability and should be spot-checked.
/// </summary>
public static class LogoLayoutCalculator
{
	private static readonly IReadOnlyDictionary<QRCodeGenerator.ECCLevel, double> MaxSizePercentByEcc = new Dictionary<QRCodeGenerator.ECCLevel, double>
	{
		[QRCodeGenerator.ECCLevel.L] = 15,
		[QRCodeGenerator.ECCLevel.M] = 20,
		[QRCodeGenerator.ECCLevel.Q] = 25,
		[QRCodeGenerator.ECCLevel.H] = 30,
	};

	public static double GetMaxSizePercent(QRCodeGenerator.ECCLevel eccLevel) =>
		MaxSizePercentByEcc.TryGetValue(eccLevel, out var max) ? max : 15;

	public static LogoPlacement Calculate(int matrixSize, QRCodeGenerator.ECCLevel eccLevel, LogoOptions options)
	{
		double maxPercent = GetMaxSizePercent(eccLevel);
		double clampedPercent = Math.Clamp(options.SizePercent, 0, maxPercent);

		double knockoutSize = matrixSize * clampedPercent / 100.0;
		double padding = knockoutSize * Math.Clamp(options.PaddingPercent, 0, 50) / 100.0;
		double logoSize = Math.Max(0, knockoutSize - (padding * 2));

		double center = matrixSize / 2.0;
		double knockoutOrigin = center - (knockoutSize / 2.0);
		double logoOrigin = center - (logoSize / 2.0);

		return new LogoPlacement(logoOrigin, logoOrigin, logoSize, knockoutOrigin, knockoutOrigin, knockoutSize);
	}
}
