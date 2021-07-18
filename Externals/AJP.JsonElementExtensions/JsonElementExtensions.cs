using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Buffers;
using System.Collections;
using System.IO;

namespace AJP
{
    /// <summary>
    /// Methods which allow the addition or removal of properties on a JsonElement.
    /// JsonElement is immutable, so these methods work by enumerating the existing properties and writing them into a new jsonstring in memory.
    /// Additional properties can be added and existing properties can be removed and the resulting string is parsed into a new JsonElement which is returned.
    /// Please note this roundtrip process happens for every call, so if lots of changes are needed, please consider/test using ParseAsJsonStringAndMutate()
    /// OR ParseAsJsonStringAndMutatePreservingOrder() so that all changes can be done together, minimising roudtrip processes.
    /// A new JsonElement is returned, the original is unchanged.
    /// </summary>
    public static class JsonElementExtensions
    {
        /// <summary>
        /// Method which recreates a new JsonElement from an existing one, with an extra null valued property added to the start of the list of properties.
        /// If you care about where the new property should appear in the list of properties, use InsertNullProperty(), although its a slightly more expensive operation.
        /// If you have multiple changes to make to the JsonElement, please consider/test using ParseAsJsonStringAndMutate() 
        /// so that all changes can be done together, with only one roudtrip process. 
        /// </summary>
        /// <param name="name">A string containing the name of the property to add</param>
        /// <param name="options">The json serializer options to respect.</param>
        /// <returns>A new JsonElement containing the old properties plus the new property</returns>
        public static JsonElement AddNullProperty(this JsonElement jElement, string name, JsonSerializerOptions options = null) =>
            jElement.ParseAsJsonStringAndMutate((utf8JsonWriter, _) => HandleNull(utf8JsonWriter, name, options));

        /// <summary>
        /// Method which recreates a new JsonElement from an existing one, with an extra property added to the start of the list of properties.
        /// If you care about where the new property should appear in the list of properties, use InsertProperty(), although its a slightly more expensive operation.
        /// If you have multiple changes to make to the JsonElement, please consider/test using ParseAsJsonStringAndMutate() 
        /// so that all changes can be done together, with only one roudtrip process. 
        /// </summary>
        /// <param name="name">A string containing the name of the property to add</param>
        /// <param name="value">The value of the property to add, primitives and simple objects are supported.</param>
        /// <param name="options">The serializer options to respect when converting values.</param>
        /// <returns>A new JsonElement containing the old properties plus the new property</returns>
        public static JsonElement AddProperty(this JsonElement jElement, string name, object value, JsonSerializerOptions options = null) =>
            jElement.ParseAsJsonStringAndMutate((utf8JsonWriter, _) =>
            {
                if (value is null)
                {
                    HandleNull(utf8JsonWriter, name, options);
                    return;
                }

                utf8JsonWriter.WritePropertyName(name);
                RenderValue(utf8JsonWriter, value, options);
            });

        /// <summary>
        /// Method which recreates a new JsonElement from an existing one, with an extra property inserted at a specified position in the list of properties.
        /// If you don't care about where the new property should appear in the list of properties, or if you care more about performance,
        /// use AddProperty(), which is a slightly less expensive operation.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="insertAt"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static JsonElement InsertProperty(this JsonElement jElement, string name, object value, int insertAt, JsonSerializerOptions options = null) =>
            jElement.ParseAsJsonStringAndMutatePreservingOrder(props => props.Insert(name, value, insertAt), options);

        /// <summary>
        /// Method which recreates a new JsonElement from an existing one, with an extra null property inserted at a specified position in the list of properties.
        /// If you don't care about where the new property should appear in the list of properties, or if you care more about performance,
        /// use AddNullProperty(), which is a slightly less expensive operation.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="insertAt"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static JsonElement InsertNullProperty(this JsonElement jElement, string name, int insertAt, JsonSerializerOptions options = null) =>
            jElement.ParseAsJsonStringAndMutatePreservingOrder(props => props.Insert(name, null, insertAt), options);

        /// <summary>
        /// Method which recreates a new JsonElement from an existing one, with a property updated, preserving the order of the list of properties.
        /// If you don't care about preserving the order of the list of properties, or if you care more about performance,
        /// you could use ParseAsJsonStringAndMutate() which is a slightly less expensive operation.
        /// </summary>
        /// <param name="nameOfPropertyToUpdate"></param>
        /// <param name="newValue"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static JsonElement UpdateProperty(this JsonElement jElement, string nameOfPropertyToUpdate, object newValue, JsonSerializerOptions options = null) 
            => jElement.ParseAsJsonStringAndMutatePreservingOrder(props =>
        {
            var propToUpdate = props.FirstOrDefault(p => p.Name == nameOfPropertyToUpdate);
            if (propToUpdate is null)
                throw new ArgumentException($"Could not find a property named {nameOfPropertyToUpdate} in the list.");
            propToUpdate.Value = newValue;
        }, options ?? new JsonSerializerOptions());
        
