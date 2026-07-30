using System.Diagnostics;
using BeagleLib.Engine;
using BeagleLib.Util;
using BeagleLib.VM;

namespace BeagleLib.Agent;

public static class CommandSpanMutationExt
{
    #region Mutation Methods
    public static void Mutate(this Span<Command> me, ref int mutationCommandsLength, byte inputsCount, OpEnum[] allowedOperations, int allowedAdjunctOperationsCount)
    {
        var randomPct = Rnd.Random.Next(100);

        int mutationsCount;
        if (randomPct < 33) mutationsCount = 1; // 33%
        else if (randomPct < 55) mutationsCount = 2; // 22%
        else if (randomPct < 70) mutationsCount = 3; // 15%
        else if (randomPct < 80) mutationsCount = 4; // 10%
        else if (randomPct < 87) mutationsCount = 5; // 7%
        else if (randomPct < 91) mutationsCount = 6; // 4%
        else if (randomPct < 94) mutationsCount = 7; // 3%
        else if (randomPct < 96) mutationsCount = 8; // 2%
        else if (randomPct < 97) mutationsCount = 9; // 1%
        else if (randomPct < 98) mutationsCount = 10; // 1%
        else if (randomPct < 99) mutationsCount = 11; // 1%
        else mutationsCount = 12; // 1%

        for (var i = 0; i < mutationsCount; i++)
        {
            me.MutateOnce(ref mutationCommandsLength, inputsCount, allowedOperations, allowedAdjunctOperationsCount);
        }

        if (MLSetup.Current.RemoveRedundantCommandsAfterMutation) me.RemoveRedundantCommands(ref mutationCommandsLength);
    }
    public static void MutateOnce(this Span<Command> me, ref int length, byte inputsCount, OpEnum[] allowedOperations, int allowedAdjunctOperationsCount)
    {
        var probabilityOfMutationPerCommand = 1.0 / length;
        for (var addr = 0; addr <= length; addr++)
        {
            if (Rnd.RandomBoolWithChance(probabilityOfMutationPerCommand))
            {
                var addrDelta = me.MutateAt(ref length, addr, inputsCount, allowedOperations, allowedAdjunctOperationsCount);
                addr += addrDelta;
            }
        }
    }
    private static int MutateAt(this Span<Command> me, ref int length, int addr, byte inputsCount, OpEnum[] allowedOperations, int allowedAdjunctOperationsCount)
    {
        #if DEBUG
        me.VerifyScriptValid(length, true);
        #endif

        var stackAtAddr = me.GetStackAt(addr);

        var mutationType = (MutationTypeEnum)Rnd.Random.Next(3) ;
        if (addr == length) mutationType = MutationTypeEnum.Insert;
        else if (addr == 0) mutationType = MutationTypeEnum.Replace;

        int stackEffect;
        int compensatingAddr;
        int newStackSize;
        int addrDelta;

        if (mutationType == MutationTypeEnum.Delete)
        {
            var commandAtAddr = me[addr];

            stackEffect = -commandAtAddr.StackEffect;
            compensatingAddr = addr;
            newStackSize = stackAtAddr + stackEffect; //used to be without stackEffect

            //we don't modify copy adjunct operation directly
            if (commandAtAddr.Operation == OpEnum.Copy) return 0;

            //Special handling for copy/paste pair
            if (commandAtAddr.Operation == OpEnum.Paste)
            {
                var idx = commandAtAddr.Idx;
                var copyAddr = me.GetCopyAddr(idx, addr);

                Debug.Assert(addr > copyAddr);
                me.RemoveAt(ref length, addr);
                me.RemoveAt(ref length, copyAddr);
                compensatingAddr--;

                Debug.Assert(stackEffect == -1);
                addrDelta = -2 + me.InsertRandomAt(ref length, compensatingAddr, -stackEffect, newStackSize, inputsCount, allowedOperations, allowedAdjunctOperationsCount);

                #if DEBUG
                me.VerifyScriptValid(length, true);
                #endif

                return addrDelta;
            }

            me.RemoveAt(ref length, addr);

            if (stackEffect == 0)
            {
                #if DEBUG
                me.VerifyScriptValid(length, true);
                #endif

                return -1;
            }
            
            addrDelta = -1 + me.InsertRandomAt(ref length, compensatingAddr, -stackEffect, newStackSize, inputsCount, allowedOperations, allowedAdjunctOperationsCount);
            
            #if DEBUG
            me.VerifyScriptValid(length, true);
            #endif

            return addrDelta;
        }
        
        if (mutationType == MutationTypeEnum.Replace)
        {
            var commandAtAddr = me[addr];

            //we don't modify adjunct copy operations directly
            if (commandAtAddr.Operation == OpEnum.Copy) return 0;

            //60% chance to modify existing const
            if (commandAtAddr.Operation == OpEnum.Const && Rnd.Random.Next(100) < 60) 
            {
                var rnd = Rnd.Random.Next(100);

                if (rnd > 60) //40%
                {
                    //increment/decrement
                    me[addr] = new Command(OpEnum.Const, commandAtAddr.ConstValue + Rnd.RandomSign);
                }
                else if (rnd > 40) //20%
                {
                    //change sign
                    me[addr] = new Command(OpEnum.Const, commandAtAddr.ConstValue * -1);
                }
                else if (rnd > 20) //20%
                {
                    //double or half
                    me[addr] = new Command(OpEnum.Const, Rnd.RandomDoubleOrHalf(commandAtAddr.ConstValue));
                }
                else if (rnd > 10) //10%
                {
                    //x10 or /10 
                    me[addr] = new Command(OpEnum.Const, Rnd.RandomMul10OrDiv10(commandAtAddr.ConstValue));
                }
                else //10%
                {
                    // +/- NextDouble(0.1)
                    me[addr] = new Command(OpEnum.Const, commandAtAddr.ConstValue + (float)(Rnd.Random.NextDouble() * 0.2 - 0.1));
                }

                #if DEBUG
                me.VerifyScriptValid(length, true);
                #endif

                return 0;
            }

            //5% chance to keep load command and modify load variable
            if (commandAtAddr.Operation == OpEnum.Load && Rnd.Random.Next(100) < 5) 
            {
                me[addr] = Command.CreateRandomLoad(inputsCount);


                #if DEBUG
                me.VerifyScriptValid(length, true);
                #endif

                return 0;
            }

            //Special case for replacing first command in the script which must be Load or Const
            if (addr == 0)
            {
                Debug.Assert(me[addr].Operation == OpEnum.Load || me[addr].Operation == OpEnum.Const);
                var newFirstCommand = Command.CreateRandomLoadOrConst(inputsCount);
                me[addr] = newFirstCommand;


                #if DEBUG
                me.VerifyScriptValid(length, true);
                #endif

                return 0;
            }
            
            var maxCopyIdx = me.GetMaxCopyIdx(length);
            
            var newCommand = Command.CreateRandom(inputsCount, maxCopyIdx, null, stackAtAddr, allowedOperations, allowedAdjunctOperationsCount);

            stackEffect = newCommand.StackEffect - commandAtAddr.StackEffect;
            compensatingAddr = addr + 1;
            newStackSize = stackAtAddr + stackEffect;

            if (newStackSize < 1) return 0;

            int adjunctCommandCount;

            //Special handling for replacing paste
            if (commandAtAddr.Operation == OpEnum.Paste)
            {
                var idx = commandAtAddr.Idx;
                var copyAddr = me.GetCopyAddr(idx, addr);

                Debug.Assert(addr > copyAddr);
                me[addr] = newCommand;
                me.RemoveAt(ref length, copyAddr);
                adjunctCommandCount = me.InsertAdjunctCommandIfNeeded(ref length, newCommand, addr - 1);

                if (stackEffect == 0)
                {
                    #if DEBUG
                    me.VerifyScriptValid(length, true);
                    #endif

                    return -1;
                }
                
                addrDelta = -1 + me.InsertRandomAt(ref length, compensatingAddr, -stackEffect, newStackSize, inputsCount, allowedOperations, allowedAdjunctOperationsCount) + adjunctCommandCount;

                #if DEBUG
                me.VerifyScriptValid(length, true);
                #endif

                return addrDelta;
            }

            me[addr] = newCommand;
            adjunctCommandCount = me.InsertAdjunctCommandIfNeeded(ref length, newCommand, addr);
            if (adjunctCommandCount > 0) compensatingAddr++;

            if (stackEffect == 0)
            {
                #if DEBUG
                me.VerifyScriptValid(length, true);
                #endif

                return 0;
            }
            
            addrDelta = me.InsertRandomAt(ref length, compensatingAddr, -stackEffect, newStackSize, inputsCount, allowedOperations, allowedAdjunctOperationsCount) + adjunctCommandCount;
            
            #if DEBUG
            me.VerifyScriptValid(length, true);
            #endif

            return addrDelta;
        }
        
        if (mutationType == MutationTypeEnum.Insert)
        {
            var maxCopyIdx = me.GetMaxCopyIdx(length);

            //var newCommand = addr == 0 ? 
            //    Command.CreateRandomLoadOrConst(inputsCount) : 
            //    Command.CreateRandom(inputsCount, maxCopyIdx, null, stackAtAddr, allowedOperations, allowedAdjunctOperationsCount);
            var newCommand = Command.CreateRandom(inputsCount, maxCopyIdx, null, stackAtAddr, allowedOperations, allowedAdjunctOperationsCount);

            stackEffect = newCommand.StackEffect;
            compensatingAddr = addr + 1;
            newStackSize = stackAtAddr + stackEffect;

            if (newStackSize < 1) return 0;

            me.Insert(ref length, addr, newCommand);
            var adjunctCommandCount = me.InsertAdjunctCommandIfNeeded(ref length, newCommand, addr);
            if (adjunctCommandCount > 0) compensatingAddr++;

            if (stackEffect == 0)
            {
                #if DEBUG
                me.VerifyScriptValid(length, true);
                #endif

                return 1;
            }
            
            addrDelta = 1 + me.InsertRandomAt(ref length, compensatingAddr, -stackEffect, newStackSize, inputsCount, allowedOperations, allowedAdjunctOperationsCount) + adjunctCommandCount;
            
            #if DEBUG 
            me.VerifyScriptValid(length, true);
            #endif

            return addrDelta;
        }

        throw new Exception("Unknown mutation type");
    }
    #endregion
}