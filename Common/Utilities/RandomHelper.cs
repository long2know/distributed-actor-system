using System;
using System.Linq;

namespace Common.Utilities
{
    public static class RandomHelper
    {
        private static Random _random = new Random();
        private static string _alphabet = "abcdefghijklmnopqrstuvwxyz";
        private static string _numbers = "0123456789";

        /// <summary>
        /// Generate a random code
        /// </summary>
        /// <returns></returns>
        public static string RandCode()
        {
            var dealerCode = string.Format("{0}{1}",
                string.Concat(Enumerable.Range(0, 2).Select(x => _alphabet[_random.Next(0, _alphabet.Length)])),
                string.Concat(Enumerable.Range(0, 4).Select(x => _numbers[_random.Next(0, _numbers.Length)]))
                );
            return dealerCode;
        }

        /// <summary>
        /// Generate a random name
        /// </summary>
        /// <returns></returns>
        public static string RandName()
        {
            var firstName = string.Concat(Enumerable.Range(0, _random.Next(5, 10)).Select(x => _alphabet[_random.Next(0, _alphabet.Length)]));
            var lastName = string.Concat(Enumerable.Range(0, _random.Next(5, 10)).Select(x => _alphabet[_random.Next(0, _alphabet.Length)]));
            return string.Format("{0} {1}", firstName, lastName);
        }

        /// <summary>
        /// Generate a random MTN
        /// </summary>
        /// <returns></returns>
        public static string RandMTN()
        {
            var mtn = string.Format("{0}-{1}-{2}",
                string.Concat(Enumerable.Range(0, 3).Select(x => _numbers[_random.Next(0, _numbers.Length)])),
                 string.Concat(Enumerable.Range(0, 3).Select(x => _numbers[_random.Next(0, _numbers.Length)])),
                 string.Concat(Enumerable.Range(0, 4).Select(x => _numbers[_random.Next(0, _numbers.Length)]))
             );
            return mtn;
        }

        /// <summary>
        /// Generate a random MTN
        /// </summary>
        /// <returns></returns>
        public static string RandESN()
        {
            var selection = string.Concat(_alphabet, _numbers);
            var esn = string.Concat(Enumerable.Range(0, 11).Select(x => selection[_random.Next(0, selection.Length)]));
            return esn;
        }

        /// <summary>
        /// Random market a random code
        /// </summary>
        /// <returns></returns>
        public static string RandMarket()
        {
            var market = string.Format("{0}",
                string.Concat(Enumerable.Range(0, 2).Select(x => _alphabet[_random.Next(0, _alphabet.Length)]))
            );
            return market.ToUpper();
        }
    }
}