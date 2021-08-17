using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGameBase
{
    /// <summary>
    /// Basic module manifest attributes.
    /// </summary>
    public interface IManifest
    {
        /// <summary>
        /// The ID of this module.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// The name of this module.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The description of this module.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// The author(s) of this module.
        /// </summary>
        string Author { get; }

        /// <summary>
        /// The version of this module.
        /// TODO: Make semantic version?
        /// </summary>
        string Version { get; }

        /// <summary>
        /// The IDs of the dependencies of this module.
        /// </summary>
        string[] Dependencies { get; }
    }

    /// <summary>
    /// Basic implementation of IManifest.
    /// </summary>
    public class Manifest : IManifest
    {
        /// <inheritdoc/>
        public string Id { get; set; }

        /// <inheritdoc/>
        public string Name { get; set; }

        /// <inheritdoc/>
        public string Description { get; set; }

        /// <inheritdoc/>
        public string Author { get; set; }

        /// <inheritdoc/>
        public string Version { get; set; }

        /// <inheritdoc/>
        public string[] Dependencies { get; set; }
    }
}
