namespace OceanApocalypse.RSML.Abstractions.Cache;

/// <summary>
/// Represents a service or a type that supports cached data.
/// </summary>
public interface ISupportsCache
{
	/// <summary>
	/// Whether there's cached data.
	/// </summary>
	bool CacheExists { get; }

	/// <summary>
	/// Builds the cache if it doesn't exist yet.
	/// </summary>
	void BuildCache();

	/// <summary>
	/// Builds the cache. If <paramref name="forceRebuild"/> is set to <c>true</c>,
	/// the cache will be built even if it already exists.
	/// </summary>
	/// <param name="forceRebuild">Whether to force the cache to be built even if it exists.</param>
	void BuildCache(bool forceRebuild);
}
