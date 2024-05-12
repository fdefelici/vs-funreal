using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using C = FUnreal.ContextMenuVsctSymbols;

namespace FUnreal
{
    public class ContextMenuTimer
    {
        private DateTime lastHit;

        public TimeSpan CacheDuration { get; }

        public ContextMenuTimer() 
        {
            lastHit = DateTime.Now;
            CacheDuration = TimeSpan.FromMilliseconds(500);
        }

        public bool IsInTime()
        {
            var now = DateTime.Now;
            TimeSpan delta = now - lastHit;

            bool isValid = delta.CompareTo(CacheDuration) < 0;
            lastHit = now;

            return isValid;
        }
    }


    public class ContextMenuManager 
    {
        public static ContextMenuManager Instance { get; private set; } 

        private FUnrealService _unrealService;
        private FUnrealVS _unrealVS;


        private Dictionary<Func<Task<bool>>, Dictionary<Func<Task<bool>>, S>> menuContexts;
        private Dictionary<int, XActionCmdConfig> cmdConfigPerScenario;

        private ContextMenuTimer _timer;
        private S _cachedScenario;

        public ContextMenuManager(FUnrealService unrealService, FUnrealVS unrealVS)
        {
            _unrealService = unrealService;
            _unrealVS = unrealVS;

            _timer = new ContextMenuTimer();
            _cachedScenario = S.NOT_FOUND;

            PrepareMenus();

            Instance = this;
        }

        //NOTE: To simplify the VSCT file and make dynamic the context menu, each time the user does right-click, all the menu buttons are evaluated
        //     Optimizations done:
        //     - Reduce thh button number at minimum and make it generic (Cmd11, Cmd12....). The number is give by the biggest context menu of an item.
        //     - Each time evaluate for a button, which label and controller (behaviour) to be used
        //     - Searching for the right button config pass through some filtering:
        //     -- First: context filtering based on type: CTX_ITEMNODE, FOLDERNODE, PROJNODE etc... (by the fact it doesn't seems possibile to retrive the context value, it will be recomputed)
        //     -- Second: Caching timer to reuse last scenario found for all the buttons
        public async Task ConfigureCommandAsync(IXActionCmd cmd)
        {
#if DEBUG
            Stopwatch stopwatch = Stopwatch.StartNew();
            bool cacheHit = true;
#endif //DEBUG
            if (!_timer.IsInTime())
            {
                _cachedScenario = await FindScenarioForCmdAsync();
#if DEBUG
                cacheHit = false;
#endif //DEBUG
            }


            bool found = false;
            if (_cachedScenario != S.NOT_FOUND)
            {
                int id = ID(_cachedScenario, cmd.ID);
                found = cmdConfigPerScenario.TryGetValue(id, out XActionCmdConfig config);
                if (found)
                {
                    cmd.Label = config.Label;
                    cmd.Controller = config.Controller;
                    cmd.Enabled = config.Enabled;
                    cmd.Visible = true;
                } 
            }

            if (!found)
            {
                cmd.Label = string.Empty;
                cmd.Controller = null;
                cmd.Enabled = false;
                cmd.Visible = false;
            }
#if DEBUG
            stopwatch.Stop();
            XDebug.Info($"CtxMenu Cmd 0x{cmd.ID:X4} [sceneario: 0x{(int)_cachedScenario:X4}, cached: {cacheHit} active: {cmd.Visible}] configured in {stopwatch.ElapsedMilliseconds} ms");
#endif //DEBUG
        }

        private async Task<S> FindScenarioForCmdAsync()
        {
            //If item selected not belong to current projct skip it.
            var justFirstItem = await _unrealVS.GetSelectedItemAsync();
            if (justFirstItem.ProjectName != _unrealService.ProjectName) return S.NOT_FOUND;
           
            //Choose Context
            foreach (var ctxPair in menuContexts)
            {
                if (await ctxPair.Key.Invoke())
                {
                    //Find Menu config
                    foreach (var pair in ctxPair.Value)
                    {
                        if (await pair.Key.Invoke())
                        {
                            return pair.Value;
                        }
                    }
                    return S.NOT_FOUND;
                }
            }
            return S.NOT_FOUND;
        }

