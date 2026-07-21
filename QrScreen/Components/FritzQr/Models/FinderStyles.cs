namespace QrScreen.Components.FritzQr.Models;

/// <summary>
/// Holds the independent styling for all 3 finder pattern markers of a QR symbol.
/// </summary>
public sealed class FinderStyles
{
	public FinderCornerStyle TopLeft { get; set; } = new();

	public FinderCornerStyle TopRight { get; set; } = new();

	public FinderCornerStyle BottomLeft { get; set; } = new();

	public FinderCornerStyle Get(FinderCorner corner) => corner switch
	{
		FinderCorner.TopLeft => TopLeft,
		FinderCorner.TopRight => TopRight,
		FinderCorner.BottomLeft => BottomLeft,
		_ => throw new ArgumentOutOfRangeException(nameof(corner), corner, "Unknown finder corner.")
	};
}
