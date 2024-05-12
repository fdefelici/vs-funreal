namespace FUnreal
{
    public class ExtensionMenuManager
    {
        public ExtensionMenuManager(FUnrealService unrealService, FUnrealVS unrealVS)
        {
            ReloadExtensionMenuCmd.Instance.Controller = new ReloadTemplatesController(unrealService, unrealVS);
            OpenOptionsExtensionMenuCmd.Instance.Controller = new OpenOptionsController(unrealService, unrealVS);
        }
    }
}
