using System.Collections.ObjectModel;
using System.Threading;
using NAPS2.App.Tests.Targets;
using NAPS2.App.Tests.Verification;
using OpenQA.Selenium.Appium.Windows;
using Xunit;
// ReSharper disable StringLiteralTypo

namespace NAPS2.App.Tests.Appium;

[Collection("appium")]
public class LanguageSelectionTests : AppiumTests
{
    private static readonly HashSet<string> ExpectedMissingLanguages = new() { "bn", "hi", "id", "th", "ur" };

    [VerifyTheory(AllowDebug = true, WindowsAppium = true)]
    [ClassData(typeof(AppiumTestData))]
    public void OpenLanguageDropdown(IAppTestTarget target)
    {
        Init(target);
        // Open the Language dropdown
        ClickAtName("Language");
        var menuItems = GetMenuItems();

        VerifyMissingLanguages(menuItems);

        // Verify French (fr) translation as a standard language example
        ClickAndResetWindow("Français");
        ClickAndResetWindow("Langue");
        
        // Verify Portuguese (pt-BR) translation as a country-specific language example
        ClickAndResetWindow("Português (Brasil)");
        ClickAndResetWindow("Idioma");
        
        // Verify Hebrew translation as a RTL language example
        ClickAndResetWindow("עברית");
        ClickAndResetWindow("שפה");
    
        // And back to English
        ClickAndResetWindow("English");
        ClickAtName("Language");
    
        AppTestHelper.AssertNoErrorLog(FolderPath);
    }

    private void VerifyMissingLanguages(ReadOnlyCollection<WindowsElement> menuItems)
    {
#if !DEBUG_LANG
        // In Debug mode (without DEBUG_LANG) we don't expect to have all languages
        if (Debugger.IsAttached) return;
#endif
        // Verify all expected languages have menu items
        var menuItemTexts = menuItems.Select(x => x.Text).ToHashSet();
        var allLanguages = GetAllLanguages();
        var missingLanguages = allLanguages
            .Where(x => !menuItemTexts.Contains(x.langName) && !ExpectedMissingLanguages.Contains(x.langCode))
            .ToList();
        Assert.True(missingLanguages.Count == 0, $"Missing languages: {string.Join(",", missingLanguages)}");
    }

    private void ClickAndResetWindow(string name)
    {
        ClickAtName(name);
        Thread.Sleep(100);
        ResetMainWindow();
    }

    private List<(string langCode, string langName)> GetAllLanguages()
    {
        return new CultureHelper(Naps2Config.Stub()).GetAllCultures().ToList();
    }

    private ReadOnlyCollection<WindowsElement> GetMenuItems()
    {
        return _session.FindElementsByTagName("MenuItem");
    }
}