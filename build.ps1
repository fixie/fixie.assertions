$ErrorActionPreference = "Stop"

function step($command) {
    write-host ([Environment]::NewLine + $command.ToString().Trim()) -fore CYAN
    & $command
    if ($lastexitcode -ne 0) { throw $lastexitcode }
}

step { dotnet tool restore }
step { dotnet clean src -c Release --nologo -v minimal }
step { dotnet build src -c Release --nologo --tl }
step { dotnet fixie *.Tests -c Release --no-build }
