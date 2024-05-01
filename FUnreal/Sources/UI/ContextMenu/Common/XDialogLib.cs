using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;


namespace FUnreal
{
    public static class XDialogLib
    {
        public const string ErrorMsg_SomethingWentWrong = "Ops! Something went wrong...";
        public const string ErrorMsg_PluginNotExists = "It seems that selected plugin doesn't exists. Maybe filesystem and VS Solution are misaligned!";
        internal static string ErrorMsg_ModuleNotExists = "It seems that selected module doesn't exists. Maybe filesystem and VS Solution are misaligned!";
        internal static string ErrorMsg_ModuleAlreadyExists = "Module already exist at {0}";
        public static string InfoMsg_PluginDelete = "This plugin will be deleted permanently!";
        internal static string ErrorMsg_FileAlreadyExists = "A file already exists with this name!";
        internal static string ErrorMsg_FolderAlreadyExists = "A folder already exists with this name!";
        public static string ErrorMsg_InvalidInput = "Input is not valid!";
        public static string ErrorMsg_InvalidPath = "Invalid Path {0}";
        internal static string ErrorMsg_SourcePathNotFound = "Path not found!";
        internal static string ErrorMsg_PathNotExists = "Path doesn't exist on filesystem!";
        public static string InfoMsg_SourcePathDelete = "This path will be deleted permanently!";


        public static string Error_DirectoryAlreadyExists = "Directory already exists {0}";
        public static string Error_FileAlreadyExists = "File already exists {0}";

        

        public static string Ctx_CheckProjectPlayout = "Checking project layout ...";
        public static string Ctx_UpdatingModuleDependency = "Updating module dependency ...";
        internal static string Ctx_DeletingModule = "Deleting module ...";
        internal static string Ctx_UpdatingModule = "Updating module ...";
        internal static string Ctx_UpdatingFiles = "Updating files ...";
        internal static string Ctx_UpdatingPlugin = "Updating plugin ...";
        internal static string Info_DeletingModuleFolder = "Deleting module folder: {0} ...";
        internal static string Info_CleaningDependencyFromFile = "Cleaning dependency in {0} ...";
        internal static string Info_UpdatingDependencyFromFile = "Updating dependency in {0} ...";
        internal static string Info_UpdatingPluginDescriptorFile = "Updating plugin descriptor: {0} ...";
        internal static string Info_RenamingPluginDescriptorFile = "Renaming plugin descriptor: {0} to {1}";

        public static string Ctx_CheckTemplate = "Checking template ...";

        public static string Error_PluginNotFound = "Plugin not found: {0}";
        public static string Error_FileLockedByOtherProcess = "A file is locked by an other process: {0}";
        public static string Error_MaybeLockedByUnreal = "Maybe Unreal Editor is running! Please stop it and retry!";
        public static string Error_PluginModuleNotFound = "Module not found: {0}::{1}";
        public static string Error_ModuleAlreadyExists = "Module already exists at: {0}";
        public static string Error_GameModuleAlreadyExists = "Module already exists: {0}";
        public static string Error_ModuleNotFound = "Module not found: {0}";
        internal static string Ctx_RegenSolutionFiles = "Regenerating VS Soluton files ...";
        internal static string Error_TemplateNotFound = "Template not found for ({0}, {1}, {2})";
        internal static string Error_TemplateWrongConfig = "Template configuration error for ({0}, {1}, {2})";
        internal static string Ctx_ConfiguringTemplate = "Configuring template ...";
        internal static string Info_TemplateCopyingFiles = "Copying template files to {0} ...";
        internal static string Info_CreatingFile = "Creating file {0} ...";
        internal static string Info_CannotOpenFile = "Cannot open file {0} ...";
        internal static string Info_UpdatingModuleTargetFile = "Updating module target file {0} ...";
        internal static string Warn_ProjectTargetFileNotFound = "Project target file not found: {0}";
        internal static string Info_UpdatingProjectTargetFile = "Updating project target file {0} ...";
        internal static string Warn_ModuleSourcesNotFound = "Cannot locate module source files in {0}";
        internal static string Info_UpdatingFile = "Updating file {0} ...";
        internal static string Info_RenamingFile = "Renaming file {0} to {1} ...";
        internal static string Info_RenamingFolder = "Renaming folder: {0} to {1}";
        internal static string Error_PluginAlreadyExists = "Plugin already exists: {0}";
        internal static string Ctx_UpdatingProject = "Updating UProject ...";
        internal static string Info_UpdatingProjectDescriptorFile = "Updating uproject file {0} ...";
        internal static string Error_SourceDirectoryNotFound = "Directory not found: {0}";
        internal static string Ctx_DeletingFiles = "Deleting files ...";
        internal static string Ctx_RenamingFiles = "Renaming files ...";
        internal static string Ctx_DeletingDirectories = "Deleting directories ...";
        internal static string Info_UpdatingApiMacroInFile = "Updating API macro in {0} ...";

