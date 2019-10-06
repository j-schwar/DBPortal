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

        /// <summary>
        /// Deletes the managed directory with a given name.
        /// </summary>
        /// <param name="name">The name of the directory to delete.</param>
        public void DeleteDirectory(string name)
        {
            var path = Path.Join(_root.FullName, name);
            Directory.Delete(path, true);
        }

        /// <summary>
        /// Creates a new file with a given name in a given directory.
        /// </summary>
        /// <param name="directory">The directory to place the file in.</param>
        /// <param name="filename">The name of the file to create.</param>
        /// <returns>A stream for the new file.</returns>
        public FileStream CreateFile(string directory, string filename)
        {
            var info = GetDirectory(directory);
            var path = Path.Combine(info.FullName, filename);
            return File.Create(path);
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