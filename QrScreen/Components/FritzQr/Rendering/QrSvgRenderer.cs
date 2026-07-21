using System.Text;
using QrScreen.Components.FritzQr.Models;

namespace QrScreen.Components.FritzQr.Rendering;

/// <summary>
/// Converts a <see cref="QrMatrix"/> plus <see cref="FritzQrRenderOptions"/> into drawable
/// elements and, ultimately, a self-contained SVG string.
/// </summary>
public static class QrSvgRenderer
{
	/// <summary>
	/// Resolves the shape + color for every dark module, applying independent finder-corner
	/// styling where applicable. Exposed separately from <see cref="Render"/> so the resolution
	/// logic can be unit tested without parsing SVG markup.
	/// </summary>
	public static IReadOnlyList<QrRenderElement> BuildElements(QrMatrix matrix, FritzQrRenderOptions options)
	{
		ArgumentNullException.ThrowIfNull(matrix);
		ArgumentNullException.ThrowIfNull(options);

		var elements = new List<QrRenderElement>();
		int size = matrix.Size;
		LogoPlacement? logoPlacement = options.Logo is not null
			? LogoLayoutCalculator.Calculate(size, matrix.EccLevel, options.Logo)
			: null;

		for (int y = 0; y < size; y++)
		{
			for (int x = 0; x < size; x++)
			{
				if (!matrix.IsDark(x, y))
					continue;

				var role = matrix.GetRole(x, y);

				// Leave finder patterns alone; only carve the logo knockout out of the data area.
				if (role == ModuleRole.Data && logoPlacement is { } placement && OverlapsKnockout(x, y, placement))
					continue;

				var (shape, color) = ResolveStyle(role, x, y, size, options);
				elements.Add(new QrRenderElement(x, y, shape, color));
			}
		}

		return elements;
	}

	private static bool OverlapsKnockout(int x, int y, LogoPlacement placement)
	{
		// Each module occupies the unit cell [x, x+1) x [y, y+1) in matrix space.
		return x + 1 > placement.KnockoutX && x < placement.KnockoutX + placement.KnockoutSize &&
			y + 1 > placement.KnockoutY && y < placement.KnockoutY + placement.KnockoutSize;
	}

	public static string Render(QrMatrix matrix, FritzQrRenderOptions options)
	{
		var elements = BuildElements(matrix, options);
		int quiet = Math.Max(0, options.QuietZoneModules);
		int viewSize = matrix.Size + (quiet * 2);

		var sb = new StringBuilder();
		sb.Append("<svg viewBox=\"0 0 ").Append(viewSize).Append(' ').Append(viewSize)
			.Append("\" xmlns=\"http://www.w3.org/2000/svg\" shape-rendering=\"geometricPrecision\">");
		sb.Append("<rect x=\"0\" y=\"0\" width=\"").Append(viewSize).Append("\" height=\"").Append(viewSize)
			.Append("\" fill=\"").Append(ShapePathFactory.SanitizeColor(options.BackgroundColor)).Append("\" />");
		sb.Append("<g transform=\"translate(").Append(quiet).Append(',').Append(quiet).Append(")\">");

		foreach (var element in elements)
		{
			sb.Append(ShapePathFactory.CreateMarkup(element.Shape, element.X, element.Y, 1, element.Color));
		}

		if (options.Logo is not null)
		{
			AppendLogo(sb, matrix, options);
		}

		sb.Append("</g></svg>");
		return sb.ToString();
	}

	private static readonly HashSet<string> AllowedLogoMimeTypes = new(StringComparer.OrdinalIgnoreCase)
	{
		"image/png", "image/jpeg", "image/gif", "image/webp"
	};

	private static void AppendLogo(StringBuilder sb, QrMatrix matrix, FritzQrRenderOptions options)
	{
		var logo = options.Logo!;
		if (logo.ImageBytes.Length == 0)
			return;

		var placement = LogoLayoutCalculator.Calculate(matrix.Size, matrix.EccLevel, logo);
		if (placement.KnockoutSize <= 0 || placement.LogoSize <= 0)
			return;

		string knockoutColor = ShapePathFactory.SanitizeColor(logo.BackgroundColor ?? options.BackgroundColor);
		sb.Append("<rect x=\"").Append(ShapePathFactory.FormatNumber(placement.KnockoutX))
			.Append("\" y=\"").Append(ShapePathFactory.FormatNumber(placement.KnockoutY))
			.Append("\" width=\"").Append(ShapePathFactory.FormatNumber(placement.KnockoutSize))
			.Append("\" height=\"").Append(ShapePathFactory.FormatNumber(placement.KnockoutSize))
			.Append("\" fill=\"").Append(knockoutColor).Append("\" />");

		string mimeType = AllowedLogoMimeTypes.Contains(logo.MimeType) ? logo.MimeType : "image/png";
		string base64 = Convert.ToBase64String(logo.ImageBytes);
		sb.Append("<image x=\"").Append(ShapePathFactory.FormatNumber(placement.LogoX))
			.Append("\" y=\"").Append(ShapePathFactory.FormatNumber(placement.LogoY))
			.Append("\" width=\"").Append(ShapePathFactory.FormatNumber(placement.LogoSize))
			.Append("\" height=\"").Append(ShapePathFactory.FormatNumber(placement.LogoSize))
			.Append("\" preserveAspectRatio=\"xMidYMid meet\" href=\"data:").Append(mimeType).Append(";base64,").Append(base64)
			.Append("\" />");
	}

	private static (ModuleShape Shape, string Color) ResolveStyle(ModuleRole role, int x, int y, int size, FritzQrRenderOptions options)
	{
		if (role is ModuleRole.FinderOuter or ModuleRole.FinderInner)
		{
			var corner = ResolveCorner(x, y, size);
			var style = options.FinderStyles.Get(corner);
			return role == ModuleRole.FinderOuter
				? (style.OuterShape, style.OuterColor)
				: (style.InnerShape, style.InnerColor);
		}

		return (options.DataModuleShape, options.ForegroundColor);
	}

	private static FinderCorner ResolveCorner(int x, int y, int size)
	{
		bool right = x >= size - 7;
		bool bottom = y >= size - 7;

		if (bottom && !right)
			return FinderCorner.BottomLeft;
		if (right && !bottom)
			return FinderCorner.TopRight;
		return FinderCorner.TopLeft;
	}
}
