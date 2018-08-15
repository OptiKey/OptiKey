using System.Linq;
using System.Reflection;
using log4net;

namespace JuliusSweetland.OptiKey.Services
{
    public class WebApiService : IWebApiService
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly ISuggestionStateService suggestionStateService;

        public WebApiService(ISuggestionStateService suggestionStateService)
        {
            this.suggestionStateService = suggestionStateService;
        }

        public void SetSuggestions(string[] suggestions)
        {
            if (suggestions == null)
            {
                Log.ErrorFormat("SetSuggestions called with no suggestions. Doing nothing.");
            }
            else
            {
                Log.InfoFormat("SetSuggestions called with {0} suggestions.", suggestions.Length);
                suggestionStateService.Suggestions = suggestions.ToList();
            }
        }
    }
}
