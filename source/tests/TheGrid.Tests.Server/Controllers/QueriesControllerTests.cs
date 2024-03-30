// <copyright file="QueriesControllerTests.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using TheGrid.Models;
using TheGrid.Server.Controllers;
using TheGrid.Services;
using TheGrid.Shared.Models;
using TheGrid.TestHelpers.Fixtures;

namespace TheGrid.Tests.Server.Controllers
{
    /// <summary>
    /// Tests for the <see cref="QueriesController"/> class.
    /// </summary>
    public class QueriesControllerTests : IClassFixture<QueryFixture>
    {
        private readonly QueryFixture _fixture;
        private readonly IQueryManager _queryManager = Substitute.For<IQueryManager>();
        private readonly Random _random = new Random();

        /// <summary>
        /// Initializes a new instance of the <see cref="QueriesControllerTests"/> class.
        /// </summary>
        /// <param name="fixture">Fixture for tests.</param>
        public QueriesControllerTests(QueryFixture fixture)
        {
            _fixture = fixture;
        }

        /// <summary>
        /// Tests the ability to create a query.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task CreateQuery_Created_Test()
        {
            // Arrange
            var controller = new QueriesController(_fixture.Db, _queryManager);

            var request = new CreateQueryRequest
            {
                Command = "SELECT TOP 5 * FROM TestTable",
                ConnectionId = _fixture.ConnectionId,
                Name = "Test query",
            };

            // Act
            var actionResult = await controller.CreateQuery(request);

            // Assert
            Assert.IsType<CreatedAtActionResult>(actionResult);
            var result = actionResult as CreatedAtActionResult;
            Assert.NotNull(result);
            Assert.IsType<CreateQueryResponse>(result.Value);
            var response = result.Value as CreateQueryResponse;
            Assert.NotNull(response);
        }

        /// <summary>
        /// Tests the ability to fetch a specific query.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetQuery_Ok_Test()
        {
            // Arrange
            var controller = new QueriesController(_fixture.Db, _queryManager);

            // Act
            var actionResult = await controller.GetQuery(_fixture.QueryId);

            // Assert
            Assert.IsType<OkObjectResult>(actionResult);
            var result = actionResult as OkObjectResult;
            Assert.NotNull(result);
            Assert.IsType<GetQueryResponse>(result.Value);
            var response = result.Value as GetQueryResponse;
            Assert.NotNull(response);
            Assert.NotNull(response.ConnectionName);
        }

        /// <summary>
        /// Tests that when trying to fetch a query that does not exist that a <see cref="NotFoundResult"/> is returned.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetQuery_NotFound_Test()
        {
            // Arrange
            var controller = new QueriesController(_fixture.Db, _queryManager);

            // Act
            var actionResult = await controller.GetQuery(-5);

            // Assert
            Assert.NotNull(actionResult);
            Assert.IsType<NotFoundResult>(actionResult);
        }

        /// <summary>
        /// Tests the ability to update an existing query.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task UpdateQuery_Ok_Test()
        {
            // Arrange
            var controller = new QueriesController(_fixture.Db, _queryManager);

            var request = new UpdateQueryRequest
            {
                ConnectionId = _fixture.ConnectionId,
                Command = "SELECT TOP " + _random.Next(1, 1000) + " * FROM SomeTable",
                Name = "Updated Query " + _random.Next(1, 1000),
                Description = "Test description " + _random.Next(1, 1000),
            };

            // Act
            var actionResult = await controller.UpdateQuery(_fixture.QueryId, request);

            // Assert
            Assert.IsType<OkResult>(actionResult);

            // See that the query was updated
            var updatedQuery = await _fixture.Db.Queries.FirstOrDefaultAsync(q => q.Id == _fixture.QueryId);
            Assert.NotNull(updatedQuery);
            Assert.Equal(request.Name, updatedQuery.Name);
            Assert.Equal(request.Command, updatedQuery.Command);
        }

        /// <summary>
        /// Tests that when update a query that does not exist that a <see cref="NotFoundResult"/> is returned.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task UpdateQuery_NotFound_Test()
        {
            // Arrange
            var controller = new QueriesController(_fixture.Db, _queryManager);

            // Act
            var actionResult = await controller.UpdateQuery(-5, new());

            // Assert
            Assert.NotNull(actionResult);
            Assert.IsType<NotFoundResult>(actionResult);
        }

        /// <summary>
        /// Tests the ability to get a list of queries in the system.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetList_Ok_Test()
        {
            // Arrange
            var controller = new QueriesController(_fixture.Db, _queryManager);

            // Act
            var actionResult = await controller.GetList(_fixture.OrganizationId, null);

            // Assert
            Assert.IsType<OkObjectResult>(actionResult);
            var result = actionResult as OkObjectResult;
            Assert.NotNull(result);
            Assert.IsType<PaginatedResult<QueryListItem>>(result.Value);
            var response = result.Value as PaginatedResult<QueryListItem>;
            Assert.NotNull(response);
            Assert.NotEmpty(response.Items);
        }