        private void PrepareMenus()
        {
            Func<Task<bool>> ProjectNodeContext = async () =>
            {
                return await _unrealVS.IsSelectCtxProjectNodeAsync();
            };

            Func<Task<bool>> ItemNodeContext = async () =>
            {
                return await _unrealVS.IsSelectCtxItemNodeAsync();
            };

            Func<Task<bool>> FolderNodeContext = async () =>
            {
                return await _unrealVS.IsSelectCtxFolderNodeAsync();
            };

            Func<Task<bool>> MiscNodeContext = async () =>
            {
                return await _unrealVS.IsSelectCtxMiscNodeAsync();
            };

            

            //Project
            Func<Task<bool>> SingleProjectScenario = async () => 
            {
                if (!await _unrealVS.IsSingleSelectionAsync()) return false;
                //var justFirstItem = await _unrealVS.GetSelectedItemAsync();
                //if (justFirstItem.ProjectName != _unrealService.ProjectName) return false;
                return true;
            };

            Func<Task<bool>> DotProjectScenario = async () =>
            {
                if (!await _unrealVS.IsSingleSelectionAsync()) return false;

                var item = await _unrealVS.GetSelectedItemAsync();
                return _unrealService.IsProjectDescriptorFile(item.FullPath);
            };

            //Plugin 
            Func<Task<bool>> DotPluginScenario = async () =>
            {    
                var item = await _unrealVS.GetSelectedItemAsync();
                return await _unrealVS.IsSingleSelectionAsync() && _unrealService.IsPluginDescriptorFile(item.FullPath);
            };

            Func<Task<bool>> SinglePluginFolder = async () =>
            {
                if (!await _unrealVS.IsSingleSelectionAsync()) return false;

                var item = await _unrealVS.GetSelectedItemAsync();
                return _unrealService.IsPluginFolder(item.FullPath);
            };
            
            //Plugin Module
            Func<Task<bool>> DotBuildCsPlugModScenario = async () =>
            {
                if (!await _unrealVS.IsSingleSelectionAsync()) return false;
                
                var item = await _unrealVS.GetSelectedItemAsync();
                bool isBuildFile = _unrealService.IsPluginModulePath(item.FullPath) && _unrealService.IsModuleBuildFile(item.FullPath);   
                return isBuildFile;
            };
            Func<Task<bool>> SinglePluginModuleFolder = async () =>
            {
                if (!await _unrealVS.IsSingleSelectionAsync()) return false;

                var item = await _unrealVS.GetSelectedItemAsync();
                return _unrealService.IsPluginModulePath(item.FullPath);
            };

            //Primary Game Module
            Func<Task<bool>> DotBuildCsPrimaryGameModScenario = async () =>
            {
                if (!await _unrealVS.IsSingleSelectionAsync()) return false;

                var item = await _unrealVS.GetSelectedItemAsync();
                bool isBuildFile = _unrealService.IsPrimaryGameModulePath(item.FullPath) && _unrealService.IsModuleBuildFile(item.FullPath);
                return isBuildFile;
            };

            //Game Module
            Func<Task<bool>> DotBuildCsGameModScenario = async () =>
            {
                if (!await _unrealVS.IsSingleSelectionAsync()) return false;

                var item = await _unrealVS.GetSelectedItemAsync();
                bool isBuildFile = _unrealService.IsGameModulePath(item.FullPath) && _unrealService.IsModuleBuildFile(item.FullPath);
                return isBuildFile;
            };

            Func<Task<bool>> SinglePrimaryGameModuleFolder = async () =>
            {
                if (!await _unrealVS.IsSingleSelectionAsync()) return false;

                var item = await _unrealVS.GetSelectedItemAsync();
                return _unrealService.IsPrimaryGameModulePath(item.FullPath);
            };


            Func<Task<bool>> SingleGameModuleFolder = async () =>
            {
                if (!await _unrealVS.IsSingleSelectionAsync()) return false;

                var item = await _unrealVS.GetSelectedItemAsync();
                return _unrealService.IsGameModulePath(item.FullPath);
            };


            //Sources  (Common)
            Func<Task<bool>> SingleFileScenario = async () =>
            {
                var item = await _unrealVS.GetSelectedItemAsync();
                return await _unrealVS.IsSingleSelectionAsync() && _unrealService.IsSourceCodePath(item.FullPath);
            };

            Func<Task<bool>> MultiFileScenario = async () =>
            {
                if (!await _unrealVS.IsMultiSelectionAsync()) return false;

                var items = await _unrealVS.GetSelectedItemsAsync();
                foreach (var eachItem in items)
                {
                    if (!_unrealService.IsSourceCodePath(eachItem.FullPath)) return false;
                }
                return true;
            };

            Func<Task<bool>> SingleSourceFolder = async () =>
            {
                if (!await _unrealVS.IsSingleSelectionAsync()) return false;

                var item = await _unrealVS.GetSelectedItemAsync();
                return _unrealService.IsSourceCodePath(item.FullPath, true);
            };

            Func<Task<bool>> MultiSourceFolder = async () =>
            {
                if (!await _unrealVS.IsMultiSelectionAsync()) return false;

                var items = await _unrealVS.GetSelectedItemsAsync();
                foreach (var eachItem in items)
                {
                    if (!_unrealService.IsSourceCodePath(eachItem.FullPath, true)) return false;
                }
                return true;
            };

            Func<Task<bool>> MultiMiscItem = async () =>
            {
                var items = await _unrealVS.GetSelectedItemsAsync();
                foreach (var eachItem in items)
                {
                    if (!_unrealService.IsSourceCodePath(eachItem.FullPath, true)) return false;
                }
                return true;
            };

            menuContexts = new Dictionary<Func<Task<bool>>, Dictionary<Func<Task<bool>>, S >>();
            
            //CTX ProjectNode
            {
                var projectMenu = new Dictionary<Func<Task<bool>>, S>();
                projectMenu[SingleProjectScenario] = S.SingleProject;
                menuContexts[ProjectNodeContext] = projectMenu;
            }

            //CTX ItemNode
            var itemMenu = new Dictionary<Func<Task<bool>>, S>();
            {
                itemMenu[DotPluginScenario]     = S.DotPlugin;
                itemMenu[DotProjectScenario] = S.DotProject;
                itemMenu[DotBuildCsPlugModScenario] = S.DotBuildCsPlugMod;
                itemMenu[DotBuildCsPrimaryGameModScenario] = S.DotBuildCsPrimaryGameMod;
                itemMenu[DotBuildCsGameModScenario] = S.DotBuildCsGameMod;
                itemMenu[SingleFileScenario] = S.SingleFile;
                itemMenu[MultiFileScenario] = S.MultiFile;
                menuContexts[ItemNodeContext] = itemMenu;
            }

            //CTX FolderNode
            { 
                var folderMenu = new Dictionary<Func<Task<bool>>, S>();
                folderMenu[SingleSourceFolder] = S.SingleSourceFolder;
                folderMenu[MultiSourceFolder] = S.MultiSourceFolder;
                folderMenu[SinglePluginModuleFolder] = S.SinglePluginModuleFolder;
                folderMenu[SinglePluginFolder] = S.SinglePluginFolder;
                folderMenu[SinglePrimaryGameModuleFolder] = S.SinglePrimaryGameModuleFolder;
                folderMenu[SingleGameModuleFolder] = S.SingleGameModuleFolder;
                menuContexts[FolderNodeContext] = folderMenu;
            }

            //CTX MULTINODE   [VirtualFolder(s) + File(s)]
            { 
                var miscMenu = new Dictionary<Func<Task<bool>>, S>();
                miscMenu[MultiMiscItem] = S.MultiMiscItem;
                menuContexts[MiscNodeContext] = miscMenu;
            }

            cmdConfigPerScenario = new Dictionary<int, XActionCmdConfig>();

            cmdConfigPerScenario[ID(S.SingleProject, C.Cmd11)] = new XActionCmdConfig("New Plugin...", new AddPluginController(_unrealService, _unrealVS, this));
            cmdConfigPerScenario[ID(S.SingleProject, C.Cmd12)] = new XActionCmdConfig("New Game Module...", new AddGameModuleController(_unrealService, _unrealVS, this));

            cmdConfigPerScenario[ID(S.DotProject, C.Cmd11)] = cmdConfigPerScenario[ID(S.SingleProject, C.Cmd11)];
            cmdConfigPerScenario[ID(S.DotProject, C.Cmd12)] = cmdConfigPerScenario[ID(S.SingleProject, C.Cmd12)];

            cmdConfigPerScenario[ID(S.DotPlugin, C.Cmd11)] = new XActionCmdConfig("New Module...", new AddModuleController(_unrealService, _unrealVS, this));
            cmdConfigPerScenario[ID(S.DotPlugin, C.Cmd21)] = new XActionCmdConfig("Rename Plugin...", new RenamePluginController(_unrealService, _unrealVS, this));
            cmdConfigPerScenario[ID(S.DotPlugin, C.Cmd22)] = new XActionCmdConfig("Delete Plugin", new DeletePluginController(_unrealService, _unrealVS, this));

            cmdConfigPerScenario[ID(S.DotBuildCsPlugMod, C.Cmd11)] = new XActionCmdConfig("Rename Module...", new RenameModuleController(_unrealService, _unrealVS, this));
            cmdConfigPerScenario[ID(S.DotBuildCsPlugMod, C.Cmd12)] = new XActionCmdConfig("Delete Module", new DeleteModuleController(_unrealService, _unrealVS, this));

            cmdConfigPerScenario[ID(S.DotBuildCsGameMod, C.Cmd11)] = new XActionCmdConfig("Rename Module...", new RenameGameModuleController(_unrealService, _unrealVS, this));
            cmdConfigPerScenario[ID(S.DotBuildCsGameMod, C.Cmd12)] = new XActionCmdConfig("Delete Module", new DeleteGameModuleController(_unrealService, _unrealVS, this));

            cmdConfigPerScenario[ID(S.DotBuildCsPrimaryGameMod, C.Cmd11)] = cmdConfigPerScenario[ID(S.DotBuildCsGameMod, C.Cmd11)];
            cmdConfigPerScenario[ID(S.DotBuildCsPrimaryGameMod, C.Cmd12)] = new XActionCmdConfig("Delete Module");

            cmdConfigPerScenario[ID(S.SingleFile, C.Cmd11)] = new XActionCmdConfig("Rename...", new RenameSourceFileController(_unrealService, _unrealVS, this));
            cmdConfigPerScenario[ID(S.SingleFile, C.Cmd12)] = new XActionCmdConfig("Delete", new DeleteSourceController(_unrealService, _unrealVS, this));

            cmdConfigPerScenario[ID(S.MultiFile, C.Cmd11)] = cmdConfigPerScenario[ID(S.SingleFile, C.Cmd12)];

            cmdConfigPerScenario[ID(S.SingleSourceFolder, C.Cmd11)] = new XActionCmdConfig("New Class...", new AddSourceClassController(_unrealService, _unrealVS, this));
            cmdConfigPerScenario[ID(S.SingleSourceFolder, C.Cmd12)] = new XActionCmdConfig("New File...", new AddSourceFileController(_unrealService, _unrealVS, this));
            cmdConfigPerScenario[ID(S.SingleSourceFolder, C.Cmd13)] = new XActionCmdConfig("New Folder...", new AddFolderController(_unrealService, _unrealVS, this));
            cmdConfigPerScenario[ID(S.SingleSourceFolder, C.Cmd21)] = new XActionCmdConfig("Rename...", new RenameFolderController(_unrealService, _unrealVS, this));
            cmdConfigPerScenario[ID(S.SingleSourceFolder, C.Cmd22)] = cmdConfigPerScenario[ID(S.SingleFile, C.Cmd12)];

            cmdConfigPerScenario[ID(S.MultiSourceFolder, C.Cmd11)] = cmdConfigPerScenario[ID(S.SingleSourceFolder, C.Cmd22)];

            cmdConfigPerScenario[ID(S.SinglePluginModuleFolder, C.Cmd11)] = cmdConfigPerScenario[ID(S.SingleSourceFolder, C.Cmd11)];
            cmdConfigPerScenario[ID(S.SinglePluginModuleFolder, C.Cmd12)] = cmdConfigPerScenario[ID(S.SingleSourceFolder, C.Cmd12)];
            cmdConfigPerScenario[ID(S.SinglePluginModuleFolder, C.Cmd13)] = cmdConfigPerScenario[ID(S.SingleSourceFolder, C.Cmd13)];
            cmdConfigPerScenario[ID(S.SinglePluginModuleFolder, C.Cmd21)] = cmdConfigPerScenario[ID(S.DotBuildCsPlugMod, C.Cmd11)];
            cmdConfigPerScenario[ID(S.SinglePluginModuleFolder, C.Cmd22)] = cmdConfigPerScenario[ID(S.DotBuildCsPlugMod, C.Cmd12)];

            cmdConfigPerScenario[ID(S.SinglePluginFolder, C.Cmd11)] = cmdConfigPerScenario[ID(S.DotPlugin, C.Cmd11)];
            cmdConfigPerScenario[ID(S.SinglePluginFolder, C.Cmd21)] = cmdConfigPerScenario[ID(S.DotPlugin, C.Cmd21)];
            cmdConfigPerScenario[ID(S.SinglePluginFolder, C.Cmd22)] = cmdConfigPerScenario[ID(S.DotPlugin, C.Cmd22)];


            cmdConfigPerScenario[ID(S.SingleGameModuleFolder, C.Cmd11)] = cmdConfigPerScenario[ID(S.SingleSourceFolder, C.Cmd11)];
            cmdConfigPerScenario[ID(S.SingleGameModuleFolder, C.Cmd12)] = cmdConfigPerScenario[ID(S.SingleSourceFolder, C.Cmd12)];
            cmdConfigPerScenario[ID(S.SingleGameModuleFolder, C.Cmd13)] = cmdConfigPerScenario[ID(S.SingleSourceFolder, C.Cmd13)];
            cmdConfigPerScenario[ID(S.SingleGameModuleFolder, C.Cmd21)] = cmdConfigPerScenario[ID(S.DotBuildCsGameMod, C.Cmd11)];
            cmdConfigPerScenario[ID(S.SingleGameModuleFolder, C.Cmd22)] = cmdConfigPerScenario[ID(S.DotBuildCsGameMod, C.Cmd12)];

            cmdConfigPerScenario[ID(S.SinglePrimaryGameModuleFolder, C.Cmd11)] = cmdConfigPerScenario[ID(S.SingleSourceFolder, C.Cmd11)];
            cmdConfigPerScenario[ID(S.SinglePrimaryGameModuleFolder, C.Cmd12)] = cmdConfigPerScenario[ID(S.SingleSourceFolder, C.Cmd12)];
            cmdConfigPerScenario[ID(S.SinglePrimaryGameModuleFolder, C.Cmd13)] = cmdConfigPerScenario[ID(S.SingleSourceFolder, C.Cmd13)];
            cmdConfigPerScenario[ID(S.SinglePrimaryGameModuleFolder, C.Cmd21)] = cmdConfigPerScenario[ID(S.DotBuildCsGameMod, C.Cmd11)];
            cmdConfigPerScenario[ID(S.SinglePrimaryGameModuleFolder, C.Cmd22)] = cmdConfigPerScenario[ID(S.DotBuildCsPrimaryGameMod, C.Cmd12)];

            cmdConfigPerScenario[ID(S.MultiMiscItem, C.Cmd11)] = cmdConfigPerScenario[ID(S.SingleSourceFolder, C.Cmd22)];
        }

