using NAPS2.Ocr;

namespace NAPS2.EtoForms.Widgets;

public class OcrLanguagesListViewBehavior : ListViewBehavior<Language>
{
    public OcrLanguagesListViewBehavior(ColorScheme colorScheme)
    {
        ColorScheme = colorScheme;
        ShowLabels = true;
        Checkboxes = true;
    }

    public override string GetLabel(Language item)
    {
        return item.Name;
    }
}