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

        // Scan to left of crossoverEnd to find potential crossoverStart points (this version will find just one)
        me.IdentifyCrossoverChunk( crossoverEnd, ref crossoverStart);

        if (crossoverStart == -1) // catch if crossover chunk search fails
        {
            crossoverCommandsLength = -1;
            return;
        }

        int partnerCommandsLength = partner.Commands.Length;
        Span < Command > partnerCommands = stackalloc Command[BConfig.MaxScriptLength];
        partner.Commands.CopyTo(partnerCommands);

        var partnerCrossoverEnd = Rnd.Random.Next(partnerCommandsLength);
        var partnerCrossoverStart = 0;
        IdentifyPartnerCrossoverChunk(ref partnerCommands, ref partnerCommandsLength, ref partnerCrossoverEnd, ref partnerCrossoverStart);

        // catch if fails to find crossover chunk in partner
        if (partnerCommandsLength == -1 || (crossoverCommandsLength-(crossoverEnd-crossoverStart)+(partnerCrossoverEnd-partnerCrossoverStart)>BConfig.MaxScriptLength))
        {
            crossoverCommandsLength = -1;
            return;
        }

        me.InsertCrossoverChunk(partnerCommands, ref crossoverCommandsLength, crossoverEnd, crossoverStart, partnerCrossoverEnd, partnerCrossoverStart);

        if (MLSetup.Current.RemoveRedundantCommandsAfterMutation) me.RemoveRedundantCommands(ref crossoverCommandsLength);

        // check for invalid crossover
        int stackEffect = 0;
        for (int i = 0; i < crossoverCommandsLength; i++)
        {
            if (me[i].Operation == OpEnum.Copy || me[i].Operation == OpEnum.Paste || stackEffect < me[i].MinStackRequired)
            {
                crossoverCommandsLength = -1;
                break;
            }

            stackEffect += me[i].StackEffect;
        }
    }

    public static void IdentifyCrossoverChunk(this Span<Command> me, int crossoverEnd, ref int crossoverStart)
    {
        int[] numbers = new int[crossoverEnd+1];
        int validCrossCount = 0;
        int stackEffect = 0;

        for (int i = crossoverEnd; i>=0; i--)
        {
            stackEffect += me[i].StackEffect;
            if (me[i].Operation == OpEnum.Copy || me[i].Operation == OpEnum.Paste)
            {
                crossoverStart = -1;
                return;
            }
            if (stackEffect ==1)
            {
                numbers[validCrossCount] = i;
                validCrossCount++;
                break;
            }
        }
        // for generalizability to select one if we find multiple
        crossoverStart = numbers[Rnd.Random.Next(validCrossCount)];
    }

    public static void IdentifyPartnerCrossoverChunk(ref Span<Command> partner, ref int partnerLength, ref int crossoverEnd, ref int crossoverStart)
    {
        int[] numbers = new int[crossoverEnd+1];
        int validCrossCount = 0;
        int stackEffect = 0;

        // if endpoint is swap, move endpoint to the left
        while (partner[crossoverEnd].Operation == OpEnum.Swap) crossoverEnd -= 1;

        // find crossover candidate chunks
        for (int i = crossoverEnd; i>=0; i--)
        {
            // abort if a copy or paste
            if (partner[i].Operation == OpEnum.Copy || partner[i].Operation == OpEnum.Paste)
            {
                partnerLength = -1;
                return;
            }
            // if duplicate isn't needed by this chunk, skip it
            if (stackEffect == 0 && partner[i].Operation == OpEnum.Dup)
            {
                partner.RemoveAt(ref partnerLength, i);
                crossoverEnd -= 1;
                continue;
            }
            // if swap isn't needed by this chunk, skip it and the chunk that isn't needed
            if (stackEffect == 0 && partner[i].Operation == OpEnum.Swap)
            {
                partner.RemoveAt(ref partnerLength, i);
                crossoverEnd--;
                int skipSize = partner[i-1].MinStackRequired;
                for (int j = 1; j <= skipSize+1; j++)
                {
                    partner.RemoveAt(ref partnerLength, i-(j));
                    crossoverEnd--;
                }
                i = i - (1 + skipSize);
                continue;
            }
            stackEffect += partner[i].StackEffect;
            if (stackEffect==1) // check if valid chunk found
            {
                numbers[validCrossCount] = i;
                validCrossCount++;
                break;
            }
        }
        // for generalizability to select one if we find multiple
        crossoverStart = numbers[Rnd.Random.Next(validCrossCount)];
    }

    public static void InsertCrossoverChunk(this Span<Command> me, Span<Command> partner, ref int length, int crossoverEnd, int crossoverStart, int partnerCrossoverEnd, int partnerCrossoverStart)
    {
        // remove old chunk and then drop in new chunk
        for (var i = 0; i<=crossoverEnd-crossoverStart; i++)
        {
            me.RemoveAt(ref length, crossoverEnd-i);
        }
        for (var i = 0; i <= partnerCrossoverEnd - partnerCrossoverStart; i++)
        {
            me.Insert(ref length, crossoverStart+i,partner[i+partnerCrossoverStart]);
        }
    }


    #endregion

}