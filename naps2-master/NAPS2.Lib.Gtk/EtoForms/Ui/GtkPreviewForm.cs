using Gtk;
using NAPS2.EtoForms.Desktop;

namespace NAPS2.EtoForms.Ui;

public class GtkPreviewForm : PreviewForm
{
    public GtkPreviewForm(Naps2Config config, DesktopCommands desktopCommands, UiImageList imageList,
        IIconProvider iconProvider, KeyboardShortcutManager ksm) : base(config, desktopCommands, imageList,
        iconProvider, ksm)
    {
    }

    protected override void CreateToolbar()
    {
        base.CreateToolbar();
        var toolBar = (Toolbar) ToolBar.ControlObject;
        toolBar.IconSize = IconSize.SmallToolbar;
        toolBar.Style = ToolbarStyle.Icons;
        foreach (var item in toolBar.Children)
        {
            if (item is ToolItem toolItem)
            {
                toolItem.Homogeneous = false;
                item.StyleContext.AddClass("preview-toolbar-button");
            }
        }
    }
}