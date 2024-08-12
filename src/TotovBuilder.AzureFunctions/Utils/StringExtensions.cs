﻿using System.Text;
using System.Text.RegularExpressions;

namespace TotovBuilder.AzureFunctions.Utils
{
    /// <summary>
    /// Represents extension methods for the <see cref="string"/> type.
    /// </summary>
    public static partial class StringExtensions
    {
        /// <summary>
        /// Changes the case of a string to match pascal case.
        /// </summary>
        /// <param name="stringToFormat">String to format.</param>
        /// <returns>Formatted string.</returns>
        public static string ToPascalCase(this string stringToFormat)
        {
            stringToFormat = stringToFormat.Trim();
            string camelCaseString = ToPascalCaseRegex().Replace(stringToFormat, m =>
                {
                    StringBuilder result = new();

                    foreach (Group? group in m.Groups.Cast<Group?>())
                    {
                        switch (group!.Name)
                        {
                            case "M1L1":
                                result.Append(group.Value);
                                break;
                            case "M1R":
                                result.Append(group.Value.ToLowerInvariant());
                                break;
                            case "M2":
                                if (group.Length > 0)
                                {
                                    string value = group.Value.Replace(" ", string.Empty);
                                    result.Append(group.Value[0]);
                                    result.Append(group.Value[1..].ToUpperInvariant());
                                }

                                break;
                            case "M3":
                                result.Append(group.Value.ToUpperInvariant());
                                break;
                        }
                    }

                    return result.ToString();
                });

            return camelCaseString.Replace(" ", string.Empty);
        }

        [GeneratedRegex("((?'M1L1'^[A-Z])(?'M1R'[A-Z]+$))|(?'M2'[ ][a-zA-Z])|(?'M3'^[a-z])")]
        private static partial Regex ToPascalCaseRegex();
    }
}
