using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGameBase.Util
{
    /// <summary>
    /// File utilities.
    /// </summary>
    public static class FileUtil
    {
        /// <summary>
        /// Read a text file from a path.
        /// It uses Godot APIs so that it works with res:// and user://
        /// </summary>
        /// <param name="path">The path to read from.</param>
        /// <returns>The contents of the text file.</returns>
        public static string ReadTextFile( string path )
        {
            var file = new File();
            file.Open( path, File.ModeFlags.Read );
            var contents = file.GetAsText();
            file.Close();
            file.Dispose();
            return contents;
        }
    }
}
