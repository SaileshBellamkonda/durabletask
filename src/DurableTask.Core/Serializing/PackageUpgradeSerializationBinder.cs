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

namespace DurableTask.Core.Serializing;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

/// <summary>
/// TypeInfoResolver to be used for deserializing DurableTask types that are pre v-2.0, this allows upgrade compatibility.
/// This is not sufficient to deserialize objects from 1.0 which had the Tags Property set.
/// </summary>
[ComVisible(false)]
public class PackageUpgradeTypeInfoResolver : DefaultJsonTypeInfoResolver
{
    static readonly Lazy<IDictionary<string, Type>> KnownTypes = new Lazy<IDictionary<string, Type>>(() =>
    {
        //Get all types in the DurableTask.Core Namespace
        return typeof(PackageUpgradeTypeInfoResolver).Assembly.GetTypes()
            .Where(t => t?.Namespace?.StartsWith("DurableTask.Core") ?? false)
            .ToDictionary(x => x.FullName ?? x.Name);
    });

    static readonly string CurrentAssemblyName = typeof(PackageUpgradeTypeInfoResolver).Assembly.GetName().Name ?? string.Empty;
    static readonly ISet<string> UpgradeableAssemblyNames = new HashSet<string> { "DurableTask", "DurableTaskFx" };

    /// <summary>
    /// Attempts to resolve a type from old assembly names to the current DurableTask.Core namespace
    /// </summary>
    public static Type? ResolveType(string? assemblyName, string? typeName)
    {
        Type? resolvedType = null;

        if (assemblyName != CurrentAssemblyName && !string.IsNullOrWhiteSpace(typeName))
        {
            //Separator Index if TypeNameAssemblyFormat Full
            int separatorIndex = assemblyName?.IndexOf(',') ?? -1;

            //If no assembly name is specified or this is a type from the v1.0 or vnext assemblies
            if (string.IsNullOrWhiteSpace(assemblyName) || 
                UpgradeableAssemblyNames.Contains(separatorIndex < 0 ? assemblyName : assemblyName!.Substring(0, separatorIndex)))
            {
                KnownTypes.Value.TryGetValue(typeName.Replace("DurableTask.", "DurableTask.Core."), out resolvedType);
            }
        }

        return resolvedType;
    }
}
