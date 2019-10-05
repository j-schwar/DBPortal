using System.IO;
using System.Linq;
using DBPortal.Util;

namespace DBPortal.Services
{
    public class FileSystemService
    {
        private readonly DirectoryInfo _root;
        private const uint DirectoryNameLength = 16;

        public FileSystemService(string root)
        {
            _root = Directory.CreateDirectory(root);
        }

        /// <summary>
        /// Constructs a new managed directory.
        /// </summary>
        /// <returns>A <code>DirectoryInfo</code> object for the new directory.</returns>
        public DirectoryInfo CreateDirectory()
        {
            return _root.CreateSubdirectory(NewDirectoryName());
        }

        /// <summary>
        /// Returns the directory information for a managed directory with a given name.
        /// </summary>
        /// <param name="name">Name of the directory.</param>
        /// <returns>Directory information for the directory.</returns>
        public DirectoryInfo GetDirectory(string name)
        {
            return _root.EnumerateDirectories().First(dir => dir.Name == name);
        }

        private string NewDirectoryName()
        {
            var nameCandidate = RandomString.Generate(DirectoryNameLength, RandomString.AlphaNumericCharset);
            while (_root.EnumerateDirectories().Any(dir => dir.Name == nameCandidate))
            {
                nameCandidate = RandomString.Generate(DirectoryNameLength, RandomString.AlphaNumericCharset);
            }

            return nameCandidate;
        }
    }
}