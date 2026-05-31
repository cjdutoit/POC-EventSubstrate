// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

using StudentApp.Infrastructure.Services;

namespace StudentApp.Infrastructure
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var scriptGenerationService = new ScriptGenerationService();

            scriptGenerationService.GenerateBuildScript(
                branchName: "main",
                projectName: "StudentApp.Core",
                dotNetVersion: "10.x");

            scriptGenerationService.GeneratePrLintScript(branchName: "main");
        }
    }
}
