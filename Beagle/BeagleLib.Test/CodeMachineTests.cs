using BeagleLib.VM;
using NUnit.Framework.Legacy;

namespace BeagleLib.Test;

#pragma warning disable NUnit2005
#pragma warning disable NUnit2007
public class CodeMachineTests
{
    #region Setup
    [SetUp]
    public void Setup()
    {
        _codeMachine = new CodeMachine();
        _inputs = [1f, 2f, 3f];
    }
    #endregion

    #region Tests
    [Test] public void TestAddLoadConst()
    {
        var commands = new[]
        {
            new Command(OpEnum.Load, 0),
            new Command(OpEnum.Const, 5.0f),
            new Command(OpEnum.Add)
        };
        var result = _codeMachine.RunCommands(_inputs, commands);
        ClassicAssert.AreEqual(result, 6);
    }
    [Test] public void TestMulLoadDup()
    {
        var commands = new[]
        {
            new Command(OpEnum.Load, 1),
            new Command(OpEnum.Dup),
            new Command(OpEnum.Mul)
        };
        var result = _codeMachine.RunCommands(_inputs, commands);
        ClassicAssert.AreEqual(result, 4);
    }
    [Test] public void TestSubLoadConst()
    {
        var commands = new[]
        {
            new Command(OpEnum.Const, 4.0f),
            new Command(OpEnum.Load, 2),
            new Command(OpEnum.Sub)
        };
        var result = _codeMachine.RunCommands(_inputs, commands);
        ClassicAssert.AreEqual(result, 1);
    }
    [Test] public void TestDivLoadConstSign()
    {
        var commands = new[]
        {
            new Command(OpEnum.Const, 9.0f),
            new Command(OpEnum.Load, 2),
            new Command(OpEnum.Div),
            new Command(OpEnum.Sign)
        };
        var result = _codeMachine.RunCommands(_inputs, commands);
        ClassicAssert.AreEqual(result, -3);
    }
    [Test] public void TestConstDupSqrt()
    {
        var commands = new[]
        {
            new Command(OpEnum.Const, 9.0f),
            new Command(OpEnum.Dup),
            new Command(OpEnum.Mul),
            new Command(OpEnum.Sqrt)
        };
        var result = _codeMachine.RunCommands(_inputs, commands);
        ClassicAssert.AreEqual(result, 9);
    }
    [Test] public void TestConstDupCbrt()
    {
        var commands = new[]
        {
            new Command(OpEnum.Const, -9.0f),
            new Command(OpEnum.Dup),
            new Command(OpEnum.Dup),
            new Command(OpEnum.Mul),
            new Command(OpEnum.Mul),
            new Command(OpEnum.Cbrt)
        };
        var result = _codeMachine.RunCommands(_inputs, commands);
        ClassicAssert.AreEqual(result, -9);
    }
    [Test] public void TestConstSwap()
    {
        var commands = new[]
        {
            new Command(OpEnum.Const, 9.0f),
            new Command(OpEnum.Const, 10f),
            new Command(OpEnum.Swap),
            new Command(OpEnum.Sub)
        };
        var result = _codeMachine.RunCommands(_inputs, commands);
        ClassicAssert.AreEqual(result, 1);
    }
    [Test] public void TestCopyPaste()
    {
        var commands = new[]
        {
            new Command(OpEnum.Const, 9.0f),
            new Command(OpEnum.Copy, 1),
            new Command(OpEnum.Const, 10f),
            new Command(OpEnum.Copy, 2),
            new Command(OpEnum.Swap),
            new Command(OpEnum.Sub),
            new Command(OpEnum.Paste, 1),
            new Command(OpEnum.Add),
            new Command(OpEnum.Paste, 2),
            new Command(OpEnum.Sub)
        };
        var result = _codeMachine.RunCommands(_inputs, commands);
        ClassicAssert.AreEqual(result, 0);
    }
    [Test] public void TestSquareAndCube()
    {
        var commands = new[]
        {
            new Command(OpEnum.Const, 2.0f),
            new Command(OpEnum.Square),
            new Command(OpEnum.Cube)
        };
        var result = _codeMachine.RunCommands(_inputs, commands);
        ClassicAssert.AreEqual(result, 64);
    }
    [Test] public void TestLn()
    {
        var commands = new[]
        {
            new Command(OpEnum.Const, 16.0f),
            new Command(OpEnum.Ln),
        };
        var result = _codeMachine.RunCommands(_inputs, commands);
        ClassicAssert.AreEqual(result, 2.77258873f);
    }
    [Test] public void TestSin()
    {
        var commands = new[]
        {
            new Command(OpEnum.Const, (float)Math.PI/2),
            new Command(OpEnum.Sin),
        };
        var result = _codeMachine.RunCommands(_inputs, commands);
        ClassicAssert.AreEqual(result, (float)Math.Sin((float)Math.PI / 2));
    }
    [Test]
    public void TestCos()
    {
        var commands = new[]
        {
            new Command(OpEnum.Const, (float)Math.PI/2),
            new Command(OpEnum.Cos),
        };
        var result = _codeMachine.RunCommands(_inputs, commands);
        ClassicAssert.AreEqual(result, (float)Math.Cos((float)Math.PI / 2));
    }

