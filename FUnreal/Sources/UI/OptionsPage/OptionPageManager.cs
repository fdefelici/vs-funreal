using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUnreal
{
    public class OptionPageManager
    {
        private FUnrealService _unrealService;
        private FUnrealVS _unrealVS;

        public OptionPageManager(FUnrealService unrealService, FUnrealVS unrealVS)
        {
            _unrealService = unrealService;
            _unrealVS = unrealVS;
            _unrealVS.OnOptionsSaved += () => { OnOptionSaved_UpdateTemplates(); };
        }

        private void OnOptionSaved_UpdateTemplates()
        {
            _unrealVS.Output.Info("Templates options changed!");
            FUnrealTemplateLoader.UpdateTemplates(_unrealVS, _unrealService);
        }
    }
}
