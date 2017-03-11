using System;

namespace WorkingTimeTracker.Core.Commands
{
    public class DeleteUserCommand
    {
        public string UserId { get; set; }

        public bool ForceDelete { get; set; }
    }
}