    [Test]
    public void TestTan()
    {
        var commands = new[]
        {
            new Command(OpEnum.Const, (float)Math.PI/2),
            new Command(OpEnum.Tan),
        };
        var result = _codeMachine.RunCommands(_inputs, commands);
        ClassicAssert.AreEqual(result, (float)Math.Tan((float)Math.PI / 2));
    }
    [Test]
    public void TestArcos()
    {
        var commands = new[]
        {
            new Command(OpEnum.Const, (float)Math.PI/2),
            new Command(OpEnum.Arcos),
        };
        var result = _codeMachine.RunCommands(_inputs, commands);
        ClassicAssert.AreEqual(result, (float)Math.Acos((float)Math.PI / 2));
    }
    [Test]
    public void TestArcsin()
    {
        var commands = new[]
        {
            new Command(OpEnum.Const, (float)Math.PI/2),
            new Command(OpEnum.Arcsin),
        };
        var result = _codeMachine.RunCommands(_inputs, commands);
        ClassicAssert.AreEqual(result, (float)Math.Asin((float)Math.PI / 2));
    }
    [Test]
    public void TestArctan()
    {
        var commands = new[]
        {
            new Command(OpEnum.Const, (float)Math.PI/2),
            new Command(OpEnum.Arctan),
        };
        var result = _codeMachine.RunCommands(_inputs, commands);
        ClassicAssert.AreEqual(result, (float)Math.Atan((float)Math.PI / 2));
    }
    [Test]
    public void TestTanh()
    {
        var commands = new[]
        {
            new Command(OpEnum.Const, (float)Math.PI/2),
            new Command(OpEnum.Tanh),
        };
        var result = _codeMachine.RunCommands(_inputs, commands);
        ClassicAssert.AreEqual(result, (float)Math.Tanh((float)Math.PI / 2));
    }
    [Test]
    public void TestExp()
    {
        var commands = new[]
        {
            new Command(OpEnum.Const, (float)Math.PI/2),
            new Command(OpEnum.Exp),
        };
        var result = _codeMachine.RunCommands(_inputs, commands);
        ClassicAssert.AreEqual(result, (float)Math.Exp((float)Math.PI / 2));
    }

    [Test]
    public void TestPowLoadDup()
    {
        var commands = new[]
        {
            new Command(OpEnum.Load, 1),
            new Command(OpEnum.Dup),
            new Command(OpEnum.Pow)
        };
        var result = _codeMachine.RunCommands(_inputs, commands);
        ClassicAssert.AreEqual(result, 4);
    }
    //[Test] public void TestAbs()
    //{
    //    var commands = new[]
    //    {
    //        new Command(OpEnum.Const, -5f),
    //        new Command(OpEnum.Abs),
    //    };
    //    var result = _codeMachine.RunCommands(_inputs, commands);
    //    ClassicAssert.AreEqual(result, 5f);
    //}

    [Test] public void TestDel1()
    {
        var commands = new[]
        {
            new Command(OpEnum.Const, 1f),
            new Command(OpEnum.Const, 2f),
            new Command(OpEnum.Del)
        };
        var result = _codeMachine.RunCommands(_inputs, commands);
        ClassicAssert.AreEqual(result, 1f);
    }
    [Test] public void TestDel2()
    {
        var commands = new[]
        {
            new Command(OpEnum.Const, 1f),
            new Command(OpEnum.Del),
            new Command(OpEnum.Const, 2f)
        };
        var result = _codeMachine.RunCommands(_inputs, commands);
        ClassicAssert.AreEqual(result, 2f);
    }

    //[Test]
    //public void TestConstRound1()
    //{
    //    var (result, _) = _codeMachine.RunCommands(new[]
    //    {
    //        new Command(OpEnum.Const, 9.6f),
    //        new Command(OpEnum.Round)
    //    });
    //    ClassicAssert.AreEqual(result, 10);
    //}
    //[Test]
    //public void TestConstRound2()
    //{
    //    var (result, _) = _codeMachine.RunCommands(new[]
    //    {
    //        new Command(OpEnum.Const, 9.3f),
    //        new Command(OpEnum.Round)
    //    });
    //    ClassicAssert.AreEqual(result, 9);
    //}
    #endregion

    #region Fields
    protected CodeMachine _codeMachine;
    protected float[] _inputs; 
    #endregion
}
#pragma warning restore NUnit2007
#pragma warning restore NUnit2005
