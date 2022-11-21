using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using S = FUnreal.VSCTSymbols;

namespace FUnreal
{
    
    public class ContextMenuManager 
    {
        private FUnrealService _unrealService;
        private FUnrealVS _unrealVS;


        private Dictionary<Func<Task<bool>>, Dictionary<Func<Task<bool>>, List<int>>> menuContexts;

        public ContextMenuManager(FUnrealService unrealService, FUnrealVS unrealVS)
        {
            _unrealService = unrealService;
            _unrealVS = unrealVS;

            PrepareMenus();
        }

        //optimization: prefiltro sceneario menu in base al CTX_ITEMNODE, FOLDERNODE, PROJNODE etc...
        //Idea ulteriore ottimizzazione:  numero ridotto di bottoni configurati su VSCT e poi quando test IsActive gli attacco: label e controller.
        public async Task<bool> IsActiveAsync(int symbolId)
        {
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
                            return pair.Value.Contains(symbolId);
                        }
                    }
                    return false;
                }
            }

            return false;
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
                return await _unrealVS.IsSingleSelectionAsync();
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

            //Game Module
            Func<Task<bool>> DotBuildCsGameModScenario = async () =>
            {
                if (!await _unrealVS.IsSingleSelectionAsync()) return false;

                var item = await _unrealVS.GetSelectedItemAsync();
                bool isBuildFile = _unrealService.IsGameModulePath(item.FullPath) && _unrealService.IsModuleBuildFile(item.FullPath);
                return isBuildFile;
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

            menuContexts = new Dictionary<Func<Task<bool>>, Dictionary<Func<Task<bool>>, List<int>>>();
            
            //CTX ProjectNode
            {
                var projectMenu = new Dictionary<Func<Task<bool>>, List<int>>();
                projectMenu[SingleProjectScenario] = new List<int>() { S.ToolboxMenu, S.AddPluginCmd, S.AddGameModuleCmd }; ;
                menuContexts[ProjectNodeContext] = projectMenu;
            }

            //CTX ItemNode
            var itemMenu = new Dictionary<Func<Task<bool>>, List<int>>();
            { 
                itemMenu[DotPluginScenario]     = new List<int>() { S.ToolboxMenu, S.AddModuleCmd, S.RenamePluginCmd, S.DeletePluginCmd };
                itemMenu[DotProjectScenario]    = new List<int>() { S.ToolboxMenu, S.AddGameModuleCmd };
                itemMenu[DotBuildCsPlugModScenario]    = new List<int>() { S.ToolboxMenu, S.RenameModuleCmd, S.DeleteModuleCmd };
                itemMenu[DotBuildCsGameModScenario] = new List<int>() { S.ToolboxMenu, S.RenameGameModuleCmd, S.DeleteGameModuleCmd };
                itemMenu[SingleFileScenario]    = new List<int>() { S.ToolboxMenu, S.RenameSourceFileCmd, S.DeleteSourceCmd };
                itemMenu[MultiFileScenario]     = itemMenu[SingleFileScenario];
                menuContexts[ItemNodeContext] = itemMenu;
            }

            //CTX FolderNode
            { 
                var folderMenu = new Dictionary<Func<Task<bool>>, List<int>>();
                folderMenu[SingleSourceFolder] = new List<int>() { S.ToolboxMenu, S.AddSourceClassCmd, S.AddSourceFileCmd, S.AddFolderCmd, S.RenameFolderCmd, S.DeleteSourceCmd };
                folderMenu[MultiSourceFolder] = new List<int>() { S.ToolboxMenu, S.DeleteSourceCmd };
                folderMenu[SinglePluginModuleFolder] = new List<int>() { S.ToolboxMenu, S.AddSourceClassCmd, S.AddSourceFileCmd, S.AddFolderCmd, S.RenameModuleCmd, S.DeleteModuleCmd };
                folderMenu[SinglePluginFolder] = itemMenu[DotPluginScenario];
                folderMenu[SingleGameModuleFolder] = new List<int>() { S.ToolboxMenu, S.AddSourceClassCmd, S.AddSourceFileCmd, S.AddFolderCmd, S.RenameGameModuleCmd, S.DeleteGameModuleCmd };
                menuContexts[FolderNodeContext] = folderMenu;
            }

            //CTX MULTINODE   [VirtualFolder(s) + File(s)]
            { 
                var miscMenu = new Dictionary<Func<Task<bool>>, List<int>>();
                miscMenu[MultiMiscItem] = itemMenu[SingleFileScenario];
                menuContexts[MiscNodeContext] = miscMenu;
            }
        }
    }
}