        /// <summary>
        /// Tests the ability to delete a query from the system.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task DeleteQuery_Ok_Test()
        {
            // Arrange
            var query = new Query
            {
                Name = "Delete this query",
                Command = "SELECT * FROM TestTable",
                Description = "Something something",
                ConnectionId = _fixture.ConnectionId,
            };

            _fixture.Db.Queries.Add(query);
            await _fixture.Db.SaveChangesAsync();

            var controller = new QueriesController(_fixture.Db, _queryManager);

            // Act
            var result = await controller.DeleteQuery(query.Id);

            // Assert
            Assert.Empty(await _fixture.Db.Queries.Where(q => q.Id == query.Id).ToListAsync());
        }

        /// <summary>
        /// Tests that when delete a query that does not exist that a <see cref="NotFoundResult"/> is returned.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task DeleteQuery_NotFound_Test()
        {
            // Arrange
            var controller = new QueriesController(_fixture.Db, _queryManager);

            // Act
            var actionResult = await controller.DeleteQuery(-5);

            // Assert
            Assert.NotNull(actionResult);
            Assert.IsType<NotFoundResult>(actionResult);
        }

        /// <summary>
        /// Tests the ability to add tags to add tags to a query.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task AddTags_Ok_Test()
        {
            // Arrange
            var controller = new QueriesController(_fixture.Db, _queryManager);
            var request = new TagsRequest
            {
                Tags = [
                    "tag" + _random.Next(1, 1000),
                ],
            };

            // Act
            var actionResult = await controller.AddTags(_fixture.QueryId, request);

            // Assert
            Assert.IsType<OkObjectResult>(actionResult);
            var result = actionResult as OkObjectResult;
            Assert.NotNull(result);
            Assert.IsType<TagsResponse>(result.Value);
            var response = result.Value as TagsResponse;
            Assert.NotNull(response);
            Assert.Equal(request.Tags.Length, response.TagsModified);

            // Verify the query has the tags
            var updatedQuery = await _fixture.Db.Queries.FirstOrDefaultAsync(q => q.Id == _fixture.QueryId);
            Assert.NotNull(updatedQuery);
            Assert.Contains(updatedQuery.Tags, t => t == request.Tags.First());
        }

        /// <summary>
        /// Tests that when add tags to a query that does not exist that a <see cref="NotFoundResult"/> is returned.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task AddTags_NotFound_Test()
        {
            // Arrange
            var controller = new QueriesController(_fixture.Db, _queryManager);
            var request = new TagsRequest
            {
                Tags = [
                    "tag"
                ],
            };

            // Act
            var actionResult = await controller.AddTags(-5, request);

            // Assert
            Assert.NotNull(actionResult);
            Assert.IsType<NotFoundResult>(actionResult);
        }

        /// <summary>
        /// Tests the ability to delete tags from a query.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task DeleteTags_Ok_Test()
        {
            // Arrange
            var controller = new QueriesController(_fixture.Db, _queryManager);

            var tagToDelete = "tagtodelete" + _random.Next();

            var originalQuery = await _fixture.Db.Queries.FirstOrDefaultAsync(q => q.Id == _fixture.QueryId);
            Assert.NotNull(originalQuery);
            originalQuery.Tags.Add(tagToDelete);
            await _fixture.Db.SaveChangesAsync();

            var request = new TagsRequest
            {
                Tags = [
                    tagToDelete,
                ],
            };

            // Act
            var actionResult = await controller.DeleteTags(_fixture.QueryId, request);

            // Assert
            Assert.IsType<OkObjectResult>(actionResult);
            var result = actionResult as OkObjectResult;
            Assert.NotNull(result);
            Assert.IsType<TagsResponse>(result.Value);
            var response = result.Value as TagsResponse;
            Assert.NotNull(response);
            Assert.Equal(request.Tags.Length, response.TagsModified);

            // Verify the query has the tags
            var updatedQuery = await _fixture.Db.Queries.FirstOrDefaultAsync(q => q.Id == _fixture.QueryId);
            Assert.NotNull(updatedQuery);
            Assert.DoesNotContain(updatedQuery.Tags, t => t == tagToDelete);
        }

        /// <summary>
        /// Tests that when delete tags from a query that does not exist that a <see cref="NotFoundResult"/> is returned.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task DeleteTags_NotFound_Test()
        {
            // Arrange
            var controller = new QueriesController(_fixture.Db, _queryManager);
            var request = new TagsRequest
            {
                Tags = [
                    "tag"
                ],
            };

            // Act
            var actionResult = await controller.DeleteTags(-5, request);

            // Assert
            Assert.NotNull(actionResult);
            Assert.IsType<NotFoundResult>(actionResult);
        }
    }
}
