using BeagleLib.Engine;
using BeagleLib.Util;
using BeagleLib.VM;

namespace BeagleLib.Agent;

public static class CommandSpanCrossoverExt
{
    #region Crossover Methods
    public static void Crossover(this Span<Command> me, ref int myCommandsLength, Organism partner)
    {
        //find chunk in me
        var crossoverEnd = Rnd.Random.Next(myCommandsLength);
        var crossoverStart = me.FindStartOfValidCrossoverChunk(crossoverEnd);
        if (crossoverStart == -1) // catch if crossover chunk search fails
        {
            myCommandsLength = -1;
            return;
        }

        //find chunk in partner
        Span<Command> partnerCommands = stackalloc Command[BConfig.MaxScriptLength];
        partner.Commands.CopyTo(partnerCommands);
        var (partnerCrossoverEnd, partnerCrossoverStart) = partnerCommands.FindEndAndStartOfValidCrossoverChunkForPartner();
        var crossoverChunkLength = crossoverEnd - crossoverStart;
        var partnerCrossoverChunkLength = partnerCrossoverEnd - partnerCrossoverStart;
        if (partnerCrossoverEnd == -1 || myCommandsLength - crossoverChunkLength + partnerCrossoverChunkLength > BConfig.MaxScriptLength)
        {
            myCommandsLength = -1;
            return;
        }

        myCommandsLength = me.InsertCrossoverChunk(partnerCommands, myCommandsLength, crossoverEnd, crossoverStart, partnerCrossoverEnd, partnerCrossoverStart);

        if (MLSetup.Current.RemoveRedundantCommandsAfterMutation) me.RemoveRedundantCommands(ref myCommandsLength);

        // check for invalid crossover
        int stackEffect = 0;
        for (int i = 0; i < myCommandsLength; i++)
        {
            if (me[i].Operation == OpEnum.Copy || me[i].Operation == OpEnum.Paste || stackEffect < me[i].MinStackRequired)
            {
                myCommandsLength = -1;
                break;
            }

            stackEffect += me[i].StackEffect;
        }
    }
    public static int FindStartOfValidCrossoverChunk(this Span<Command> me, int crossoverEnd)
    {
        int[] possibleCrossoverStarts = GetPossibleCrossoverStartsThreadStaticArray();
        int possibleCrossoverStartsCount = 0;
        int stackEffect = 0;

        for (int i = crossoverEnd; i >= 0; i--)
        {
            stackEffect += me[i].StackEffect;
            if (me[i].Operation == OpEnum.Copy || me[i].Operation == OpEnum.Paste) return -1;

            if (stackEffect ==1)
            {
                possibleCrossoverStarts[possibleCrossoverStartsCount] = i;
                possibleCrossoverStartsCount++;
                break;
            }
        }

        // for generalizability to select one if we find multiple possible crossover chunks
        return possibleCrossoverStarts[Rnd.Random.Next(possibleCrossoverStartsCount)];
    }
    //this returns crossoverEnd = -1 if can't find a valid chunk
    public static (int, int) FindEndAndStartOfValidCrossoverChunkForPartner(this Span<Command> me)
    {
        int commandsLength = me.Length;
        int crossoverEnd = Rnd.Random.Next(commandsLength);
        int crossoverStart = 0;

        int[] possibleCrossoverStarts = GetPossibleCrossoverStartsThreadStaticArray();
        int possibleCrossoverStartsCount = 0;
        int stackEffect = 0;

        // if endpoint is swap, move endpoint to the left
        while (me[crossoverEnd].Operation == OpEnum.Swap)
        {
            crossoverEnd -= 1;
        }

        // find crossover candidate chunks
        for (int i = crossoverEnd; i>=0; i--)
        {
            // abort if a copy or paste
            if (me[i].Operation == OpEnum.Copy || me[i].Operation == OpEnum.Paste) return (-1, crossoverStart);

            // if duplicate isn't needed by this chunk, skip it
            if (stackEffect == 0 && me[i].Operation == OpEnum.Dup)
            {
                me.RemoveAt(ref commandsLength, i);
                crossoverEnd -= 1;
                continue;
            }
            // if swap isn't needed by this chunk, skip it and the chunk that isn't needed
            if (stackEffect == 0 && me[i].Operation == OpEnum.Swap)
            {
                me.RemoveAt(ref commandsLength, i);
                crossoverEnd--;
                int skipSize = me[i-1].MinStackRequired;
                for (int j = 1; j <= skipSize+1; j++)
                {
                    me.RemoveAt(ref commandsLength, i-j);
                    crossoverEnd--;
                }
                i -= 1 + skipSize;
                continue;
            }
            stackEffect += me[i].StackEffect;
            if (stackEffect==1) // check if valid chunk found
            {
                possibleCrossoverStarts[possibleCrossoverStartsCount] = i;
                possibleCrossoverStartsCount++;
                break;
            }
        }
        // for generalizability to select one if we find multiple
        crossoverStart = possibleCrossoverStarts[Rnd.Random.Next(possibleCrossoverStartsCount)];

        return (crossoverEnd, crossoverStart);
    }
    public static int InsertCrossoverChunk(this Span<Command> me, Span<Command> partner, int myCommandsLength, int crossoverEnd, int crossoverStart, int partnerCrossoverEnd, int partnerCrossoverStart)
    {
        // remove old chunk and then drop in new chunk
        for (var i = 0; i <= crossoverEnd - crossoverStart; i++)
        {
            me.RemoveAt(ref myCommandsLength, crossoverEnd-i);
        }

        for (var i = 0; i <= partnerCrossoverEnd - partnerCrossoverStart; i++)
        {
            me.Insert(ref myCommandsLength, crossoverStart+i,partner[i+partnerCrossoverStart]);
        }

        return myCommandsLength;
    }
    #endregion

    #region Thread static storage
    [ThreadStatic] private static int[]? _possibleCrossoverStarts;
    private static int[] GetPossibleCrossoverStartsThreadStaticArray()
    {
        return _possibleCrossoverStarts ??= new int[BConfig.MaxScriptLength];
    }    
    #endregion
}