using Serilog;
using Worker.Models;
using Worker.Services.Database;

namespace Worker.Settings;

public class ScanEventSettings
{
    public string Uri { set; get; }

    public string DefaultLimit { set; get; }
}
