namespace DurableFunctionsTricks.Model
{
    public class LearnLiveModule
    {
        public LearnLiveSeries Parent { get; private set; }

        public string Title { get; private set; }

        public LearnLiveModule(LearnLiveSeries parent, string title)
        {
            Parent = parent;
            Title = title;
        }
    }
}