#if !NET5_0_OR_GREATER
using System.ComponentModel;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace System.Runtime.CompilerServices;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// Reserved to be used by the compiler for tracking metadata.
/// This class should not be used by developers in source code.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
#pragma warning disable MA0182 // Avoid unused internal types
internal static class IsExternalInit
#pragma warning restore MA0182 // Avoid unused internal types
{
}
#endif
