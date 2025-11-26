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
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

/// <summary>
/// Class for serializing and deserializing data to and from json
/// </summary>
public class JsonDataConverter : DataConverter
{
    /// <summary>
    /// Default JsonDataConverter
    /// </summary>
    public static readonly JsonDataConverter Default = new JsonDataConverter();

    readonly JsonSerializerOptions options;

    /// <summary>
    /// Creates a new instance of the JsonDataConverter with default settings
    /// </summary>
    public JsonDataConverter()
        : this(CreateDefaultOptions())
    { }

    /// <summary>
    /// Creates default JsonSerializerOptions with polymorphic serialization and type resolution
    /// </summary>
    private static JsonSerializerOptions CreateDefaultOptions()
    {
        var options = new JsonSerializerOptions
        {
            // Enable polymorphic serialization with $type discriminator
            TypeInfoResolver = new PolymorphicTypeResolver(),
            // Write indented is false by default for performance
            WriteIndented = false,
            // Default handling for null values
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.Never,
            // Preserve references to handle circular references
            ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve,
            // Enum as strings for readability
            Converters =
            {
                new System.Text.Json.Serialization.JsonStringEnumConverter()
            }
        };
        
        return options;
    }

    /// <summary>
    /// Creates a new instance of the JsonDataConverter with supplied options
    /// </summary>
    /// <param name="options">Options for the json serializer</param>
    public JsonDataConverter(JsonSerializerOptions options)
    {
        this.options = options;
    }

    /// <summary>
    /// Serialize an Object to string with default formatting
    /// </summary>
    /// <param name="value">Object to serialize</param>
    /// <returns>Object serialized to a string</returns>
    public override string Serialize(object value)
    {
        return Serialize(value, false);
    }

    /// <summary>
    /// Serialize an Object to string with supplied formatting
    /// </summary>
    /// <param name="value">Object to serialize</param>
    /// <param name="formatted">Boolean indicating whether to format the results or not</param>
    /// <returns>Object serialized to a string</returns>
    public override string Serialize(object value, bool formatted)
    {
        if (value == null)
        {
            // This avoids serializing null into "null"
            return null;
        }

        var optionsToUse = formatted 
            ? new JsonSerializerOptions(this.options) { WriteIndented = true }
            : this.options;

        return System.Text.Json.JsonSerializer.Serialize(value, value.GetType(), optionsToUse);
    }

    /// <summary>
    /// Deserialize a string to an Object of supplied type
    /// </summary>
    /// <param name="data">String data of the Object to deserialize</param>
    /// <param name="objectType">Type to deserialize to</param>
    /// <returns>Deserialized Object</returns>
    public override object Deserialize(string data, Type objectType)
    {
        if (data == null)
        {
            return null;
        }

        return System.Text.Json.JsonSerializer.Deserialize(data, objectType, this.options);
    }
}

/// <summary>
/// Custom type resolver for polymorphic serialization that adds $type property
/// compatible with Newtonsoft.Json TypeNameHandling.Objects
/// </summary>
internal class PolymorphicTypeResolver : DefaultJsonTypeInfoResolver
{
    public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
    {
        JsonTypeInfo jsonTypeInfo = base.GetTypeInfo(type, options);

        // Add type discriminator handling for compatibility with Newtonsoft.Json format
        if (jsonTypeInfo.Kind == JsonTypeInfoKind.Object)
        {
            jsonTypeInfo.PolymorphismOptions = new JsonPolymorphismOptions
            {
                TypeDiscriminatorPropertyName = "$type",
                IgnoreUnrecognizedTypeDiscriminators = true,
                UnknownDerivedTypeHandling = System.Text.Json.Serialization.JsonUnknownDerivedTypeHandling.FallBackToNearestAncestor,
            };
        }

        return jsonTypeInfo;
    }
}
