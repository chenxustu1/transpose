using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Transpose.Translator.Tests
{
    /// <summary>
    /// Tests for the null-forgiving operator (!) emit fix.
    /// The null-forgiving operator is a C# compile-time construct that should not appear
    /// in the generated JavaScript. Previously, Transpose incorrectly emitted `expr!` which
    /// caused NUglify parse errors (the `!` was interpreted as logical NOT after an expression).
    /// </summary>
    [TestClass]
    public class SuppressNullableWarningTests
    {
        [TestMethod]
        public void NullForgivingOnMethodCallDoesNotEmitExclamation()
        {
            var code = @"
using System;
public class Program
{
    static string? GetString() => ""hello"";
    public static void Main()
    {
        var result = GetString()!;
        Console.WriteLine(result);
    }
}";
            var result = new RoslynTranslator().Translate(code);
            Assert.IsTrue(result.Success, "translation should succeed");
            // The null-forgiving operator should not appear in the output
            Assert.IsFalse(result.Javascript!.Contains("GetString()!"),
                "null-forgiving operator should not be emitted in JavaScript\n" + result.Javascript);
        }

        [TestMethod]
        public void NullForgivingOnVariableDoesNotEmitExclamation()
        {
            var code = @"
using System;
public class Program
{
    public static void Main()
    {
        string? s = ""hello"";
        string nonNull = s!;
        Console.WriteLine(nonNull);
    }
}";
            var result = new RoslynTranslator().Translate(code);
            Assert.IsTrue(result.Success, "translation should succeed");
            // Should not contain pattern like `variable!;`
            Assert.IsFalse(result.Javascript!.Contains("s!;"),
                "null-forgiving operator should not be emitted in JavaScript\n" + result.Javascript);
        }

        [TestMethod]
        public void NullForgivingOnNewExpressionDoesNotEmitExclamation()
        {
            var code = @"
using System;
public class Program
{
    public static void Main()
    {
        var obj = new object()!;
        Console.WriteLine(obj);
    }
}";
            var result = new RoslynTranslator().Translate(code);
            Assert.IsTrue(result.Success, "translation should succeed");
            Assert.IsFalse(result.Javascript!.Contains("$ctor1()!"),
                "null-forgiving operator should not be emitted in JavaScript\n" + result.Javascript);
        }

        [TestMethod]
        public void NullForgivingOnConditionalExpressionDoesNotEmitExclamation()
        {
            var code = @"
using System;
public class Program
{
    public static void Main()
    {
        string? s = null;
        var result = (s ?? ""default"")!;
        Console.WriteLine(result);
    }
}";
            var result = new RoslynTranslator().Translate(code);
            Assert.IsTrue(result.Success, "translation should succeed");
            // The ?? operator should be present, but not followed by !
            Assert.IsFalse(result.Javascript!.Contains("?? \"default\")!"),
                "null-forgiving operator should not be emitted after conditional expression\n" + result.Javascript);
        }

        [TestMethod]
        public void NullForgivingOnCastDoesNotEmitExclamation()
        {
            var code = @"
using System;
public class Program
{
    public static void Main()
    {
        object obj = ""hello"";
        string s = (string)obj!;
        Console.WriteLine(s);
    }
}";
            var result = new RoslynTranslator().Translate(code);
            Assert.IsTrue(result.Success, "translation should succeed");
            Assert.IsFalse(result.Javascript!.Contains("obj)!"),
                "null-forgiving operator should not be emitted in JavaScript\n" + result.Javascript);
        }

        [TestMethod]
        public void RegularPostfixIncrementStillWorks()
        {
            var code = @"
using System;
public class Program
{
    public static void Main()
    {
        int i = 0;
        int j = i++;
        Console.WriteLine(j);
    }
}";
            var result = new RoslynTranslator().Translate(code);
            Assert.IsTrue(result.Success, "translation should succeed");
            // Postfix increment should still work
            Assert.IsTrue(result.Javascript!.Contains("++") || result.Javascript!.Contains("= $v + 1"),
                "postfix increment should still be emitted correctly\n" + result.Javascript);
        }
    }
}