        private static int ID(S scenario, int cmd)
        {
            return (int)scenario | cmd;
        }
    }

    


    public enum S
    {
        NOT_FOUND = 0,
        SingleProject                   = 0x0100,
        DotPlugin                       = 0x0200,
        DotProject                      = 0x0300,
        DotBuildCsPlugMod               = 0x0400,
        DotBuildCsPrimaryGameMod        = 0x0500,
        DotBuildCsGameMod               = 0x0600,
        SingleFile                      = 0x0700,
        MultiFile                       = 0x0800,
        SingleSourceFolder              = 0x0900,
        MultiSourceFolder               = 0x1000,
        SinglePluginModuleFolder        = 0x1100,
        SinglePluginFolder              = 0x1200,
        SinglePrimaryGameModuleFolder   = 0x1300,
        SingleGameModuleFolder          = 0x1400,
        MultiMiscItem                   = 0x1500
    }



    public class XActionCmdConfig
    {
        public string Label { get; }
        public AXActionController Controller { get; }

        public bool Enabled { get; }

        public XActionCmdConfig(string label) 
            :this(label, null)
        {

        }
    
        public XActionCmdConfig(string label, AXActionController controller)
        {
            Label = label;
            Controller = controller;
            Enabled = Controller != null;
        }

    }
}