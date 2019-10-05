namespace DBPortal.Models
{
    /// <summary>
    /// Port model contains information about a port configuration for a
    /// container.
    ///
    /// Information is derived from <code>Docker.DotNet.Models.Port</code>.
    /// </summary>
    public class Port
    {
        /// <summary>
        /// Constructs a <code>DBPortal.Models.Port</code> object from a
        /// <code>Docker.DotNet.Models.Port</code> object.
        /// </summary>
        /// <param name="port">A port object.</param>
        /// <returns>A new port object.</returns>
        public static Port From(Docker.DotNet.Models.Port port)
        {
            return new Port
            {
                IP = port.IP,
                PrivatePort = port.PrivatePort,
                PublicPort = port.PublicPort,
                Type = port.Type
            };
        }

        /// <summary>
        /// IP Address
        /// </summary>
        public string IP;

        /// <summary>
        /// Private port number (i.e., internal port).
        /// </summary>
        public ushort PrivatePort;

        /// <summary>
        /// Public port number (i.e., external port).
        /// </summary>
        public ushort PublicPort;

        /// <summary>
        /// Port type (e.g., tcp, udp, etc.).
        /// </summary>
        public string Type;

        /// <summary>
        /// Returns a textual description of this object.
        /// </summary>
        /// <returns>This object as a string.</returns>
        public override string ToString()
        {
            return $"{IP} {PublicPort}:{PrivatePort}/{Type}";
        }
    }
}