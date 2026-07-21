namespace QrScreen.Components.FritzQr.Rendering;

/// <summary>Computed module-space geometry for an embedded logo and its background knockout area.</summary>
public readonly record struct LogoPlacement(double LogoX, double LogoY, double LogoSize, double KnockoutX, double KnockoutY, double KnockoutSize);
