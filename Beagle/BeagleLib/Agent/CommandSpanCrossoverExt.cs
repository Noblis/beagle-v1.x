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
        IdentifyPartnerCrossoverChunk(ref partnerCommands, ref partnerCommandsLength, ref partnerCrossoverEnd, ref partnerCrossoverStart);

        me.InsertCrossoverChunk(partnerCommands, ref crossoverCommandsLength, crossoverEnd, crossoverStart, partnerCrossoverEnd, partnerCrossoverStart);
        int se = 0;
        for (int i = 0; i < crossoverCommandsLength; i++)
        {
            if (se < me[i].MinStackRequired)
            {
                Output.WriteLine("Error");
            }

            se += me[i].StackEffect;
        }

        if (MLSetup.Current.RemoveRedundantCommandsAfterMutation) me.RemoveRedundantCommands(ref crossoverCommandsLength);

        for (int i = 0; i < crossoverCommandsLength; i++)
        {
            if (me[i].Operation == OpEnum.Copy || me[i].Operation == OpEnum.Paste)
            {
                crossoverCommandsLength = -1;
                break;
            }
        }
    }
    public static void IdentifyCrossoverChunk(this Span<Command> me, int crossoverEnd, ref int crossoverStart)
    {

        //TODO: Find valid starting point
        int[] numbers = new int[crossoverEnd+1];
        int validCrossCount = 0;
        int stackEffect = 0;

        for (int i = crossoverEnd; i>=0; i--)
        {
            stackEffect += me[i].StackEffect;
            if (stackEffect ==1)
            {
                numbers[validCrossCount] = i;
                validCrossCount++;
                break;
            }
        }

        crossoverStart = numbers[Rnd.Random.Next(validCrossCount)];
    }

    public static void IdentifyPartnerCrossoverChunk(ref Span<Command> partner, ref int partnerLength, ref int crossoverEnd, ref int crossoverStart)
    {
        int[] numbers = new int[crossoverEnd+1];
        int validCrossCount = 0;
        int stackEffect = 0;

        for (int i = crossoverEnd; i>=0; i--)
        {
            if (stackEffect == 0 && partner[i].Operation == OpEnum.Dup)
            {
                partner.RemoveAt(ref partnerLength, i);
                crossoverEnd -= 1;
                continue;
            }
            stackEffect += partner[i].StackEffect;
            if (stackEffect==1)
            {
                numbers[validCrossCount] = i;
                validCrossCount++;
                break;
            }
        }
        crossoverStart = numbers[Rnd.Random.Next(validCrossCount)];
    }

    public static void InsertCrossoverChunk(this Span<Command> me, Span<Command> partner, ref int length, int crossoverEnd, int crossoverStart, int partnerCrossoverEnd, int partnerCrossoverStart)
    {
        //TODO: Remove old chunk and then drop in new chunk
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

    #region Chunk Validation Methods
    public static bool VerifyCrossoverScriptValid(this Span<Command> me, int crossoverEnd, int crossoverStart)
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