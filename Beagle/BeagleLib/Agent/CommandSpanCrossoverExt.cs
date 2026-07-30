using BeagleLib.Engine;
using BeagleLib.Util;
using BeagleLib.VM;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Net.Mail;

namespace BeagleLib.Agent;

public static class CommandSpanCrossoverExt
{
    #region Crossover Methods
    public static void Crossover(this Span<Command> me, ref int crossoverCommandsLength, Organism partner)
    {

        var crossoverEnd = Rnd.Random.Next(crossoverCommandsLength);
        var crossoverStart = 0;

        // Scan to left of crossoverEnd to find potential crossoverStart points
        me.IdentifyCrossoverChunk( crossoverEnd, ref crossoverStart);

        int partnerCommandsLength = partner.Commands.Length;
        Span < Command > partnerCommands = stackalloc Command[BConfig.MaxScriptLength];
        partner.Commands.CopyTo(partnerCommands);

        var partnerCrossoverEnd = Rnd.Random.Next(partnerCommandsLength);
        var partnerCrossoverStart = 0;
        IdentifyPartnerCrossoverChunk(partnerCommands, partnerCrossoverEnd, ref partnerCrossoverStart);




        if (MLSetup.Current.RemoveRedundantCommandsAfterMutation) me.RemoveRedundantCommands(ref crossoverCommandsLength);
    }
    public static void IdentifyCrossoverChunk(this Span<Command> me, int crossoverEnd, ref int crossoverStart)
    {

        //TODO: Find valid starting point
        int[] numbers = new int[crossoverEnd];
        int validCrossCount = 0;

        for (int i = crossoverEnd; i>=0; i--)
        {
            if (me.VerifyScriptValid(crossoverEnd, i))
            {
                numbers[validCrossCount] = i;
                validCrossCount++;         
            }
        }

        crossoverStart = numbers[Rnd.Random.Next(validCrossCount)];
    }

    public static void IdentifyPartnerCrossoverChunk(Span<Command> partner, int crossoverEnd, ref int crossoverStart)
    {
        int[] numbers = new int[crossoverEnd];
        int validCrossCount = 0;

        for (int i = crossoverEnd; i>=0; i--)
        {
            if (partner.VerifyScriptValid(crossoverEnd,i))
            {
                numbers[validCrossCount] = i;
                validCrossCount++;
            }
        }
        crossoverStart = numbers[Rnd.Random.Next(validCrossCount)];
    }

    public static void InsertCrossoverChunk(this Span<Command> me, Span<Command> partner, ref int length, int crossoverEnd, int crossoverStart, int partnerCrossoverEnd, int partnerCrossoverStart)
    {
        //TODO: Remove old chunk and then drop in new chunk
        for (int i = 0; i<=crossoverEnd-crossoverStart; i++)
        {
            me.RemoveAt(ref length, crossoverStart);
        }
        for (int i = 0; i <= partnerCrossoverEnd - partnerCrossoverStart; i++)
        {
            me.Add(ref length, partner[i]);
        }
    }


    #endregion

    #region Chunk Validation Methods
    public static bool VerifyScriptValid(this Span<Command> me, int crossoverEnd, int crossoverStart)
    {

        var stackCount = 0;
        // ReSharper disable once ForCanBeConvertedToForeach
        // ReSharper disable once LoopCanBeConvertedToQuery
        for (var addr = crossoverEnd; addr >= crossoverStart; addr--)
        {
            var command = me[addr];
            if (command.Operation == OpEnum.Paste) if(me.GetCopyAddr(command.Idx, addr)<crossoverStart) return false;

            stackCount += me[addr].StackEffect;
        }
        if (stackCount != 1) return false;
        return true;


    }
    #endregion
}