using Microsoft.VisualStudio.PlatformUI;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace FUnreal
{
    public static class XDialogLib
    {
        public const string ErrorMsg_SomethingWentWrong = "Ops! Something went wrong...";
        public const string ErrorMsg_PluginNotExists = "It seems that selected plugin doesn't exists. Maybe filesystem and VS Solution are misaligned!";
        internal static string ErrorMsg_TemplatesNotFound = "Templates not found for FUnreal!";
        internal static string ErrorMsg_ModuleNotExists = "It seems that selected module doesn't exists. Maybe filesystem and VS Solution are misaligned!";
        public static string InfoMsg_PluginDelete = "This plugin will be deleted permanently!";
        internal static string ErrorMsg_ClassAlreadyExists = "A file already exists with this name!";
        public static string ErrorMsg_InvalidPath = "Invalid Path {0}";
        internal static string ErrorMsg_SourcePathNotFound = "Path not found {0}";
        public static string InfoMsg_SourcePathDelete = "This path will be deleted permanently!";


        private const string Input_ValidationRegex = "^[a-zA-Z][a-zA-Z0-9_]*$";

        public static string Ctx_CheckProjectPlayout = "Checking project layout ...";
        public static string Ctx_UpdatingModuleDependency = "Updating module dependency ...";
        internal static string Ctx_DeletingModule = "Deleting module ...";
        internal static string Ctx_UpdatingModule = "Updating module ...";
        internal static string Ctx_UpdatingPlugin = "Updating plugin ...";
        internal static string Info_DeletingModuleFolder = "Deleting module folder: {0} ...";
        internal static string Info_CleaningDependencyFromFile = "Cleaning dependency in {0} ...";
        internal static string Info_UpdatingDependencyFromFile = "Updating dependency in {0} ...";
        internal static string Info_UpdatingPluginDescriptorFile = "Updating plugin descriptor: {0} ...";
        internal static string Info_RenamingPluginDescriptorFile = "Renaming plugin descriptor: {0} to {1}";
        public static string Ctx_CheckTemplate = "Checking template ...";

        public static string Error_PluginNotFound = "Plugin not found: {0}";
        public static string Error_PluginModuleNotFound = "Module not found: {0}::{1}";
        public static string Error_PluginModuleAlreadyExists = "Module already exists: {0}::{1}";
        internal static string Ctx_RegenSolutionFiles = "Regenerating VS Soluton files ...";
        internal static string Error_TemplateNotFound = "Template not found for ({0}, {1}, {2})";
        internal static string Error_TemplateWrongConfig = "Template configuration error for ({0}, {1}, {2})";
        internal static string Ctx_ConfiguringTemplate = "Configuring template ...";
        internal static string Info_TemplateCopyingFiles = "Copying template files to {0} ...";
        internal static string Info_UpdatingModuleTargetFile = "Updating module target file {0} ...";
        internal static string Warn_ModuleCppFileNotFound = "Module cpp file not detected as: {0} or {1}";
        internal static string Info_UpdatingModuleNameInCpp = "Updating name in module file {0} ...";
        internal static string Info_RenamingCppFiles = "Renaming C++ file: {0}";
        internal static string Info_RenamingFolder = "Renaming folder: {0} to {1}";
        internal static string Error_PluginAlreadyExists = "Plugin already exists: {0}";
        internal static string Ctx_UpdatingProject = "Updating UProject ...";
        internal static string Info_UpdatingProjectDescriptorFile = "Updating uproject file {0} ...";
        internal static string Error_SourceDirectoryNotFound = "Directory not found: {0}";
        internal static string Ctx_DeletingFiles = "Deleting files ...";
        internal static string Ctx_DeletingDirectories = "Deleting directories ...";

        public static string Info_DeletingFile = "Deleting file {0} ...";
        public static string Info_DeletingFolder = "Deleting folder {0} ...";
        internal static string Error_Delete = "Delete failed!";

        public static void TextBoxInputValidation(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            if (!(sender is TextBox)) return;
            
            string insertedText = e.Text;
            TextBox tbx = (TextBox)sender;
            string currentText = tbx.Text;
            string futureText = currentText.Remove(tbx.SelectionStart, tbx.SelectionLength);
            futureText = futureText.Insert(tbx.CaretIndex, insertedText);

            bool isGoodFormat = Regex.IsMatch(futureText, Input_ValidationRegex);
            if (!isGoodFormat) e.Handled = true;
        }
    }
}
