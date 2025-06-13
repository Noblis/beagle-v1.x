﻿using System.Diagnostics;
using BeagleLib.Util;

namespace BeagleLib.VM;

public readonly struct Command
{
    #region Constructors & Factory Methods
    public Command(OpEnum operation)
    {
        _operation = operation;
        _value = 0;

        Debug.Assert(CommandType == CommandTypeEnum.CommandOnly, $"Command constructor for operation {operation} has invalid  parameters");
    }
    public Command(OpEnum operation, float constValue, bool assertDisabled = false)
    {
        _operation = operation;
        _value = constValue;

        Debug.Assert(assertDisabled || CommandType == CommandTypeEnum.CommandPlusFloat, $"Command constructor for operation {operation} has invalid  parameters");
    }
    public Command(OpEnum operation, int idx)
    {
        _operation = operation;
        _value = idx;

        Debug.Assert(CommandType == CommandTypeEnum.CommandPlusIndex, $"Command constructor for operation {operation} has invalid  parameters");
    }

    public static Command EndOfScript { get; } = new(OpEnum.EndOfScript, 0f, true);

    public static Command CreateRandom(byte inputsCount, int maxCopyIdx, Span<OpEnum> allowedOperations, int allowedAdjunctOperationsCount)
    {
        var randomOperation = allowedOperations[Rnd.Random.Next(allowedOperations.Length - allowedAdjunctOperationsCount)];
        Debug.Assert(randomOperation != OpEnum.Copy);
        var randomOperationProperties = randomOperation.GetOperationProperties();

        switch (randomOperationProperties.CommandType)
        {
            case CommandTypeEnum.CommandOnly:
            {
                return new Command(randomOperation);
            }
            case CommandTypeEnum.CommandPlusIndex:
            {
                //special handling for Paste
                if (randomOperation == OpEnum.Paste) return new Command(randomOperation, maxCopyIdx + 1);

                return new Command(randomOperation, Rnd.Random.Next(inputsCount));
            }
            case CommandTypeEnum.CommandPlusFloat:
            {
                //1% chance for Pi or E, 98% chance for a random floating point number 0-10
                var randomPct = Rnd.Random.Next(101);
                if (randomPct >= 99) return new Command(randomOperation, (float)Math.PI);
                if (randomPct >= 98) return new Command(randomOperation, (float)Math.E);
                return new Command(randomOperation, (float)Rnd.Random.Next(MaxRandomFloatPlus1));
            }
            default: throw new Exception($"Unknown command type {randomOperationProperties.CommandType}");
        }
    }
    public static Command CreateRandom(byte inputsCount, int maxCopyIdx, int? stackEffect, int stackSize, OpEnum[] allowedOperations, int allowedAdjunctOperationsCount)
    {
        if (stackEffect == -1 && stackSize <= 1) throw new Exception("stackEffect == -1 && stackSize <= 1");

        //create span for VALID allowed operations based on stackEffect and stackSize
        if (_validAllowedOperations == null)
        {
            var operationEnumValues = Enum.GetValues(typeof(OpEnum));
            _validAllowedOperations = new OpEnum[operationEnumValues.Length - 1]; //we do -1 because the first command is EndOfScript 
        }

        var validAllowedOperationsLength = 0;
        for (var i = 0; i < allowedOperations.Length; i++)
        {
            var opProp = allowedOperations[i].GetOperationProperties();

            if (stackEffect != null && opProp.StackEffect != stackEffect) continue;
            if (opProp.MinStackRequired > stackSize) continue;

            _validAllowedOperations[validAllowedOperationsLength++] = allowedOperations[i];
        }
        var validAllowedOperationsSpan = new Span<OpEnum>(_validAllowedOperations, 0, validAllowedOperationsLength);

        var command = CreateRandom(inputsCount, maxCopyIdx, validAllowedOperationsSpan, allowedAdjunctOperationsCount);
        return command;

        // ReSharper disable once TooWideLocalVariableScope
        // ReSharper disable once RedundantAssignment
        //var count = 1000;
        //while (true)
        //{
        //    Debug.Assert(--count > 0);

        //    var command = CreateRandom(inputsCount, maxCopyIdx, allowedOperations, allowedAdjunctOperationsCount);

        //    if (stackEffect != null && command.StackEffect != stackEffect) continue;
        //    if (command.MinStackRequired > stackSize) continue;

        //    return command;
        //}
    }
    public static Command CreateRandomLoadOrConst(byte inputsCount)
    {
        if (Rnd.RandomBool()) return CreateRandomLoad(inputsCount);
        else return CreateRandomConst();
    }
    public static Command CreateRandomConst()
    {
        var constValue = (float)Rnd.Random.Next(MaxRandomFloatPlus1);
        return new Command(OpEnum.Const, constValue);
    }
    public static Command CreateRandomLoad(byte inputsCount)
    {
        var idx = Rnd.Random.Next(inputsCount);
        return new Command(OpEnum.Load, idx);
    }
    #endregion

    #region Methods
    public override string ToString()
    {
        switch (CommandType)
        {
            case CommandTypeEnum.CommandOnly:
            {
                return $"{Operation.ToString().ToUpper()}";
            }
            case CommandTypeEnum.CommandPlusFloat:
            {
                return $"{Operation.ToString().ToUpper()} {ConstValue}";
            }
            case CommandTypeEnum.CommandPlusIndex:
            {
                //special case
                if (Operation == OpEnum.Copy || Operation == OpEnum.Paste) return $"{Operation.ToString().ToUpper()} @{Idx}";

                //default for loads
                return $"{Operation.ToString().ToUpper()} {(char)('0' + Idx)}";
            }
            default: throw new Exception($"Unknown CommandType {CommandType}");
        }
    }
    #endregion

    #region Properties
    public OpEnum Operation => _operation;
    public CommandTypeEnum CommandType => _operation.GetOperationProperties().CommandType;
    public sbyte StackEffect => _operation.GetOperationProperties().StackEffect;
    public byte MinStackRequired => _operation.GetOperationProperties().MinStackRequired;
    public float ConstValue
    {
        get
        {
            Debug.Assert(CommandType == CommandTypeEnum.CommandPlusFloat);
            return _value;
        }
    }
    public int Idx
    {
        get
        {
            Debug.Assert(CommandType == CommandTypeEnum.CommandPlusIndex);
            return (int)_value;
        }
    }
    #endregion

    #region Fields
    private readonly OpEnum _operation;
    private readonly float _value;
    private const int MaxRandomFloatPlus1 = 11;

    [ThreadStatic] private static OpEnum[]? _validAllowedOperations;
    #endregion
}