namespace WorkingTimeTracker.Core.Entities
{
    public class UserWithRoles
    {
        public UserWithRoles(User user)
            : base()
        {
            this.Id = user.Id;
            this.Name = user.Name;
            this.Email = user.Email;
            this.Roles = new string[0];
        }

        public string Id { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }

        public string[] Roles { get; set; }
    }

    public class CurrentUserData
    {
        public CurrentUserData(User user)
            : base()
        {
            Id = user.Id;
            Name = user.Name;
            Email = user.Email;
            Roles = new string[0];
            PreferredWorkingHourPerDay = user.PreferredWorkingHourPerDay;
        }

        public string Id { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }

        public double PreferredWorkingHourPerDay { get; set; }

        public string[] Roles { get; set; }
    }
}
