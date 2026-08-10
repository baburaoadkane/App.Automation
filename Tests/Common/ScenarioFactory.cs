using App.Automation.Core.Utilities;
using NUnit.Framework;

namespace App.Automation.Tests.Common;

public static class ScenarioFactory
{
    public static IEnumerable<TestCaseData> FromFolder(
        string folderPath)
    {
        IEnumerable<string> files;

        try
        {
            files = JsonLoader.GetAllFiles(folderPath);
        }
        catch
        {
            yield break;
        }

        foreach (string filePath in files)
        {
            string testName =
                Path.GetFileNameWithoutExtension(filePath);

            yield return new TestCaseData(filePath)
                .SetName(testName)
                .SetDescription(testName);
        }
    }
}