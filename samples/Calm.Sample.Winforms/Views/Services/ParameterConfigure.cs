namespace Calm.Sample.Winforms.Views.Services;

#pragma warning disable RCS1060 // Declare each type in separate file
#pragma warning disable MA0048 // File name must match type name

/// <summary>
/// The delegeate to configure parameters.
/// </summary>
/// <typeparam name="T">The type of parameter.</typeparam>
/// <param name="dialog">The parameter to be configured.</param>
internal delegate void ParameterConfigure<in T>(T dialog);

/// <summary>
/// The delegeate to configure parameters.
/// </summary>
/// <typeparam name="T">The type of parameter.</typeparam>
/// <param name="dialog">The parameter to be configured.</param>
internal delegate void RefParameterConfigure<T>(ref T dialog);
