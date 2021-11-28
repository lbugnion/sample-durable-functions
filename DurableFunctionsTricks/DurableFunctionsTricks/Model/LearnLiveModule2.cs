using Newtonsoft.Json;

namespace DurableFunctionsTricks.Model
{
    public class LearnLiveModule2
    {
        [JsonIgnore]
        public LearnLiveSeries Parent { get; set; }

        public string Title { get; private set; }

        public LearnLiveModule2(string title)
        {
            Title = title;
        }
    }
}