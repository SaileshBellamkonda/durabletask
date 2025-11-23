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
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
///     Helper class for supporting deserialization from JSON into a custom class hierarchy
/// </summary>
internal abstract class JsonCreationConverter<T> : JsonConverter<T> where T : class
{
    public override bool CanConvert(Type objectType)
    {
        return typeof(T).IsAssignableFrom(objectType);
    }

    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            return null;
        }

        // Parse the JSON to a JsonDocument to allow inspection
        using JsonDocument document = JsonDocument.ParseValue(ref reader);
        JsonElement rootElement = document.RootElement;

        // Create target object based on JsonElement
        T? target = CreateObject(typeToConvert, rootElement);
        
        if (target == null)
        {
            return null;
        }

        // Deserialize the properties into the target object
        string json = rootElement.GetRawText();
        JsonSerializer.Deserialize(json, target.GetType(), options);
        
        // Manually populate properties from the root element
        foreach (JsonProperty property in rootElement.EnumerateObject())
        {
            var propertyInfo = target.GetType().GetProperty(property.Name);
            if (propertyInfo != null && propertyInfo.CanWrite)
            {
                object? value = JsonSerializer.Deserialize(property.Value.GetRawText(), propertyInfo.PropertyType, options);
                propertyInfo.SetValue(target, value);
            }
        }

        return target;
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, value.GetType(), options);
    }

    /// <summary>
    ///     Create an instance of objectType, based properties in the JSON object
    /// </summary>
    protected abstract T? CreateObject(Type objectType, JsonElement jsonElement);
}