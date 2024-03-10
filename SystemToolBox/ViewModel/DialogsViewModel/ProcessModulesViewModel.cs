using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using SystemToolBox.Framwork;

namespace SystemToolBox.ViewModel.DialogsViewModel
{
    public class ProcessModulesViewModel : BaseNotifyPropertyChanged
    {
        #region 构造函数

        public ProcessModulesViewModel()
        {
        }

        public ProcessModulesViewModel(ProcessModuleCollection modules,
            ref ObservableCollection<ProcessModulesViewModel> targetCollection)
        {
            if (0 < modules.Count)
                foreach (ProcessModule module in modules)
                    targetCollection.Add(new ProcessModulesViewModel
                    {
                        ModuleName = module.ModuleName,
                        BaseAddress =
                            IntPtr.Size == 8
                                ? string.Format("0x{0:x}", module.BaseAddress.ToInt64())
                                : string.Format("0x{0:x}", module.BaseAddress.ToInt32()),
                        ModulePath = module.FileName,
                        ModuleMemorySize = module.ModuleMemorySize,
                        EntryPointAddres =
                            IntPtr.Size == 8
                                ? string.Format("0x{0:x}", module.EntryPointAddress.ToInt64())
                                : string.Format("0x{0:x}", module.EntryPointAddress.ToInt32())
                    });
        }

        #endregion

        #region 成员

        /// <summary>
        ///     基地址
        /// </summary>
        private string _baseAddress;

        public string BaseAddress
        {
            get { return _baseAddress; }
            set
            {
                _baseAddress = value;
                FirePropertyChanged(() => BaseAddress);
            }
        }

        /// <summary>
        ///     入口地址
        /// </summary>
        private string _entryPointAddres;

        public string EntryPointAddres
        {
            get { return _entryPointAddres; }
            set
            {
                _entryPointAddres = value;
                FirePropertyChanged(() => EntryPointAddres);
            }
        }

        /// <summary>
        ///     模块路径
        /// </summary>
        private string _modulePath;

        public string ModulePath
        {
            get { return _modulePath; }
            set
            {
                _modulePath = value;
                FirePropertyChanged(() => ModulePath);
            }
        }

        /// <summary>
        ///     模块名
        /// </summary>
        private string _moduleName;

        public string ModuleName
        {
            get { return _moduleName; }
            set
            {
                _moduleName = value;
                FirePropertyChanged(() => ModuleName);
            }
        }

        /// <summary>
        ///     模块内存大小
        /// </summary>
        private int _moduleMemorySize;

        public int ModuleMemorySize
        {
            get { return _moduleMemorySize; }
            set
            {
                _moduleMemorySize = value;
                FirePropertyChanged(() => ModuleMemorySize);
            }
        }

        #endregion
    }
}