using System;
using System.Collections.Generic;
using System.Linq;
using DBPortal.Services;

namespace DBPortal.Models
{
    /// <summary>
    /// Container model contains information pertaining to a single docker
    /// container.
    ///
    /// Information is derived from
    /// <code>Docker.DotNet.Models.ContainerListResponse</code>.
    /// </summary>
    public class Container
    {
        /// <summary>
        /// Constructs a new <code>Container</code> object from an existing
        /// <code>Docker.DotNet.Models.ContainerListResponse</code> object.
        /// </summary>
        /// <param name="container">A container list response object.</param>
        /// <returns>A new container object.</returns>
        public static Container From(Docker.DotNet.Models.ContainerListResponse container)
        {
            return new Container
            {
                Names = container.Names,
                Id = container.ID,
                Image = container.Image,
                Created = container.Created,
                Ports = container.Ports.Select(Port.From).ToList(),
                State = container.State,
                Mounts = container.Mounts.Select(MountPoint.From).ToList()
            };
        }

        /// <summary>
        /// The unique identifier for the container.
        /// </summary>
        public string Id;
        
        /// <summary>
        /// A list of friendly names describing the container.
        /// </summary>
        public IList<string> Names;

        /// <summary>
        /// The image used to create this container.
        /// </summary>
        public string Image;

        /// <summary>
        /// The time at which this container was created.
        /// </summary>
        public DateTime Created;

        /// <summary>
        /// Ports used by this container.
        /// </summary>
        public IList<Port> Ports;

        /// <summary>
        /// Description of the state of the container.
        /// </summary>
        public string State;

        /// <summary>
        /// List of mount points for the container.
        /// </summary>
        public IList<MountPoint> Mounts;

        /// <summary>
        /// The name of the directory associated with the container.
        /// </summary>
        public string ContainerDirectoryName;

        /// <summary>
        /// As list of SQL script files located in the container's directory.
        /// </summary>
        public IList<ScriptFile> SqlScriptFiles;

        /// <summary>
        /// Returns <code>true</code> if this container is a MySQL container.
        /// </summary>
        public bool IsMySqlContainer => Image == MySqlContainerService.ContainerImageName;

        /// <summary>
        /// The public facing port for this container.
        /// </summary>
        public int? DatabasePort => Ports.FirstOrDefault(port => port.PrivatePort == 3306)?.PublicPort;

        /// <summary>
        /// IntelliJ build environment configuration string for connecting to this container.
        /// </summary>
        public string EnvironmentConfiguration => 
            DatabasePort == null ? null : $"DB_HOST=50.67.54.205;DB_PORT={DatabasePort};DB_USER=admin;DB_PASSWORD=password";

        public string FriendlyName()
        {
            var builder = "";
            for (var i = 0; i < Names.Count; ++i)
            {
                if (i != 0)
                    builder += ", ";
                builder += Names[i];
            }

            return builder;
        }

        /// <summary>
        /// Returns a string describing the ports used by this container.
        /// </summary>
        /// <returns>A description of this container's ports</returns>
        public string PortDescription()
        {
            var builder = "";
            for (var i = 0; i < Ports.Count; ++i)
            {
                if (i != 0)
                    builder += ", ";
                builder += $"{Ports[i].PublicPort}:{Ports[i].PrivatePort}";
            }

            return builder;
        }
    }
}