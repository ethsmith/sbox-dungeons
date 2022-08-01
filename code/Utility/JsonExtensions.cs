
using System.Text.Json;

namespace Dungeons.Utility;

internal static class JsonExtensions
{
	public static T JsonDeserialize<T>( this string json ) => JsonSerializer.Deserialize<T>( json );
	public static string JsonSerialize( this object obj ) => JsonSerializer.Serialize( obj );
	public static T JsonClone<T>( this T obj ) => JsonDeserialize<T>( JsonSerialize( obj ) );
}
