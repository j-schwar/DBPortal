using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DBPortal.Models;
using Docker.DotNet;
using Docker.DotNet.Models;

namespace DBPortal.Services
{
    /// <summary>
    /// Uses Docker.DotNet library to interface with the running docker service on the host machine.
    /// </summary>
    public class DockerService
    {
        private readonly DockerClient _client;

        public DockerService()
        {
            _client = new DockerClientConfiguration(new Uri("npipe://./pipe/docker_engine")).CreateClient();
        }

        /// <summary>
        /// Returns a enumerable sequence of all docker containers on the host.
        ///
        /// Equivalent to running <code>docker container list</code>.
        /// </summary>
        /// <returns>This set of containers</returns>
        public async Task<IEnumerable<Container>> GetAllContainersAsync()
        {
            var containers = await _client.Containers.ListContainersAsync(new ContainersListParameters
            {
                All = true
            });
            return containers.Select(Container.From);
        }

        /// <summary>
        /// Returns the container with a given <paramref name="id"/>.
        /// </summary>
        /// <param name="id">Id field for the requested container.</param>
        /// <returns>A container or <code>null</code> if no such container exists</returns>
        public async Task<Container> GetContainerWithIdAsync(string id)
        {
            var containers = await GetAllContainersAsync();
            try
            {
                return containers.First(container => container.Id == id);
            }
            catch (InvalidOperationException)
            {
                return null;
            }
        }

        /// <summary>
        /// Stops the container with a given id, returning <code>true</code> if successful.
        /// </summary>
        /// <param name="id">Id of the container to stop.</param>
        /// <returns><code>true</code> iff container was stopped successfully.</returns>
        public async Task<bool> StopContainerWithIdAsync(string id)
        {
            return await _client.Containers.StopContainerAsync(id, new ContainerStopParameters());
        }

        /// <summary>
        /// Starts the container with a given id, retuning <code>true</code> if successful.
        /// </summary>
        /// <param name="id">The id of the container to start.</param>
        /// <returns><code>true</code> iff container was started successfully.</returns>
        public async Task<bool> StartContainerWithIdAsync(string id)
        {
            return await _client.Containers.StartContainerAsync(id, new ContainerStartParameters());
        }

        /// <summary>
        /// Deletes a container with a given id.
        /// </summary>
        /// <param name="id">The id of the container to delete.</param>
        public async Task DeleteContainerWithIdAsync(string id)
        {
            await _client.Containers.RemoveContainerAsync(id, new ContainerRemoveParameters());
        }

        /// <summary>
        /// Creates a new container using some given <paramref name="parameters"/>.
        /// </summary>
        /// <param name="parameters">Parameters to use when creating the container.</param>
        /// <returns>The response of the creation call.</returns>
        public async Task<CreateContainerResponse> CreateContainer(CreateContainerParameters parameters)
        {
            return await _client.Containers.CreateContainerAsync(parameters);
        }
    }
}