// <copyright file="SchemaController.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace TheGrid.Server.Controllers
{
    /// <summary>
    /// Controller for managing connection schemas.
    /// </summary>
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    public class SchemaController : ControllerBase
    {
        /// <summary>
        /// Gets the schema for a given connection.
        /// </summary>
        /// <param name="connectionId">Unique ID of the connection to get the schema for.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Schema information for the given connection.</returns>
        /// <exception cref="NotImplementedException">Currently not implemented.</exception>
        [HttpGet]
        public Task<ActionResult> Get([FromRoute] int connectionId, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
