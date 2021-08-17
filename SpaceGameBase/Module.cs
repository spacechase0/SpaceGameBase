using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGameBase
{
    /// <summary>
    /// Module base class.
    /// </summary>
    public abstract class Module
    {
        /// <summary>
        /// The module's manifest, set when loading.
        /// </summary>
        public IManifest Manifest { get; internal set; }

        /// <summary>
        /// The module manager.
        /// </summary>
        public IModuleManager ModuleManager { get; internal set; }

        /// <summary>
        /// Callback for after all modules have finished loading.
        /// </summary>
        public abstract void AfterModulesLoaded();
    }
}
