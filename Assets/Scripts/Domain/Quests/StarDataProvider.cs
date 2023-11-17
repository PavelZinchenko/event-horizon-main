namespace Domain.Quests
{
    public class StarDataProvider : IStarDataProvider
    {
        private readonly Galaxy.Star _star;
        private RegionDataProvider _region;

        public StarDataProvider(Galaxy.Star star)
        {
            _star = star; 
        }

        public int Id => _star.Id;
        public int Level => _star.Level;
        public bool IsSecured => !_star.Occupant.IsAggressive;
        public IRegionDataProvider Region => _region ??= new RegionDataProvider(_star.Region);
    }
}
