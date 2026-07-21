namespace QrScreen.Components.FritzQr.Models;

/// <summary>
/// Classifies what a given module coordinate represents within the QR symbol, so the
/// renderer can apply different shapes/colors to finder pattern markers vs. plain data modules.
/// </summary>
public enum ModuleRole
{
	/// <summary>A normal encoded data/ECC module (or an unset module within a finder pattern's separator ring).</summary>
	Data,

	/// <summary>The 1-module-thick outer ring of one of the 3 finder pattern markers.</summary>
	FinderOuter,

	/// <summary>The 3x3 center "eye" of one of the 3 finder pattern markers.</summary>
	FinderInner
}