        public static string Info_DeletingFile = "Deleting file {0} ...";
        public static string Info_DeletingFolder = "Deleting folder {0} ...";
        public static string Info_DeletingPluginsFolderBecauseEmpty = "Deleting Plugins folder because empty {0} ...";
        internal static string Error_Delete = "Delete failed!";
        internal static string Error_SourcePathNotFound = "Path not found {0}";
        internal static string ErrorMsg_PluginAlreadyExists = "Plugin already exists!";
        internal static string Error_FailureRenamingFolder = "Renaming folder failed! Probably folders/files in the folder tree are locked by another process.";
        internal static string Error_FileNotFound = "File not found: {0}";
        internal static string Error_FileRenameFailed = "File rename failed!";
        internal static string Info_RenamingFileToNewName = "Renaming file {0} to {1}";
        internal static string Info_CheckingModuleDependency = "Checking dependency to module {0} ...";
        internal static string Info_CheckingPluginDependency = "Checking dependency to plugin {0} ...";
        internal static string Info_DependentModule = "Dependent module {0}";
        internal static string info_UpdatingFile = "Updating file {0} ...";
        internal static string Title_FUnreal = "FUnreal";
        internal static string NothingToDelete = "Nothing to delete on filesystem.";
        internal static string Error_WrongTargetName = "Wrong Target Name {0}";
        public const string Input_ClassName_ValidationRegex = "^[a-zA-Z][a-zA-Z0-9_]*$";
        public const string Input_FileNameWithExt_ValidationRegex = @"^[a-zA-Z0-9_\.]+$";
        //private const string Input_SubPath_ValidationRegex = @"^(?:[a-zA-Z0-9_]+\\{0,1}){1,}(?<!\\\\)$"; //@"^[a-zA-Z0-9_]+(?:\\[a-zA-Z0-9_]+){0,}"; //not end with \ [^\\]$";
        //private const string Input_SubPath_ValidationRegex = @"^([a-zA-Z0-9_]+\\{0,1})+\\{0,1}$";
        public const string Input_SubPath_ValidationRegex = @"^[a-zA-Z0-9_][a-zA-Z0-9_\\]*$";
        public const string Input_FolderName_ValidationRegex = "^[a-zA-Z0-9_]*$";


        public static bool IsValidFileNameWitExt(string fileName)
        {
            bool validChars = Regex.IsMatch(fileName, Input_FileNameWithExt_ValidationRegex);
            if (!validChars) return false;

            string regexOnlyThisChar = "^(?:\\.+|_+)$";
            if (Regex.IsMatch(fileName, regexOnlyThisChar)) return false;
           
            if (fileName.Contains("..")) return false;

            return true;
        }


        public static void TextBox_ClassName_InputValidation(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            TextBox_InputValidation(sender, e, Input_ClassName_ValidationRegex);
        }

        public static void TextBox_FileNameWithExt_InputValidation(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            TextBox_InputValidation(sender, e, Input_FileNameWithExt_ValidationRegex);
        }

        public static void TextBox_FolderName_InputValidation(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            TextBox_InputValidation(sender, e, Input_FolderName_ValidationRegex);
        }

        public static void TextBox_SubPath_InputValidation(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            string futureText = TextBox_InputValidation(sender, e, Input_SubPath_ValidationRegex);
            
            string doubleSepar = $"{XFilesystem.PathSeparatorStr}{XFilesystem.PathSeparatorStr}";
            if (futureText.Contains(doubleSepar))
            {
                e.Handled = true;
            }
        }

