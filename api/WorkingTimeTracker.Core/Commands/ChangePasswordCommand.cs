namespace WorkingTimeTracker.Core.Commands
{
    public class ChangePasswordCommand
    {
        public string OldPassword { get; set; }

        public string NewPassword { get; set; }

        public string NewPasswordConfirm { get; set; }
    }
}
