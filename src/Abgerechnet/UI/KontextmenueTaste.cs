using System.Runtime.InteropServices;

namespace Abgerechnet.UI;

/// <summary>
/// Shift+F10 opens the context menu of the focused control, as everywhere in Windows. Without this filter the menu
/// bar of the main window takes F10 before the control sees it. Controls without a ContextMenuStrip (e.g. text
/// boxes with their own Windows menu) are left alone.
/// </summary>
internal sealed class KontextmenueTaste : IMessageFilter
{
    private const int WM_KEYDOWN = 0x0100;
    private const int WM_SYSKEYDOWN = 0x0104;

    public bool PreFilterMessage(ref Message m)
    {
        if (m.Msg is not (WM_KEYDOWN or WM_SYSKEYDOWN)
            || (Keys)(int)m.WParam != Keys.F10
            || Control.ModifierKeys != Keys.Shift
            || Control.FromHandle(GetFocus()) is not { ContextMenuStrip: { } menu } control)
            return false;

        menu.Show(control, Position(control));
        return true;
    }

    /// <summary>Below the selected row of a list, otherwise at the top left of the control.</summary>
    private static Point Position(Control control) =>
        control is ListView { FocusedItem: { } item }
            ? new Point(item.Bounds.Left + 24, item.Bounds.Bottom)
            : new Point(8, 8);

    [DllImport("user32.dll")]
    private static extern IntPtr GetFocus();
}
