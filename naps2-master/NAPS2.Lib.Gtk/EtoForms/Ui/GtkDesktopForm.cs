using System.Threading;
using Eto.GtkSharp;
using Eto.GtkSharp.Forms.ToolBar;
using Gdk;
using Gtk;
using NAPS2.EtoForms.Desktop;
using NAPS2.EtoForms.Gtk;
using NAPS2.EtoForms.Notifications;
using NAPS2.EtoForms.Widgets;
using NAPS2.ImportExport.Images;
using Command = Eto.Forms.Command;

namespace NAPS2.EtoForms.Ui;

public class GtkDesktopForm : DesktopForm
{
    private readonly Dictionary<DesktopToolbarMenuType, MenuToolButton> _menuButtons = new();
    private Toolbar _toolbar = null!;
    private int _toolbarButtonCount;
    private int _toolbarMenuToggleCount;
    private int _toolbarPadding;
    private CssProvider? _toolbarPaddingCssProvider;

    public GtkDesktopForm(
        Naps2Config config,
        DesktopKeyboardShortcuts keyboardShortcuts,
        NotificationManager notificationManager,
        CultureHelper cultureHelper,
        ColorScheme colorScheme,
        IProfileManager profileManager,
        UiImageList imageList,
        ImageTransfer imageTransfer,
        ThumbnailController thumbnailController,
        UiThumbnailProvider thumbnailProvider,
        DesktopController desktopController,
        IDesktopScanController desktopScanController,
        ImageListActions imageListActions,
        ImageListViewBehavior imageListViewBehavior,
        DesktopFormProvider desktopFormProvider,
        IDesktopSubFormController desktopSubFormController,
        DesktopCommands commands)
        : base(config, keyboardShortcuts, notificationManager, cultureHelper, colorScheme, profileManager,
            imageList, imageTransfer, thumbnailController, thumbnailProvider, desktopController, desktopScanController,
            imageListActions, imageListViewBehavior, desktopFormProvider, desktopSubFormController, commands)
    {
        var cssProvider = new CssProvider();
        // TODO: Adjust colors based on color scheme
        cssProvider.LoadFromData(@"
            .desktop-toolbar-button * { min-width: 0; padding-left: 0; padding-right: 0; }
            .desktop-toolbar .image-button { min-width: 50px; padding-left: 0; padding-right: 0; }
            .desktop-toolbar .toggle { min-width: 0; padding-left: 0; padding-right: 0; }
            .preview-toolbar-button * { min-width: 0; padding-left: 0; padding-right: 0; }
            .preview-toolbar-button button { padding: 0 5px; }
            toolbar { border-bottom: 1px solid #ddd; }
            .listview .frame { background-color: #fff; }
            .listview .drop-before { border-radius: 0; border-left: 3px solid #000000; padding-left: 0; }
            .listview .drop-after { border-radius: 0; border-right: 3px solid #000000; padding-right: 0; }
            .desktop-listview .listview-item image { border: 1px solid #000; }
            .link { padding: 0; }
            .accessible-image-button { border: none; background: none; }
            .zoom-button { background: white; border: 1px solid; border-radius: 0; }
        ");
        StyleContext.AddProviderForScreen(Gdk.Screen.Default, cssProvider, 800);
    }

    protected override void OnLoad(EventArgs e)
    {
        // TODO: What's the best place to initialize this? It needs to happen from the UI event loop.
        Invoker.Current = new SyncContextInvoker(SynchronizationContext.Current!);
        base.OnLoad(e);
        var listView = (GtkListView<UiImage>) _listView;
        listView.NativeControl.StyleContext.AddClass("desktop-listview");
    }

    protected override void OnSizeChanged(EventArgs e)
    {
        base.OnSizeChanged(e);
        UpdateToolbarPadding();
    }

    private void UpdateToolbarPadding()
    {
        // TODO: We need to handle exceptions from event handlers (if they're not already?)
        _toolbar.GetPreferredWidth(out _, out var toolbarWidth);
        var toolbarWidthLessPadding = toolbarWidth - _toolbarPadding;
        var excessWidth = Width - toolbarWidthLessPadding - 4;
        var div = excessWidth / (_toolbarButtonCount * 2 + _toolbarMenuToggleCount);
        div = div.Clamp(0, 4);
        var buttonPadding = div;
        var togglePadding = div / 2;
        _toolbarPadding = buttonPadding * 2 * _toolbarButtonCount + togglePadding * 2 * _toolbarMenuToggleCount;
        if (_toolbarPaddingCssProvider == null)
        {
            _toolbarPaddingCssProvider = new CssProvider();
            StyleContext.AddProviderForScreen(Gdk.Screen.Default, _toolbarPaddingCssProvider, 800);
        }
        _toolbarPaddingCssProvider.LoadFromData($@"
            .desktop-toolbar .image-button {{ padding-left: {buttonPadding}px; padding-right: {buttonPadding}px; }}
            .desktop-toolbar .toggle {{ padding-left: {togglePadding}px; padding-right: {togglePadding}px; }}
        ");
    }

    protected override void ConfigureToolbar()
    {
        _toolbar = ((ToolBarHandler) ToolBar.Handler).Control;
        _toolbar.Style = ToolbarStyle.Both;
        _toolbar.StyleContext.AddClass("desktop-toolbar");
    }

    protected override void CreateToolbarButton(Command command)
    {
        var button = new ToolButton(command.Image.ToGtk(), command.ToolBarText)
        {
            Homogeneous = false,
            Sensitive = command.Enabled
        };
        button.Clicked += (_, _) => command.Execute();
        command.EnabledChanged += (_, _) => button.Sensitive = command.Enabled;
        button.StyleContext.AddClass("desktop-toolbar-button");
        _toolbar.Add(button);
        _toolbarButtonCount++;
    }

    protected override void CreateToolbarSeparator()
    {
        _toolbar.Add(new SeparatorToolItem());
    }

    protected override void CreateToolbarStackedButtons(Command command1, Command command2)
    {
        var button1 = CreateToolButton(command1, Orientation.Horizontal);
        var button2 = CreateToolButton(command2, Orientation.Horizontal);
        var vbox = new Box(Orientation.Vertical, 0);
        vbox.Add(button1);
        vbox.Add(button2);
        AddCustomToolItem(vbox);
        _toolbarButtonCount++;
    }

    protected override void CreateToolbarButtonWithMenu(Command command, DesktopToolbarMenuType menuType,
        MenuProvider menu)
    {
        var button = new MenuToolButton(command.Image.ToGtk(), command.ToolBarText)
        {
            Homogeneous = false,
            Sensitive = command.Enabled
        };
        button.Clicked += (_, _) => command.Execute();
        command.EnabledChanged += (_, _) => button.Sensitive = command.Enabled;
        button.Menu = CreateMenuWidget(command, menu);
        button.StyleContext.AddClass("desktop-toolbar-button");
        _toolbar.Add(button);
        _toolbarButtonCount++;
        _toolbarMenuToggleCount++;
        _menuButtons[menuType] = button;
    }

    protected override void CreateToolbarMenu(Command command, MenuProvider menu)
    {
        var button = new ToolButton(command.Image.ToGtk(), command.ToolBarText)
        {
            Homogeneous = false,
            Sensitive = command.Enabled
        };
        command.EnabledChanged += (_, _) => button.Sensitive = command.Enabled;
        var menuDelegate = GetMenuDelegate(CreateMenuWidget(command, menu), button);
        button.Clicked += menuDelegate;
        button.StyleContext.AddClass("desktop-toolbar-button");
        _toolbar.Add(button);
        _toolbarButtonCount++;
    }

    private Menu CreateMenuWidget(Command command, MenuProvider menu)
    {
        var subMenu = CreateSubMenu(command, menu);
        var menuItem = (ImageMenuItem) subMenu.ControlObject;
        return (Menu) menuItem.Submenu;
    }

    private EventHandler GetMenuDelegate(Menu menuWidget, Widget button)
    {
        return (_, _) => menuWidget.PopupAtWidget(button, Gravity.SouthWest, Gravity.NorthWest, null);
    }

    public override void ShowToolbarMenu(DesktopToolbarMenuType menuType)
    {
        var button = _menuButtons.Get(menuType);
        (button?.Menu as Menu)?.PopupAtWidget(button, Gravity.SouthWest, Gravity.NorthWest, null);
    }

    private void AddCustomToolItem(Widget item)
    {
        var toolItem = new ToolItem();
        toolItem.Add(item);
        _toolbar.Add(toolItem);
    }

    private static Button CreateToolButton(Command command, Orientation orientation = Orientation.Vertical,
        int spacing = 4)
    {
        var box = new Box(orientation, spacing);
        box.Add(command.Image.ToGtk());
        var label = new Label(command.ToolBarText);
        box.Add(label);
        var button = new Button(box)
        {
            Relief = ReliefStyle.None,
            Sensitive = command.Enabled
        };
        button.Clicked += (_, _) => command.Execute();
        command.EnabledChanged +=
            (_, _) => button.Sensitive = command.Enabled;
        return button;
    }
}