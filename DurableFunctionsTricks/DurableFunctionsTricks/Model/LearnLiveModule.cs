using Newtonsoft.Json;

namespace DurableFunctionsTricks.Model
{
    public class LearnLiveModule
    {
        [JsonIgnore]
        public LearnLiveSeries Parent { get; set; }

        public string Title { get; set; }

        public LearnLiveModule(string title)
        {
            Title = title;
        }

        public LearnLiveModule(LearnLiveSeries parent, string title)
        {
            Parent = parent;
            Title = title;
        }
    }
}