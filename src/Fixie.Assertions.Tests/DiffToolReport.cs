﻿using Fixie.Reports;
using DiffEngine;

namespace Tests;

class DiffToolReport : IHandler<TestFailed>, IHandler<ExecutionCompleted>
{
    int failures;
    Exception? singleFailure;

    public Task Handle(TestFailed message)
    {
        failures++;

        singleFailure = failures == 1 ? message.Reason : null;

        return Task.CompletedTask;
    }

    public async Task Handle(ExecutionCompleted message)
    {
        if (singleFailure is ComparisonException failure)
        {
            if (failure.HasMultilineRepresentation)
                await LaunchDiffTool(failure.Expected, failure.Actual);
        }
        else if (singleFailure is ContradictionException contradiction)
        {
            await LaunchDiffTool(contradiction.Expected, contradiction.Actual);
        }
    }

    static async Task LaunchDiffTool(string expected, string actual)
    {
        var tempPath = Path.GetTempPath();
        var expectedPath = Path.Combine(tempPath, "expected.txt");
        var actualPath = Path.Combine(tempPath, "actual.txt");

        File.WriteAllText(expectedPath, expected);
        File.WriteAllText(actualPath, actual);

        await DiffRunner.LaunchAsync(expectedPath, actualPath);
    }
}