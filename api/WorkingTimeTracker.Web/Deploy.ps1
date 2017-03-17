$connectionString = $OctopusParameters["SQL.WorkingTimeTracker.DeployConnectionString"]

.\bin\migrate.exe WorkingTimeTracker.Implementations.dll /connectionString="$($connectionString)" /connectionProviderName="System.Data.SqlClient" /verbose