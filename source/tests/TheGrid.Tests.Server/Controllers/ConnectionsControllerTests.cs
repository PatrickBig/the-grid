// <copyright file="ConnectionsControllerTests.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NSubstitute;
using System.Security.Claims;
using TheGrid.Connectors;
using TheGrid.Models;
using TheGrid.Models.Configuration;
using TheGrid.Server.Controllers;
using TheGrid.Services.Authorization;
using TheGrid.Services.Security;
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
        private readonly ISecretProtector _secretProtector = new AesGcmSecretProtector(Options.Create(new SecretProtectionOptions
        {
            EncryptionKey = Convert.ToBase64String(new byte[32]),
        }));

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
            var controller = new ConnectionsController(_fixture.Db, _authorizationService, _secretProtector)
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
            var controller = new ConnectionsController(_fixture.Db, _authorizationService, _secretProtector)
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
            var controller = new ConnectionsController(_fixture.Db, _authorizationService, _secretProtector)
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
        /// Tests that creating a connection with a secret parameter value stores it encrypted, separate from non-secret values.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task CreateConnection_WithSecretParameter_StoresCiphertext_Test()
        {
            // Arrange
            await EnsurePostgreSqlConnectorSeededAsync();

            var controller = new ConnectionsController(_fixture.Db, _authorizationService, _secretProtector)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = _testUser },
                },
            };

            const string plaintextPassword = "correct horse battery staple";

            var request = new CreateConnectionRequest
            {
                Name = "Postgres connection",
                ConnectorId = typeof(PostgreSqlConnector).FullName!,
                OrganizationId = _fixture.OrganizationId,
                ConnectionProperties = new Dictionary<string, string?>
                {
                    [CommonConnectionParameters.Username] = "postgres",
                    [CommonConnectionParameters.Password] = plaintextPassword,
                },
            };

            // Act
            var actionResult = await controller.Post(request);

            // Assert
            var result = Assert.IsType<CreatedAtActionResult>(actionResult);
            var response = Assert.IsType<CreateConnectionResponse>(result.Value);

            var storedConnection = await _fixture.Db.Connections.SingleAsync(c => c.Id == response.ConnectionId);

            Assert.Equal("postgres", storedConnection.ConnectionProperties[CommonConnectionParameters.Username]);
            Assert.False(storedConnection.ConnectionProperties.ContainsKey(CommonConnectionParameters.Password));

            Assert.True(storedConnection.SecretProperties.TryGetValue(CommonConnectionParameters.Password, out var storedPassword));
            Assert.NotEqual(plaintextPassword, storedPassword);
            Assert.Equal(plaintextPassword, _secretProtector.Unprotect(storedPassword!));
        }

        /// <summary>
        /// Tests the ability to get a connection.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetConnection_Ok_Test()
        {
            // Arrange
            var controller = new ConnectionsController(_fixture.Db, _authorizationService, _secretProtector)
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
            Assert.IsType<GetConnectionResponse>(result.Value);
            var response = result.Value as GetConnectionResponse;
            Assert.NotNull(response);
            Assert.NotNull(response.Name);
        }

        /// <summary>
        /// Tests that fetching a connection with a stored secret returns presence-only information for the secret and real values for non-secret properties.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetConnection_RedactsSecretValues_Test()
        {
            // Arrange
            await EnsurePostgreSqlConnectorSeededAsync();

            var connection = new Connection
            {
                Name = "Connection with secret",
                OrganizationId = _fixture.OrganizationId,
                ConnectorId = typeof(PostgreSqlConnector).FullName!,
                ConnectionProperties = new Dictionary<string, string?>
                {
                    [CommonConnectionParameters.Username] = "postgres",
                },
                SecretProperties = new Dictionary<string, string?>
                {
                    [CommonConnectionParameters.Password] = _secretProtector.Protect("super-secret"),
                },
            };

            _fixture.Db.Connections.Add(connection);
            await _fixture.Db.SaveChangesAsync();

            var controller = new ConnectionsController(_fixture.Db, _authorizationService, _secretProtector)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = _testUser },
                },
            };

            // Act
            var actionResult = await controller.Get(connection.Id);

            // Assert
            var result = Assert.IsType<OkObjectResult>(actionResult);
            var response = Assert.IsType<GetConnectionResponse>(result.Value);

            Assert.Equal("postgres", response.ConnectionProperties[CommonConnectionParameters.Username]);
            Assert.True(response.SecretProperties[CommonConnectionParameters.Password]);
        }

        /// <summary>
        /// Tests that reading a connection is authorized against a read-level operation, not create.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetConnection_AuthorizesUsingReadOperation_Test()
        {
            // Arrange
            var controller = new ConnectionsController(_fixture.Db, _authorizationService, _secretProtector)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = _testUser },
                },
            };

            // Act
            await controller.Get(_fixture.ConnectionId);

            // Assert
            await _authorizationService.Received().AuthorizeAsync(
                Arg.Any<ClaimsPrincipal>(),
                Arg.Any<object>(),
                Arg.Is<IEnumerable<IAuthorizationRequirement>>(r => r.Single() == GridOperations.Read));
        }

        /// <summary>
        /// Tests that when trying to fetch a connection that does not exist that a <see cref="NotFoundResult"/> is returned.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetConnection_NotFound_Test()
        {
            // Arrange
            var controller = new ConnectionsController(_fixture.Db, _authorizationService, _secretProtector)
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
            var controller = new ConnectionsController(_fixture.Db, _authorizationService, _secretProtector)
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

        /// <summary>
        /// Tests that omitting a secret key from an update request leaves the previously stored encrypted value unchanged.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task UpdateConnection_OmittingSecretKey_PreservesIt_Test()
        {
            // Arrange
            await EnsurePostgreSqlConnectorSeededAsync();

            var originalProtectedPassword = _secretProtector.Protect("original-password");

            var connection = new Connection
            {
                Name = "Connection to update",
                OrganizationId = _fixture.OrganizationId,
                ConnectorId = typeof(PostgreSqlConnector).FullName!,
                SecretProperties = new Dictionary<string, string?>
                {
                    [CommonConnectionParameters.Password] = originalProtectedPassword,
                },
            };

            _fixture.Db.Connections.Add(connection);
            await _fixture.Db.SaveChangesAsync();

            var controller = new ConnectionsController(_fixture.Db, _authorizationService, _secretProtector)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = _testUser },
                },
            };

            var request = new UpdateConnectionRequest
            {
                ConnectionProperties = new Dictionary<string, string?>
                {
                    [CommonConnectionParameters.Username] = "postgres",
                },
            };

            // Act
            var actionResult = await controller.Put(connection.Id, request);

            // Assert
            Assert.IsType<OkResult>(actionResult);

            var updatedConnection = await _fixture.Db.Connections.SingleAsync(c => c.Id == connection.Id);
            Assert.Equal(originalProtectedPassword, updatedConnection.SecretProperties[CommonConnectionParameters.Password]);
        }

        /// <summary>
        /// Tests that including a secret key in an update request replaces the stored encrypted value.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task UpdateConnection_IncludingSecretKey_ReplacesIt_Test()
        {
            // Arrange
            await EnsurePostgreSqlConnectorSeededAsync();

            var connection = new Connection
            {
                Name = "Connection to update",
                OrganizationId = _fixture.OrganizationId,
                ConnectorId = typeof(PostgreSqlConnector).FullName!,
                SecretProperties = new Dictionary<string, string?>
                {
                    [CommonConnectionParameters.Password] = _secretProtector.Protect("original-password"),
                },
            };

            _fixture.Db.Connections.Add(connection);
            await _fixture.Db.SaveChangesAsync();

            var controller = new ConnectionsController(_fixture.Db, _authorizationService, _secretProtector)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = _testUser },
                },
            };

            const string newPassword = "new-password";

            var request = new UpdateConnectionRequest
            {
                SecretProperties = new Dictionary<string, string?>
                {
                    [CommonConnectionParameters.Password] = newPassword,
                },
            };

            // Act
            var actionResult = await controller.Put(connection.Id, request);

            // Assert
            Assert.IsType<OkResult>(actionResult);

            var updatedConnection = await _fixture.Db.Connections.SingleAsync(c => c.Id == connection.Id);
            var storedPassword = updatedConnection.SecretProperties[CommonConnectionParameters.Password];
            Assert.NotEqual(newPassword, storedPassword);
            Assert.Equal(newPassword, _secretProtector.Unprotect(storedPassword!));
        }

        /// <summary>
        /// Tests that an update request replaces non-secret connection properties wholesale.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task UpdateConnection_ChangesNonSecretValues_Test()
        {
            // Arrange
            await EnsurePostgreSqlConnectorSeededAsync();

            var connection = new Connection
            {
                Name = "Connection to update",
                OrganizationId = _fixture.OrganizationId,
                ConnectorId = typeof(PostgreSqlConnector).FullName!,
                ConnectionProperties = new Dictionary<string, string?>
                {
                    [CommonConnectionParameters.Username] = "old-username",
                },
            };

            _fixture.Db.Connections.Add(connection);
            await _fixture.Db.SaveChangesAsync();

            var controller = new ConnectionsController(_fixture.Db, _authorizationService, _secretProtector)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = _testUser },
                },
            };

            var request = new UpdateConnectionRequest
            {
                ConnectionProperties = new Dictionary<string, string?>
                {
                    [CommonConnectionParameters.Username] = "new-username",
                },
            };

            // Act
            var actionResult = await controller.Put(connection.Id, request);

            // Assert
            Assert.IsType<OkResult>(actionResult);

            var updatedConnection = await _fixture.Db.Connections.SingleAsync(c => c.Id == connection.Id);
            Assert.Equal("new-username", updatedConnection.ConnectionProperties[CommonConnectionParameters.Username]);
        }

        /// <summary>
        /// Tests that updating a connection that does not exist returns a <see cref="NotFoundResult"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task UpdateConnection_NotFound_Test()
        {
            // Arrange
            var controller = new ConnectionsController(_fixture.Db, _authorizationService, _secretProtector)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = _testUser },
                },
            };

            // Act
            var actionResult = await controller.Put(-5, new UpdateConnectionRequest());

            // Assert
            Assert.IsType<NotFoundResult>(actionResult);
        }

        private async Task EnsurePostgreSqlConnectorSeededAsync()
        {
            var connectorId = typeof(PostgreSqlConnector).FullName!;

            if (!await _fixture.Db.Connectors.AnyAsync(c => c.Id == connectorId))
            {
                _fixture.Db.Connectors.Add(new TheGrid.Shared.Models.Connector
                {
                    Id = connectorId,
                    Name = "PostgreSQL",
                });

                await _fixture.Db.SaveChangesAsync();
            }
        }
    }
}
