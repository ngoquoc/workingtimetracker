namespace WorkingTimeTracker.Core.Authorization
{
    public struct RequirementValidationResult
    {
        public RequirementValidationStatus Status { get; set; }

        public string[] Errors { get; set; }

        public static readonly RequirementValidationResult Skip
            = new RequirementValidationResult() { Status = RequirementValidationStatus.Skip };

        public static readonly RequirementValidationResult Succeed
            = new RequirementValidationResult() { Status = RequirementValidationStatus.Succeed };

        public static RequirementValidationResult Failed(params string[] errors)
        {
            return new RequirementValidationResult()
            {
                Status = RequirementValidationStatus.Fail,
                Errors = errors
            };
        }
    }

    public enum RequirementValidationStatus
    {
        Fail = 0,
        Skip = 1,
        Succeed = 2
    }
}
