using AutoMapper;
using SyllabusPlusPanopto.Transform.Domain;
using SyllabusPlusPanopto.Transform.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace SyllabusPlusPanopto.Transform.TransformationServices
{
    public sealed class AutoMapperTransformService : ITransformService
    {
        private readonly IMapper _mapper;

        public AutoMapperTransformService(IMapper mapper)
        {
            _mapper = mapper;
        }

        public ScheduledSession Transform(SourceEvent sourceEvent)
        {
            var scheduledSession = _mapper.Map<ScheduledSession>(sourceEvent);
            scheduledSession.Raw = sourceEvent; // keep original for audit/troubleshooting
            return scheduledSession;
        }

        public IReadOnlyList<ScheduledSession> Transform(IEnumerable<SourceEvent> rows)
        {
            // materialise so we can attach Raw to each
            var list = rows.ToList();
            var mapped = _mapper.Map<List<ScheduledSession>>(list);

            for (int i = 0; i < mapped.Count; i++)
            {
                mapped[i].Raw = list[i];
            }

            return mapped;
        }
    }
}
