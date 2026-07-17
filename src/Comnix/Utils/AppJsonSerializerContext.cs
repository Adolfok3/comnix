using System.Text.Json.Serialization;

namespace Comnix.Utils;

[JsonSerializable(typeof(string))]
internal sealed partial class AppJsonSerializerContext : JsonSerializerContext;
