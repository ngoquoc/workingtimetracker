using System;

namespace WorkingTimeTracker.Core.Commands
{
    public class UpsertUserCommand
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }

        public string[] Roles { get; set; }
    }
}
