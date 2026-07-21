using System.Globalization;

namespace QrScreen.Components.FritzQr.Rendering;

/// <summary>
/// Parses CSS-style colors (hex or <c>rgb()</c>/<c>rgba()</c>) and computes WCAG-style relative
/// contrast ratios between them, so callers can warn when two colors are too close to reliably
/// distinguish (e.g. a QR code's background vs. its foreground/finder marker colors).
/// </summary>
public static class ColorContrastValidator
{
	/// <summary>A reasonable general-purpose minimum contrast ratio for QR code scannability.</summary>
	public const double DefaultMinimumRatio = 3.0;

	/// <summary>Computes the WCAG relative contrast ratio between two colors, from 1 (identical) to 21 (black vs. white).</summary>
	public static double CalculateContrastRatio(string colorA, string colorB)
	{
		var (r1, g1, b1) = ParseColor(colorA);
		var (r2, g2, b2) = ParseColor(colorB);

		double l1 = RelativeLuminance(r1, g1, b1);
		double l2 = RelativeLuminance(r2, g2, b2);

		double lighter = Math.Max(l1, l2);
		double darker = Math.Min(l1, l2);

		return (lighter + 0.05) / (darker + 0.05);
	}

	/// <summary>True when the contrast ratio between the two colors meets or exceeds <paramref name="minimumRatio"/>.</summary>
	public static bool HasSufficientContrast(string colorA, string colorB, double minimumRatio = DefaultMinimumRatio)
		=> CalculateContrastRatio(colorA, colorB) >= minimumRatio;

	/// <summary>Parses a CSS-style hex (<c>#RGB</c>, <c>#RRGGBB</c>, <c>#RRGGBBAA</c>) or <c>rgb()</c>/<c>rgba()</c> color string.</summary>
	public static (byte R, byte G, byte B) ParseColor(string color)
	{
		if (string.IsNullOrWhiteSpace(color))
			throw new ArgumentException("Color must not be null or empty.", nameof(color));

		string trimmed = color.Trim();

		if (trimmed.StartsWith('#'))
			return ParseHex(trimmed);

		if (trimmed.StartsWith("rgb", StringComparison.OrdinalIgnoreCase))
			return ParseRgbFunction(trimmed);

		throw new FormatException($"Unsupported color format: '{color}'.");
	}

	private static double RelativeLuminance(byte r, byte g, byte b)
		=> 0.2126 * ToLinear(r) + 0.7152 * ToLinear(g) + 0.0722 * ToLinear(b);

	private static double ToLinear(byte channel)
	{
		double c = channel / 255.0;
		return c <= 0.03928 ? c / 12.92 : Math.Pow((c + 0.055) / 1.055, 2.4);
	}

	private static (byte R, byte G, byte B) ParseHex(string hex)
	{
		string h = hex.TrimStart('#');

		if (h.Length == 3 || h.Length == 4)
			h = string.Concat(h.Select(c => new string(c, 2)));

		if (h.Length is not (6 or 8) || !uint.TryParse(h[..6], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out _))
			throw new FormatException($"Unsupported hex color: '{hex}'.");

		byte r = byte.Parse(h.AsSpan(0, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
		byte g = byte.Parse(h.AsSpan(2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
		byte b = byte.Parse(h.AsSpan(4, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
		return (r, g, b);
	}

	private static (byte R, byte G, byte B) ParseRgbFunction(string rgb)
	{
		int start = rgb.IndexOf('(');
		int end = rgb.IndexOf(')');

		if (start < 0 || end < 0 || end <= start)
			throw new FormatException($"Unsupported color format: '{rgb}'.");

		string[] parts = rgb[(start + 1)..end]
			.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

		if (parts.Length < 3)
			throw new FormatException($"Unsupported color format: '{rgb}'.");

		return (ParseChannel(parts[0]), ParseChannel(parts[1]), ParseChannel(parts[2]));
	}

	private static byte ParseChannel(string value)
	{
		value = value.Trim();
		bool isPercent = value.EndsWith('%');
		if (isPercent)
			value = value[..^1];

		if (!double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out double parsed))
			throw new FormatException($"Unsupported color channel value: '{value}'.");

		double normalized = isPercent ? parsed / 100.0 * 255.0 : parsed;
		return (byte)Math.Clamp(Math.Round(normalized), 0, 255);
	}
}
