using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DBPortal.Models;
using DBPortal.Models.MySql;
using Docker.DotNet;
using Docker.DotNet.Models;

namespace DBPortal.Services
{
    /// <summary>
    /// A service for managing the creation and deletion of MySQL containers.
    /// </summary>
    public class MySqlContainerService
    {
        private readonly DockerService _dockerService;
        private readonly FileSystemService _fileSystemService;
        private readonly PortManagerService _portManagerService;

        private const string UsersFileName = "users.sql";
        private const string SqlScriptMountDestination = "/docker-entrypoint-initdb.d";

        public const string ContainerImageName = "mysql/mysql-server";

        public MySqlContainerService(DockerService dockerService, FileSystemService fileSystemService,
            PortManagerService portManagerService)
        {
            _dockerService = dockerService;
            _fileSystemService = fileSystemService;
            _portManagerService = portManagerService;
        }

        /// <summary>
        /// Creates a new MySQL Server container with default configuration.
        /// </summary>
        /// <param name="name">The name of the container to create.</param>
        /// <returns>The ID of the newly created container.</returns>
        public async Task<string> CreateNewContainer(string name)
        {
            var containerDir = _fileSystemService.CreateDirectory();
            ConstructUsersFile(containerDir, new[] {User.Default});
            var hostConfig = new HostConfig
            {
                Binds = BindingData(containerDir),
                PortBindings = PortBindings(_portManagerService.AllocatePort())
            };
            var createParams = new CreateContainerParameters
            {
                Name = name,
                Image = ContainerImageName,
                HostConfig = hostConfig
            };
            try
            {
                var result = await _dockerService.CreateContainer(createParams);
                return result.ID;
            }
            catch (DockerContainerNotFoundException)
            {
                // CreateContainer will throw an exception if unable to load the image.
                // Pull the image and try again.
                await PullMySqlImageAsync();
                return await CreateNewContainer(name);
            }
        }

        /// <summary>
        /// Returns the container with a given id.
        /// </summary>
        /// <param name="id">Id of the container to find.</param>
        /// <returns>A container.</returns>
        public async Task<Container> GetContainerAsync(string id)
        {
            var container = await _dockerService.GetContainerWithIdAsync(id);
            if (container == null || !container.IsMySqlContainer)
                return container;
            try
            {
                var directoryName = container.Mounts
                    .First(mount => mount.Destination == SqlScriptMountDestination)
                    .Source
                    .Split('/')
                    .Last();
                container.ContainerDirectoryName = directoryName;
                container.SqlScriptFiles = GetScriptFiles(directoryName);
            }
            catch (InvalidOperationException)
            {
                // do nothing if unable to set the container's directory
            }

            return container;
        }

        /// <summary>
        /// Creates a new file associated with the container with a given id.
        /// </summary>
        /// <param name="id">Id of a container.</param>
        /// <param name="filename">Name of the file to create.</param>
        /// <returns>A writeable stream for the file.</returns>
        public async Task<FileStream> CreateNewFileForContainerAsync(string id, string filename)
        {
            var container = await GetContainerAsync(id);
            var directory = container.ContainerDirectoryName;
            if (string.IsNullOrEmpty(directory))
                throw new Exception("container does not have an associated directory");
            return _fileSystemService.CreateFile(directory, filename);
        }

        /// <summary>
        /// Deletes the container with a given id cleaning up any resources it used.
        /// </summary>
        /// <param name="id">Id of the container to delete.</param>
        /// <returns>An async task.</returns>
        public async Task DeleteContainerAsync(string id)
        {
            var container = await GetContainerAsync(id);

            // delete the container's directory
            if (!string.IsNullOrEmpty(container.ContainerDirectoryName))
                _fileSystemService.DeleteDirectory(container.ContainerDirectoryName);

            // release the ports used by the container
            foreach (var port in container.Ports.Select(port => port.PublicPort))
                _portManagerService.ReleasePort(port);
            await _dockerService.DeleteContainerWithIdAsync(id);
        }

        /// <summary>
        /// Creates a new container by copying data, and deleting, an existing container.
        /// </summary>
        /// <param name="id">Id of the container to rebuild.</param>
        /// <returns>The new id for the container.</returns>
        public async Task<string> RebuildContainerAsync(string id)
        {
            // save the current container, we'll copy over data when creating the new one
            var container = await GetContainerAsync(id);
            
            // delete container using DockerService as to not delete it's directory as well
            await _dockerService.DeleteContainerWithIdAsync(id);
            
            // create the new container
            var bindingDate = string.IsNullOrEmpty(container.ContainerDirectoryName)
                ? null
                : BindingData(_fileSystemService.GetDirectory(container.ContainerDirectoryName));
            var hostConfig = new HostConfig
            {
                Binds = bindingDate,
                PortBindings = PortBindings(_portManagerService.AllocatePort())
            };
            var createParams = new CreateContainerParameters
            {
                Name = container.FriendlyName(),
                Image = container.Image,
                HostConfig = hostConfig
            };
            
            var result = await _dockerService.CreateContainer(createParams);
            return result.ID;
        }

        /// <summary>
        /// Pulls the MysSQL image from Docker Hub.
        /// </summary>
        /// <returns>An async task.</returns>
        private async Task PullMySqlImageAsync()
        {
            await _dockerService.PullImage("mysql/mysql-server", "latest");
        }

        /// <summary>
        /// Returns the script files contained in the directory of a given container.
        /// </summary>
        /// <param name="directoryName">If of a container.</param>
        /// <returns>A list of files.</returns>
        private IList<ScriptFile> GetScriptFiles(string directoryName)
        {
            var directoryInfo = _fileSystemService.GetDirectory(directoryName);
            return directoryInfo.EnumerateFiles()
                .Select(file => new ScriptFile
                {
                    Name = file.Name,
                    Contents = File.ReadAllText(file.FullName)
                })
                .ToList();
        }

        /// <summary>
        /// Constructs a file containing user data for a MySQL database container.
        /// </summary>
        /// <param name="directory">The directory to place the file in.</param>
        /// <param name="users">A list of users to add to the file.</param>
        private static void ConstructUsersFile(FileSystemInfo directory, IEnumerable<User> users)
        {
            var fileStream = File.Create(Path.Combine(directory.FullName, UsersFileName));
            foreach (var user in users)
            {
                var bytes = new UTF8Encoding(true).GetBytes(user.ToSql() + "\n");
                fileStream.Write(bytes);
            }

            fileStream.Close();
        }

        /// <summary>
        /// Constructs volume data for a container.
        /// <paramref name="directory"/> will contain the startup scripts for the container. 
        /// </summary>
        /// <param name="directory">The directory to use as a volume.</param>
        /// <returns>Volume data for a container.</returns>
        private static IList<string> BindingData(FileSystemInfo directory)
        {
            return new[]
            {
                $"{directory.FullName}:{SqlScriptMountDestination}",
            };
        }

        /// <summary>
        /// Constructs the port bindings for a MySQL container.
        /// </summary>
        /// <param name="hostPort">The host port to map the container to.</param>
        /// <returns>The port mapping configuration.</returns>
        private static Dictionary<string, IList<PortBinding>> PortBindings(uint hostPort)
        {
            var binding = new PortBinding
            {
                HostPort = hostPort.ToString()
            };
            return new Dictionary<string, IList<PortBinding>>
            {
                {"3306/tcp", new[] {binding}}
            };
        }
    }
}