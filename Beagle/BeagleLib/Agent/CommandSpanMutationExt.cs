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
        if (addr == 0)
        {
            mutationType = MutationTypeEnum.Replace;
        }
        else if (length > BConfig.MaxScriptLength - 10)
        {
            mutationType = MutationTypeEnum.Delete;
            if (addr == length) return 0;
        }
        else if (addr == length)
        {
            mutationType = MutationTypeEnum.Insert;
        }
        
        int stackEffect;
        int compensatingAddr;
        int newStackSize;
        int addrDelta;

        if (mutationType == MutationTypeEnum.Delete && Rnd.Random.Next(100)<0)
        {
            var commandAtAddr = me[addr];
            //for (int i = 0; i < length; i++) Console.Write(me[i]);
            //Console.WriteLine(addr);

            if (commandAtAddr.Operation is OpEnum.Arccos or OpEnum.Arcsin or OpEnum.Arctan or OpEnum.Cbrt or OpEnum.Cos
                or OpEnum.Cube or OpEnum.Exp or OpEnum.Ln or OpEnum.Sign or OpEnum.Sin or OpEnum.Sqrt or OpEnum.Square
                or OpEnum.Swap or OpEnum.Tan or OpEnum.Tanh)
            {
                // simple delete
                me.RemoveAt(ref length, addr);
                return -1;
            }
            else if (commandAtAddr.Operation is OpEnum.Add or OpEnum.Div or OpEnum.Mul or OpEnum.Pow or OpEnum.Sub or OpEnum.Del)
            {
                
                int swapIndex = addr + 1;
                while (me[swapIndex].Operation == OpEnum.Swap && swapIndex<length)
                {
                    me.RemoveAt(ref length, swapIndex);
                }
                
                // binary element delete and del
                int offset = 0;
                int deleteEnd = addr - 1;
                int firstOrSecondOperand = Rnd.Random.Next(2);
                int deleteStart = me.FindStartOfValidMutationChunk(deleteEnd);
                if (firstOrSecondOperand == 0 && commandAtAddr.Operation != OpEnum.Del && me[deleteStart].Operation!=OpEnum.Dup && me[deleteStart].Operation!=OpEnum.Copy)
                {
                    var oldEnd = deleteEnd;
                    var oldStart = deleteStart;
                    deleteEnd = deleteStart - 1;
                    deleteStart = me.FindStartOfValidMutationChunk(deleteEnd);
                    for (int i = deleteStart; i <= deleteEnd; i++)
                    {
                        if (me[i].Operation == OpEnum.Copy)
                        {
                            deleteStart = oldStart;
                            deleteEnd = oldEnd;
                            break;
                        }
                    }
                }
                me.RemoveAt(ref length, addr);
                offset -= 1;
                for (int i = deleteEnd; i >= deleteStart; i--)
                {
                    if (me[i].Operation == OpEnum.Copy) continue;
                    if (me[i].Operation == OpEnum.Paste)
                    {
                        var idx = me[i].Idx;
                        var copyIndex = me.GetCopyAddr(idx, i);
                        me.RemoveAt(ref length, i);
                        me.RemoveAt(ref length, copyIndex);
                        if (copyIndex < deleteStart) deleteStart--;
                        i--;
                        offset -= 2;
                    }
                    else
                    {
                        me.RemoveAt(ref length, i);
                        offset -= 1;
                    }
                }

                // Might need this to eliminate swaps that no longer work. 
                int tmpStackEffect = 0;
                for (int i = 0; i < length; i++)
                {
                    if (me[i].MinStackRequired > tmpStackEffect)
                    {
                        me.RemoveAt(ref length, i);
                        if (i<addr+offset) offset -= 1;
                        i--;
                        continue; // don't run tmpStackEffect for shift
                    }

                    tmpStackEffect += me[i].StackEffect;
                }

                return offset;
            }
            else if (commandAtAddr.Operation is OpEnum.Const or OpEnum.Load or OpEnum.Dup or OpEnum.Copy or OpEnum.Paste)
            {
                int copyDeleteOffset = 0;
                if (commandAtAddr.Operation == OpEnum.Paste)
                {
                    var idx = me[addr].Idx;
                    var copyIndex = me.GetCopyAddr(idx, addr);
                    me.RemoveAt(ref length, copyIndex);
                    copyDeleteOffset = -1;
                }

                if (commandAtAddr.Operation == OpEnum.Copy)
                {
                    return 0;
                }

                bool brokePattern = false; // to track end of unary ops
                stackEffect = 1;
                me.RemoveAt(ref length, addr+copyDeleteOffset);
                int rightTracking = 0;
                while (stackEffect != 0 && (addr + rightTracking + copyDeleteOffset) < length)
                {
                    if (me[addr+rightTracking+copyDeleteOffset].Operation==OpEnum.Copy)
                    {
                        rightTracking++;
                    }
                    else if (me[addr+copyDeleteOffset+rightTracking].StackEffect == 0 && brokePattern == false)
                    {
                        me.RemoveAt(ref length, addr+copyDeleteOffset+rightTracking);
                    }
                    else if (me[addr + copyDeleteOffset + rightTracking].Operation == OpEnum.Swap && stackEffect <= 2)
                    {
                        me.RemoveAt(ref length, addr+copyDeleteOffset+rightTracking);
                    }
                    else if (me[addr+rightTracking+copyDeleteOffset].MinStackRequired>=stackEffect && me[addr+rightTracking+copyDeleteOffset].StackEffect==-1)//me[addr+rightTracking+copyDeleteOffset].StackEffect == -1 && stackEffect == 1)
                    {
                        me.RemoveAt(ref length, addr+rightTracking+copyDeleteOffset);
                        stackEffect -= 1;
                        break;
                    }
                    else
                    {
                        stackEffect += me[addr + rightTracking + copyDeleteOffset].StackEffect;
                        rightTracking += 1;
                        brokePattern = true;
                    }
                }
                int swapIndex = addr + rightTracking + copyDeleteOffset;
                while (me[swapIndex].Operation == OpEnum.Swap && swapIndex < length)
                {
                    me.RemoveAt(ref length, swapIndex);
                }


                return -1+copyDeleteOffset;
            }
            #if DEBUG
                        me.VerifyScriptValid(length, true);
            #endif
            return 0;

            //return addrDelta;
            //var commandAtAddr = me[addr];

            //stackEffect = -commandAtAddr.StackEffect;
            //compensatingAddr = addr;
            //newStackSize = stackAtAddr + stackEffect; //used to be without stackEffect

            ////we don't modify copy adjunct operation directly
            //if (commandAtAddr.Operation == OpEnum.Copy) return 0;

            ////Special handling for copy/paste pair
            //if (commandAtAddr.Operation == OpEnum.Paste)
            //{
            //    var idx = commandAtAddr.Idx;
            //    var copyAddr = me.GetCopyAddr(idx, addr);

            //    Debug.Assert(addr > copyAddr);
            //    me.RemoveAt(ref length, addr);
            //    me.RemoveAt(ref length, copyAddr);
            //    compensatingAddr--;

            //    Debug.Assert(stackEffect == -1);
            //    addrDelta = -2 + me.InsertRandomAt(ref length, compensatingAddr, -stackEffect, newStackSize, inputsCount, allowedOperations, allowedAdjunctOperationsCount);

            //    #if DEBUG
            //    me.VerifyScriptValid(length, true);
            //    #endif

            //    return addrDelta;
            //}

            //me.RemoveAt(ref length, addr);

            //if (stackEffect == 0)
            //{
            //    #if DEBUG
            //    me.VerifyScriptValid(length, true);
            //    #endif

            //    return -1;
            //}

            //addrDelta = -1 + me.InsertRandomAt(ref length, compensatingAddr, -stackEffect, newStackSize, inputsCount, allowedOperations, allowedAdjunctOperationsCount);

            //#if DEBUG
            //me.VerifyScriptValid(length, true);
            //#endif

            //return addrDelta;
        }
        else if (mutationType == MutationTypeEnum.Delete && Rnd.Random.Next(100) < 100)
        {
            var commandAtAddr = me[addr];
            if (commandAtAddr.Operation is OpEnum.Arccos or OpEnum.Arcsin or OpEnum.Arctan or OpEnum.Cbrt or OpEnum.Cos
                or OpEnum.Cube or OpEnum.Exp or OpEnum.Ln or OpEnum.Sign or OpEnum.Sin or OpEnum.Sqrt or OpEnum.Square
                or OpEnum.Swap or OpEnum.Tan or OpEnum.Tanh)
            {
                // simple delete
                me.RemoveAt(ref length, addr);
                return -1;
            }
            else if (commandAtAddr.Operation is OpEnum.Add or OpEnum.Div or OpEnum.Mul or OpEnum.Pow or OpEnum.Sub or OpEnum.Del)
            {

                //int swapIndex = addr + 1;
                //while (me[swapIndex].Operation == OpEnum.Swap && swapIndex < length)
                //{
                //    me.RemoveAt(ref length, swapIndex);
                //}

                // binary element delete and del
                int offset = 0;
                
                me.RemoveAt(ref length, addr);
                offset -= 1;
                for (int i = addr-1; i >= 0; i--)
                {
                    if (me[i].Operation == OpEnum.Paste)
                    {
                        var idx = me[i].Idx;
                        var copyIndex = me.GetCopyAddr(idx, i);
                        me.RemoveAt(ref length, i);
                        me.RemoveAt(ref length, copyIndex);
                        i--;
                        offset -= 2;
                        break;
                    }
                    else if (me[i].StackEffect==1)
                    {
                        me.RemoveAt(ref length, i);
                        offset -= 1;
                        break;
                    }
                }

                // Might need this to eliminate swaps that no longer work. 
                int tmpStackEffect = 0;
                for (int i = 0; i < length; i++)
                {
                    if (me[i].MinStackRequired > tmpStackEffect)
                    {
                        me.RemoveAt(ref length, i);
                        if (i < addr + offset) offset -= 1;
                        i--;
                        continue; // don't run tmpStackEffect for shift
                    }

                    tmpStackEffect += me[i].StackEffect;
                }

                return offset;
            }
            else if (commandAtAddr.Operation is OpEnum.Const or OpEnum.Load or OpEnum.Dup or OpEnum.Copy or OpEnum.Paste)
            {
                int copyDeleteOffset = 0;
                if (commandAtAddr.Operation == OpEnum.Paste)
                {
                    var idx = me[addr].Idx;
                    var copyIndex = me.GetCopyAddr(idx, addr);
                    me.RemoveAt(ref length, copyIndex);
                    copyDeleteOffset = -1;
                }

                if (commandAtAddr.Operation == OpEnum.Copy)
                {
                    return 0;
                }

                stackEffect = 1;
                me.RemoveAt(ref length, addr + copyDeleteOffset);
                int rightTracking = 0;
                while ((addr + rightTracking + copyDeleteOffset) < length)
                {
                    if (me[addr + copyDeleteOffset + rightTracking].Operation == OpEnum.Swap && stackEffect <= 2)
                    {
                        me.RemoveAt(ref length, addr + copyDeleteOffset + rightTracking);
                    }
                    else if (me[addr + rightTracking + copyDeleteOffset].StackEffect==-1)//me[addr+rightTracking+copyDeleteOffset].StackEffect == -1 && stackEffect == 1)
                    {
                        me.RemoveAt(ref length, addr + rightTracking + copyDeleteOffset);
                        stackEffect -= 1;
                        break;
                    }
                    else
                    {
                        stackEffect += me[addr + rightTracking + copyDeleteOffset].StackEffect;
                        rightTracking += 1;
                    }
                }
                int swapIndex = addr + rightTracking + copyDeleteOffset;
                while (me[swapIndex].Operation == OpEnum.Swap && swapIndex < length)
                {
                    me.RemoveAt(ref length, swapIndex);
                }


                return -1 + copyDeleteOffset;
            }
#if DEBUG
                        me.VerifyScriptValid(length, true);
#endif
            return 0;

        }
        else if (mutationType == MutationTypeEnum.Delete)
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

                if (rnd > 90) //10%
                {
                    //increment/decrement
                    me[addr] = new Command(OpEnum.Const, commandAtAddr.ConstValue + Rnd.RandomSign);
                }
                else if (rnd > 80) //10%
                {
                    //change sign
                    me[addr] = new Command(OpEnum.Const, commandAtAddr.ConstValue * -1);
                }
                else if (rnd > 70) //10%
                {
                    //double or half
                    me[addr] = new Command(OpEnum.Const, Rnd.RandomDoubleOrHalf(commandAtAddr.ConstValue));
                }
                else if (rnd > 60) //10%
                {
                    //x10 or /10 
                    me[addr] = new Command(OpEnum.Const, Rnd.RandomMul10OrDiv10(commandAtAddr.ConstValue));
                }
                else if (rnd > 50) //10%
                {
                    // +/- NextDouble(0.1)
                    me[addr] = new Command(OpEnum.Const, commandAtAddr.ConstValue + (float)(Rnd.Random.NextDouble() * 0.2 - 0.1));
                }
                else if (rnd > 25) //25%
                {
                    me[addr] = new Command(OpEnum.Const, commandAtAddr.ConstValue * (Rnd.Random.NextSingle() * 0.5f + 1f));
                }
                else //25%
                {
                    me[addr] = new Command(OpEnum.Const, commandAtAddr.ConstValue / (Rnd.Random.NextSingle() * 0.5f + 1f));
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

    private static int FindStartOfValidMutationChunk(this Span<Command> me, int end)
    {

        int mutationEnd = end;
        int mutationStart = 0;

        int stackEffect = 0;


        // find mutation candidate chunks
        for (int i = mutationEnd; i >= 0; i--)
        {
            // abort if a copy or paste
            //if (me[i].Operation == OpEnum.Copy || me[i].Operation == OpEnum.Paste) return (-1);

            stackEffect += me[i].StackEffect;
            if (stackEffect == 1) // check if valid chunk found
            {
                mutationStart = i;
                break;
            }
        }

        return (mutationStart);
    }
}