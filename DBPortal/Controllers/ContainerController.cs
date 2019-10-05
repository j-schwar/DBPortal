using System.Threading.Tasks;
using DBPortal.Services;
using DBPortal.Util;
using Docker.DotNet;
using Microsoft.AspNetCore.Mvc;

namespace DBPortal.Controllers
{
    /// <summary>
    /// MVC Controller for managing containers.
    /// </summary>
    public class ContainerController : Controller
    {
        private readonly DockerService _dockerService;
        private readonly MySqlContainerService _mysqlService;

        public ContainerController(DockerService dockerService, MySqlContainerService mysqlService)
        {
            _dockerService = dockerService;
            _mysqlService = mysqlService;
        }

        public async Task<ActionResult> Index()
        {
            var containers = await _dockerService.GetAllContainersAsync();
            return View(containers);
        }

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
                await _dockerService.DeleteContainerWithIdAsync(id);
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
    }
}