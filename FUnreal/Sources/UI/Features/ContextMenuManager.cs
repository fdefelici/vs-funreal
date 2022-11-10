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
        private Dictionary<Func<Task<bool>>, List<int>> menuScenarios;

        public ContextMenuManager(FUnrealService unrealService, FUnrealVS unrealVS)
        {
            _unrealService = unrealService;
            _unrealVS = unrealVS;

            PrepareMenus();
        }

        public async Task<bool> IsActiveAsync(int symbolId)
        {
            foreach(var pair in menuScenarios)
            {
                if (await pair.Key.Invoke()) 
                { 
                    return pair.Value.Contains(symbolId);
                }
            }
            return false;
        }

        private void PrepareMenus()
        {
            //NOTA: Ricerca migliorabile se prefiltro gli sceneario in base al CTX_ITEMNODE, FOLDERNODE, PROJNODE etc...

            //Project
            Func<Task<bool>> SingleProjectScenario = async () => 
            {
                if (!await _unrealVS.IsSelectCtxProjectNodeAsync()) return false;
                return await _unrealVS.IsSingleSelectionAsync();
            };
            Func<Task<bool>> DotProjectScenario = async () =>
            {
                if (!await _unrealVS.IsSelectCtxItemNodeAsync()) return false;
                if (!await _unrealVS.IsSingleSelectionAsync()) return false;

                var item = await _unrealVS.GetSelectedItemAsync();
                return _unrealService.IsProjectDescriptorFile(item.FullPath);
            };

            //Plugin 
            Func<Task<bool>> DotPluginScenario = async () =>
            {    
                if (!await _unrealVS.IsSelectCtxItemNodeAsync()) return false;

                var item = await _unrealVS.GetSelectedItemAsync();
                return await _unrealVS.IsSingleSelectionAsync() && _unrealService.IsPluginDescriptorFile(item.FullPath);
            };
            Func<Task<bool>> SinglePluginFolder = async () =>
            {
                if (!await _unrealVS.IsSelectCtxFolderNodeAsync()) return false;
                if (!await _unrealVS.IsSingleSelectionAsync()) return false;

                var item = await _unrealVS.GetSelectedItemAsync();
                return _unrealService.IsPluginFolder(item.FullPath);
            };
            
            //Plugin Module
            Func<Task<bool>> DotBuildCsPlugModScenario = async () =>
            {
                if (!await _unrealVS.IsSelectCtxItemNodeAsync()) return false;
                if (!await _unrealVS.IsSingleSelectionAsync()) return false;
                
                var item = await _unrealVS.GetSelectedItemAsync();
                bool isBuildFile = _unrealService.IsPluginModulePath(item.FullPath) && _unrealService.IsModuleTargetFile(item.FullPath);   
                return isBuildFile;
            };
            Func<Task<bool>> SinglePluginModuleFolder = async () =>
            {
                if (!await _unrealVS.IsSelectCtxFolderNodeAsync()) return false;
                if (!await _unrealVS.IsSingleSelectionAsync()) return false;

                var item = await _unrealVS.GetSelectedItemAsync();
                return _unrealService.IsPluginModulePath(item.FullPath);
            };

            //Game Module
            Func<Task<bool>> DotBuildCsGameModScenario = async () =>
            {
                if (!await _unrealVS.IsSelectCtxItemNodeAsync()) return false;
                if (!await _unrealVS.IsSingleSelectionAsync()) return false;

                var item = await _unrealVS.GetSelectedItemAsync();
                bool isBuildFile = _unrealService.IsGameModulePath(item.FullPath) && _unrealService.IsModuleTargetFile(item.FullPath);
                return isBuildFile;
            };
            Func<Task<bool>> SingleGameModuleFolder = async () =>
            {
                if (!await _unrealVS.IsSelectCtxFolderNodeAsync()) return false;
                if (!await _unrealVS.IsSingleSelectionAsync()) return false;

                var item = await _unrealVS.GetSelectedItemAsync();
                return _unrealService.IsGameModulePath(item.FullPath);
            };


            //Sources  (Common)
            Func<Task<bool>> SingleFileScenario = async () =>
            {
                if (!await _unrealVS.IsSelectCtxItemNodeAsync()) return false;
                var item = await _unrealVS.GetSelectedItemAsync();
                return await _unrealVS.IsSingleSelectionAsync() && _unrealService.IsSourceCodePath(item.FullPath);
            };

            Func<Task<bool>> MultiFileScenario = async () =>
            {
                if (!await _unrealVS.IsSelectCtxItemNodeAsync()) return false;
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
                if (!await _unrealVS.IsSelectCtxFolderNodeAsync()) return false;
                
                var item = await _unrealVS.GetSelectedItemAsync();
                return await _unrealVS.IsSingleSelectionAsync() && _unrealService.IsSourceCodePath(item.FullPath, true);
            };

            Func<Task<bool>> MultiSourceFolder = async () =>
            {
                if (!await _unrealVS.IsSelectCtxFolderNodeAsync()) return false;

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
                if (!await _unrealVS.IsSelectCtxMiscNodeAsync()) return false;

                var items = await _unrealVS.GetSelectedItemsAsync();
                foreach (var eachItem in items)
                {
                    if (!_unrealService.IsSourceCodePath(eachItem.FullPath, true)) return false;
                }
                return true;
            };

            menuScenarios = new Dictionary<Func<Task<bool>>, List<int>>();

            //CTX ProjectNode
            menuScenarios[SingleProjectScenario] = new List<int>() { S.ToolboxMenu, S.AddPluginCmd }; ;
            
            //CTX ItemNode
            menuScenarios[DotPluginScenario]     = new List<int>() { S.ToolboxMenu, S.AddModuleCmd, S.RenamePluginCmd, S.DeletePluginCmd };
            menuScenarios[DotBuildCsGameModScenario] = new List<int>() { S.ToolboxMenu, S.RenameGameModuleCmd, S.DeleteGameModuleCmd };
            menuScenarios[DotProjectScenario]    = new List<int>() { S.ToolboxMenu, S.AddGameModuleCmd };
            menuScenarios[SingleFileScenario]    = new List<int>() { S.ToolboxMenu, S.DeleteSourceCmd };
            
            menuScenarios[DotBuildCsPlugModScenario]    = new List<int>() { S.ToolboxMenu, S.RenameModuleCmd, S.DeleteModuleCmd };
            
            menuScenarios[MultiFileScenario]     = menuScenarios[SingleFileScenario];            
            
            //CTX FolderNode
            menuScenarios[SingleSourceFolder] = new List<int>() { S.ToolboxMenu, S.AddSourceClassCmd, S.DeleteSourceCmd };
            menuScenarios[MultiSourceFolder] = new List<int>() { S.ToolboxMenu, S.DeleteSourceCmd };
            menuScenarios[SinglePluginModuleFolder] = new List<int>() { S.ToolboxMenu, S.AddSourceClassCmd, S.RenameModuleCmd, S.DeleteModuleCmd };
            menuScenarios[SinglePluginFolder] = menuScenarios[DotPluginScenario];

            menuScenarios[SingleGameModuleFolder] = new List<int>() { S.ToolboxMenu, S.AddSourceClassCmd, S.RenameGameModuleCmd, S.DeleteGameModuleCmd };
            


            //CTX MULTINODE   [VirtualFolder(s) + File(s)]
            menuScenarios[MultiMiscItem]      = menuScenarios[MultiFileScenario];
        }
    }
}