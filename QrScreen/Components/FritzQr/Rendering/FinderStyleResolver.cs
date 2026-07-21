using QrScreen.Components.FritzQr.Models;

namespace QrScreen.Components.FritzQr.Rendering;

/// <summary>
/// Resolves the effective <see cref="FinderStyles"/> to render with: an explicit, fully
/// independent per-corner style always wins; otherwise a single marker color (falling back to
/// the foreground color) is applied uniformly to all 3 finder patterns' outer ring + inner eye.
/// </summary>
public static class FinderStyleResolver
{
	public static FinderStyles Resolve(FinderStyles? explicitStyles, string? markerColor, string foregroundColor)
	{
		if (explicitStyles is not null)
			return explicitStyles;

		string color = markerColor ?? foregroundColor;
		return new FinderStyles
		{
			TopLeft = new FinderCornerStyle { OuterColor = color, InnerColor = color },
			TopRight = new FinderCornerStyle { OuterColor = color, InnerColor = color },
			BottomLeft = new FinderCornerStyle { OuterColor = color, InnerColor = color }
		};
	}
}
