﻿param([switch]$pack)

$ErrorActionPreference = "Stop"

function step($command) {
    write-host ([Environment]::NewLine + $command.ToString().Trim()) -fore CYAN
    & $command
    if ($lastexitcode -ne 0) { throw $lastexitcode }
}

function analyze-msbuild-properties {
    $projects = ("Fixie.Assertions", "Fixie.Assertions.Tests")
    $properties = (
        "Deterministic",
        "ContinuousIntegrationBuild",
        "EmbedUntrackedSources",
        "DebugType",
        "IncludeSymbols",
        "SymbolPackageFormat"
        )

    foreach ($project in $projects) {
        write-host "=============================================="
        write-host "Analyzing $project Properties"
        write-host
    
        foreach ($property in $properties) {
            write-host $property
            dotnet build src/$project -c Release --nologo --getProperty:$property
            write-host
        }
    }
}

if (test-path packages) { remove-item packages -Recurse }

step { dotnet tool restore }
step { dotnet clean src -c Release --nologo -v minimal }
step { dotnet build src -c Release --nologo --tl }
step { dotnet fixie *.Tests -c Release --no-build }

if ($pack) {
    step { dotnet pack src/Fixie.Assertions -o packages -c Release --no-build --nologo }

    analyze-msbuild-properties

    step { dotnet tool install dotnet-validate --version 0.0.1-preview.304  }
    $packages = get-childitem packages/*.nupkg
    foreach ($package in $packages) {
        dotnet validate package local $package
    }
}