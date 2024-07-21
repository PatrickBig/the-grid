using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheGrid.Shared.Constants;

namespace TheGrid.Shared.Models
{
    public class CreateGroupRequest
    {
        [StringLength(150)]
        [Required]
        public string Name { get; set; }

        [StringLength(250)]
        public string? Description { get; set; }

        public string OrganizationId { get; set; }

        public IEnumerable<ApplicationPermission> Permissions { get; set; }
    }
}
