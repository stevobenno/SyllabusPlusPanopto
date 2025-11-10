using AutoMapper;

namespace SyllabusPlusPanopto.Transform.Services;

public sealed class AutoMapperTransformService : ITransformService
{
    private readonly IMapper _mapper;

    public AutoMapperTransformService(IMapper mapper) => _mapper = mapper;

    public SpTransformed Transform(SpRawRow row)
    {
        // Later: map into a richer DTO if needed, for now create a stable key
        var key = $"{row.ModuleCode}|{row.StartUtc:O}";
        return new SpTransformed(key, row);
    }
}
