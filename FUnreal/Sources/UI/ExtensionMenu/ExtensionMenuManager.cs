namespace FUnreal
{
    public class ExtensionMenuManager
    {
        public ExtensionMenuManager(FUnrealService unrealService, FUnrealVS unrealVS)
        {
            //Note: Extension buttons starts as invisible and set to visible only if is a VS Soluton for Unreal.
            //      By this, is possibile to hide Extension menu, when opening NON-Unreal VS Solution

            ReloadExtensionMenuCmd.Instance.Visible = true;
            ReloadExtensionMenuCmd.Instance.Controller = new ReloadTemplatesController(unrealService, unrealVS);

            OpenOptionsExtensionMenuCmd.Instance.Visible = true;
            OpenOptionsExtensionMenuCmd.Instance.Controller = new OpenOptionsController(unrealService, unrealVS);
            
            RegenSolutionMenuCmd.Instance.Visible = true;
            RegenSolutionMenuCmd.Instance.Controller = new RegenSolutionController(unrealService, unrealVS);
        }
    }
}
