using System;
using System.Linq;
using WorkingTimeTracker.Core.Queries;

namespace WorkingTimeTracker.Core.Validators
{
    public static class Check
    {
        public static void NotEmpty(string value, string name = "", string errorMessage = "")
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                var varName = Fallback(name, "Value");
                string error = Fallback(errorMessage,
                    $"{varName} can not be empty.");
                throw new ValidationException(error);
            }
        }

        public static void NotEqual(Guid actual, Guid expected, string name = "", string errorMessage = "")
        {
            if (actual == expected)
            {
                var varName = Fallback(name, "Value");
                string error = Fallback(errorMessage,
                    $"{varName} must be different from {expected}.");
                throw new ValidationException(error);
            }
        }

        public static void Max(double actual, double expected, string name = "", string errorMessage = "")
        {
            if (actual > expected)
            {
                var varName = Fallback(name, "Value");
                string error = Fallback(errorMessage,
                    $"{varName} can not be greater than {expected}.");
                throw new ValidationException(error);
            }
        }

        public static void GreaterThan(double actual, double expected, string name = "", string errorMessage = "")
        {
            if (actual <= expected)
            {
                var varName = Fallback(name, "Value");
                string error = Fallback(errorMessage,
                    $"{varName} must be different from {expected}.");
                throw new ValidationException(error);
            }
        }

        public static void Max(int actual, int expected, string name = "", string errorMessage = "")
        {
            if (actual > expected)
            {
                var varName = Fallback(name, "Value");
                string error = Fallback(errorMessage,
                    $"{varName} must be less than or equal {expected}.");
                throw new ValidationException(error);
            }
        }

        public static void Equal(string actual, string expected, string name = "", string errorMessage = "")
        {
            if (actual != expected)
            {
                var varName = Fallback(name, "Value");
                string error = Fallback(errorMessage,
                    $"{varName} must equal {expected}.");
                throw new ValidationException(error);
            }
        }

        public static void In(string actual, string[] expectedValues, string name = "", string errorMessage = "")
        {
            if (!expectedValues.Contains(actual))
            {
                var varName = Fallback(name, "Value");
                string error = Fallback(errorMessage,
                    $"{varName} must equal following values: {string.Join(", ", expectedValues)}.");
                throw new ValidationException(error);
            }
        }

        public static void True(bool value, string name = "", string errorMessage = "")
        {
            if (!value)
            {
                var varName = Fallback(name, "Value");
                string error = Fallback(errorMessage,
                    $"{varName} must be true.");
                throw new ValidationException(error);
            }
        }

        public static void Equal(bool actual, bool expected, string name = "", string errorMessage = "")
        {
            if (actual != expected)
            {
                var varName = Fallback(name, "Value");
                string error = Fallback(errorMessage,
                    $"{varName} must equal {expected}.");
                throw new ValidationException(error);
            }
        }

        public static void NotEqual(string actual, string expected, string name = "", string errorMessage = "")
        {
            if (actual == expected)
            {
                var varName = Fallback(name, "Value");
                string error = Fallback(errorMessage,
                    $"{varName} can not equal {expected}.");
                throw new ValidationException(error);
            }
        }

        public static void RegEx(string value, string pattern, string name = "", string errorMessage = "")
        {
            if (!System.Text.RegularExpressions.Regex.IsMatch(value, pattern))
            {
                var varName = Fallback(name, "Value");
                string error = Fallback(errorMessage,
                    $"{varName} must match following pattern '{pattern}'.");
                throw new ValidationException(error);
            }
        }

        public static void NotNull(object obj, string name = "", string errorMessage = "")
        {
            if (obj == null)
            {
                var varName = Fallback(name, "Value");
                string error = Fallback(errorMessage,
                    $"{varName} must not be null.");
                throw new ValidationException(error);
            }
        }

        public static void Min(int actual, int expected, string name = "", string errorMessage = "")
        {
            if (actual < expected)
            {
                var varName = Fallback(name, "Value");
                string error = Fallback(errorMessage,
                    $"{varName} must be greater than {expected}.");
                throw new ValidationException(error);
            }
        }

        public static void PasswordStrength(string password)
        {
            RegEx(password, "^(?=.*[A-za-z])(?=.*[^A-Za-z]).{6,}$",
                errorMessage: "Password must contain at least 1 alphabetical character and 1 non-alphabetical character.");
        }

        private static string Fallback(params string[] values)
        {
            foreach (var value in values)
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value;
                }
            }

            return string.Empty;
        }
    }
}
