// <copyright file="CreateGroupResponse.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheGrid.Shared.Models
{
    public class CreateGroupResponse : CreateGroupRequest
    {
        public string Id { get; set; }
    }
}
