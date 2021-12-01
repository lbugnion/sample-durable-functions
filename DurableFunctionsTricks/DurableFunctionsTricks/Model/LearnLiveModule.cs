using Newtonsoft.Json;

namespace DurableFunctionsTricks.Model
{
    public class LearnLiveModule
    {
        public LearnLiveSeries Parent { get; set; }

        public string Title { get; set; }

        public LearnLiveModule(string title)
        {
            Title = title;
        }
    }
}