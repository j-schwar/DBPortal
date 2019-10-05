using System;
using System.Collections.Generic;

namespace DBPortal.Services
{
    /// <summary>
    /// Service managing host ports.
    /// </summary>
    public class PortManagerService
    {
        private readonly Stack<uint> _availablePorts;

        /// <summary>
        /// Constructs this service with a range of available ports.
        /// </summary>
        /// <param name="minPort">The inclusive lower bound port number.</param>
        /// <param name="maxPort">The exclusive upper bound port number.</param>
        /// <exception cref="ArgumentException">If <code>maxPort &lt;= minPort</code>.</exception>
        public PortManagerService(uint minPort, uint maxPort)
        {
            if (maxPort <= minPort)
                throw new ArgumentException("maxPort must be larger than minPort");
            _availablePorts = new Stack<uint>((int) (maxPort - minPort));
            for (var i = minPort; i < maxPort; ++i)
                _availablePorts.Push(i);                
        }

        /// <summary>
        /// Returns an available port, marking it as unavailable.
        /// </summary>
        /// <returns>A port number.</returns>
        /// <exception cref="Exception">If no more ports remain.</exception>
        public uint AllocatePort()
        {
            if (_availablePorts.Count == 0)
                throw new Exception("no more ports");
            return _availablePorts.Pop();
        }

        /// <summary>
        /// Returns a port back to the system.
        /// </summary>
        /// <param name="port">The port to return, must have been a port allocated by the system.</param>
        /// <exception cref="Exception">If <paramref name="port"/> was not allocated.</exception>
        public void ReleasePort(uint port)
        {
            if (_availablePorts.Contains(port))
                throw new Exception("port already exists");
            _availablePorts.Push(port);
        }
    }
}