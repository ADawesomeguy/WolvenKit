using WolvenKit.Common;
using WolvenKit.Common.Interfaces;
using WolvenKit.Common.Services;
using WolvenKit.Core.Services;

namespace WolvenKit.Modkit.RED3
{
    public partial class Red3ModTools : IRed3ModTools
    {
        private readonly ILoggerService _loggerService;
        private readonly IProgressService<double> _progressService;

        public Red3ModTools(
            ILoggerService loggerService,
            IProgressService<double> progressService
        )
        {
            _loggerService = loggerService;
            _progressService = progressService;
        }
    }
}
