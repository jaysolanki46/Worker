namespace Worker.Settings;

/* Database connection settings - mapping configuration values from appsettings.json */
public class DatabaseConnectionSettings
{
    public string? ConnectionString { get; set; }
    public string? Database { get; set; }
}
