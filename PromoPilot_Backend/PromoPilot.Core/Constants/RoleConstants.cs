using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PromoPilot.Core.Constants
{
    public static class RoleConstants
    {
        public const string Marketing = "Marketing";
        public const string Finance = "Finance";
        public const string StoreManager = "StoreManager";

        public static readonly List<string> PublicRoles = new()
        {
            Marketing, Finance, StoreManager
        };
    }
}
