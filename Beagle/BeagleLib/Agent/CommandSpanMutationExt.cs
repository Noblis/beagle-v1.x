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
                //if (length<1) Console.WriteLine(length);
            }
        }
    }
    private static int MutateAt(this Span<Command> me, ref int length, int addr, byte inputsCount, OpEnum[] allowedOperations, int allowedAdjunctOperationsCount)
    {
        #if DEBUG
        me.VerifyScriptValid(length, true);
        #endif

        if (addr == BConfig.MaxScriptLength) return 0;

        MutationTypeEnum mutationType;
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
        else
        {
            mutationType = (MutationTypeEnum)Rnd.Random.Next(3);
        }

        var stackAtAddr = me.GetStackAt(addr);

        int stackCount;
        int compensatingAddr;
        int newStackSize;
        int addrDelta;

        if (mutationType == MutationTypeEnum.Delete)
        {
            if (Rnd.Random.Next(100) < 50) // large delete
            {
                var commandAtAddr = me[addr];
                var opProps = commandAtAddr.Operation.GetOperationProperties();
                //for (int i = 0; i < length; i++) Console.Write(me[i]);
                //Console.WriteLine(addr);

                // simple delete
                if (opProps.StackEffect == 0 && commandAtAddr.Operation != OpEnum.Copy)
                {
                    int offset = -1;
                    me.RemoveAt(ref length, addr);
                    while (me[addr + 1].StackEffect == 0 && me[addr + 1].Operation != OpEnum.Copy &&
                           Rnd.Random.Next(10) < 5 && addr+1<length)
                    {
                        me.RemoveAt(ref length, addr+1);
                    }

                    while (me[addr + offset].StackEffect == 0 && me[addr + offset].Operation != OpEnum.Copy &&
                           Rnd.Random.Next(10) < 5)
                    {
                        me.RemoveAt(ref length, addr + offset);
                        offset--;
                    }
                    return offset;
                }

                // binary element delete and del
                if (opProps.StackEffect == -1) 
                {
                    int offset = 0;
                    int deleteRightAddr = addr - 1;
                    int firstOrSecondOperand = Rnd.Random.Next(2);
                    int deleteLeftAddr = me.FindLeftAddrOfValidMutationChunk(deleteRightAddr);

                    firstOrSecondOperand = firstOrSecondOperand == 0 &&
                                           commandAtAddr.Operation != OpEnum.Del &&
                                           me[deleteLeftAddr].Operation != OpEnum.Dup &&
                                           me[deleteLeftAddr].Operation != OpEnum.Copy 
                                            ? 0 : 1; //0 means first chunk to be removed, 1 means second

                    var chunk2DeleteRightAddr = deleteRightAddr;
                    var chunk2DeleteLeftAddr = deleteLeftAddr;

                    if (firstOrSecondOperand == 0)
                    {
                        deleteRightAddr = deleteLeftAddr - 1;
                        deleteLeftAddr = me.FindLeftAddrOfValidMutationChunk(deleteRightAddr);

                        //We will delete all commands but will preserve Copy commands.
                        //We can't have a copy command as a very first command in the script.
                        //This could never happen in the second chunk of the code but could theoretically happen with this first one.
                        //That is why, if we find a copy in the first chunk, we will revert to the second one as a candidate for deleting.
                        for (int i = deleteLeftAddr; i <= deleteRightAddr; i++)
                        {
                            if (me[i].Operation == OpEnum.Copy)
                            {
                                deleteLeftAddr = chunk2DeleteLeftAddr;
                                deleteRightAddr = chunk2DeleteRightAddr;
                                firstOrSecondOperand = 1;
                                break;
                            }
                        }
                    }
                    
                    //at this point we have figured out what we want to delete
                    me.RemoveAt(ref length, addr);
                    offset -= 1;
                    for (int i = deleteRightAddr; i >= deleteLeftAddr; i--)
                    {
                        //we don't delete Commands to not mess up the Paste
                        if (me[i].Operation == OpEnum.Copy) continue;

                        if (me[i].Operation == OpEnum.Paste)
                        {
                            //If we find the Paste we remove it and the corresponding Copy
                            var idx = me[i].Idx;
                            var copyAddr = me.GetCopyAddr(idx, i);
                            me.RemoveAt(ref length, i);
                            me.RemoveAt(ref length, copyAddr);
                            if (copyAddr < deleteLeftAddr) deleteLeftAddr--;
                            i--;
                            offset -= 2;
                        }
                        else
                        {
                            //otherwise we remove one command
                            me.RemoveAt(ref length, i);
                            offset -= 1;
                        }
                    }

                    // Might need this to eliminate swaps that no longer work. 
                    if (firstOrSecondOperand == 0)
                    {
                        int tmpStackEffect = 0;
                        Debug.Assert(length > 0);
                        for (int i = 0; i <= chunk2DeleteRightAddr + offset + 1; i++)
                        {
                            Debug.Assert(length > chunk2DeleteRightAddr + offset + 1);
                            if (me[i].MinStackRequired > tmpStackEffect)
                            {
                                me.RemoveAt(ref length, i);
                                offset--;
                                i--;
                            }
                            else
                            {
                                tmpStackEffect += me[i].StackEffect;
                            }
                        }
                    }
                    //// Might need this to eliminate swaps that no longer work. 
                    //int tmpStackEffect = 0;
                    //for (int i = 0; i < length; i++)
                    //{
                    //    if (me[i].MinStackRequired > tmpStackEffect)
                    //    {
                    //        me.RemoveAt(ref length, i);
                    //        if (i < addr + offset) offset -= 1;
                    //        i--;
                    //    }
                    //    else
                    //    {
                    //        tmpStackEffect += me[i].StackEffect;
                    //    }
                    //}

                    #if DEBUG
                    me.VerifyScriptValid(length, true);
                    #endif

                    return offset;
                }
                else if (opProps.StackEffect == 1)
                {
                    int offset = 0;
                    if (commandAtAddr.Operation == OpEnum.Paste)
                    {
                        var idx = me[addr].Idx;
                        var copyAddr = me.GetCopyAddr(idx, addr);
                        me.RemoveAt(ref length, copyAddr);
                        offset = -1;
                    }

                    // to track end of unary ops immediately after the command at addr
                    bool brokeUnaryPattern = false; 
                    
                    stackCount = 1;
                    me.RemoveAt(ref length, addr + offset);
                    
                    int rightTrackingAddr = addr + offset;
                    while (stackCount != 0 && rightTrackingAddr < length)
                    {
                        if (me[rightTrackingAddr].Operation == OpEnum.Copy)
                        {
                            rightTrackingAddr++;
                        }
                        else if (me[rightTrackingAddr].StackEffect == 0 && brokeUnaryPattern == false)
                        {
                            me.RemoveAt(ref length, rightTrackingAddr);
                        }
                        else if (me[rightTrackingAddr].Operation == OpEnum.Swap && stackCount <= 2)
                        {
                            me.RemoveAt(ref length, rightTrackingAddr);
                        }
                        else if (me[rightTrackingAddr].MinStackRequired >= stackCount && me[rightTrackingAddr].StackEffect == -1)
                        {
                            me.RemoveAt(ref length, rightTrackingAddr);
                            //stackCount -= 1;
                            break;
                        }
                        else
                        {
                            stackCount += me[rightTrackingAddr].StackEffect;
                            rightTrackingAddr++;
                            brokeUnaryPattern = true;
                        }
                    }

                    //int swapIndex = rightTrackingAddr;
                    //while (swapIndex < length && me[swapIndex].Operation == OpEnum.Swap )
                    //{
                    //    me.RemoveAt(ref length, swapIndex);
                    //}
                    // Might need this to eliminate swaps that no longer work. 
                    int tmpStackEffect = 0;
                    for (int i = 0; i < length; i++)
                    {
                        if (me[i].MinStackRequired > tmpStackEffect)
                        {
                            me.RemoveAt(ref length, i);
                            if (i < addr + offset) offset -= 1;
                            i--;
                        }
                        else
                        {
                            tmpStackEffect += me[i].StackEffect;
                        }
                    }

#if DEBUG
                    me.VerifyScriptValid(length, true);
                    #endif
                    return offset - 1;
                }

                #if DEBUG
                me.VerifyScriptValid(length, true);
                #endif
                return 0;
            }
            else if (length > BConfig.MaxScriptLength - 10 || Rnd.Random.Next(100) < 50) //small delete (we force it if within 10 commands from MaxScriptLength
            {
                var commandAtAddr = me[addr];

                var opProps = commandAtAddr.Operation.GetOperationProperties();
                
                // simple delete
                if (opProps.StackEffect == 0 && commandAtAddr.Operation != OpEnum.Copy)
                {
                    me.RemoveAt(ref length, addr);
                    return -1;
                }

                // binary element delete and del
                if (opProps.StackEffect == -1)
                {
                    int offset = 0;

                    me.RemoveAt(ref length, addr);
                    offset -= 1;

                    for (int i = addr + offset; i >= 0; i--)
                    {
                        if (me[i].Operation == OpEnum.Paste)
                        {
                            var idx = me[i].Idx;
                            var copyAddr = me.GetCopyAddr(idx, i);
                            me.RemoveAt(ref length, i);
                            me.RemoveAt(ref length, copyAddr);
                            //i--;
                            offset -= 2;
                            break;
                        }
                        
                        if (me[i].StackEffect == 1)
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
                        }
                        else
                        {
                            tmpStackEffect += me[i].StackEffect;
                        }
                    }

                    #if DEBUG
                    me.VerifyScriptValid(length, true);
                    #endif

                    return offset;
                }
                else if (opProps.StackEffect == 1)
                {
                    int offset = 0;
                    if (commandAtAddr.Operation == OpEnum.Paste)
                    {
                        var idx = me[addr].Idx;
                        var copyAddr = me.GetCopyAddr(idx, addr);
                        me.RemoveAt(ref length, copyAddr);
                        offset = -1;
                    }

                    stackCount = 1;
                    me.RemoveAt(ref length, addr + offset);

                    int rightTrackingAddr = addr + offset;
                    while (rightTrackingAddr < length)
                    {
                        if (me[rightTrackingAddr].Operation == OpEnum.Swap && stackCount <= 2)
                        {
                            me.RemoveAt(ref length, rightTrackingAddr);
                        }
                        else if (me[rightTrackingAddr].StackEffect == -1)
                        {
                            me.RemoveAt(ref length, rightTrackingAddr);
                            //stackEffect -= 1;
                            break;
                        }
                        else
                        {
                            stackCount += me[rightTrackingAddr].StackEffect;
                            rightTrackingAddr++;
                        }
                    }

                    //int swapIndex = rightTrackingAddr;
                    //while (swapIndex < length && me[swapIndex].Operation == OpEnum.Swap)
                    //{
                    //    me.RemoveAt(ref length, swapIndex);
                    //}
                    // Might need this to eliminate swaps that no longer work. 
                    int tmpStackEffect = 0;
                    for (int i = 0; i < length; i++)
                    {
                        if (me[i].MinStackRequired > tmpStackEffect)
                        {
                            me.RemoveAt(ref length, i);
                            if (i < addr + offset) offset -= 1;
                            i--;
                        }
                        else
                        {
                            tmpStackEffect += me[i].StackEffect;
                        }
                    }

#if DEBUG
                    me.VerifyScriptValid(length, true);
                    #endif

                    return offset - 1;
                }
                #if DEBUG
                me.VerifyScriptValid(length, true);
                #endif
                return 0;
            }
            else //old delete that might end up not even being a delete
            {
                var commandAtAddr = me[addr];

                stackCount = -commandAtAddr.StackEffect;
                compensatingAddr = addr;
                newStackSize = stackAtAddr + stackCount; //used to be without stackEffect

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

                    Debug.Assert(stackCount == -1);
                    addrDelta = -2 + me.InsertRandomAt(ref length, compensatingAddr, -stackCount, newStackSize, inputsCount, allowedOperations, allowedAdjunctOperationsCount);

                    #if DEBUG
                    me.VerifyScriptValid(length, true);
                    #endif

                    return addrDelta;
                }

                me.RemoveAt(ref length, addr);

                if (stackCount == 0)
                {
                    #if DEBUG
                    me.VerifyScriptValid(length, true);
                    #endif

                    return -1;
                }

                addrDelta = -1 + me.InsertRandomAt(ref length, compensatingAddr, -stackCount, newStackSize, inputsCount, allowedOperations, allowedAdjunctOperationsCount);

                #if DEBUG
                me.VerifyScriptValid(length, true);
                #endif

                return addrDelta;
            }
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

            stackCount = newCommand.StackEffect - commandAtAddr.StackEffect;
            compensatingAddr = addr + 1;
            newStackSize = stackAtAddr + stackCount;

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

                if (stackCount == 0)
                {
                    #if DEBUG
                    me.VerifyScriptValid(length, true);
                    #endif

                    return -1;
                }
                
                addrDelta = -1 + me.InsertRandomAt(ref length, compensatingAddr, -stackCount, newStackSize, inputsCount, allowedOperations, allowedAdjunctOperationsCount) + adjunctCommandCount;

                #if DEBUG
                me.VerifyScriptValid(length, true);
                #endif

                return addrDelta;
            }

            me[addr] = newCommand;
            adjunctCommandCount = me.InsertAdjunctCommandIfNeeded(ref length, newCommand, addr);
            if (adjunctCommandCount > 0) compensatingAddr++;

            if (stackCount == 0)
            {
                #if DEBUG
                me.VerifyScriptValid(length, true);
                #endif

                return 0;
            }
            
            addrDelta = me.InsertRandomAt(ref length, compensatingAddr, -stackCount, newStackSize, inputsCount, allowedOperations, allowedAdjunctOperationsCount) + adjunctCommandCount;
            
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

            stackCount = newCommand.StackEffect;
            compensatingAddr = addr + 1;
            newStackSize = stackAtAddr + stackCount;

            if (newStackSize < 1) return 0;

            me.Insert(ref length, addr, newCommand);
            var adjunctCommandCount = me.InsertAdjunctCommandIfNeeded(ref length, newCommand, addr);
            if (adjunctCommandCount > 0) compensatingAddr++;

            if (stackCount == 0)
            {
                #if DEBUG
                me.VerifyScriptValid(length, true);
                #endif

                return 1;
            }
            
            addrDelta = 1 + me.InsertRandomAt(ref length, compensatingAddr, -stackCount, newStackSize, inputsCount, allowedOperations, allowedAdjunctOperationsCount) + adjunctCommandCount;
            
            #if DEBUG 
            me.VerifyScriptValid(length, true);
            #endif

            return addrDelta;
        }

        throw new Exception("Unknown mutation type");
    }
    #endregion

    private static int FindLeftAddrOfValidMutationChunk(this Span<Command> me, int rightAddr)
    {
        // find mutation candidate chunks
        int stackEffect = 0;
        for (int addr = rightAddr; addr >= 0; addr--)
        {
            stackEffect += me[addr].StackEffect;

            // check if valid chunk found
            if (stackEffect == 1) return addr;
        }

        throw new Exception("This should never happen!");
    }
}