#!/usr/bin/env dotnet-script

using System.Text.RegularExpressions;

var commitMsgFile = Args[0].Trim(',');
var commitMessage = File.ReadAllText(commitMsgFile);

//conventional commits format
var pattern = @"^(feat|fix|docs|style|refactor|test|chore|perf|ci|build|revert)(\(.+\))?: .{1,}$";

if (!Regex.IsMatch(commitMessage.Split('\n')[0], pattern))
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("Invalid commit message format!");
    Console.ResetColor();
    Console.WriteLine("\nCommit message must follow conventional commits format:");
    Console.WriteLine("  <type>[optional scope]: <description>\n");
    Console.WriteLine("Types: feat, fix, docs, style, refactor, test, chore, perf, ci, build, revert");
    Console.WriteLine("\nExamples:");
    Console.WriteLine("  feat: add user login");
    Console.WriteLine("  fix(auth): resolve token expiration issue");
    Environment.Exit(1);
}

Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine("✓ Commit message is valid");
Console.ResetColor();