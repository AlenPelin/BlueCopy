using System;

namespace BlueCopy.Core
{
  public class CustomAlphabetKeyGenerator : IKeyGenerator
  {
    public char[] CustomAlphabet { get; }

    public long Radix { get; }

    public CustomAlphabetKeyGenerator(char[] customAlphabet)
    {
      CustomAlphabet = customAlphabet;
      Radix = customAlphabet.Length;
    }

    public CustomAlphabetKeyGenerator()
      : this("0123456789abcdefghijklmnopqrstuvwxyq".ToCharArray()) // base36
    {
    }

    public string[] GenerateNewId()
    {
      var now = DateTime.UtcNow;
      var num = now.Ticks - new DateTime(2018, 3, 21).Ticks;
      var result = ConvertLongToAnyBase(num);

      return new[] { result.Substring(0, 3), result.Substring(3) };
    }

    private string ConvertLongToAnyBase(long decimalNumber)
    {
      // http://www.pvladov.com/2012/05/decimal-to-arbitrary-numeral-system.html

      const int BitsInLong = sizeof(long) * 8; // 64

      if (decimalNumber == 0)
      {
        return "0000";
      }

      var index = BitsInLong - 1;
      var currentNumber = Math.Abs(decimalNumber);
      var charArray = new char[BitsInLong];

      while (currentNumber != 0)
      {
        var remainder = (int)(currentNumber % Radix);

        charArray[index--] = CustomAlphabet[remainder];
        currentNumber = currentNumber / Radix;
      }

      var result = new String(charArray, index + 1, BitsInLong - index - 1);
      switch (result.Length)
      {
        case 1:
          result = $"000{result}";
          break;

        case 2:
          result = $"00{result}";
          break;

        case 3:
          result = $"0{result}";
          break;
      }

      var prefix = decimalNumber < 0 ? "-" : "";

      return prefix + result;
    }
  }
}
