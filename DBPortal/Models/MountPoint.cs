namespace DBPortal.Models
{
    /// <summary>
    /// MountPoint model contains information about a mount point for a container.
    /// </summary>
    public class MountPoint
    {
        /// <summary>
        /// Constructs a new <code>MountPoint</code> object from an existing
        /// <code>Docker.DotNet.Models.MountPoint</code> object.
        /// </summary>
        /// <param name="mountPoint">An exiting mount point.</param>
        /// <returns>A new mount point.</returns>
        public static MountPoint From(Docker.DotNet.Models.MountPoint mountPoint)
        {
            return new MountPoint
            {
                Type = mountPoint.Type,
                Name = mountPoint.Name,
                Source = mountPoint.Source,
                Destination = mountPoint.Destination,
                Driver = mountPoint.Driver,
                Mode = mountPoint.Mode
            };
        }
        
        public string Type;

        public string Name;

        public string Source;

        public string Destination;

        public string Driver;

        public string Mode;
    }
}