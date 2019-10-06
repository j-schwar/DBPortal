using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DBPortal.Services;
using DBPortal.Util;
using Docker.DotNet;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DBPortal.Controllers
{
    /// <summary>
    /// MVC Controller for managing containers.
    /// </summary>
    public class ContainerManagerController : Controller
    {
        private readonly DockerService _dockerService;
        private readonly MySqlContainerService _mysqlService;

        public ContainerManagerController(DockerService dockerService, MySqlContainerService mysqlService)
        {
            _dockerService = dockerService;
            _mysqlService = mysqlService;
        }

        /// <summary>
        /// Returns a list of all containers.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> Index()
        {
            var containers = await _dockerService.GetAllContainersAsync();
            return View(containers);
        }

        /// <summary>
        /// Gets the container with a given id.
        /// </summary>
        /// <param name="id">Id of the container to get.</param>
        [HttpGet]
        public async Task<ActionResult> Container(string id)
        {
            var container = await _mysqlService.GetContainerAsync(id);
            if (container == null)
                return NotFound();
            return View(container);
        }

        /// <summary>
        /// Stops the container with a given id.
        /// </summary>
        /// <param name="id">Id of the container to stop.</param>
        [HttpPost]
        public async void Stop(string id)
        {
            await _dockerService.StopContainerWithIdAsync(id);
        }

        /// <summary>
        /// Stats the container with a given id.
        /// </summary>
        /// <param name="id">Id of the container to start.</param>
        [HttpPost]
        public async void Start(string id)
        {
            await _dockerService.StartContainerWithIdAsync(id);
        }

        /// <summary>
        /// Deletes the container with a given id.
        /// </summary>
        /// <param name="id">Id of the container to delete.</param>
        [HttpPost]
        public async Task<ActionResult> Delete(string id)
        {
            try
            {
                await _mysqlService.DeleteContainerAsync(id);
                return Ok();
            }
            catch (DockerApiException exception)
            {
                return BadRequest(exception);
            }
        }

        /// <summary>
        /// Constructs a new MySQL Container with default configuration.
        /// </summary>
        [HttpPost]
        public async void NewMySqlContainer()
        {
            var name = "test_container_" + RandomString.Generate(8, RandomString.AlphaNumericCharset);
            await _mysqlService.CreateNewContainer(name);
        }

        /// <summary>
        /// Uploads a list of files to the directory managed by the container
        /// with a given id.
        /// </summary>
        /// <param name="id">Id of the container to upload files to.</param>
        /// <param name="files">The files to upload.</param>
        [HttpPost]
        public async Task<IActionResult> Upload(string id, IList<IFormFile> files)
        {
            var size = files.Sum(f => f.Length);
            const int fourMb = 4 * 1024 * 1024;
            if (size >= fourMb)
                return BadRequest(new {message = "files too large"});
            foreach (var formFile in files)
            {
                if (formFile.Length <= 0) 
                    continue;
                var fileStream = await _mysqlService.CreateNewFileForContainerAsync(id, formFile.FileName);
                await formFile.CopyToAsync(fileStream);
                fileStream.Close();
            }

            return RedirectToAction("Container", new {id});
        }

        /// <summary>
        /// Deletes the file named in the request body from the container with
        /// a given id.
        /// </summary>
        /// <param name="id">A container Id.</param>
        [HttpPost]
        public async void DeleteFile(string id)
        {
            using (var reader = new StreamReader(Request.Body, Encoding.UTF8))
            {
                var filename = await reader.ReadToEndAsync();
                Console.WriteLine($"Deleting file: {filename}");
                if (filename != null)
                    await _mysqlService.DeleteFileFromContainerAsync(id, filename);
            }
        }

        /// <summary>
        /// Rebuilds a container with a given id by deleting the existing
        /// container and creating a new one. Returns HTTP along with some
        /// JSON text containing the new id of the container.
        /// </summary>
        /// <param name="id">Id of the container to rebuild.</param>
        /// <returns>The new id of the container.</returns>
        [HttpPost]
        public async Task<IActionResult> RebuildContainer(string id)
        {
            try
            {
                return Ok(new {id = await _mysqlService.RebuildContainerAsync(id)});
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }
    }
}