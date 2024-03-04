using Microsoft.Extensions.Options;
using Worker.Settings;

namespace Worker.Functions;

public class ParcelEventWorker
{
    private readonly DatabaseConnectionSettings _databaseSettings;

    public ParcelEventWorker(DatabaseConnectionSettings databaseSettings)
    {
        _databaseSettings = databaseSettings;
    }

    public async Task ExecuteAsync()
    {
        var str = _databaseSettings.ConnectionString;
        Console.WriteLine(str);
    }

}
