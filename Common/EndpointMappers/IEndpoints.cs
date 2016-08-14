using Microsoft.AspNetCore.Builder;

namespace Common.EndpointMappers
{
    public interface IEndpoints
    {
        void MapEndpoints(IApplicationBuilder app);
    }
}
