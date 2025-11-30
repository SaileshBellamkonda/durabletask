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
using System.Text.Json.Nodes;

/// <summary>
///     Helper class for supporting deserialization from JSON into a custom class hierarchy
/// </summary>
internal abstract class JsonCreationConverter<T> : System.Text.Json.Serialization.JsonConverter<T> where T : class
{
    public override bool CanConvert(Type objectType)
    {
        return typeof(T).IsAssignableFrom(objectType);
    }

    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            return null;
        }

        // Parse the JSON into a JsonObject for inspection
        JsonObject jsonObject = JsonNode.Parse(ref reader)?.AsObject();
        if (jsonObject == null)
        {
            return null;
        }

        // Create target object based on JsonObject properties
        T target = CreateObject(typeToConvert, jsonObject);

        // Deserialize the properties into the target object
        JsonSerializer.Deserialize(jsonObject.ToJsonString(), target.GetType(), options);
        
        // Populate target with values from jsonObject
        string jsonString = jsonObject.ToJsonString();
        target = JsonSerializer.Deserialize(jsonString, target.GetType(), options) as T;

        return target;
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        // Write using default serialization
        JsonSerializer.Serialize(writer, value, value.GetType(), options);
    }

    /// <summary>
    ///     Create an instance of objectType, based properties in the JSON object
    /// </summary>
    protected abstract T CreateObject(Type objectType, JsonObject jsonObject);
}
