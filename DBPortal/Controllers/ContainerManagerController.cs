using System;
using System.Collections.Generic;
using System.Linq;
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

        [HttpGet]
        public async Task<ActionResult> Index()
        {
            var containers = await _dockerService.GetAllContainersAsync();
            return View(containers);
        }

        [HttpGet]
        public async Task<ActionResult> Container(string id)
        {
            var container = await _mysqlService.GetContainerAsync(id);
            if (container == null)
                return NotFound();
            return View(container);
        }

        [HttpPost]
        public async void Stop(string id)
        {
            await _dockerService.StopContainerWithIdAsync(id);
        }

        [HttpPost]
        public async void Start(string id)
        {
            await _dockerService.StartContainerWithIdAsync(id);
        }

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

        [HttpPost]
        public async void NewMySqlContainer()
        {
            var name = "test_container_" + RandomString.Generate(8, RandomString.AlphaNumericCharset);
            await _mysqlService.CreateNewContainer(name);
        }

        [HttpPost]
        public async Task<IActionResult> Upload(string id, IList<IFormFile> files)
        {
            Console.WriteLine($"Got upload request for id: {id}");
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