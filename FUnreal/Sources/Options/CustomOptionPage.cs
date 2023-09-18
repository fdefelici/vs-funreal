using System.Windows;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;

namespace FUnreal
{
    [ComVisible(true)]
    [Guid("3E7CDE05-2D92-4A90-838C-9B0D60AF0805")]
    public class CustomOptionPage : UIElementDialogPage
    {
        protected override UIElement Child 
        {
            get
            {
                GeneralOptions page = new GeneralOptions()
                {
                    generalOptionsPage = this
                };

                page.Initialize();
                return page;
            }
        }
    }
}
