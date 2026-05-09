namespace tennis_wave_api.Models;

/// <summary>
/// MongoDB configuration settings
/// </summary>
public class MongoDbSettings
{
    public const string SectionName = "MongoDb";
    
    /// <summary>
    /// Database name
    /// </summary>
    public string DatabaseName { get; set; } = "prompt_generator";
    
    /// <summary>
    /// Connection string
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;
    
    /// <summary>
    /// Collection names
    /// </summary>
    public CollectionNames Collections { get; set; } = new();
}

/// <summary>
/// MongoDB collection names
/// </summary>
public class CollectionNames
{
    /// <summary>
    /// Users collection name
    /// </summary>
    public string Users { get; set; } = "users";
    
}
