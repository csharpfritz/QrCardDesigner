using QrScreen.Components.FritzQr.Models;

namespace QrScreen.Components.FritzQr.Rendering;

/// <summary>A single shape to be drawn at a module coordinate (1 module = 1 unit in the SVG grid).</summary>
public readonly record struct QrRenderElement(int X, int Y, ModuleShape Shape, string Color);