        /// <summary>
		/// Method which recreates a new JsonElement from an existing one, but without one of the exiting properties
		/// </summary>
		/// <param name="nameOfPropertyToRemove">A string containing the name of the property to remove</param>
		/// <returns>A new JsonElement containing the old properties apart from the named property to remove</returns>
		public static JsonElement RemoveProperty(this JsonElement jElement, string nameOfPropertyToRemove) => 
			jElement.ParseAsJsonStringAndMutate((writer, namesOfPropertiesToRemove) => namesOfPropertiesToRemove.Add(nameOfPropertyToRemove));

		/// <summary>
		/// Method which recreates a new JsonElement from an existing one, but without some of the exiting properties
		/// </summary>
		/// <param name="propertyNamesToRemove">A list of names of the properties to remove</param>
		/// <returns>A new JsonElement without the properties listed</returns>
		public static JsonElement RemoveProperties(this JsonElement jElement, IEnumerable<string> propertyNamesToRemove) =>
			jElement.ParseAsJsonStringAndMutate((writer, namesOfPropertiesToRemove) => namesOfPropertiesToRemove.AddRange(propertyNamesToRemove));

        /// <summary>
        /// Method which recreates a new JsonElement from an existing one, with the opportunity to add new properties to the start of the object
        /// and remove existing properties. New properties will be added to the beginning of the list, if you care about the order of the properties,
        /// use ParseAsJsonStringAndMutatePreservingOrder() however that method is slightly more expensive in terms of time and allocation.
        /// </summary>
        /// <param name="mutate">An Action of Utf8JsonWriter and List of strings. 
        /// The Utf8JsonWriter allows the calling code to write additional properties, its possible to add highly complex nested structures,
        /// the list of strings is a list names of any existing properties to be removed from the resulting JsonElement</param>
        /// <returns>A new JsonElement</returns>
        public static JsonElement ParseAsJsonStringAndMutate(this JsonElement jElement, Action<Utf8JsonWriter, List<string>> mutate)
		{
			if (jElement.ValueKind != JsonValueKind.Object)
				throw new Exception("Only able to add properties to json objects (i.e. jElement.ValueKind == JsonValueKind.Object)");

            var arrayBufferWriter = new MemoryStream();// ArrayBufferWriter<byte>();
            using (var jsonWriter = new Utf8JsonWriter(arrayBufferWriter))
            {
                jsonWriter.WriteStartObject();

                var namesOfPropertiesToRemove = new List<string>();

                mutate?.Invoke(jsonWriter, namesOfPropertiesToRemove);

                foreach (var jProp in jElement.EnumerateObject())
                {
                    if (!(namesOfPropertiesToRemove.Contains(jProp.Name)))
                    {
                        jProp.WriteTo(jsonWriter);
                    }
                }
                jsonWriter.WriteEndObject();
            }
            var resultJson = Encoding.UTF8.GetString(arrayBufferWriter.ToArray()/* .WrittenSpan*/);
            return JsonDocument.Parse(resultJson).RootElement;
        }

        /// <summary>
        /// Method which recreates a new JsonElement from an existing one,
        /// with the opportunity to add, remove and change properties while preserving the order of the properties.
        /// This method is slightly more expensive in terms of time and allocation, than ParseAsJsonStringAndMutate()
        /// </summary>
        /// <param name="mutateProps">An Action on a list of Name/Value.
        /// This list contains the properties from the JsonElement in order, items can be added, removed or updated.
        /// The resulting JsonElement will be built from the mutated list of properties. 
        /// </param>
        /// <param name="options">JsonSerializerOptions that will be respected when a value is rendered.</param>
        /// <returns></returns>
        public static JsonElement ParseAsJsonStringAndMutatePreservingOrder(this JsonElement jElement, Action<PropertyList> mutateProps, JsonSerializerOptions options)
        {
            if (jElement.ValueKind != JsonValueKind.Object)
                throw new Exception("Only able to add properties to json objects (i.e. jElement.ValueKind == JsonValueKind.Object)");
            
            var props = new PropertyList();
            foreach (var prop in jElement.EnumerateObject())
            {
                props.Add(prop.Name, prop.Value);
            }

			mutateProps.Invoke(props);

            var arrayBufferWriter = new MemoryStream();// ArrayBufferWriter<byte>();
            using (var jsonWriter = new Utf8JsonWriter(arrayBufferWriter))
            {
                jsonWriter.WriteStartObject();

                foreach (Property prop in props)
                {
                    if (prop.Value is null)
                    {
                        HandleNull(jsonWriter, prop.Name, options);
                    }
                    else
                    {
                        jsonWriter.WritePropertyName(options?.PropertyNamingPolicy?.ConvertName(prop.Name) ?? prop.Name);
                        if (prop.Value is JsonElement jProp)
                        {
                            jProp.WriteTo(jsonWriter);
                        }
                        else
                        {
                            RenderValue(jsonWriter, prop.Value, options);
                        }
                    }
                }

                jsonWriter.WriteEndObject();
			}
            var resultJson = Encoding.UTF8.GetString(arrayBufferWriter.ToArray()/*.WrittenSpan*/);
            return JsonDocument.Parse(resultJson).RootElement;
        }
		
