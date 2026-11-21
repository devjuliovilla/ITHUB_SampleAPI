using ITHUB.SampleAPI.Application.Contracts.Tasks;
using ITHUB.SampleAPI.Domain.Entities;
using Mapster;

namespace ITHUB.SampleAPI.Application.Mapping;

public static class SampleApiMappingConfiguration
{
    public static TypeAdapterConfig Create()
    {
        var configuration = new TypeAdapterConfig();
        configuration.NewConfig<StatusEntity, StatusDto>();
        configuration.NewConfig<TaskActivityEntity, TaskActivityDto>();
        configuration.NewConfig<TaskEntity, TaskDto>();
        return configuration;
    }
}
