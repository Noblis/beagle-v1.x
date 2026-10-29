namespace BeagleLib.Agent;

public struct OrganismScoreComparer : IComparer<Organism>
{
    public int Compare(Organism? x, Organism? y)
    {
        if (x == null && y == null) return 0;
        if (x == null && y != null) return -1;
        if (x != null && y == null) return 1;

        if (x!.Score == y!.Score)
        {
            if (x.IsRecalculatedMoreThanOnce == y.IsRecalculatedMoreThanOnce)
            {
                if (x.Commands.Length == y.Commands.Length) return 0;

                if (x.Commands.Length > y.Commands.Length) return -1;
                else return 1;
            }

            if (!x.IsRecalculatedMoreThanOnce && y.IsRecalculatedMoreThanOnce) return -1;
            if (x.IsRecalculatedMoreThanOnce && !y.IsRecalculatedMoreThanOnce) return 1;
        }

        if (x.Score < y.Score) return -1;
        else return 1;
    }
}

public readonly struct OrganismScoreReverseComparer : IComparer<Organism>
{
    #region Constructors
    public OrganismScoreReverseComparer() 
    {
        _organismScoreComparer = new OrganismScoreComparer();
    }
    #endregion

    #region Methods
    public int Compare(Organism? x, Organism? y)
    {
        return -_organismScoreComparer.Compare(x, y);
    }
    #endregion

    #region Fields
    private readonly OrganismScoreComparer _organismScoreComparer;
    #endregion
}
