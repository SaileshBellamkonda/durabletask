//  ----------------------------------------------------------------------------------
//  Copyright Microsoft Corporation
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//  http://www.apache.org/licenses/LICENSE-2.0
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
//  ----------------------------------------------------------------------------------

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DurableTask.Core.Serialization;
/// <summary>
/// Provides consistent JSON serialization settings across DurableTask.
/// Configured to match Newtonsoft.Json defaults where possible for compatibility.
/// </summary>
public static class JsonSettings
{
    private static readonly Lazy<JsonSerializerOptions> _defaultOptions = new(() =>
    {
        var options = new JsonSerializerOptions
        {
            // Match Newtonsoft.Json defaults where possible for compatibility
            PropertyNamingPolicy = null, // Use PascalCase (same as Newtonsoft default)
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.Never,
            PropertyNameCaseInsensitive = true, // More forgiving than Newtonsoft
            Converters =
            {
                new JsonStringEnumConverter() // Serialize enums as strings (Newtonsoft default)
            }
        };
        
        return options;
    });

    /// <summary>
    /// Gets the default JSON serialization options for DurableTask.
    /// Uses PascalCase property naming and serializes enums as strings for compatibility.
    /// </summary>
    public static JsonSerializerOptions Default => _defaultOptions.Value;
    
    /// <summary>
    /// Gets JSON serialization options with indented formatting.
    /// Useful for debugging and logging.
    /// </summary>
    public static JsonSerializerOptions Indented
    {
        get
        {
            var options = new JsonSerializerOptions(Default)
            {
                WriteIndented = true
            };
            return options;
        }
    }
}
