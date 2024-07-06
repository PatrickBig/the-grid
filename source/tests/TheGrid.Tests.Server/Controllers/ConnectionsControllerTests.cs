// <copyright file="ConnectionsControllerTests.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using System.Security.Claims;
using TheGrid.Models;
using TheGrid.Server.Controllers;
using TheGrid.Shared.Constants;
using TheGrid.Shared.Models;
using TheGrid.TestHelpers.Fixtures;

namespace TheGrid.Tests.Server.Controllers
{
    /// <summary>
    /// Tests for the <see cref="ConnectionsController"/> class.
    /// </summary>
    public class ConnectionsControllerTests : IClassFixture<OrganizationWithConnection>
    {
        private readonly OrganizationWithConnection _fixture;
        private readonly IAuthorizationService _authorizationService;
        private readonly ClaimsPrincipal _testUser = new(new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, "admin"),
            new Claim(GridClaimTypes.Organization, "default"),
        ]));

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionsControllerTests"/> class.
        /// </summary>
        /// <param name="fixture">Testing fixture.</param>
        public ConnectionsControllerTests(OrganizationWithConnection fixture)
        {
            _fixture = fixture;

            // Mock the authorization service so it always returns success if the user ID is "admin"
            _authorizationService = Substitute.For<IAuthorizationService>();
            _authorizationService.AuthorizeAsync(Arg.Any<ClaimsPrincipal>(), Arg.Any<object>(), Arg.Any<IEnumerable<IAuthorizationRequirement>>())
                .Returns(Task.FromResult(AuthorizationResult.Success()));
        }

        /// <summary>
        /// Tests the ability to create a new connection.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task CreateConnection_Success_Test()
        {
            // Arrange
            var controller = new ConnectionsController(_fixture.Db, _authorizationService)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = _testUser },
                },
            };

            var request = new CreateConnectionRequest
            {
                Name = "Test connection",
                ConnectorId = OrganizationWithConnection.GetTestConnectorId(),
                OrganizationId = _fixture.OrganizationId,
            };

            // Act
            var actionResult = await controller.Post(request);

            // Assert
            Assert.IsType<CreatedAtActionResult>(actionResult);
            var result = actionResult as CreatedAtActionResult;
            Assert.NotNull(result);
            Assert.IsType<CreateConnectionResponse>(result.Value);
            var response = result.Value as CreateConnectionResponse;
            Assert.NotNull(response);
        }

        /// <summary>
        /// Tests that a problem is returned if the connector id is invalid.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task CreateConnection_Invalid_ConnectorId_Test()
        {
            // Arrange
            var controller = new ConnectionsController(_fixture.Db, _authorizationService)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = _testUser },
                },
            };

            var request = new CreateConnectionRequest
            {
                Name = "Test connection",
                ConnectorId = "invalid_connector_id",
                OrganizationId = _fixture.OrganizationId,
            };

            // Act
            var actionResult = await controller.Post(request);

            // Assert
            Assert.IsType<ObjectResult>(actionResult);
            var result = actionResult as ObjectResult;
            Assert.NotNull(result);
            Assert.IsType<ValidationProblemDetails>(result.Value);
            var response = result.Value as ValidationProblemDetails;
            Assert.NotNull(response);

            // Make sure we have a validation error with the connector ID as an affected property
            Assert.True(response.Errors.ContainsKey(nameof(CreateConnectionRequest.ConnectorId)));
        }

        /// <summary>
        /// Tests that a problem is returned if the organization is invalid.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task CreateConnection_Invalid_Organization_Test()
        {
            // Arrange
            var controller = new ConnectionsController(_fixture.Db, _authorizationService)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = _testUser },
                },
            };

            var request = new CreateConnectionRequest
            {
                Name = "Test connection",
                ConnectorId = OrganizationWithConnection.GetTestConnectorId(),
                OrganizationId = "invalid_org",
            };

            // Act
            var actionResult = await controller.Post(request);

            // Assert
            Assert.IsType<ObjectResult>(actionResult);
            var result = actionResult as ObjectResult;
            Assert.NotNull(result);
            Assert.IsType<ValidationProblemDetails>(result.Value);
            var response = result.Value as ValidationProblemDetails;
            Assert.NotNull(response);

            // Make sure we have a validation error with the connector ID as an affected property
            Assert.True(response.Errors.ContainsKey(nameof(CreateConnectionRequest.OrganizationId)));
        }

        /// <summary>
        /// Tests the ability to get a connection.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetConnection_Ok_Test()
        {
            // Arrange
            var controller = new ConnectionsController(_fixture.Db, _authorizationService)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = _testUser },
                },
            };
            var connectionId = _fixture.ConnectionId;

            // Act
            var actionResult = await controller.Get(connectionId);

            // Assert
            Assert.IsType<OkObjectResult>(actionResult);
            var result = actionResult as OkObjectResult;
            Assert.NotNull(result);
            Assert.IsType<Connection>(result.Value);
            var response = result.Value as Connection;
            Assert.NotNull(response);
            Assert.NotNull(response.Name);
        }

        /// <summary>
        /// Tests that when trying to fetch a connection that does not exist that a <see cref="NotFoundResult"/> is returned.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetConnection_NotFound_Test()
        {
            // Arrange
            var controller = new ConnectionsController(_fixture.Db, _authorizationService)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = _testUser },
                },
            };

            // Act
            var actionResult = await controller.Get(-5);

            // Assert
            Assert.NotNull(actionResult);
            Assert.IsType<NotFoundResult>(actionResult);
        }

        /// <summary>
        /// Tests the ability to get a list of connections.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetConnectionList_Ok_Test()
        {
            // Arrange
            var controller = new ConnectionsController(_fixture.Db, _authorizationService)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = _testUser },
                },
            };

            // Act
            var actionResult = await controller.GetList(_fixture.OrganizationId);

            // Assert
            Assert.IsType<OkObjectResult>(actionResult);
            var result = actionResult as OkObjectResult;
            Assert.NotNull(result);
            Assert.IsType<PaginatedResult<ConnectionListItem>>(result.Value);
            var response = result.Value as PaginatedResult<ConnectionListItem>;
            Assert.NotNull(response);
            Assert.NotEmpty(response.Items);
        }
    }
}
