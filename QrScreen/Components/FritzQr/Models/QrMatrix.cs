using QRCoder;

namespace QrScreen.Components.FritzQr.Models;

/// <summary>
/// A decoded QR symbol's module grid (quiet-zone stripped) plus a per-module role
/// classification that the renderer uses to pick shapes/colors.
/// </summary>
public sealed class QrMatrix
{
	public QrMatrix(int size, int version, QRCodeGenerator.ECCLevel eccLevel, bool[,] modules, ModuleRole[,] roles)
	{
		if (size <= 0)
			throw new ArgumentOutOfRangeException(nameof(size), size, "Size must be positive.");
		if (modules.GetLength(0) != size || modules.GetLength(1) != size)
			throw new ArgumentException("Module matrix dimensions must match size.", nameof(modules));
		if (roles.GetLength(0) != size || roles.GetLength(1) != size)
			throw new ArgumentException("Role matrix dimensions must match size.", nameof(roles));

		Size = size;
		Version = version;
		EccLevel = eccLevel;
		Modules = modules;
		Roles = roles;
	}

	/// <summary>Module count on one side of the symbol, not including any quiet zone.</summary>
	public int Size { get; }

	public int Version { get; }

	/// <summary>
	/// The error correction level used to encode this symbol. Needed to determine how much of the
	/// symbol can safely be covered by an embedded logo.
	/// </summary>
	public QRCodeGenerator.ECCLevel EccLevel { get; }

	public bool[,] Modules { get; }

	public ModuleRole[,] Roles { get; }

	public bool IsDark(int x, int y) => Modules[x, y];

	public ModuleRole GetRole(int x, int y) => Roles[x, y];
}