        /// <summary>
		/// Method which returns a list of property name and value, from a given object
		/// </summary>
		public static IEnumerable<(string Name, object Value)> GetProperties(this object source)
        {
            if (source is IDictionary<string, object> dictionary)
			{
				return dictionary.Select(x => (x.Key, x.Value));
			}
			
			return source.GetType()
				.GetProperties(BindingFlags.Public | BindingFlags.Instance)
				.Where(p => !p.GetGetMethod().GetParameters().Any())
				.Select(x => (x.Name, x.GetValue(source)));
		}
		
		/// <summary>
		/// Method which attempts to convert a given JsonElement into the specified type
		/// </summary>
		/// <param name="jElement">The JsonElement to convert</param>
		/// <param name="options">JsonSerializerOptions to use</param>
		/// <typeparam name="T">The specified type</typeparam>
		/// <returns></returns>
		public static T ConvertToObject<T>(this JsonElement jElement, JsonSerializerOptions options = null)
		{
            var arrayBufferWriter = new MemoryStream();// ArrayBufferWriter<byte>();
			using (var writer = new Utf8JsonWriter(arrayBufferWriter))
				jElement.WriteTo(writer);
			
			return JsonSerializer.Deserialize<T>(arrayBufferWriter.ToArray()/*.WrittenSpan*/, options);
		}

		/// <summary>
		/// Method which attempts to convert a given JsonDocument into the specified type
		/// </summary>
		/// <param name="jDocument">The JsonDocument to convert</param>
		/// <param name="options">JsonSerializerOptions to use</param>
		/// <typeparam name="T">The specified type</typeparam>
		/// <returns>An instance of the specified type from the supplied JsonDocument</returns>
		/// <exception cref="ArgumentNullException">Thrown if the JsonDocument cannot be dserialised into the specified type</exception>
		public static T ConvertToObject<T>(this JsonDocument jDocument, JsonSerializerOptions options = null)
		{
			if (jDocument == null)
				throw new ArgumentNullException(nameof(jDocument));
			
			return jDocument.RootElement.ConvertToObject<T>(options);
		}

        private static void RenderValue(this Utf8JsonWriter writer, object value, JsonSerializerOptions options = null)
        {
            // The value is not a primitive.
            if (Convert.GetTypeCode(value) == TypeCode.Object && !(value is IEnumerable))
            {
                writer.WriteStartObject();
                foreach (var (propName, propValue) in value.GetProperties())
                {
                    if (propValue is null)
                    {
                        HandleNull(writer, propName, options);
                        continue;
                    }

                    writer.WritePropertyName(options?.PropertyNamingPolicy.ConvertName(propName) ?? propName);
                    writer.RenderValue(propValue);
                }

                writer.WriteEndObject();
                return;
            }

            switch (value)
            {
                case string v:
                    writer.WriteStringValue(v);
                    break;
                case bool v:
                    writer.WriteBooleanValue(v);
                    break;
                case decimal v:
                    writer.WriteNumberValue(v);
                    break;
                case int v:
                    writer.WriteNumberValue(v);
                    break;
                case double v:
                    writer.WriteNumberValue(v);
                    break;
                case float v:
                    writer.WriteNumberValue(v);
                    break;
                case DateTime v:
                    writer.WriteStringValue(v);
                    break;
                case Guid v:
                    writer.WriteStringValue(v);
                    break;
                case IEnumerable<object> arr:
                    writer.WriteStartArray();
                    arr.ToList()
                        .ForEach(obj => RenderValue(writer, obj));
                    writer.WriteEndArray();
                    break;
                default:
                    writer.WriteStringValue(value.ToString());
                    break;
            }
        }

        private static void HandleNull(Utf8JsonWriter writer, string propName, JsonSerializerOptions options = null)
        {
            if (options?.IgnoreNullValues == true)
                return;

            writer.WriteNull(options?.PropertyNamingPolicy?.ConvertName(propName) ?? propName);
        }
    }
}
