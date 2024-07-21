using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TheGrid.Models;
using TheGrid.Shared.Constants;

namespace TheGrid.Services
{
    public interface IGroupManager
    {
        public Task CreateGroupAsync(string name, string? organizationId, string? description, IEnumerable<ApplicationPermission> permissions, CancellationToken cancellationToken = default);
    }
}
