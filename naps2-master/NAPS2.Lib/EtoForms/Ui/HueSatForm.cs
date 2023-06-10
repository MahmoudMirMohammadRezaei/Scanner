using Eto.Drawing;
using NAPS2.EtoForms.Widgets;

namespace NAPS2.EtoForms.Ui;

public class HueSatForm : ImageFormBase
{
    private readonly SliderWithTextBox _hueSlider = new();
    private readonly SliderWithTextBox _saturationSlider = new();

    public HueSatForm(Naps2Config config, ThumbnailController thumbnailController, IIconProvider iconProvider) :
        base(config, thumbnailController)
    {
        Icon = new Icon(1f, Icons.color_management.ToEtoImage());
        Title = UiStrings.HueSaturation;

        _hueSlider.Icon = iconProvider.GetIcon("color_wheel");
        _saturationSlider.Icon = iconProvider.GetIcon("color_gradient");
        Sliders = new[] { _hueSlider, _saturationSlider };
    }

    protected override IEnumerable<Transform> Transforms =>
        new Transform[]
        {
            new HueTransform(_hueSlider.IntValue),
            new SaturationTransform(_saturationSlider.IntValue)
        };
}