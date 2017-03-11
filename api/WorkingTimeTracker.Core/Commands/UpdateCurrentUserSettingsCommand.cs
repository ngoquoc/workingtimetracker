namespace WorkingTimeTracker.Core.Commands
{
    public class UpdateCurrentUserSettingsCommand
    {
        public string Name { get; set; }

        public double PreferredWorkingHourPerDay { get; set; }
    }
}
