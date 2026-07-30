using System.Diagnostics;
using BeagleLib.Engine;
using BeagleLib.Util;
using BeagleLib.VM;

namespace BeagleLib.Agent;

public static class CommandSpanCrossoverExt
{
    #region Crossover Methods
    public static void Crossover(this Span<Command> me, ref int crossoverCommandsLength, Organism partner)
    {

        var crossoverEnd = Rnd.Random.Next(crossoverCommandsLength);
        var crossoverStart = 0;

        // Scan to left of crossoverEnd to find potential crossoverStart points
        me.IdentifyCrossoverChunk(ref crossoverCommandsLength, crossoverEnd, ref crossoverStart);


        if (MLSetup.Current.RemoveRedundantCommandsAfterMutation) me.RemoveRedundantCommands(ref crossoverCommandsLength);
    }
    public static void IdentifyCrossoverChunk(this Span<Command> me, ref int length, int crossoverEnd, ref int crossoverStart)
    {

        //TODO: Find valid starting point
        crossoverStart = 0;
    }
    
    
    #endregion
}