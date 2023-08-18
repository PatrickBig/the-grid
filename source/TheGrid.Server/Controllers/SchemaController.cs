using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TheGrid.Data;

namespace TheGrid.Server.Controllers
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    public class SchemaController : ControllerBase
    {
        private readonly TheGridDbContext _db;

        [HttpGet]
        public async Task<ActionResult> Get([FromRoute]int dataSourceId, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
