using BeagleLib.Engine;
using BeagleLib.Util;
using BeagleLib.VM;
using System.Diagnostics;
using System.Net.Mail;
using System.Text;

namespace BeagleLib.Agent
{
    public static class CommandSpanExt
    {
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
            // Skip ANY insertion (including compensating/adjunct inserts that funnel through here) when the
            // script is already at its max length — otherwise the shift write me[i] = me[i-1] at
            // i == length == me.Length writes past the span and throws IndexOutOfRange.
            if (length >= me.Length) return;
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

        #region Helper Methods
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
}
