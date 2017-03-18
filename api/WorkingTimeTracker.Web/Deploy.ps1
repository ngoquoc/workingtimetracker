$authConnectionString = $OctopusParameters["SQL.Auth.ConnectionString"]
$wttConnectionString = $OctopusParameters["SQL.WorkingTimeTracker.ConnectionString"]

Write-Host "Updating Auth DB"
.\bin\migrate.exe WorkingTimeTracker.Implementations.dll ConfigurationAuth /connectionString="$($authConnectionString)" /connectionProviderName="System.Data.SqlClient" /verbose

Write-Host "Updating Working time tracker DB"
.\bin\migrate.exe WorkingTimeTracker.Implementations.dll ConfigurationWorkingTimeTracker /connectionString="$($wttConnectionString)" /connectionProviderName="System.Data.SqlClient" /verbose