        public static void TextBox_ClassName_PasteValidation(object sender, DataObjectPastingEventArgs e)
        {
            TextBox_PasteValidation(sender, e, Input_ClassName_ValidationRegex);
        }

        public static void TextBox_FileNameWithExt_PasteValidation(object sender, DataObjectPastingEventArgs e)
        {
            TextBox_PasteValidation(sender, e, Input_FileNameWithExt_ValidationRegex);
        }

        public static void TextBox_FolderName_PasteValidation(object sender, DataObjectPastingEventArgs e)
        {
            TextBox_PasteValidation(sender, e, Input_FolderName_ValidationRegex);
        }

        public static void TextBox_SubPath_PasteValidation(object sender, DataObjectPastingEventArgs e)
        {
            string futureText = TextBox_PasteValidation(sender, e, Input_SubPath_ValidationRegex);

            string doubleSepar = $"{XFilesystem.PathSeparatorStr}{XFilesystem.PathSeparatorStr}";
            if (futureText.Contains(doubleSepar))
            {
                e.CancelCommand();
            }
        }

        public static async Task ShowErrorDialogAsync(string title, string message = "")
        {
            var dialog = new MessageDialog(title, message);
            await dialog.ShowDialogAsync();
        }

        private static string TextBox_InputValidation(object sender, System.Windows.Input.TextCompositionEventArgs e, string regex)
        {
            string insertedText = e.Text;
            string futureText = TextBox_ComputeFutureText(sender, insertedText);

            try { 
                bool isGoodFormat = Regex.IsMatch(futureText, regex);
                if (!isGoodFormat) e.Handled = true;
            } catch (Exception ex)
            {
                XDebug.Erro(ex.ToString()); 
                return string.Empty;
            }

            return futureText;
        }

        private static string TextBox_PasteValidation(object sender, DataObjectPastingEventArgs e, string regex)
        {
            bool valid = false;
            string futureText = String.Empty;
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string pasteText = e.DataObject.GetData(typeof(string)) as string;
                futureText = TextBox_ComputeFutureText(sender, pasteText);

                bool isGoodFormat = Regex.IsMatch(futureText, regex);

                valid = isGoodFormat;
            }

            if (!valid)
            {
                e.CancelCommand();
            }
            return futureText;
        }


        private static string TextBox_ComputeFutureText(object sender, string insertedText)
        {
            if (!(sender is TextBox)) return string.Empty;
            TextBox tbx = (TextBox)sender;

            string currentText = tbx.Text;
            string futureText = currentText.Remove(tbx.SelectionStart, tbx.SelectionLength);
            futureText = futureText.Insert(tbx.CaretIndex, insertedText);
            return futureText;
        }
       

        public static void SetProgressMessage(FProgressPanel taskProgressPanel, FUnrealNotifier.MessageType Type, string headMessage, string traceMessage)
        {
            //Trying to locate UI FProgressPanel dispatcher to 
            //avoid Exception 'The calling thread cannot access this object because a different thread owns it'
            //when FUnrealNotifier try to send message to the UI Progress Bar
            //because could be triggered from Background thread in FUnrealService (in particular the one due to Parallel.ForEach

            ThreadHelper.JoinableTaskFactory.Run(async delegate
            {
                // Switch to main thread
                await XThread.SwitchToUIThreadIfItIsNotAsync();

                Action action = () =>
                {
                    // Do your work on the main thread here.
                    if (Type == FUnrealNotifier.MessageType.ERRO) taskProgressPanel.SetFailureMode();
                    else taskProgressPanel.SetProgressMode();

                    string prefix = $"[{Type}]";
                    string trace = $"{prefix} {traceMessage}";
                    taskProgressPanel.AddMessage(headMessage, trace);
                };

                System.Windows.Threading.Dispatcher dispatcher = taskProgressPanel.Dispatcher;

                if (dispatcher == null)
                {
                    XDebug.Erro("FProgressPanel Dispatcher is NULL!");
                    return;
                }

                //using the Async version make the UI freeze. Maybe produce some bad deadlock!
                //await taskProgressPanel.Dispatcher.BeginInvoke(action);
#pragma warning disable VSTHRD001 //switch to main thread done with XThread custom class
                dispatcher.Invoke(action);
#pragma warning restore 
            });
    
        }
    }
}
