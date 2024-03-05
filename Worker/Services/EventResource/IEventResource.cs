using Worker.Services.Database;

namespace Worker.Services.EventResource;

/* IEventResource interface - includes the method to generate URI for API request */
public interface IEventResource
{
    Task<string> GenerateUriAsync();
}
