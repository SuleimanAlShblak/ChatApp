using Microsoft.AspNetCore.Mvc;

namespace ChatAppApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly DataService.DataService _dataService;

        public ChatController(DataService.DataService dataService)
        {
            _dataService = dataService;
        }

    }
}
