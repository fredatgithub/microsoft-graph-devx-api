﻿using System;
using System.Linq;
using System.Text.RegularExpressions;
using CodeSnippetsReflection.StringExtensions;

namespace CodeSnippetsReflection.OpenAPI;

internal static class StringExtensions 
{
    internal static bool IsCollectionIndex(this string pathSegment) =>
        !string.IsNullOrEmpty(pathSegment) && pathSegment.StartsWith('{') && pathSegment.EndsWith('}');
    internal static bool IsFunction(this string pathSegment) => !string.IsNullOrEmpty(pathSegment) && pathSegment.Contains('.');

    private static readonly Regex FunctionWithParameterRegex = new(@"\([\w\s\d=':${}<>|\-,]+\)", RegexOptions.Compiled, TimeSpan.FromMilliseconds(200));
    internal static bool IsFunctionWithParameters(this string pathSegment) => !string.IsNullOrEmpty(pathSegment) 
                                                                              && FunctionWithParameterRegex.Match(pathSegment).Success;

    internal static bool IsFunctionWithParametersMatch(this string pathSegment, string segment)
    {
        // verify both have parameters
        if (!pathSegment.IsFunctionWithParameters() || !segment.IsFunctionWithParameters())
            return false;
        
        // verify both have same prefix/name
        if (!pathSegment.Split('(').First().Equals(segment.Split('(').First(), StringComparison.OrdinalIgnoreCase))
            return false;

        var originalParameters = pathSegment.Split('(').Last().TrimEnd(')').Split(',').Select(static s => s.Split('=').First());
        var compareParameters = segment.Split('(').Last().TrimEnd(')').Split(',').Select(static s => s.Split('=').First());

        return compareParameters.All(parameter => originalParameters.Contains(parameter.Split('=').First(), StringComparer.OrdinalIgnoreCase));
    }
    internal static string RemoveFunctionBraces(this string pathSegment) => pathSegment.TrimEnd('(',')');
    internal static string ReplaceValueIdentifier(this string original) =>
        original?.Replace("$value", "Content", StringComparison.Ordinal);
   
    internal static string Append(this string original, string suffix) =>
        string.IsNullOrEmpty(original) ? original : original + suffix;
        
    private static readonly Regex PropertyCleanupRegex = new(@"[""\s!#$%&'()*+,./:;<=>?@\[\]\\^`{}|~-](?<followingLetter>\w)?", RegexOptions.Compiled, TimeSpan.FromSeconds(5));
    private const string CleanupGroupName = "followingLetter";
    internal static string CleanupSymbolName(this string original)
    {
        if (string.IsNullOrEmpty(original))
            return original;

        var result = PropertyCleanupRegex.Replace(original, static x => x.Groups.Keys.Contains(CleanupGroupName) ? 
            x.Groups[CleanupGroupName].Value.ToFirstCharacterUpperCase() :
            string.Empty); //strip out any invalid characters, and replace any following one by its uppercase version

        return result;
    }
}
