using System.Diagnostics;
using System.Net.Mail;
using System.Text;
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

        me.RemoveRedundantCommands(ref mutationCommandsLength);
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

    #region Mutation Helper Methods
    public static void RemoveRedundantCommands(this Span<Command> me, ref int length)
    {
        for (var addr = 0; addr < length - 1; addr++)
        {
            var command1 = me[addr];
            var command2 = me[addr + 1];

            //Add or subtract 0 => remove both
            if (command1 is { Operation: OpEnum.Const, ConstValue: 0 } &&
                (command2.Operation == OpEnum.Add || command2.Operation == OpEnum.Sub))
            {
                me.RemoveAt(ref length, addr);
                me.RemoveAt(ref length, addr);

                //start inspection over
                addr = -1;

                #if DEBUG
                me.VerifyScriptValid(length, true);
                #endif

                continue;
            }

            //Multiply or divide by 1 => remove both
            if (command1 is { Operation: OpEnum.Const, ConstValue: 1 } &&
                (command2.Operation == OpEnum.Mul || command2.Operation == OpEnum.Div))
            {
                me.RemoveAt(ref length, addr);
                me.RemoveAt(ref length, addr);

                //start inspection over
                addr = -1;

                #if DEBUG
                me.VerifyScriptValid(length, true);
                #endif

                continue;
            }

            //Swap before adding or multiplying => remove swap
            if (command1.Operation == OpEnum.Swap &&
                (command2.Operation == OpEnum.Add || command2.Operation == OpEnum.Mul))
            {
                me.RemoveAt(ref length, addr);

                //start inspection over
                addr = -1;

                #if DEBUG
                me.VerifyScriptValid(length, true);
                #endif

                continue;
            }

            //Sign Sign => remove both
            if (command1.Operation == OpEnum.Sign && command2.Operation == OpEnum.Sign)
            {
                me.RemoveAt(ref length, addr);
                me.RemoveAt(ref length, addr);

                //start inspection over
                addr = -1;

                #if DEBUG
                me.VerifyScriptValid(length, true);
                #endif

                continue;
            }

            //Swap Swap => remove both
            if (command1.Operation == OpEnum.Swap && command2.Operation == OpEnum.Swap)
            {
                me.RemoveAt(ref length, addr);
                me.RemoveAt(ref length, addr);

                //start inspection over
                addr = -1;
                
                #if DEBUG
                me.VerifyScriptValid(length, true);
                #endif

                continue;
            }

            //Dup Del => remove both
            if (command1.Operation == OpEnum.Dup && command2.Operation == OpEnum.Del)
            {
                me.RemoveAt(ref length, addr);
                me.RemoveAt(ref length, addr);

                //start inspection over
                addr = -1;
                
                #if DEBUG
                me.VerifyScriptValid(length, true);
                #endif

                continue;
            }

            //Copy and Paste right back => Dup
            if (command1.Operation == OpEnum.Copy && command2.Operation == OpEnum.Paste && command1.Idx == command2.Idx)
            {
                me.RemoveAt(ref length, addr);
                me.RemoveAt(ref length, addr);
                me.Insert(ref length, addr, new Command(OpEnum.Dup));

                //start inspection over
                addr = -1;
                
                #if DEBUG
                me.VerifyScriptValid(length, true);
                #endif

                continue;
            }

            //Square and sqrt => remove both
            if (command1.Operation == OpEnum.Square && command2.Operation == OpEnum.Sqrt)
            {
                me.RemoveAt(ref length, addr);
                me.RemoveAt(ref length, addr);

                //start inspection over
                addr = -1;
                
                #if DEBUG
                me.VerifyScriptValid(length, true);
                #endif

                continue;
            }

            //Cube and Cbrt => remove both
            if (command1.Operation == OpEnum.Cube && command2.Operation == OpEnum.Cbrt)
            {
                me.RemoveAt(ref length, addr);
                me.RemoveAt(ref length, addr);

                //start inspection over
                addr = -1;
                
                #if DEBUG
                me.VerifyScriptValid(length, true);
                #endif

                continue;
            }

            //Cbrt and Cube => remove both
            if (command1.Operation == OpEnum.Cbrt && command2.Operation == OpEnum.Cube)
            {
                me.RemoveAt(ref length, addr);
                me.RemoveAt(ref length, addr);

                //start inspection over
                addr = -1;
                
                #if DEBUG
                me.VerifyScriptValid(length, true);
                #endif

                continue;
            }

            //Const and Del => remove both
            if (command1.Operation == OpEnum.Const && command2.Operation == OpEnum.Del)
            {
                me.RemoveAt(ref length, addr);
                me.RemoveAt(ref length, addr);

                //start inspection over
                addr = -1;
                
                #if DEBUG
                me.VerifyScriptValid(length, true);
                #endif

                continue;
            }

            //Paste and Del => remove both + Copy
            if (command1.Operation == OpEnum.Paste && command2.Operation == OpEnum.Del)
            {
                var idx = command1.Idx;
                var copyAddr = me.GetCopyAddr(idx, addr);

                Debug.Assert(addr > copyAddr);

                me.RemoveAt(ref length, addr);
                me.RemoveAt(ref length, addr);
                me.RemoveAt(ref length, copyAddr);

                //start inspection over
                addr = -1;
                
                #if DEBUG
                me.VerifyScriptValid(length, true);
                #endif

                continue;
            }

            //Load and Del => remove both
            if (command1.Operation == OpEnum.Load && command2.Operation == OpEnum.Del)
            {
                me.RemoveAt(ref length, addr);
                me.RemoveAt(ref length, addr);

                //start inspection over
                addr = -1;
                
                #if DEBUG
                me.VerifyScriptValid(length, true);
                #endif

                continue;
            }

            //Dup and Swap => remove Swap
            if (command1.Operation == OpEnum.Dup && command2.Operation == OpEnum.Swap)
            {
                me.RemoveAt(ref length, addr + 1);

                //start inspection over
                addr = -1;
                
                #if DEBUG
                me.VerifyScriptValid(length, true);
                #endif

                continue;
            }

            ////Abs and Abs => remove one Abs
            //if (command1.Operation == OpEnum.Abs && command2.Operation == OpEnum.Abs)
            //{
            //    me.RemoveAt(ref length, addr);

            //    //start inspection over
            //    addr = -1;
                
            //    #if DEBUG
            //    me.VerifyScriptValid(length);
            //    #endif

            //    continue;
            //}

            //Sign and Abs => remove Sign
            //if (command1.Operation == OpEnum.Sign && command2.Operation == OpEnum.Abs)
            //{
            //    me.RemoveAt(ref length, addr);

            //    //start inspection over
            //    addr = -1;
                
            //    #if DEBUG
            //    me.VerifyScriptValid(length);
            //    #endif

            //    continue;
            //}

            //Dup & Mul => Square
            if (command1.Operation == OpEnum.Dup && command2.Operation == OpEnum.Mul)
            {
                me.RemoveAt(ref length, addr);
                me[addr] = new Command(OpEnum.Square);

                //start inspection over
                addr = -1;
                
                #if DEBUG
                me.VerifyScriptValid(length, true);
                #endif

                continue;
            }
            
            //Square Sqrt => remove both
            if (command1.Operation == OpEnum.Square && command2.Operation == OpEnum.Sqrt)
            {
                me.RemoveAt(ref length, addr);
                me.RemoveAt(ref length, addr);

                //start inspection over
                addr = -1;

                #if DEBUG
                me.VerifyScriptValid(length, true);
                #endif

                // ReSharper disable once RedundantJumpStatement
                continue;
            }

            //3 command combinations
            //if (addr < length - 2)
            //{
            //    var command3 = me[addr];

            //    //dup, mul, square root
            //    if (command1.Operation == OpEnum.Dup && command2.Operation == OpEnum.Mul && command3.Operation == OpEnum.Sqrt)
            //    {
            //        me.RemoveAt(ref length, addr);
            //        me.RemoveAt(ref length, addr);
            //        me.RemoveAt(ref length, addr);

            //        //start inspection over
            //        addr = -1;

            //        #if DEBUG
            //        me.VerifyScriptValid(length);
            //        #endif

            //        // ReSharper disable once RedundantJumpStatement
            //        continue;
            //    }
            //}
        }
    }

    public static int GetCopyAddr(this Span<Command> me, int idx, int pasteAddr)
    {
        for (var addr = pasteAddr; addr >= 0; addr--)
        {
            if (me[addr].Operation == OpEnum.Copy && me[addr].Idx == idx) return addr;
        }
        throw new Exception("TLSCommandArrayFindCopyAddr: can't find Copy");
    }
    public static int GetStackAt(this Span<Command> me, int addr)
    {
        var totalStackEffect = 0;
        // ReSharper disable once ForCanBeConvertedToForeach
        // ReSharper disable once LoopCanBeConvertedToQuery
        for (var i = 0; i < addr; i++)
        {
            totalStackEffect += me[i].StackEffect;
        }
        return totalStackEffect;
    }
    public static int GetMaxCopyIdx(this Span<Command> me, int length)
    {
        var maxCopyIdx = 0;
        for (var addr = 0; addr < length; addr++)
        {
            if (me[addr].Operation == OpEnum.Copy && me[addr].Idx > maxCopyIdx) maxCopyIdx = me[addr].Idx;
        }
        return maxCopyIdx;
    }

    public static int InsertRandomAt(this Span<Command> me, ref int length, int addr, int stackEffect, int stackSize, byte inputsCount, OpEnum[] allowedOperations, int allowedAdjunctOperationsCount)
    {
        var maxCopyId = me.GetMaxCopyIdx(length);

        if (stackEffect == 2)
        {
            var newCommand1 = Command.CreateRandom(inputsCount, maxCopyId, 1, stackSize, allowedOperations, allowedAdjunctOperationsCount);
            var newCommand2 = Command.CreateRandom(inputsCount, maxCopyId + 1, 1, stackSize + newCommand1.StackEffect, allowedOperations, allowedAdjunctOperationsCount);
            //Console.WriteLine($"Inserting compensating commands {newCommand1.Print('A')} & {newCommand2.Print('A')} at {addr+1}");
            me.Insert(ref length, addr, newCommand2);
            me.Insert(ref length, addr, newCommand1);

            var commandsAdded = 2;
            commandsAdded += me.InsertAdjunctCommandIfNeeded(ref length, newCommand1, addr);
            commandsAdded += me.InsertAdjunctCommandIfNeeded(ref length, newCommand2, addr + commandsAdded - 1);
            return commandsAdded;
        }
        if (stackEffect == -2)
        {
            var newCommand1 = Command.CreateRandom(inputsCount, maxCopyId, -1, stackSize, allowedOperations, allowedAdjunctOperationsCount);
            var newCommand2 = Command.CreateRandom(inputsCount, maxCopyId + 1, -1, stackSize + newCommand1.StackEffect, allowedOperations, allowedAdjunctOperationsCount);
            me.Insert(ref length, addr, newCommand2);
            me.Insert(ref length, addr, newCommand1);

            var commandsAdded = 2;
            commandsAdded += me.InsertAdjunctCommandIfNeeded(ref length, newCommand1, addr);
            commandsAdded += me.InsertAdjunctCommandIfNeeded(ref length, newCommand2, addr + commandsAdded - 1);
            return commandsAdded;
        }

        var newCommand = Command.CreateRandom(inputsCount, maxCopyId, stackEffect, stackSize, allowedOperations, allowedAdjunctOperationsCount);
        me.Insert(ref length, addr, newCommand);

        return me.InsertAdjunctCommandIfNeeded(ref length, newCommand, addr) + 1;
    }
    public static int InsertAdjunctCommandIfNeeded(this Span<Command> me, ref int length, Command mainCommand, int mainCommandAddress)
    {
        if (mainCommand.Operation == OpEnum.Paste) return me.InsertCopy(ref length, mainCommand.Idx, mainCommandAddress);
        else return 0;
    }
    public static int InsertCopy(this Span<Command> me, ref int length, int idx, int pasteAddr)
    {
        var copyCommand = new Command(OpEnum.Copy, idx);
        var addr = Rnd.Random.Next(pasteAddr) + 1;
        me.Insert(ref length, addr, copyCommand);
        return 1;
    }
    #endregion

    #region Script Validation Methods
    public static void VerifyScriptValid(this Span<Command> me, int length, bool isForDebugging)
    {
        try
        {
            var stackCount = 0;
            // ReSharper disable once ForCanBeConvertedToForeach
            // ReSharper disable once LoopCanBeConvertedToQuery
            for (var addr = 0; addr < length; addr++)
            {
                var command = me[addr];
                if (command.MinStackRequired > stackCount) throw new Exception("command.MinStackRequired > stackCount");
                if (command.Operation == OpEnum.Paste) me.GetCopyAddr(command.Idx, addr);

                stackCount += me[addr].StackEffect;
            }
            if (stackCount != 1) throw new Exception("stackCount != 1");
        }
        catch (Exception ex)
        {
            if (isForDebugging)
            {
                Notifications.SendSystemMessageSMTP(BConfig.ToEmail, $"Beagle {BConfig.Version}: Invalid mutation detected on {Environment.MachineName}!", ex.ToString(), MailPriority.High);
                Console.WriteLine(ex);
                Debugger.Break();
            }
            throw;
        }
    }
    #endregion

    #region Quasi-List Methods
    public static void Add(this Span<Command> me, ref int length, Command command)
    {
        me[length++] = command;
    }
    public static void Insert(this Span<Command> me, ref int length, int addr, Command command)
    {
        for (var i = length; i > addr; i--)
        {
            me[i] = me[i - 1];
        }
        me[addr] = command;
        length++;
    }
    public static void RemoveAt(this Span<Command> me, ref int length, int addr)
    {
        for (var i = addr + 1; i < length; i++)
        {
            me[i - 1] = me[i];
        }
        length--;
    }
    #endregion

    #region ToString Methods
    public static string ToString(this Span<Command> me, ref int length)
    {
        var sb = new StringBuilder();
        for (var addr = 0; addr < length; addr++)
        {
            sb.Append(addr + 1);
            sb.Append(": ");
            sb = me[addr].AppendToStringBuilder(MLSetup.Current.GetInputLabels(), sb);
            sb.AppendLine();
        }
        return sb.ToString();
    }
    #endregion
}