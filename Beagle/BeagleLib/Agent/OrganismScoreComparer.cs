namespace BeagleLib.Agent;

public struct OrganismScoreComparer : IComparer<Organism>
{
    public int Compare(Organism? x, Organism? y)
    {
        if (x.Score == y.Score) return 0;

        if (x.Score < y.Score) return -1;
        else return 1;
    }
}